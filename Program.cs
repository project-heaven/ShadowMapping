using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.IO;
using OpenTK.Input;

namespace ShadowMapping
{
    class Game : GameWindow
    {
        [STAThread]
        static void Main()
        {
            Game game = new Game();
            game.Run();
        }

        static int window_width = 1000;
        static int window_height = 1000;

        static int shadow_map_width = 2000;
        static int shadow_map_height = 2000;

        public Game() : base(window_width, window_height, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 32, 0, 16), "Sample")
        {
            VSync = VSyncMode.On;
        }

        protected override void OnResize(EventArgs E)
        {
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
        }

        int render_shader;
        int shadow_map_shader;

        int framebuffer;
        int shadow_tex;

        int triangle_count;

        void CreateVertBuffers()
        {
            int VAO = GL.GenVertexArray();
            int VBO = GL.GenBuffer();

            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

            // Load scene processed by objtoglbuffer
            byte[] vert_data = File.ReadAllBytes("data/scene.vbo");

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 6, sizeof(float) * 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(float) * 6, sizeof(float) * 3);
            GL.EnableVertexAttribArray(1);

            triangle_count = vert_data.Length / (6 * sizeof(float));

            GL.BufferData(BufferTarget.ArrayBuffer, vert_data.Length, vert_data, BufferUsageHint.StaticDraw);
        }


        protected override void OnLoad(EventArgs E)
        {
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.DepthTest);

            render_shader = new ShaderProgram()
            .addVertexShader(new StreamReader("shaders/vert.glsl"))
            .addFragmentShader(new StreamReader("shaders/frag.glsl"))
            .Compile();

            shadow_map_shader = new ShaderProgram()
            .addVertexShader(new StreamReader("shaders/shadow_map_vert.glsl"))
            .addFragmentShader(new StreamReader("shaders/shadow_map_frag.glsl"))
            .Compile();

            CreateVertBuffers();

            framebuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);

            shadow_tex = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, shadow_tex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32f, shadow_map_width, shadow_map_height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)All.CompareRefToTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareFunc, (int)All.Less);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, shadow_tex, 0);
        }

        Camera camera = new Camera(new Vector3(0, 3, 10), 0, -(float)Math.PI / 2);
        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, (float)window_width / (float)window_height, 0.1f, 1000);

        Matrix4 shadow_map_projection = Matrix4.LookAt(0.0f, 30.0f, 0.0f, 0.0f, 0.0f, -20.0f, 0.0f, 1.0f, 0.0f) * Matrix4.CreateOrthographicOffCenter(-20.0f, 20.0f, -20.0f, 20.0f, 0.0f, 100.0f);
        Vector3 light_direction = new Vector3(0.0f, -0.5f, -0.5f).Normalized();

        void RenderShadows()
        {
            GL.Viewport(0, 0, shadow_map_width, shadow_map_height);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            GL.CullFace(CullFaceMode.Front);

            GL.UseProgram(shadow_map_shader);

            GL.ProgramUniformMatrix4(shadow_map_shader, GL.GetUniformLocation(shadow_map_shader, "transform_mat"), false, ref shadow_map_projection);
            GL.DrawArrays(PrimitiveType.Triangles, 0, triangle_count);

            GL.CullFace(CullFaceMode.Back);
        }

        void RenderScene()
        {
            GL.Viewport(0, 0, window_width, window_height);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(render_shader);

            Matrix4 transform = camera.Matrix * projection;
            GL.ProgramUniformMatrix4(render_shader, GL.GetUniformLocation(render_shader, "transform_mat"), false, ref transform);
            GL.ProgramUniformMatrix4(render_shader, GL.GetUniformLocation(render_shader, "shadow_transform_mat"), false, ref shadow_map_projection);
            GL.ProgramUniform3(render_shader, GL.GetUniformLocation(render_shader, "light_dir"), light_direction);

            GL.DrawArrays(PrimitiveType.Triangles, 0, triangle_count);
        }

        protected override void OnRenderFrame(FrameEventArgs E)
        {
            RenderShadows();
            RenderScene();

            camera.Update(0.05f);

            SwapBuffers();
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            camera.MouseEvents(e);
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            camera.MouseEvents(e);
        }
    }
}