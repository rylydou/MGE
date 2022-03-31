using MGE;

namespace Demo;

public class PlayerNode : Node2D
{
	Texture sprite = App.content.Get<Texture>("Tree.png");
	Font _font = App.content.Get<Font>("Fonts/Kenney Future/Regular.ttf");
	SoundEffect _soundEffect = App.content.Get<SoundEffect>("What.wav");

	protected override void Update(float delta)
	{
		var kb = App.input.keyboard;

		// Get input
		var input = Vector2.zero;

		if (kb.Down(Keys.A)) input.x -= 1;
		if (kb.Down(Keys.D)) input.x += 1;

		if (kb.Down(Keys.W)) input.y -= 1;
		if (kb.Down(Keys.S)) input.y += 1;

		// Set the position
		position += input.normalized * 128 * delta;

		if (kb.Pressed(Keys.Space))
		{
			_soundEffect!.Play(RNG.shared.RandomFloat(), RNG.shared.RandomFloat(-1f, 1f), RNG.shared.RandomFloat(-1, 1));
		}
	}

	protected override void Draw(Batch2D batch)
	{
		batch.Image(sprite, position, Color.white);

		_font.DrawString(batch, $"{Time.fps}fps ({Time.rawDelta:F4}ms)\nPosition: {position}", new(8), Color.white);
	}
}
