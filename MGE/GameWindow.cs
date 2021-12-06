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

	Texture sprite;

	public GameWindow() : base(new() { RenderFrequency = 60, UpdateFrequency = 60, }, new() { Title = "Mangrove Game Engine", NumberOfSamples = 4, })
	{
		_current = this;

		sprite = Texture.LoadTexture("Icon.png");
		Icon = new(Texture.LoadImageData("Icon.png"));

		CenterWindow(new(320 * 4, 180 * 4));
		Focus();

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

		var state = KeyboardState;

		Scene.Tick();

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

		GFX.Flush();

		SwapBuffers();
	}
}
