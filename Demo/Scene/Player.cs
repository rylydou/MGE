using MGE;

namespace Demo;

public class Player : Node2D
{
	[Prop] public float moveSpeed;

	Font _font = App.content.Get<Font>("Fonts/Inter/Regular.ttf");

	Texture _sprite = App.content.Get<Texture>("Scene/Player/Sprite.ase");
	SoundEffect _soundEffect = App.content.Get<SoundEffect>("Scene/Player/What.wav");

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
		position += input.normalized * moveSpeed * delta;

		if (kb.Pressed(Keys.Space))
		{
			_soundEffect!.Play(RNG.shared.RandomFloat(), RNG.shared.RandomFloat(-1f, 1f), RNG.shared.RandomFloat(-1, 1));
		}
	}

	protected override void Draw(Batch2D batch)
	{
		batch.Image(_sprite, new(0, 0), Color.white);

		_font.DrawString(batch, $"{Time.fps}fps ({Time.rawDelta:N4}ms)\n\tPosition: {globalPosition}", new(8), Color.white);
	}
}
