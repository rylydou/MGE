using MGE;

namespace Demo;

public class Game : Module
{
	public readonly Batch2D batch = new Batch2D();
	public Vector2 position;

	Font? _font;
	SpriteFont? _spriteFont;

	Texture? _sprite;

	// This is called when the Application has Started
	protected override void Startup()
	{
		// Add a Callback to the primary window's Render loop
		App.window.onRender += Render;

		_font = new(Folder.content.GetFile("Fonts/Inter/Regular.ttf"));
		_spriteFont = new(_font, 18, @" !""#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~", TextureFilter.Nearest);
		_sprite = new(Folder.content.GetFile("Tree.png"));
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

		// Get input
		var input = Vector2.zero;

		if (kb.Down(Keys.A)) input.x -= 1;
		if (kb.Down(Keys.D)) input.x += 1;

		if (kb.Down(Keys.W)) input.y -= 1;
		if (kb.Down(Keys.S)) input.y += 1;

		// Set the position
		position += input.normalized * 128 * Time.delta;
	}

	void Render(Window window)
	{
		// Clear the batcher from the previous frame
		batch.Clear();

		batch.CheckeredPattern(new(0, 0, App.window.size.x, App.window.size.y), 24, 24, Color.darkGray, Color.gray);

		// Draw a rectangle
		batch.Image(_sprite!, position, Color.white);

		batch.Text(_spriteFont!, $"{Time.fps}fps ({Time.rawDelta:F4}ms)", window.renderMouse + new Vector2(0, -18), Color.white);

		// Clear the Window
		App.graphics.Clear(window, Color.black);

		// Draw the batcher to the Window
		batch.Render(window);
	}
}
