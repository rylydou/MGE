using System;
using MGE.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;

namespace MGE;

public class GameWindow : OpenTK.Windowing.Desktop.GameWindow
{
	static GameWindow? _current;
	public static GameWindow current { get => _current ?? throw new NullReferenceException(); }

	double updateTime;
	double renderTime;
	int updatesSinceLastStats;

	public GameWindow() : base(new() { RenderFrequency = 60, UpdateFrequency = 60, }, new() { Title = "Mangrove Game Engine", NumberOfSamples = 4, })
	{
		_current = this;

		Icon = new(Texture.LoadImageData("Icon.png"));

		CenterWindow(new(320 * 4, 180 * 4));
		Focus();
	}

	protected override void OnLoad()
	{
		base.OnLoad();

		GL.ClearColor(new Color("#394778"));
		GL.Enable(EnableCap.Blend);
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		// GL.Enable(EnableCap.Multisample);
		GL.Disable(EnableCap.Multisample);
	}

	protected override void OnUnload()
	{
		base.OnUnload();

		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
		GL.BindVertexArray(0);
		GL.UseProgram(0);
	}

	protected override void OnResize(ResizeEventArgs args)
	{
		base.OnResize(args);
	}

	protected override void OnUpdateFrame(FrameEventArgs args)
	{
		base.OnUpdateFrame(args);

		updateTime = args.Time;

		Scene.Update();
	}

	protected override void OnRenderFrame(FrameEventArgs args)
	{
		renderTime = args.Time;

		Scene.Draw();

		Font.current.DrawText($"{1f / updateTime:F0}fps ({updateTime * 1000:F2}ms) Render: {1f / renderTime:F0}fps ({renderTime * 1000:F2}ms)", new(-Size.X / 2 + 4, -Size.Y / 2 + 4));

		GFX.Flush();

		SwapBuffers();

		base.OnRenderFrame(args);
	}
}
