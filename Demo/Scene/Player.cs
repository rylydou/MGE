using MGE;

namespace Demo;

public class Player : Node2D
{
	[Prop] public float moveSpeed;
	[Prop] public float moveSpeedAir;
	[Prop] public float jumpVel;
	[Prop] public float gravity;
	[Prop] public float friction;
	[Prop] public float frictionAir;

	Font _font = App.content.Get<Font>("Fonts/Inter/Regular.ttf");

	Texture _sprite = App.content.Get<Texture>("Scene/Player/Sprite.ase");

	public bool alt;
	public Vector2 velocity;
	public float floorY;
	float _moveInput;
	bool _jumpDown;

	protected override void Update(float delta)
	{
		var kb = App.input.keyboard;

		// Get input
		_moveInput = 0;
		if (kb.Down(alt ? Keys.Left : Keys.A)) _moveInput -= 1;
		if (kb.Down(alt ? Keys.Right : Keys.D)) _moveInput += 1;

		if (kb.Pressed(alt ? Keys.Up : Keys.Space)) _jumpDown = true;
	}

	protected override void Tick(float delta)
	{
		var grounded = position.y >= floorY;

		if (grounded)
			velocity.y = 0;
		else
			velocity.y += gravity;

		if (grounded && _jumpDown)
			velocity.y = -jumpVel;
		_jumpDown = false;

		velocity.x *= grounded ? friction : frictionAir;
		if (grounded)
		{
			if (Math.Abs(_moveInput) > 0.15f)
				velocity.x = _moveInput * moveSpeed;
		}
		else velocity.x += _moveInput * moveSpeedAir;

		position += velocity * delta;
		position = new(position.x, Math.Clamp(position.y, floorY));
	}

	protected override void Draw(Batch2D batch)
	{
		var time = (float)Time.duration.TotalSeconds;
		batch.Image(_sprite, Vector2.zero, Vector2.one, Vector2.zero, 0, alt ? Color.red : Color.white);

		// _font.DrawString(batch, $"{Time.fps}fps ({Time.rawDelta:N4}ms)\n\tPosition: {globalPosition}", new(8), Color.white);
	}
}
