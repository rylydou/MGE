using MGE.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

namespace MGE
{
	public class GameWindow : OpenTK.Windowing.Desktop.GameWindow
	{
		Vector2 _position;

		SpriteBatch _sb;

		public GameWindow() : base(new(), new() { Size = new(1600, 900) })
		{
			_sb = new();
		}

		protected override void OnLoad()
		{
			base.OnLoad();

			GL.ClearColor(new Color("#161616"));
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		}

		protected override void OnUnload()
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);
			GL.UseProgram(0);

			_sb.Dispose();

			base.OnUnload();
		}

		protected override void OnResize(ResizeEventArgs e)
		{
			base.OnResize(e);

			GL.Viewport(0, 0, Size.X, Size.Y);
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e);

			var input = KeyboardState;

			if (input.IsKeyDown(Keys.Up))
			{
				_position.y += (float)e.Time;
			}
			if (input.IsKeyDown(Keys.Down))
			{
				_position.y -= (float)e.Time;
			}
			if (input.IsKeyDown(Keys.Right))
			{
				_position.x += (float)e.Time;
			}
			if (input.IsKeyDown(Keys.Left))
			{
				_position.x -= (float)e.Time;
			}
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);

			GL.Clear(ClearBufferMask.ColorBufferBit);

			_sb.Start();

			_sb.DrawBox(new(_position, 1, 1), new(1, 0, 0, 0.5f));

			_sb.End();

			SwapBuffers();
		}
	}
}
