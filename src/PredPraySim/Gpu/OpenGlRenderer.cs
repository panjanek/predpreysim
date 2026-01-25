using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenTK.GLControl;
using OpenTK.Windowing.Common;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using AppContext = PredPraySim.Models.AppContext;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Panel = System.Windows.Controls.Panel;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using PredPraySim.Models;

namespace PredPraySim.Gpu
{
    public class OpenGlRenderer
    {
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
                APIVersion = new Version(3, 3), // OpenGL 3.3
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

            glControl.Paint += GlControl_Paint;
            glControl.SizeChanged += GlControl_SizeChanged;
            GlControl_SizeChanged(this, null);
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

        private void GlControl_Paint(object? sender, PaintEventArgs e)
        {
            //TODO
            displayProgram.Draw(app.simulation, GetProjectionMatrix(), solverProgram.AgentsBuffer);

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
                solverProgram.Run(ref app.simulation.shaderConfig);
            }

            glControl.Invalidate();
        }

        public void UploadAgents(Agent[] agents) => solverProgram.UploadAgents(app.simulation.shaderConfig, app.simulation.agents);
    }
}
