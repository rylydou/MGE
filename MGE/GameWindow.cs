using System;
using MGE.Graphics;
using MGE.Nodes;
using MGE.UI;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;

namespace MGE;

public class GameWindow : OpenTK.Windowing.Desktop.GameWindow
{
	static GameWindow? _current;
	public static GameWindow current { get => _current ?? throw new NullReferenceException(); }

	Color _backgroundColor = new("#394778");

	double _updateTime;
	double _renderTime;

	RenderTexture _gameRender;
	Texture _sprite;

	UICanvas _canvas = new();
	UIButton _button1 = new();
	UIButton _button2 = new();
	UIButton _button3 = new();

	public GameWindow() : base(new() { /* RenderFrequency = 60, UpdateFrequency = 60, */ }, new() { Title = "Mangrove Game Engine", })
	{
		_current = this;

		VSync = VSyncMode.Adaptive;

		_gameRender = new(new(320 * 2, 180 * 2));
		_sprite = Texture.LoadFromFile(Folder.assets.GetFile("Icon.png"));
		Icon = new(Texture.LoadImageFromFile(Folder.assets.GetFile("Icon.png")));

		CenterWindow(new(320 * 4, 180 * 4));
		Focus();

		var world = new WorldNode();

		var ball = new DynamicNode();

		var ballColl = new CircleColliderNode();
		ball.AttachNode(ballColl);

		world.AttachNode(ball);

		Scene.root.AttachNode(world);

		Input.Init();

		_button1.fixedWidth = 90;
		_button1.fixedHeight = 30;
		_canvas.AddChild(_button1);

		_button2.fixedWidth = 90;
		_button2.fixedHeight = 30;
		_button2.horizontalResizing = UIResizing.FillContainer;
		_canvas.AddChild(_button2);

		_button3.fixedWidth = 90;
		_button3.fixedHeight = 30;
		_canvas.AddChild(_button3);

		_canvas.padding = new(15);
	}

	protected override void OnLoad()
	{
		base.OnLoad();

		GL.Enable(EnableCap.Blend);
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
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

		GFX.windowViewportTransform = Matrix.CreateOrthographicOffCenter(0, args.Width, args.Height, 0, 0, -1);
		_canvas.fixedWidth = args.Width;
		_canvas.fixedHeight = args.Height;
	}

	protected override void OnUpdateFrame(FrameEventArgs args)
	{
		base.OnUpdateFrame(args);

		_updateTime = args.Time;

		Input.UpdateKeyboard(KeyboardState);
		Input.UpdateMouse(MouseState);
		Input.UpdateJoysticks(JoystickStates);

		var alt = Input.IsButtonDown(Button.KB_RightAlt) || Input.IsButtonDown(Button.KB_RightAlt);

		if (Input.IsButtonPressed(Button.KB_F11) || (alt && Input.IsButtonPressed(Button.KB_Enter)))
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

		_renderTime = args.Time;

		GFX.SetRenderTarget(_gameRender);

		GFX.Clear(_backgroundColor);

		Scene.Draw();

		GFX.DrawBatches();

		GFX.SetRenderTarget(null);

		GFX.Clear(Color.black);

		GFX.DrawGameRender(_gameRender);

		GFX.DrawBatches();

		// var text = $"Update:{1f / _updateTime:F0}fps ({_updateTime * 1000:F2}ms) Render:{1f / _renderTime:F0}fps ({_renderTime * 1000:F2}ms)";
		// Font.monospace.DrawString(text, new(10), Color.black.translucent);
		// Font.monospace.DrawString(text, new(8), Color.white);

		Input.DrawGamepadInput();

		GFX.DrawBatches();

		_canvas.DoRender();

		GFX.DrawBatches();

		SwapBuffers();
	}

	protected override void OnKeyDown(KeyboardKeyEventArgs e)
	{
		base.OnKeyDown(e);

		Input.OnKeyDown(e);
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
