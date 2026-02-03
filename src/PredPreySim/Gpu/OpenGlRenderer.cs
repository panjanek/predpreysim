using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenTK.GLControl;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common;
using PredPreySim.Models;
using PredPreySim.Utils;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using AppContext = PredPreySim.Models.AppContext;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Panel = System.Windows.Controls.Panel;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace PredPreySim.Gpu
{
    public class OpenGlRenderer
    {
        public const double ZoomingSpeed = 0.0005;
        public int FrameCounter => frameCounter;

        public bool Paused { get; set; }

        private GLControl glControl;

        private int frameCounter;

        private AppContext app;

        private Panel placeholder;

        private System.Windows.Forms.Integration.WindowsFormsHost host;

        private SolverProgram solverProgram;

        private DisplayProgram displayProgram;

        private float zoom = 0.5f;

        private Vector2 center;

        private Agent tracked;

        public OpenGlRenderer(Panel placeholder, AppContext app)
        {
            this.placeholder = placeholder;
            this.app = app;
            host = new System.Windows.Forms.Integration.WindowsFormsHost();
            host.Visibility = Visibility.Visible;
            host.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            host.VerticalAlignment = VerticalAlignment.Stretch;
            glControl = new GLControl(new GLControlSettings
            {
                API = OpenTK.Windowing.Common.ContextAPI.OpenGL,
                APIVersion = new Version(4, 3), 
                Profile = ContextProfile.Compatability,
                Flags = ContextFlags.Default,
                IsEventDriven = false,
                NumberOfSamples = 16,
            });
            glControl.Dock = DockStyle.Fill;
            host.Child = glControl;
            placeholder.Children.Add(host);

            solverProgram = new SolverProgram();
            displayProgram = new DisplayProgram();

            var dragging = new DraggingHandler(glControl, (mousePos, isLeft) => isLeft, (prev, curr) =>
            {
                var delta = (curr - prev) / zoom;
                delta.Y = -delta.Y;
                center -= delta;

            }, () => { });

            glControl.MouseWheel += (s, e) =>
            {
                float zoomRatio = (float)(1.0 + ZoomingSpeed * e.Delta);

                var projectionMatrix = GetProjectionMatrix();
                var topLeft1 = GpuUtil.ScreenToWorld(new Vector2(0, 0), projectionMatrix, glControl.Width, glControl.Height);
                var bottomRight1 = GpuUtil.ScreenToWorld(new Vector2(glControl.Width, glControl.Height), projectionMatrix, glControl.Width, glControl.Height);
                var zoomCenter = app.configWindow.NavigationMode == 0
                                 ? GpuUtil.ScreenToWorld(new Vector2(e.X, e.Y), projectionMatrix, glControl.Width, glControl.Height)
                                 : app.renderer.tracked.position;

                var currentSize = bottomRight1 - topLeft1;
                var newSize = currentSize / (float)zoomRatio;

                var c = zoomCenter - topLeft1;
                var b = c / (float)zoomRatio;

                var topLeft2 = zoomCenter - b;
                var bottomRight2 = topLeft2 + newSize;

                center = (bottomRight2 + topLeft2) / 2;
                zoom = zoom * zoomRatio;
            };

            glControl.Paint += GlControl_Paint;
            glControl.SizeChanged += GlControl_SizeChanged;
            GlControl_SizeChanged(this, null);
        }

        public void ResetOrigin()
        {
            center = new Vector2(app.simulation.shaderConfig.width / 2, app.simulation.shaderConfig.height / 2);
            zoom = 1f;
        }

        private void GlControl_SizeChanged(object? sender, EventArgs e)
        {
            if (glControl.Width <= 0 || glControl.Height <= 0)
                return;

            if (!glControl.Context.IsCurrent)
                glControl.MakeCurrent();

            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            glControl.Invalidate();
        }

        private Matrix4 GetProjectionMatrix()
        {
            // rescale by windows display scale setting to match WPF coordinates
            var w = (float)((glControl.Width / 1) / zoom) / 2;
            var h = (float)((glControl.Height / 1) / zoom) / 2;
            var translate = Matrix4.CreateTranslation(-center.X, -center.Y, 0.0f);
            var ortho = Matrix4.CreateOrthographicOffCenter(-w, w, -h, h, -1f, 1f);
            var matrix = translate * ortho;
            return matrix;
        }

        private void Follow()
        {
            if (app.configWindow.NavigationMode != 0)
            {
                var trackedScreenPosition = tracked.position;
                var delta = trackedScreenPosition - center; 
                var move = delta * 0.03f;

                if (Math.Abs(delta.X) > 0.75 * app.simulation.shaderConfig.width)
                {
                    move.X = (float)Math.Sign(delta.X) * app.simulation.shaderConfig.width;
                }

                if (Math.Abs(delta.Y) > 0.75 * app.simulation.shaderConfig.height)
                {
                    move.Y = (float)Math.Sign(delta.Y) * app.simulation.shaderConfig.height;
                }
                center += move;
            }
        }

        private void GlControl_Paint(object? sender, PaintEventArgs e)
        {
            Follow();

            //clear
            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            float halfW = glControl.Width / zoom * 0.5f;
            float halfH = glControl.Height / zoom * 0.5f;
            Vector2 min = center - new Vector2(halfW, halfH);
            Vector2 max = center + new Vector2(halfW, halfH);

            displayProgram.Draw(app.simulation, 
                                GetProjectionMatrix(), 
                                solverProgram.AgentsBuffer, 
                                solverProgram.GreenTex, 
                                solverProgram.BlueTex, 
                                solverProgram.RedTex, 
                                min, max, zoom,
                                app.configWindow.ShowPointers);

            glControl.SwapBuffers();
            frameCounter++;

        }

        public void Step()
        {
            if (Application.Current.MainWindow == null || Application.Current.MainWindow.WindowState == System.Windows.WindowState.Minimized)
                return;

            //compute
            if (!Paused)
            {
                app.simulation.shaderConfig.t += app.simulation.shaderConfig.dt;
                app.simulation.shaderConfig.trackedIdx = GetTrackedIdx();
                solverProgram.Run(ref app.simulation.shaderConfig, app.simulation.kernelRed, app.simulation.kernelGreen, app.simulation.kernelBlue);
                tracked = solverProgram.DownloadTrackedAgent();

                app.simulation.step++;
                if (app.simulation.step % app.simulation.shaderConfig.generationDuration == 0 && app.configWindow.Evolve)
                {
                    DownloadAgents();
                    app.simulation.ChangeEpoch();
                    app.DrawStats();
                    UploadAgents();
                }
            }

            glControl.Invalidate();
        }

        private int GetTrackedIdx()
        {
            if (app.configWindow.NavigationMode == 1 && app.simulation.topBlueIds?.Count > 0)
                return app.simulation.topBlueIds[0];
            else if (app.configWindow.NavigationMode == 2 && app.simulation.topRedIds?.Count > 0)
                return app.simulation.topRedIds[0];
            else
                return 0;
        }

        public void UploadAgents() => solverProgram.UploadAgents(app.simulation.shaderConfig, app.simulation.agents, app.simulation.network);

        public void DownloadAgents() => solverProgram.DownloadAgents(app.simulation.agents);

        public Agent DownloadTrackedAgent() => solverProgram.DownloadTrackedAgent();

        public void ClearTextures() => solverProgram.ClearTextures();
    }
}
