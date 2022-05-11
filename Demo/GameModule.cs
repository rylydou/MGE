using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Demo.Screens;
using MGE;

namespace Demo;

public class GameModule : Module
{
#nullable disable
	public static GameModule instance;
#nullable restore

	public FrameBuffer gameFramebuffer = new(Game.screenSize.x, Game.screenSize.y);

	public GameModule()
	{
		instance = this;

		Game.LoadSkins(Folder.here / "Skins");
	}

	protected override void Startup()
	{
		App.window.onRender += Render;

		App.window.SetAspectRatio(new(320, 180));
		App.window.SetMinSize(new(320, 180));
	}

	protected override void Shutdown()
	{
		App.window.onRender -= Render;
	}

	protected override void Update(float delta)
	{
		var kb = App.input.keyboard;
		if (kb.Pressed(Keys.F11) || (kb.Pressed(Keys.RightAlt) && kb.Pressed(Keys.Enter)))
		{
			App.window.fullscreen = !App.window.fullscreen;
			return;
		}

		var topColor = new Color(0x3978a8);
		var bottomColor = new Color(0x394778);
		Batch2D.current.Rect(new(Game.screenSize), topColor, topColor, bottomColor, bottomColor);

		Game.screen.Update(delta);
		Game.scene.onUpdate(delta);

		var info = Batch2D.current.Render(gameFramebuffer);
		App.profiler.batch2DRenderInfo = info;
	}

	protected override void Tick(float delta)
	{
		Game.screen.Tick(delta);
		Game.scene.onTick(delta);
	}

	void Render(Window window)
	{
		var batch = Batch2D.current;

		// Draw game framebuffer onto window
		batch.Image(gameFramebuffer, new Rect(window.width, window.height), Color.white);

		// Render screen
		Game.screen.Render(Batch2D.current);

		batch.Render(window);
	}
}
