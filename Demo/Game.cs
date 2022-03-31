using System;
using MGE;

namespace Demo;

public class Game : Module
{
	PlayerNode? node;

	// This is called when the Application has Started
	protected override void Startup()
	{
		// Add a Callback to the primary window's Render loop
		App.window.onRender += Render;

		App.window.SetAspectRatio(new(320, 180));
		App.window.SetMinSize(new(320, 180));

		App.window.Center();

		node = new PlayerNode();
	}

	// This is called when the Application is shutting down, or when the Module is removed
	protected override void Shutdown()
	{
		// Remove our Callback
		App.window.onRender -= Render;
	}

	// This is called every frame of the Application
	protected override void Update(float delta)
	{
		Batch2D.current.Clear();

		Batch2D.current.CheckeredPattern(new(0, 0, App.window.size.x, App.window.size.y), 24, 24, Color.darkGray, Color.gray);

		node!.onUpdate(delta);

		// Get the keyboard
		var kb = App.input.keyboard;

		// Toggle fullscreen
		if (kb.Pressed(Keys.F11) || (kb.Pressed(Keys.RightAlt) && kb.Pressed(Keys.Enter)))
		{
			App.window.fullscreen = !App.window.fullscreen;
			return;
		}
	}

	void Render(Window window)
	{
		// Clear the Window
		App.graphics.Clear(window, Color.black);

		// Draw the batcher to the Window
		Batch2D.current.Render(window);
	}
}
