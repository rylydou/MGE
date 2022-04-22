using MGE;

namespace Demo;

public class Player : Actor
{
	[Prop] public float fallSpeed;
	[Prop] public float fallClamp;

	[Prop] public float acceleration;
	[Prop] public float deAcceleration;
	[Prop] public float accelerationAir;
	[Prop] public float deAccelerationAir;
	[Prop] public float moveClamp;

	[Prop] public float deAccelerationEx;
	[Prop] public float deAccelerationExAir;

	[Prop] public float minJumpHeight;
	float _minJumpSpeed;
	[Prop] public float maxJumpHeight;
	float _maxJumpSpeed;

	[Prop] public float jumpBufferLength;
	float _jumpBuffer;
	[Prop] public float coyoteTime;
	float _coyoteTimer;

	Texture _sprite = App.content.Get<Texture>("Scene/Player/Small Red.ase");

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

	float _moveInput;
	bool _cancelJump;

	bool _colTop;
	bool _colBottom;
	bool _colLeft;
	bool _colRight;

	public float vSpeed;
	public float hSpeed;
	public float extraHSpeed;

	protected override void Ready()
	{
		SetCollider(new HitboxCollider2D(new(12), new(2)));
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
			hSpeed = 0;
			vSpeed = 0;
		}

		if (kb.Pressed(Keys.Space))
		{
			extraHSpeed = 256 * Time.fixedDelta;
			vSpeed = -256 * Time.fixedDelta;
		}
	}

	protected override void Tick(float delta)
	{
		_colTop = CollideCheck<Ground>(globalPosition + Vector2.up);
		_colBottom = CollideCheck<Ground>(globalPosition + Vector2.down);
		_colLeft = CollideCheck<Ground>(globalPosition + Vector2.left);
		_colRight = CollideCheck<Ground>(globalPosition + Vector2.right);

		_coyoteTimer -= delta;
		if (_colBottom) _coyoteTimer = coyoteTime;

		CalcWalk(delta);
		CalcGravity(delta);
		CalcJump(delta);

		var hitV = MoveV(vSpeed);
		if (hitV.HasValue) Bump(hitV.Value);
		var hitX = MoveH(hSpeed + extraHSpeed);
		if (hitX.HasValue) Bump(hitX.Value);


		void Bump(CollisionInfo info)
		{
			if (vSpeed < 0) vSpeed = 0;
			var dir = globalPosition - info.hit.globalPosition;
			var move = new Vector2(hSpeed, vSpeed).normalized * delta;
			globalPosition += dir.normalized * move.length;
			// Log.Info("Bump:" + dir + " move:" + move);
		}
	}

	void CalcWalk(float delta)
	{
		if (_moveInput != 0)
		{
			// Set horizontal move speed
			hSpeed += _moveInput * (_colBottom ? acceleration : accelerationAir) * delta;

			// clamped by max frame movement
			hSpeed = Math.Clamp(hSpeed, -moveClamp * delta, moveClamp * delta);
		}
		else
		{
			// No input. Let's slow the character down
			hSpeed = Math.MoveTowards(hSpeed, 0, (_colBottom ? deAcceleration : deAccelerationAir) * delta);
		}

		extraHSpeed = Math.MoveTowards(extraHSpeed, 0, (_colBottom ? deAccelerationEx : deAccelerationExAir) * delta);

		if (hSpeed > 0 && _colRight || hSpeed < 0 && _colLeft)
		{
			// Don't walk through walls
			hSpeed = 0;
			extraHSpeed = 0;
		}
	}

	void CalcGravity(float delta)
	{
		if (_colTop)
		{
			if (vSpeed < 0)
			{
				// Move out of the celling
				vSpeed = 0;
			}
		}

		if (_colBottom)
		{
			if (vSpeed > 0)
			{
				// Move out of the ground
				vSpeed = 0;
			}
		}
		else
		{
			// Fall
			vSpeed += fallSpeed * delta;

			// Clamp
			if (vSpeed > fallClamp * delta) vSpeed = fallClamp * delta;
		}
	}

	void CalcJump(float delta)
	{
		_minJumpSpeed = -Math.Sqrt(2 * (fallSpeed * delta) * minJumpHeight);
		_maxJumpSpeed = -Math.Sqrt(2 * (fallSpeed * delta) * maxJumpHeight);

		if (_coyoteTimer > 0 && _jumpBuffer > 0)
		{
			_jumpBuffer = -1;
			vSpeed = _maxJumpSpeed;
		}

		if (_cancelJump && vSpeed < _minJumpSpeed)
		{
			vSpeed = _minJumpSpeed;
		}
		_cancelJump = false;
	}

	protected override void Render(Batch2D batch)
	{
		var squish = Math.Abs(vSpeed) / Time.fixedDelta / fallClamp * 0.5f;
		batch.Image(_sprite, Color.white);
	}
}
