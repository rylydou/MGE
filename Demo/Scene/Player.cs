using System;
using System.Linq;
using MGE;
using Math = MGE.Math;

namespace Demo;

public class Player : Actor
{
	[Prop] public float gravity;

	[Prop] public float moveSpeed;
	[Prop] public float moveSpeedAir;
	[Prop] public float friction;
	[Prop] public float frictionAir;

	[Prop] public float jumpBufferLength;
	float _jumpBuffer;
	[Prop] public float coyoteTime;
	float _coyoteTimer;
	[Prop] public float jumpVelMin;
	[Prop] public float jumpVelMax;

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

	float _moveInput;
	bool _cancelJump;

	protected override void Ready()
	{
		SetCollider(new HitboxCollider2D(new(24, 24)));
	}

	protected override void Update(float delta)
	{
		var kb = App.input.keyboard;

		// Get input
		_moveInput = 0;
		if (kb.Down(isPlayer2 ? Keys.Left : Keys.A)) _moveInput -= 1;
		if (kb.Down(isPlayer2 ? Keys.Right : Keys.D)) _moveInput += 1;

		_jumpBuffer -= delta;
		if (kb.Pressed(isPlayer2 ? Keys.Up : Keys.W)) _jumpBuffer = jumpBufferLength;
		if (kb.Released(isPlayer2 ? Keys.Up : Keys.W)) _cancelJump = true;

		if (kb.Pressed(Keys.R))
		{
			globalPosition = Vector2.zero;
			velocity = Vector2.zero;
		}
	}

	bool grounded;

	protected override void Tick(float delta)
	{
		grounded = OnGround();

		_coyoteTimer -= delta;
		if (grounded)
		{
			_coyoteTimer = coyoteTime;
		}

		velocity.y += gravity;

		// Jump
		if (_coyoteTimer > 0 && _jumpBuffer > 0)
		{
			_jumpBuffer = -1;
			velocity.y = -jumpVelMax;
		}

		if (_cancelJump && velocity.y < -jumpVelMin)
		{
			velocity.y = -jumpVelMin;
		}
		_cancelJump = false;

		velocity.x *= grounded ? friction : frictionAir;
		if (grounded)
		{
			if (Math.Abs(_moveInput) > 0.15f)
				velocity.x = _moveInput * moveSpeed;
		}
		else velocity.x += _moveInput * moveSpeedAir;

		var hitY = MoveV(velocity.y * delta);
		var hitX = MoveH(velocity.x * delta);
		// position += velocity * delta;
		// position = new(position.x, Math.Clamp(position.y, floorY));

		if (hitY) velocity.y = 0;
		// if (hitX) velocity.x = 0;
	}

	protected override void Render(Batch2D batch)
	{
		var time = (float)Time.duration.TotalSeconds;
		batch.Image(_sprite, Vector2.zero, Vector2.one, Vector2.zero, 0, Color.white);

		collider?.Render(batch);
	}

	/* 	bool OnGround() => CollideCheck<Ground>(globalPosition + Vector2.down);

		bool MoveX(float x)
		{
			var move = Math.CeilToInt(x);
			var direction = (int)Math.Sign(move);
			var movement = 0;

			while (move != 0)
			{
				var collision = CollideFirst<Ground>(globalPosition + Vector2.right * move);
				if (collision is not null) return true;

				movement += direction;
				move -= direction;

				globalPosition = new(globalPosition.x + direction, globalPosition.y);
			}

			return false;
		}

		bool MoveY(float y)
		{
			var move = Math.CeilToInt(y);
			var direction = (int)Math.Sign(move);
			var movement = 0;

			while (move != 0)
			{
				var collision = CollideFirst<Ground>(globalPosition + Vector2.down * move);
				if (collision is not null) return true;

				movement += direction;
				move -= direction;

				globalPosition = new(globalPosition.x, globalPosition.y + direction);
			}

			return false;
		} */
}
