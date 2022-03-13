using System;
using MGE;

namespace Demo;

public class Game : Module
{
	public readonly Batch2D batch = new Batch2D();
	public Vector2 position;

	Font? _font;

	Texture? _sprite;
	SoundEffect? _soundEffect;

	// This is called when the Application has Started
	protected override void Startup()
	{
		// Add a Callback to the primary window's Render loop
		App.window.onRender += Render;

		// Load Content
		_sprite = App.content.Get<Texture>("Tree.png");
		// _font = App.content.Get<Font>("Fonts/Inter/Regular.ttf");
		_font = App.content.Get<Font>("Fonts/Kenney Future/Regular.ttf");

		_soundEffect = new SoundEffect(App.audio, new Folder(Environment.CurrentDirectory).GetFile("Sound.wav").OpenRead());

		App.window.SetAspectRatio(new(320, 180));
		App.window.SetMinSize(new(320, 180));

		App.window.Center();
	}

	// This is called when the Application is shutting down, or when the Module is removed
	protected override void Shutdown()
	{
		// Remove our Callback
		App.window.onRender -= Render;
	}

	// This is called every frame of the Application
	protected override void Update()
	{
		// Get the keyboard
		var kb = App.input.keyboard;

		// Toggle fullscreen
		if (kb.Pressed(Keys.F11) || (kb.Pressed(Keys.RightAlt) && kb.Pressed(Keys.Enter)))
		{
			App.window.fullscreen = !App.window.fullscreen;
			return;
		}

		// Get input
		var input = Vector2.zero;

		if (kb.Down(Keys.A)) input.x -= 1;
		if (kb.Down(Keys.D)) input.x += 1;

		if (kb.Down(Keys.W)) input.y -= 1;
		if (kb.Down(Keys.S)) input.y += 1;

		// Set the position
		position += input.normalized * 128 * Time.delta;

		if (kb.Pressed(Keys.Space))
		{
			_soundEffect!.Play(Random.Shared.NextSingle(), Random.Shared.NextSingle() * 2 - 1, Random.Shared.NextSingle() * 2 - 1);
		}
	}

	void Render(Window window)
	{
		// Clear the batcher from the previous frame
		batch.Clear();

		batch.CheckeredPattern(new(0, 0, App.window.size.x, App.window.size.y), 24, 24, Color.darkGray, Color.gray);

		// Draw a rectangle
		batch.Image(_sprite!, position, Color.white);

		_font!.DrawString(batch, $"{Time.fps}fps ({Time.rawDelta:F4}ms)", window.renderMouse + new Vector2(0, -18), Color.white);

		// Clear the Window
		// App.graphics.Clear(window, Color.black);

		// Draw the batcher to the Window
		batch.Render(window);
	}
}
