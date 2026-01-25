using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using PredPraySim.Models;

namespace PredPraySim.Gpu
{
    public class SolverProgram
    {
        public int AgentsBuffer => agentsBuffer;

        public int PlantsTex => plantsTexB;

        public int PrayTex => prayTexB;

        public int PredTex => predTexB;

        private int moveProgram;

        private int configBuffer;

        private int agentsBuffer;

        private int plantsTexA = 0;

        private int plantsTexB = 0;

        private int prayTexA = 0;

        private int prayTexB = 0;

        private int predTexA = 0;

        private int predTexB = 0;

        private int currentAgentsCount = 0;

        private int currentWidth = 0;

        private int currentHeight = 0;

        private int maxGroupsX;

        private int blurProgram;

        private int blurInPlantsLocation;

        private int blurInPrayLocation;

        private int blurInPredLocation;

        private int blurTexelSizeLocation;

        private int blurKernelLocation;

        private int fboA;

        private int fboB;

        private int vao;

        private int vbo;

        public SolverProgram()
        {
            moveProgram = ShaderUtil.CompileAndLinkComputeShader("move.comp");
            GpuUtil.CreateBuffer(ref configBuffer, 1, Marshal.SizeOf<ShaderConfig>());

            blurProgram = ShaderUtil.CompileAndLinkRenderShader("blur.vert", "blur.frag");
            blurInPlantsLocation = GL.GetUniformLocation(blurProgram, "inPlants");
            if (blurInPlantsLocation == -1) throw new Exception("Uniform 'inPlants' not found. Shader optimized it out?");
            blurInPrayLocation = GL.GetUniformLocation(blurProgram, "inPray");
            if (blurInPrayLocation == -1) throw new Exception("Uniform 'inPray' not found. Shader optimized it out?");
            blurInPredLocation = GL.GetUniformLocation(blurProgram, "inPred");
            if (blurInPredLocation == -1) throw new Exception("Uniform 'inPred' not found. Shader optimized it out?");
            blurTexelSizeLocation = GL.GetUniformLocation(blurProgram, "uTexelSize");
            if (blurTexelSizeLocation == -1) throw new Exception("Uniform 'uTexelSize' not found. Shader optimized it out?");
            blurKernelLocation = GL.GetUniformLocation(blurProgram, "uKernel");
            if (blurKernelLocation == -1) throw new Exception("Uniform 'uKernel' not found. Shader optimized it out?");

            (vao, vbo) = PolygonUtil.CreateQuad();

            GL.GetInteger((OpenTK.Graphics.OpenGL.GetIndexedPName)All.MaxComputeWorkGroupCount, 0, out maxGroupsX);
        }

        public void Run(ref ShaderConfig config, float[] kernel)
        {
            lock (this)
            {
                PrepareBuffers(config);
                UploadConfig(ref config);

                //move agents
                GL.UseProgram(moveProgram);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, configBuffer);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, agentsBuffer);
                GL.BindImageTexture(5, plantsTexA, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
                GL.BindImageTexture(6, prayTexA, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
                GL.BindImageTexture(7, predTexA, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
                GL.DispatchCompute(DispatchGroupsX(config.agentsCount), 1, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);

                //blur
                GL.Viewport(0, 0, config.width, config.height); //this is important for the blur.frag, later must be set to real viewport
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fboB);

                GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.ScissorTest);
                GL.Disable(EnableCap.Blend);

                GL.UseProgram(blurProgram);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, plantsTexA);
                GL.Uniform1(blurInPlantsLocation, 0);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, prayTexA);
                GL.Uniform1(blurInPrayLocation, 1);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2D, predTexA);
                GL.Uniform1(blurInPredLocation, 2);
                GL.Uniform2(blurTexelSizeLocation, 1.0f / config.width, 1.0f / config.height);
                GL.Uniform1(blurKernelLocation, 25, kernel);
                PolygonUtil.RenderTriangles(vao);

                // Swap
                (plantsTexA, plantsTexB) = (plantsTexB, plantsTexA);
                (prayTexA, prayTexB) = (prayTexB, prayTexA);
                (predTexA, predTexB) = (predTexB, predTexA);
                (fboA, fboB) = (fboB, fboA);
            }
        }

        private int DispatchGroupsX(int count) => Math.Clamp((count + ShaderUtil.LocalSizeX - 1) / ShaderUtil.LocalSizeX, 1, maxGroupsX);

        private void UploadConfig(ref ShaderConfig config)
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, configBuffer);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, Marshal.SizeOf<ShaderConfig>(), ref config);
        }

        public void UploadAgents(ShaderConfig config, Agent[] agents)
        {
            lock (this)
            {
                PrepareBuffers(config);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, agentsBuffer);
                GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, agents.Length * Marshal.SizeOf<Agent>(), agents);
            }
        }

        private void PrepareBuffers(ShaderConfig config)
        {
            if (currentAgentsCount != config.agentsCount)
            {
                currentAgentsCount = config.agentsCount;
                GpuUtil.CreateBuffer(ref agentsBuffer, currentAgentsCount, Marshal.SizeOf<Agent>());
            }

            if (currentWidth != config.width || currentHeight != config.height)
            {
                currentWidth = config.width;
                currentHeight = config.height;

                if (plantsTexA != 0) GL.DeleteTexture(plantsTexA);
                plantsTexA = TextureUtil.CreateFloatTexture(config.width, config.height);
                if (plantsTexB != 0) GL.DeleteTexture(plantsTexB);
                plantsTexB = TextureUtil.CreateFloatTexture(config.width, config.height);

                if (prayTexA != 0) GL.DeleteTexture(prayTexA);
                prayTexA = TextureUtil.CreateFloatTexture(config.width, config.height);
                if (prayTexB != 0) GL.DeleteTexture(prayTexB);
                prayTexB = TextureUtil.CreateFloatTexture(config.width, config.height);

                if (predTexA != 0) GL.DeleteTexture(predTexA);
                predTexA = TextureUtil.CreateFloatTexture(config.width, config.height);
                if (predTexB != 0) GL.DeleteTexture(predTexB);
                predTexB = TextureUtil.CreateFloatTexture(config.width, config.height);

                fboA = TextureUtil.CreateFboForTextures(plantsTexA, prayTexA, predTexA);
                GL.ClearColor(0f, 0f, 0f, 0f);
                GL.Clear(ClearBufferMask.ColorBufferBit);
                fboB = TextureUtil.CreateFboForTextures(plantsTexB, prayTexB, predTexB);
                GL.ClearColor(0f, 0f, 0f, 0f);
                GL.Clear(ClearBufferMask.ColorBufferBit);
            }
        }
    }
}
