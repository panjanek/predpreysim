using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace PredPraySim.Gpu
{
    public static class TextureUtil
    {
        public static int CreateFloatTexture(int width, int height)
        {
            int tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex);

            GL.TexStorage2D(TextureTarget2d.Texture2D, 1,
                SizedInternalFormat.Rgba32f, width, height);

            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);


            ClearTexture(tex);

            return tex;
        }

        public static void ClearTexture(int tex)
        {
            // Clear to all zeros
            float[] clearColor = new float[] { 0f, 0f, 0f, 0f };
            GL.ClearTexImage(
                tex,
                0,
                PixelFormat.Rgba,
                PixelType.Float,
                clearColor);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public static int CreateFboForTexture(int texture)
        {
            int fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D,
                texture,
                0
            );

            GL.DrawBuffers(1, new[] { DrawBuffersEnum.ColorAttachment0 });

            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
                throw new Exception($"FBO incomplete: {status}");

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            return fbo;
        }
    }
}
