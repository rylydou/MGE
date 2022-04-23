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

	[Prop] public float bonkSpeed;

	Texture _sprite = App.content.Get<Texture>("Scene/Player/Chicken.ase");

	bool _alt;
	public bool isPlayer2
	{
		get => _alt;
		set
		{
			_alt = value;
			if (_alt)
				_sprite = App.content.Get<Texture>("Scene/Player/Amogus.ase");
		}
	}

	float _moveInput;
	bool _cancelJump;
	bool _crouching;

	bool _colTop;
	bool _colBottom;
	bool _colLeft;
	bool _colRight;

	public float vSpeed;
	public float hSpeed;
	public float extraHSpeed;

	bool facingRight = true;

	protected override void Ready()
	{
		SetCollider(new HitboxCollider2D(new(12)));
		collider!.CenterOrigin();

		Respawn();
	}

	protected override void Update(float delta)
	{
		var kb = App.input.keyboard;

		// Get input
		_moveInput = 0;
		if (kb.Down(isPlayer2 ? Keys.Left : Keys.A)) _moveInput -= 1;
		if (kb.Down(isPlayer2 ? Keys.Right : Keys.D)) _moveInput += 1;

		if (_moveInput != 0)
		{
			facingRight = _moveInput >= 0;
		}

		_jumpBuffer -= delta;
		if (kb.Pressed(isPlayer2 ? Keys.Up : Keys.W)) _jumpBuffer = jumpBufferLength;
		if (kb.Released(isPlayer2 ? Keys.Up : Keys.W)) _cancelJump = true;
		_crouching = kb.Down(isPlayer2 ? Keys.Down : Keys.S);

		// if (kb.Pressed(Keys.R)) Respawn();

		// if (kb.Pressed(Keys.Space))
		// {
		// 	extraHSpeed = 256 * Time.fixedDelta;
		// 	vSpeed = -256 * Time.fixedDelta;
		// }
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
		var hitX = MoveH(hSpeed + extraHSpeed);

		if (globalPosition.y > Game.screenSize.y + 16) Respawn();
	}

	void Respawn()
	{
		globalPosition = new(Game.screenSize.x / 2, 0);
		hSpeed = 0;
		vSpeed = fallClamp * Time.fixedDelta;
	}

	void CalcWalk(float delta)
	{
		if (_moveInput != 0)
		{
			// Set horizontal move speed
			hSpeed += _moveInput * (_colBottom ? acceleration : accelerationAir) * delta;

			// clamped by max frame movement
			hSpeed = Math.Clamp(hSpeed, -moveClamp * delta, moveClamp * delta) * (_crouching ? 0.66f : 1f);
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
				vSpeed = bonkSpeed * delta;
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
			vSpeed = _crouching ? _minJumpSpeed : _maxJumpSpeed;
		}

		if (_cancelJump && vSpeed < _minJumpSpeed)
		{
			vSpeed = _minJumpSpeed;
		}
		_cancelJump = false;
	}

	protected override void Render(Batch2D batch)
	{
		const float centerX = 8;
		const float centerY = 8;

		const float hopsPerSecond = 10;

		const float stretchFactor = 0.5f;
		const float vFallingOffset = -4;

		const float normalTiltAngle = 10;
		const float fallingTiltAngle = -15;
		const float vTiltOffset = 1;

		const float vCrouchOffset = 4;

		var realVSpeed = vSpeed / Time.fixedDelta;
		var realHSpeed = hSpeed / Time.fixedDelta;

		var vSpeedScale = realVSpeed / fallClamp;
		var hSpeedScale = realHSpeed / moveClamp;

		var fallingSpeedScale = Math.Clamp01(vSpeedScale);

		var hopWave = Math.Sin((float)Time.duration.TotalSeconds * Math.pi * hopsPerSecond) / 2 + 1;

		var offset = new Vector2(0, vTiltOffset * Math.Abs(hSpeedScale) + vFallingOffset * fallingSpeedScale);

		// Move the pivot behind when moving and hop
		var pivot = new Vector2(centerX * hSpeedScale * hopWave, centerY);

		// Stretch the player when falling
		var stretch = Math.Abs(vSpeedScale) * stretchFactor;
		var scale = new Vector2(facingRight ? 1 - stretch : -1 + stretch, 1 + stretch);

		var normalTilt = hSpeedScale * normalTiltAngle;
		var fallingTilt = hSpeedScale * fallingTiltAngle;
		// Mix between normal tilt and falling tilt based on vertical speed
		var tilt = Math.Lerp(normalTilt, fallingTilt, fallingSpeedScale);

		if (_crouching)
		{
			offset.y += vCrouchOffset;
			scale.y *= 0.66f;
			tilt = 0;
		}

		batch.Draw(_sprite, offset, pivot, scale, Math.Deg2Rad(tilt), Color.white);

		// collider!.Render(batch);
	}
}
