using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MGE
{
	public class GameWindow : OpenTK.Windowing.Desktop.GameWindow
	{
		public GameWindow() : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = new Vector2i(1600, 900), Title = "LearnOpenTK - Creating a Window", })
		{
		}

		protected override void OnLoad()
		{
			base.OnLoad();

			GL.ClearColor(22f / 255, 22f / 255, 22f / 255, 1f);
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
