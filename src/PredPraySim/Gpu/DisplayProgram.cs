using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Compute.OpenCL;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using PredPreySim.Models;

namespace PredPreySim.Gpu
{
    public class DisplayProgram
    {
        private int pointsProgram;

        private int pointsProjLocation;

        private int dispProgram;

        private int plantsImageLocation;

        private int prayImageLocation;

        private int predImageLocation;

        private int dispTexSizeLocation;

        private int dispProjLocation;

        private int dummyVao;

        private Vector2 offset = new Vector2(0, 0);

        private Vector2 size = new Vector2(1, 1);
        public DisplayProgram() 
        {
            pointsProgram = ShaderUtil.CompileAndLinkRenderShader("points.vert", "points.frag");
            pointsProjLocation = GL.GetUniformLocation(pointsProgram, "projection");
            if (pointsProjLocation == -1) throw new Exception("Uniform 'projection' not found. Shader optimized it out?");

            dispProgram = ShaderUtil.CompileAndLinkRenderShader("display.vert", "display.frag");
            plantsImageLocation = GL.GetUniformLocation(dispProgram, "uPlantsImage");
            if (plantsImageLocation == -1) throw new Exception("Uniform 'uPlantsImage' not found. Shader optimized it out?");
            prayImageLocation = GL.GetUniformLocation(dispProgram, "uPrayImage");
            if (prayImageLocation == -1) throw new Exception("Uniform 'uPrayImage' not found. Shader optimized it out?");
            predImageLocation = GL.GetUniformLocation(dispProgram, "uPredImage");
            if (predImageLocation == -1) throw new Exception("Uniform 'uPredImage' not found. Shader optimized it out?");
            dispProjLocation = GL.GetUniformLocation(dispProgram, "projection");
            if (dispProjLocation == -1) throw new Exception("Uniform 'projection' not found. Shader optimized it out?");
            dispTexSizeLocation = GL.GetUniformLocation(dispProgram, "texSize");
            if (dispTexSizeLocation == -1) throw new Exception("Uniform 'texSize' not found. Shader optimized it out?");

            GL.GenVertexArrays(1, out dummyVao);
            GL.BindVertexArray(dummyVao);
        }

        public void Draw(Simulation simulation, Matrix4 projectionMatrix, int agentsBuffer, int plantTex, int prayTex, int predTex)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //draw texture
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
            GL.UseProgram(dispProgram);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, plantTex);
            GL.Uniform1(plantsImageLocation, 0);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, prayTex);
            GL.Uniform1(prayImageLocation, 1);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, predTex);
            GL.Uniform1(predImageLocation, 2);
            GL.Uniform2(dispTexSizeLocation, new Vector2(simulation.shaderConfig.width, simulation.shaderConfig.height));
            GL.UniformMatrix4(dispProjLocation, false, ref projectionMatrix);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);


            //draw points
            GL.Enable(EnableCap.ProgramPointSize);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
            GL.BlendEquation(OpenTK.Graphics.OpenGL.BlendEquationMode.FuncAdd);
            GL.Enable(EnableCap.PointSprite);
            GL.UseProgram(pointsProgram);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, agentsBuffer);
            GL.BindVertexArray(dummyVao);
            GL.UniformMatrix4(pointsProjLocation, false, ref projectionMatrix);
            GL.DrawArrays(PrimitiveType.Points, 0, simulation.agents.Length);      
        }
    }
}
