using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using OpenTK.Graphics.OpenGL;
using PredPraySim.Models;

namespace PredPraySim.Gpu
{
    public class SolverProgram
    {
        public int AgentsBuffer => agentsBuffer;

        public int PlantsTex => plantsTex;

        private int moveProgram;

        private int configBuffer;

        private int agentsBuffer;

        private int plantsTex = 0;

        private int currentAgentsCount = 0;

        private int currentWidth = 0;

        private int currentHeight = 0;

        private int maxGroupsX;
        public SolverProgram()
        {
            moveProgram = ShaderUtil.CompileAndLinkComputeShader("move.comp");
            GpuUtil.CreateBuffer(ref configBuffer, 1, Marshal.SizeOf<ShaderConfig>());

            GL.GetInteger((OpenTK.Graphics.OpenGL.GetIndexedPName)All.MaxComputeWorkGroupCount, 0, out maxGroupsX);

        }

        public void Run(ref ShaderConfig config)
        {
            lock (this)
            {
                PrepareBuffers(config);
                UploadConfig(ref config);

                GL.UseProgram(moveProgram);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, configBuffer);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, agentsBuffer);
                GL.BindImageTexture(5, plantsTex, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
                GL.DispatchCompute(DispatchGroupsX(config.agentsCount), 1, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
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
                if (plantsTex != 0) GL.DeleteTexture(plantsTex);
                plantsTex = TextureUtil.CreateFloatTexture(config.width, config.height);
            }
        }
    }
}
