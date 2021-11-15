using System;
using MGE.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;

namespace MGE;

public class GameWindow : OpenTK.Windowing.Desktop.GameWindow
{
	class Ball
	{
		public Vector2 position;
		public Vector2 velocity;

		public Ball()
		{
			velocity.x = Random.Shared.Next(-320, 320);
			velocity.y = Random.Shared.Next(-180, 180);
			position.x = Random.Shared.Next(-GameWindow.current.Size.X / 2, GameWindow.current.Size.X / 2);
			position.y = Random.Shared.Next(-GameWindow.current.Size.Y / 2, GameWindow.current.Size.Y / 2);
		}

		public void Update(FrameEventArgs args)
		{
			if (position.x > GameWindow.current.Size.X / 2 || position.x < -GameWindow.current.Size.X / 2)
			{
				velocity.x = -velocity.x;
			}

			if (position.y > GameWindow.current.Size.Y / 2 || position.y < -GameWindow.current.Size.Y / 2)
			{
				velocity.y = -velocity.y;
			}

			foreach (var ball in GameWindow.current._balls)
			{
				if (ball == this) continue;
				if (Vector2.DistanceLessThan(position, ball.position, 32))
				{
					velocity.x = -velocity.x;
					velocity.y = -velocity.y;
				}
			}

			position += velocity * (float)args.Time;
		}

		public void Draw(SpriteBatch sb, Texture texture)
		{
			sb.DrawTexture(texture, position);
		}
	}

	static GameWindow? _current;
	public static GameWindow current { get => _current ?? throw new NullReferenceException(); }

	double updateTime;
	double renderTime;
	int updatesSinceLastStats;

	Ball[] _balls;

	Texture _ballTexture;

	SpriteBatch _sb;
	// RenderTexture _gameRender;

	public GameWindow() : base(new() { UpdateFrequency = 60, RenderFrequency = 60, }, new() { Title = "Mangrove Game Engine", })
	{
		_current = this;

		_sb = new();

		// _gameRender = new(new(320, 180));

		_ballTexture = Texture.LoadTexture("Tree.png");

		_balls = new Ball[64];

		for (int i = 0; i < _balls.Length; i++)
		{
			_balls[i] = new();
		}

		CenterWindow(new(320 * 4, 180 * 4));
		Focus();
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
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
		GL.BindVertexArray(0);
		GL.UseProgram(0);

		_ballTexture.Dispose();

		_sb.Dispose();
		// _gameRender.Dispose();

		base.OnUnload();
	}

	protected override void OnResize(ResizeEventArgs args)
	{
		// Debug.LogVariable(Size);
		GL.Viewport(0, 0, Size.X, Size.Y);
		_sb.transform = Matrix.CreateOrthographic(Size.X, Size.Y, -1, 1);

		base.OnResize(args);
	}

	protected override void OnUpdateFrame(FrameEventArgs args)
	{
		updateTime = args.Time;

		updatesSinceLastStats--;
		if (updatesSinceLastStats < 0)
		{
			updatesSinceLastStats = 10;
			Title = $"Mangrove Game Engine Update: {1f / updateTime:F0}fps ({updateTime * 1000:F2}ms) Render: {1f / renderTime:F0}fps ({renderTime * 1000:F2}ms)";
		}

		var input = KeyboardState;

		foreach (var ball in _balls)
		{
			ball.Update(args);
		}

		base.OnUpdateFrame(args);
	}

	protected override void OnRenderFrame(FrameEventArgs args)
	{
		renderTime = args.Time;

		// _gameRender.Use();

		GL.Clear(ClearBufferMask.ColorBufferBit);

		foreach (var ball in _balls)
		{
			ball.Draw(_sb, _ballTexture);
		}

		// _sb.Flush();

		// _gameRender.StopUse();

		// GL.Clear(ClearBufferMask.ColorBufferBit);

		// _sb.DrawTexture(_gameRender.texture, new Rect(0, 0, Size.X, Size.Y));

		Font.current.DrawText(_sb, "Hello World", new(8, 8));

		_sb.Flush();

		SwapBuffers();

		base.OnRenderFrame(args);
	}
}
