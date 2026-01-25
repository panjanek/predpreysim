using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using PredPraySim.Models;

namespace PredPraySim.Gpu
{
    public class DisplayProgram
    {
        private int pointsProgram;

        private int pointsProjLocation;

        private int dummyVao;
        public DisplayProgram() 
        {
            pointsProgram = ShaderUtil.CompileAndLinkRenderShader("points.vert", "points.frag");
            pointsProjLocation = GL.GetUniformLocation(pointsProgram, "projection");
            if (pointsProjLocation == -1) throw new Exception("Uniform 'projection' not found. Shader optimized it out?");

            GL.GenVertexArrays(1, out dummyVao);
            GL.BindVertexArray(dummyVao);
        }

        public void Draw(Simulation simulation, Matrix4 projectionMatrix, int agentsBuffer)
        {
            GL.Enable(EnableCap.ProgramPointSize);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
            GL.BlendEquation(OpenTK.Graphics.OpenGL.BlendEquationMode.FuncAdd);
            GL.Enable(EnableCap.PointSprite);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.UseProgram(pointsProgram);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, agentsBuffer);
            GL.BindVertexArray(dummyVao);
            GL.UniformMatrix4(pointsProjLocation, false, ref projectionMatrix);
            GL.DrawArrays(PrimitiveType.Points, 0, simulation.agents.Length);
        }
    }
}
