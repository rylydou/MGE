using System;
using MGE.Graphics;
using MGE.Nodes;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

namespace MGE;

public class GameWindow : OpenTK.Windowing.Desktop.GameWindow
{
	static GameWindow? _current;
	public static GameWindow current { get => _current ?? throw new NullReferenceException(); }

	Color backgroundColor = new("#394778");

	double updateTime;
	double renderTime;

	RenderTexture gameRender;
	Texture sprite;

	public GameWindow() : base(new() { /* RenderFrequency = 60, UpdateFrequency = 60, */ }, new() { Title = "Mangrove Game Engine", NumberOfSamples = 4, })
	{
		_current = this;

		VSync = VSyncMode.Adaptive;

		gameRender = new(new(320 * 2, 180 * 2));
		sprite = Texture.LoadTexture("Icon.png");
		Icon = new(Texture.LoadImageData("Icon.png"));

		CenterWindow(new(320 * 4, 180 * 4));
		Focus();

		var world = new WorldNode();

		var ball = new DynamicNode();

		var ballColl = new CircleColliderNode();
		ball.AttachNode(ballColl);

		world.AttachNode(ball);

		Scene.root.AttachNode(world);
	}

	protected override void OnLoad()
	{
		base.OnLoad();

		GL.Enable(EnableCap.Blend);
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		GL.Enable(EnableCap.Multisample);
	}

	protected override void OnUnload()
	{
		base.OnUnload();

		GL.BindVertexArray(0);
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

		GL.BindTexture(TextureTarget.Texture2D, 0);
		GL.UseProgram(0);
	}

	protected override void OnResize(ResizeEventArgs args)
	{
		base.OnResize(args);

		GFX.windowViewportTransform = Matrix.CreateOrthographic(Math.CeilToEven(Size.X), Math.CeilToEven(Size.Y), 0, 1);
	}

	protected override void OnUpdateFrame(FrameEventArgs args)
	{
		base.OnUpdateFrame(args);

		updateTime = args.Time;

		Input.UpdateInputs(KeyboardState, MouseState, JoystickStates);

		var state = KeyboardState;
		var alt = state.IsKeyDown(Keys.LeftAlt) || state.IsKeyDown(Keys.RightAlt);

		if (state.IsKeyPressed(Keys.F11) || (alt && state.IsKeyPressed(Keys.Enter)))
		{
			switch (WindowState)
			{
				case WindowState.Normal:
					// Goto borderless windowed
					WindowBorder = WindowBorder.Hidden;
					WindowState = WindowState.Maximized;
					break;

				case WindowState.Maximized:
					// Goto fullscreen
					WindowBorder = WindowBorder.Resizable;
					WindowState = WindowState.Fullscreen;
					break;

				case WindowState.Fullscreen:
					// Goto windowed
					WindowBorder = WindowBorder.Resizable;
					WindowState = WindowState.Normal;
					CenterWindow(new(320 * 4, 180 * 4));
					break;
			}
		}

		Scene.Tick();

		Scene.Update();
	}

	protected override void OnTextInput(TextInputEventArgs e)
	{
		base.OnTextInput(e);

		Input.OnTextInput(e);
	}

	protected override void OnJoystickConnected(JoystickEventArgs e)
	{
		base.OnJoystickConnected(e);

		Input.OnGamepadConnected(e);
	}

	protected override void OnRenderFrame(FrameEventArgs args)
	{
		base.OnRenderFrame(args);

		renderTime = args.Time;

		GFX.SetRenderTarget(gameRender);

		GFX.Clear(backgroundColor);

		Scene.Draw();

		GFX.DrawBatches();

		GFX.SetRenderTarget(null);

		GFX.Clear(Color.black);

		GFX.DrawRenderTexture(gameRender);

		Font.current.DrawText($"Update: {1f / updateTime:F0}fps ({updateTime * 1000:F2}ms) Render: {1f / renderTime:F0}fps ({renderTime * 1000:F2}ms)", new(-Size.X / 2 + 4, -Size.Y / 2 + 4));

		GFX.DrawBatches();

		SwapBuffers();
	}
}
