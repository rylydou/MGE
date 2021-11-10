using System;
using MGE.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

namespace MGE;

public class GameWindow : OpenTK.Windowing.Desktop.GameWindow
{
	class Ball
	{
		public Vector2 position;
		public Vector2 velocity;

		public Ball()
		{
			velocity.x = Random.Shared.Next(-256, 256);
			velocity.y = Random.Shared.Next(-256, 256);
		}

		public void Update(FrameEventArgs args)
		{
			if (position.x > 400 || position.x < -400)
			{
				velocity.x = -velocity.x;
			}

			if (position.y > 300 || position.y < -300)
			{
				velocity.y = -velocity.y;
			}

			position += velocity * (float)args.Time;
		}

		public void Draw(SpriteBatch sb, Texture texture)
		{
			sb.DrawTexture(texture, new Rect(position, 64, 64));
		}
	}

	static GameWindow? _current;
	public static GameWindow current { get => _current!; }

	Ball[] _balls;

	Texture _ballTexture;
	SpriteBatch _sb;

	public GameWindow() : base(new(), new() { Title = "Mangrove Game Engine", Size = new(800, 600) })
	{
		_current = this;

		_balls = new Ball[64];

		for (int i = 0; i < _balls.Length; i++)
		{
			_balls[i] = new();
		}

		_ballTexture = Texture.LoadFromFile("Tree.png");
		_sb = new();
	}

	protected override void OnLoad()
	{
		base.OnLoad();

		GL.ClearColor(new Color("#394778"));
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

	protected override void OnResize(ResizeEventArgs args)
	{
		base.OnResize(args);

		GL.Viewport(0, 0, Size.X, Size.Y);
	}

	protected override void OnUpdateFrame(FrameEventArgs args)
	{
		base.OnUpdateFrame(args);

		var input = KeyboardState;

		foreach (var ball in _balls)
		{
			ball.Update(args);
		}
	}

	protected override void OnRenderFrame(FrameEventArgs args)
	{
		base.OnRenderFrame(args);

		GL.Clear(ClearBufferMask.ColorBufferBit);

		foreach (var ball in _balls)
		{
			ball.Draw(_sb, _ballTexture);
		}

		_sb.Flush();

		SwapBuffers();
	}
}
