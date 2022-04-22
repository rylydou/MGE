using MGE;

namespace Demo;

public class Player : Actor
{
	[Prop] float _fallSpeed;
	[Prop] float _minFallSpeed;
	[Prop] float _maxFallSpeed;
	[Prop] float _fallClamp;

	[Prop] float _acceleration;
	[Prop] float _deAcceleration;
	[Prop] float _moveClamp;

	[Prop] float _jumpApexThreshold;

	float _apexPoint;
	float _apexBonus;

	[Prop] float _minJumpHeight;
	float _minJumpSpeed;
	[Prop] float _maxJumpHeight;
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
			_hSpeed = 0;
			_vSpeed = 0;
		}
	}

	public Vector2 velocity { get; private set; }
	Vector2 _lastPosition;

	bool grounded;

	float _vSpeed;
	float _hSpeed;

	protected override void Tick(float delta)
	{
		velocity = (globalPosition - _lastPosition) / delta;

		grounded = OnGround();
		_coyoteTimer -= delta;
		if (grounded) _coyoteTimer = coyoteTime;

		CalcWalk(delta);
		CalcJumpApex(delta);
		CalcGravity(delta);
		CalcJump(delta);

		var hitY = MoveV(_vSpeed);
		if (hitY.HasValue)
		{
			Bump(hitY.Value);
			// _vSpeed = 0;
		}

		var hitX = MoveH(_hSpeed);
		if (hitX.HasValue)
		{
			Bump(hitX.Value);
		}

		_lastPosition = globalPosition;

		void Bump(CollisionInfo info)
		{
			if (_vSpeed < 0) _vSpeed = 0;
			var dir = globalPosition - info.hit.globalPosition;
			var move = new Vector2(_hSpeed, _vSpeed).normalized * delta;
			globalPosition += dir.normalized * move.length;
		}
	}

	void CalcWalk(float delta)
	{
		if (_moveInput != 0)
		{
			// Set horizontal move speed
			_hSpeed += _moveInput * _acceleration * delta;

			// clamped by max frame movement
			_hSpeed = Math.Clamp(_hSpeed, -_moveClamp, _moveClamp);

			// Apply bonus at the apex of a jump
			var apexBonus = Math.Sign(_moveInput) * _apexBonus * _apexPoint;
			_hSpeed += apexBonus * delta;
		}
		else
		{
			// No input. Let's slow the character down
			_hSpeed = Math.MoveTowards(_hSpeed, 0, _deAcceleration * delta);
		}

		if (_hSpeed > 0 && CollideCheck<Ground>(globalPosition + Vector2.right) || _hSpeed < 0 && CollideCheck<Ground>(globalPosition + Vector2.left))
		{
			// Don't walk through walls
			_hSpeed = 0;
		}
	}

	void CalcJumpApex(float delta)
	{
		if (!grounded)
		{
			// Gets stronger the closer to the top of the jump
			_apexPoint = Math.InverseLerp(_jumpApexThreshold, 0, Math.Abs(_vSpeed));
			_fallSpeed = Math.Lerp(_minFallSpeed, _maxFallSpeed, _apexPoint);
		}
		else
		{
			_apexPoint = 0;
		}
	}

	void CalcGravity(float delta)
	{
		if (grounded)
		{
			if (_vSpeed > 0)
			{
				// Move out of the ground
				_vSpeed = 0;
			}
		}
		else
		{
			var fallSpeed = _fallSpeed;
			// Add downward force while ascending if we ended the jump early
			// var fallSpeed = _endedJumpEarly && _currentVerticalSpeed > 0 ? _fallSpeed * _jumpEndEarlyGravityModifier : _fallSpeed;

			// Fall
			_vSpeed += fallSpeed * delta;

			// Clamp
			if (_vSpeed > _fallClamp) _vSpeed = _fallClamp;
		}
	}

	void CalcJump(float delta)
	{
		if (_coyoteTimer > 0 && _jumpBuffer > 0)
		{
			_jumpBuffer = -1;
			_vSpeed = _maxJumpSpeed;
		}

		if (_cancelJump && _vSpeed < _minJumpSpeed)
		{
			_vSpeed = _minJumpSpeed;
		}
		_cancelJump = false;
	}

	protected override void Render(Batch2D batch)
	{
		var time = (float)Time.duration.TotalSeconds;
		batch.Image(_sprite, Vector2.zero, Vector2.one, Vector2.zero, 0, Color.white);
	}
}
