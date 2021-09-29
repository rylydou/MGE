using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGLTest
{
	public class GLWindow : GameWindow
	{
		public GLWindow() : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = new Vector2i(800, 600), Title = "LearnOpenTK - Creating a Window", })
		{
		}

		protected override void OnLoad()
		{
			base.OnLoad();

			GL.ClearColor(0f, 0.1f, 0.025f, 1f);
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e);

			if (KeyboardState.IsKeyDown(Keys.Escape))
			{
				Close();
			}
		}

		protected override void OnRenderFrame(FrameEventArgs args)
		{
			base.OnRenderFrame(args);

			GL.Clear(ClearBufferMask.ColorBufferBit);

			SwapBuffers();
		}
	}
}
