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
	UIBox _bar = new();
	UIButton _button1 = new();
	UIButton _button2 = new();
	UIButton _button3 = new();
	UIButton _button4 = new();
	UIButton _button5 = new();

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

		_canvas.direction = UIDirection.Vertical;
		_canvas.padding = new(15);
		_canvas.spacing = 15;
		{
			_bar.spacing = 15;
			_bar.padding = new(16);
			_bar.resizing = new(UIResizing.FillContainer, UIResizing.HugContents);
			_canvas.AddChild(_bar);
			{
				_button1.fixedSize = new(90, 30);
				_bar.AddChild(_button1);

				_button2.resizing = new(UIResizing.FillContainer, UIResizing.Fixed);
				_button2.fixedSize = new(90, 30);
				_bar.AddChild(_button2);

				_button3.resizing = new(UIResizing.Fixed, UIResizing.FillContainer);
				_button3.fixedSize = new(90, 30);
				_bar.AddChild(_button3);
			}

			_button4.resizing = new(UIResizing.FillContainer, UIResizing.FillContainer);
			_canvas.AddChild(_button4);

			_button5.fixedSize = new(90, 30);
			_canvas.AddChild(_button5);
		}
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

	protected override void OnRefresh()
	{
		base.OnRefresh();

		OnUpdateFrame(new(0));
	}

	protected override void OnMove(WindowPositionEventArgs e)
	{
		base.OnMove(e);

		if (Engine.isWindows)
		{
			OnUpdateFrame(new(0));
		}
	}

	protected override void OnResize(ResizeEventArgs args)
	{
		base.OnResize(args);

		GFX.windowViewportTransform = Matrix.CreateOrthographicOffCenter(0, args.Width, args.Height, 0, 0, -1);
		_canvas.position = new(16);
		_canvas.fixedSize = new(args.Width - 32, args.Height - 32);

		if (Engine.isWindows)
		{
			OnRenderFrame(new(0));
		}
	}

	protected override void OnUpdateFrame(FrameEventArgs args)
	{
		base.OnUpdateFrame(args);

		Time.updateTime = (float)args.Time;
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

		Time.drawTime = (float)args.Time;
		_renderTime = args.Time;

		GFX.SetRenderTarget(_gameRender);

		GFX.Clear(_backgroundColor);

		Scene.Draw();

		GFX.DrawBatches();

		GFX.SetRenderTarget(null);

		GFX.Clear(Color.black);

		GFX.DrawGameRender(_gameRender);

		GFX.DrawBatches();

		// Draw Stats
		// var statsText = $"Update:{1f / _updateTime:F0}fps ({_updateTime * 1000:F2}ms) Render:{1f / _renderTime:F0}fps ({_renderTime * 1000:F2}ms)";
		// Font.monospace.DrawString(statsText, new(10), Color.black.translucent);
		// Font.monospace.DrawString(statsText, new(8), Color.white);

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
