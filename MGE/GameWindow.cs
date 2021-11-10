using System;
using MGE.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
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
			velocity.x = Random.Shared.Next(-320 / 2, 320 / 2);
			velocity.y = Random.Shared.Next(-180 / 2, 180 / 2);
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
	RenderTexture _gameRender;

	public GameWindow() : base(GameWindowSettings.Default, new() { Title = "Mangrove Game Engine", DepthBits = 0, StencilBits = 0, NumberOfSamples = 0, })
	{
		CenterWindow(new(320 * 4, 180 * 4));

		_current = this;

		_balls = new Ball[64];

		for (int i = 0; i < _balls.Length; i++)
		{
			_balls[i] = new();
		}

		_ballTexture = Texture.LoadFromFile("Tree.png");

		_sb = new();
		_gameRender = new(new(320, 180));
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

		_ballTexture.Dispose();

		_sb.Dispose();
		_gameRender.Dispose();

		base.OnUnload();
	}

	protected override void OnResize(ResizeEventArgs args)
	{
		GL.Viewport(0, 0, Size.X, Size.Y);

		base.OnResize(args);
	}

	protected override void OnUpdateFrame(FrameEventArgs args)
	{
		var input = KeyboardState;

		foreach (var ball in _balls)
		{
			ball.Update(args);
		}

		base.OnUpdateFrame(args);
	}

	protected override void OnRenderFrame(FrameEventArgs args)
	{
		_gameRender.Use();

		GL.Clear(ClearBufferMask.ColorBufferBit);

		foreach (var ball in _balls)
		{
			ball.Draw(_sb, _ballTexture);
		}

		_sb.Flush();

		_gameRender.StopUse();

		GL.Clear(ClearBufferMask.ColorBufferBit);

		_sb.DrawTexture(_gameRender.texture, new Rect(0, 0, Size.X, Size.Y));

		_sb.Flush();

		SwapBuffers();

		base.OnRenderFrame(args);
	}
}
