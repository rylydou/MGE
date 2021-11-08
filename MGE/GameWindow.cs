using MGE.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

namespace MGE
{
	public class GameWindow : OpenTK.Windowing.Desktop.GameWindow
	{
		static GameWindow? _current;
		public static GameWindow current { get => _current!; }

		Vector2 _position;

		Texture _texSprite;
		Texture _texNone;

		SpriteBatch _sb;

		public GameWindow() : base(new(), new() { Size = new(1600, 900) })
		{
			_current = this;

			_sb = new();

			_texSprite = Texture.LoadFromFile("Sprite.png");
			_texNone = Texture.LoadFromFile("None.png");
		}

		protected override void OnLoad()
		{
			base.OnLoad();

			GL.ClearColor(new Color("#1C2923"));
			GL.Enable(EnableCap.Blend);
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
				_position.y += (float)e.Time * 64;
			}
			if (input.IsKeyDown(Keys.Down))
			{
				_position.y -= (float)e.Time * 64;
			}
			if (input.IsKeyDown(Keys.Right))
			{
				_position.x += (float)e.Time * 64;
			}
			if (input.IsKeyDown(Keys.Left))
			{
				_position.x -= (float)e.Time * 64;
			}
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);

			GL.Clear(ClearBufferMask.ColorBufferBit);

			_sb.Start(_texSprite);

			_sb.DrawBox(new(_position, 64, 64), Color.white);

			_sb.DrawBox(new(_position, 16, 16), Color.white);

			_sb.End();

			_sb.Start(_texNone);

			_sb.DrawBox(new(-_position, 32, 32), Color.white);

			_sb.End();

			SwapBuffers();
		}
	}
}
