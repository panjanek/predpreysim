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
        private int moveProgram;

        private int configBuffer;

        private int agentsBuffer;

        private int currentAgentsCount = 0;

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
                PrepareBuffers(config.agentsCount);
                UploadConfig(ref config);

                GL.UseProgram(moveProgram);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, configBuffer);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, agentsBuffer);
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
                PrepareBuffers(agents.Length);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, agentsBuffer);
                GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, agents.Length * Marshal.SizeOf<Agent>(), agents);
            }
        }

        private void PrepareBuffers(int agentsCount)
        {
            if (currentAgentsCount != agentsCount)
            {
                currentAgentsCount = agentsCount;
                GpuUtil.CreateBuffer(ref agentsBuffer, currentAgentsCount, Marshal.SizeOf<Agent>());
            }
        }
    }
}
