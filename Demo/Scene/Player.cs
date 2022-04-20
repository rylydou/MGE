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

	Texture _sprite = App.content.Get<Texture>("Scene/Player/Red.ase");

	bool _alt;
	public bool isPlayer2
	{
		get => _alt;
		set
		{
			_alt = value;
			if (_alt)
				_sprite = App.content.Get<Texture>("Scene/Player/Blue.ase");
		}
	}
	public Vector2 velocity;
	public float floorY;
	float _moveInput;
	bool _jumpDown;
	// bool _jumpUp;

	protected override void Update(float delta)
	{
		var kb = App.input.keyboard;

		// Get input
		_moveInput = 0;
		if (kb.Down(isPlayer2 ? Keys.Left : Keys.A)) _moveInput -= 1;
		if (kb.Down(isPlayer2 ? Keys.Right : Keys.D)) _moveInput += 1;

		if (kb.Pressed(isPlayer2 ? Keys.Up : Keys.W)) _jumpDown = true;
		// if (kb.Released(isPlayer2 ? Keys.Up : Keys.W)) _jumpUp = true;
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

	protected override void Render(Batch2D batch)
	{
		var time = (float)Time.duration.TotalSeconds;
		batch.Image(_sprite, Vector2.zero, Vector2.one, Vector2.zero, 0, Color.white);
	}
}
