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

	// RenderTexture gameRender = new(new(320 * 2, 180 * 2));
	Texture sprite;

	public GameWindow() : base(new() { /* RenderFrequency = 60, UpdateFrequency = 60, */ }, new() { Title = "Mangrove Game Engine", NumberOfSamples = 4, })
	{
		_current = this;

		VSync = VSyncMode.Adaptive;

		sprite = Texture.LoadFromFile(Folder.assets.GetFile("Icon.png"));
		Icon = new(Texture.LoadImageFromFile(Folder.assets.GetFile("Icon.png")));

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
			if (WindowState == WindowState.Fullscreen)
			{
				WindowState = WindowState.Normal;
				CenterWindow(new(320 * 4, 180 * 4));
			}
			else
			{
				WindowState = WindowState.Fullscreen;
			}
		}

		Scene.Tick();

		Scene.Update();
	}

	protected override void OnRenderFrame(FrameEventArgs args)
	{
		base.OnRenderFrame(args);

		renderTime = args.Time;

		// GFX.SetRenderTarget(gameRender);

		GFX.Clear(backgroundColor);

		// Scene.Draw();

		// GFX.DrawBatches();

		// GFX.SetRenderTarget(null);

		// GFX.Clear(Color.black);

		// GFX.DrawRenderTexture(gameRender);

		// Font.defaultFont.DrawString($"Update: {1f / updateTime:F0}fps ({updateTime * 1000:F2}ms) Render: {1f / renderTime:F0}fps ({renderTime * 1000:F2}ms)", new(-Size.X / 2 + 4, -Size.Y / 2 + 4), 12, Color.white);

		// var offset = 0;
		// foreach (var joystick in JoystickStates)
		// {
		// if (joystick is null) continue;

		// var title = GLFW.JoystickIsGamepad(joystick.Id) ? "GP" : "JS";

		// Font.defaultFont.DrawString($"{offset++} {title}. {joystick.ToString()}", new(-Size.X / 2 + 4, Size.Y / 2 - 4 - offset * 12), 12, Color.white);
		// }

		// Font.defaultFont.DrawString("Hello World!", Vector2.zero, 64, Color.white);

		GFX.DrawBox(new(64, 64, 64, 64), Color.red);

		GFX.DrawTexture(sprite, Vector2.zero, Color.white);

		GFX.DrawBatches();

		SwapBuffers();
	}

	protected override void OnTextInput(TextInputEventArgs e)
	{
		base.OnTextInput(e);

		Input.OnTextInput(e);
	}

	protected override void OnJoystickConnected(JoystickEventArgs e)
	{
		base.OnJoystickConnected(e);

		Input.OnJoystickConnected(e);
	}
}
