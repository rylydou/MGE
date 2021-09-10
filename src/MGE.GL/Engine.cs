using System.Drawing;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace MGE
{
	public class Engine : GameWindow
	{
		public Engine() : base(GameWindowSettings.Default, NativeWindowSettings.Default)
		{
		}

		protected override void OnLoad()
		{
			// #472d3c #a93b3b #122020 #24523b #70942F #394778 #3978a8 #0099db #4da6ff
			GL.ClearColor(new Color("#122020"));
			GL.Viewport(0, 0, Size.X, Size.Y);
			GL.Ortho(0, Size.X, Size.Y, 0, -1, 1);

			base.OnLoad();
		}

		protected override void OnResize(ResizeEventArgs e)
		{
			GL.Viewport(0, 0, e.Width, e.Height);

			base.OnResize(e);
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e);
		}

		protected override void OnRenderFrame(FrameEventArgs args)
		{
			base.OnRenderFrame(args);

			SwapBuffers();
		}
	}
}
