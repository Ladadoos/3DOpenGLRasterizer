﻿using System;
using System.Drawing;
using System.Globalization;
using System.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Rasterizer
{
    public class OpenTKApp : GameWindow
    {
        static int screenID;            // unique integer identifier of the OpenGL texture
        static MyApplication app;       // instance of the application
        static bool terminated = false; // application terminates gracefully when this is true

        protected override void OnLoad(EventArgs e)
        {
            Console.WriteLine("OpenGL version: " + GL.GetString(StringName.Version));

            // called during application initialization
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            ClientSize = new Size(1280, 720);
            X = 100;
            app = new MyApplication();
            app.screen = new Surface(Width, Height);
            Sprite.target = app.screen;
            screenID = app.screen.GenTexture();
            app.Initialize();

            VSync = VSyncMode.On;
        }
        protected override void OnUnload(EventArgs e)
        {
            // called upon app close
            GL.DeleteTextures(1, ref screenID);
            Environment.Exit(0);      // bypass wait for key on CTRL-F5
        }
        protected override void OnResize(EventArgs e)
        {
            // called upon window resize. Note: does not change the size of the pixel buffer.
            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 4.0);
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // called once per frame; app logic
            var keyboard = OpenTK.Input.Keyboard.GetState();
            if (keyboard[OpenTK.Input.Key.Escape]) terminated = true;
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Title = "FPS: " + (int)(1 / e.Time);
            float deltaTime = (float)e.Time;

            // called once per frame; render
            app.Tick(this, deltaTime);
            if (terminated)
            {
                Exit();
                return;
            }

            // set the state for rendering the quad
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);
            GL.Color3(1.0f, 1.0f, 1.0f);
            GL.BindTexture(TextureTarget.Texture2D, screenID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                           app.screen.width, app.screen.height, 0,
                           PixelFormat.Bgra,
                           PixelType.UnsignedByte, app.screen.pixels
                         );

            // draw screen filling quad
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(-1.0f, -1.0f);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(1.0f, -1.0f);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(1.0f, 1.0f);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(-1.0f, 1.0f);
            GL.End();

            // prepare for generic OpenGL rendering
            GL.Enable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.Disable(EnableCap.Texture2D);

            // do OpenGL rendering
            app.RenderGL(this, deltaTime);

            // swap buffers
            SwapBuffers();
        }

        public static void Main(string[] args)
        {
            // entry point
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            using (OpenTKApp app = new OpenTKApp()) { app.Run(60.0, 0.0); }
        }
    }
}