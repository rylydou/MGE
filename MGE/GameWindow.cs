using System;
using MGE.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using tainicom.Aether.Physics2D.Dynamics;

namespace MGE;

public class GameWindow : OpenTK.Windowing.Desktop.GameWindow
{
	static GameWindow? _current;
	public static GameWindow current { get => _current ?? throw new NullReferenceException(); }

	double updateTime;
	double renderTime;

	World _world;

	Body _ballBody;
	Fixture _ballFixture;

	Body _groundBody;
	Fixture _groundFixture;

	Texture sprite;

	public GameWindow() : base(new() { RenderFrequency = 60, UpdateFrequency = 60, }, new() { Title = "Mangrove Game Engine", NumberOfSamples = 4, })
	{
		_current = this;

		sprite = Texture.LoadTexture("Icon.png");
		Icon = new(Texture.LoadImageData("Icon.png"));

		CenterWindow(new(320 * 4, 180 * 4));
		Focus();

		_world = new(gravity: new(0, -64));
		_ballBody = _world.CreateBody(bodyType: BodyType.Dynamic);
		_ballFixture = _ballBody.CreateCircle(64f, 64f);
		_ballFixture.Restitution = 0.3f;
		_ballFixture.Friction = 0.5f;

		_groundBody = _world.CreateBody();
		_groundFixture = _groundBody.CreateRectangle(320, 90, 1, new(0, -320));
		_groundFixture.Restitution = 0.3f;
		_groundFixture.Friction = 0.5f;

		Scene.root.AttachNode(new SpriteNode(Texture.LoadTexture("Tree.png")));
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

		var size = new Vector2Int(args.Size.X, args.Size.Y);

		size.x = Math.CeilToEvenInt(size.x);
		size.y = Math.CeilToEvenInt(size.y);

		GL.Viewport(0, 0, size.x, size.y);
		GFX.transform = Matrix.CreateOrthographic(size.x, size.y, -1, 1);
	}

	protected override void OnUpdateFrame(FrameEventArgs args)
	{
		base.OnUpdateFrame(args);

		updateTime = args.Time;

		_world.Step((float)args.Time);

		Scene.Update();
	}

	protected override void OnRenderFrame(FrameEventArgs args)
	{
		base.OnRenderFrame(args);

		renderTime = args.Time;

		GFX.Clear(new("#394778"));

		Scene.Draw();

		GFX.Flush();

		Font.current.DrawText($"Update: {1f / updateTime:F0}fps ({updateTime * 1000:F2}ms) Render: {1f / renderTime:F0}fps ({renderTime * 1000:F2}ms)", new(-Size.X / 2 + 4, -Size.Y / 2 + 4));

		GFX.DrawTextureRegion(sprite, new(256, 256, 32, 32), new(0, 0, sprite.size));

		GFX.DrawCircleFilled(new(_ballBody.Position.X, _ballBody.Position.Y), 64, new(0, 1, 0, 0.9f));
		GFX.DrawLine(new Vector2(_ballBody.Position.X, _ballBody.Position.Y), new Vector2(_ballBody.Position.X, _ballBody.Position.Y) + Vector2.RotateAroundPoint(new(0, 64), _ballBody.Rotation), 2, Color.white);

		GFX.Flush();

		SwapBuffers();
	}
}
