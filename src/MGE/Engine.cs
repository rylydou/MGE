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
			GL.ClearColor(new Color(0xFFFFFFFF));
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			base.OnLoad();
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
