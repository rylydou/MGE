using MGE;

namespace Demo;

public class Player : Actor
{
	[Prop] public float useBufferLength;
	float _useBuffer;

	[Prop] public Damage punchDamage;
	[Prop] public float punchCooldown;
	float _punchCooldownTimmer;

	[Prop] public float fallSpeed;
	[Prop] public float fallClamp;

	[Prop] public float acceleration;
	[Prop] public float deAcceleration;
	[Prop] public float accelerationAir;
	[Prop] public float deAccelerationAir;
	[Prop] public float moveClamp;
	[Prop] public float crouchMoveClamp;
	float _moveClamp;
	[Prop] public float moveClampAceleration;
	[Prop] public float moveClampDeAceleration;

	[Prop] public float deAccelerationEx;
	[Prop] public float deAccelerationExAir;

	[Prop] public float minJumpHeight;
	float _minJumpSpeed;
	[Prop] public float maxJumpHeight;
	float _maxJumpSpeed;
	[Prop] public float bonkSpeed;

	[Prop] public float jumpBufferLength;
	float _jumpBuffer;
	[Prop] public float coyoteTime;
	float _coyoteTimer;

	public Texture sprite = App.content.Get<Texture>("Scene/Player/Red.ase");

	Node2D? holdPoint;
	Area2D? pickupArea;
	Area2D? punchArea;

	public Controls controls = new Controls(-1);
	public Item? heldItem;

	public float hMoveSpeed;

	public bool isCrouching = false;

	HitboxCollider2D _normalHitbox = new HitboxCollider2D(new(12, 12), new(-6, -6));
	HitboxCollider2D _crouchingHitbox = new HitboxCollider2D(new(12, 6), new(-6, 0));

	protected override void Ready()
	{
		base.Ready();

		holdPoint = GetChild<Node2D>("Holder");
		pickupArea = GetChild<Area2D>("Pickup area");
		punchArea = GetChild<Area2D>("Punch area");

		collider = _normalHitbox;
		collider!.CenterOrigin();

		Respawn();
	}

	protected override void Update(float delta)
	{
		controls.Update();
		var kb = App.input.keyboard;

		if (controls.move != 0)
		{
			scale = new(controls.move, 1);
		}
	}

	protected override void Tick(float delta)
	{
		base.Tick(delta);

		_punchCooldownTimmer -= delta;

		_useBuffer -= delta;
		if (controls.action)
		{
			if (controls.crouch)
			{
				_useBuffer = -1;
				// Pickup/Drop item
				if (heldItem is not null)
				{
					// Drop held item
					DropHeldItem();
				}
				else
				{
					// Pickup item from ground
					var item = pickupArea!.CollideFirst<Item>();
					if (item is not null)
					{
						Pickup(item);
					}
				}
			}
			else
			{
				// Use/Punch
				_useBuffer = useBufferLength;
			}
		}
		controls.action = false;

		if (_useBuffer > 0)
		{
			if (heldItem is not null)
			{
				// Use item
				var usedItem = heldItem.Use();

				// If used item then reset the use buffer
				if (usedItem)
				{
					_useBuffer = -1;
				}
			}
			else
			{
				// Can punch?
				if (_punchCooldownTimmer < 0)
				{
					_punchCooldownTimmer = punchCooldown;
					_useBuffer = -1;

					foreach (var actor in punchArea!.CollideAll<Actor>())
					{
						if (actor == this) continue;

						punchDamage.DamageActor(actor, right + up);

						// Give a player a double jump because getting comboed is not fun
						if (actor is Player player)
						{
							player._coyoteTimer = player.coyoteTime * 2;
						}
					}
				}
			}
		}

		_coyoteTimer -= delta;
		if (hitBottom) _coyoteTimer = coyoteTime;

		CalcCrouch(delta);
		CalcWalk(delta);
		CalcGravity(delta);
		CalcJump(delta);

		var hitV = MoveV(vSpeed);
		var hitX = MoveH(hMoveSpeed + hSpeed);

		if (position.y > Game.gameScreenSize.y + 16) OnDeath();
	}

	protected override void OnDeath()
	{
		Respawn();
	}

	void Respawn()
	{
		position = new(Game.gameScreenSize.x / 2, 0);
		hMoveSpeed = 0;
		hSpeed = 0;
		vSpeed = fallClamp * Time.tickDelta;

		health = maxHealth;

		timeSinceLastDamage = float.PositiveInfinity;
	}

	void Pickup(Item item)
	{
		if (heldItem is not null) throw new System.Exception();

		heldItem = item;
		heldItem.RemoveSelf();
		holdPoint!.AddChild(heldItem);
		heldItem.position = Vector2.zero;
		heldItem.Pickup(this);
	}

	void DropHeldItem()
	{
		if (heldItem is null) throw new System.Exception();

		heldItem.RemoveSelf();
		scene!.AddChild(heldItem);
		heldItem.position = globalPosition;
		heldItem.Drop();
		heldItem = null;
	}

	#region Movement

	void CalcCrouch(float delta)
	{
		if (isCrouching == controls.crouch) return;

		if (controls.crouch)
		{
			// No need to check when crouching
			isCrouching = true;
			collider = _crouchingHitbox;
		}
		else
		{

			// Check if can standup
			isCrouching = false;
			collider = _normalHitbox;

			var isBlocked = CollideCheck<Ground>();

			// If you can't then crouch again
			if (isBlocked)
			{
				isCrouching = true;
				collider = _crouchingHitbox;
			}
		}

		holdPoint!.position = new(8, isCrouching ? 2 : 0);
	}

	void CalcWalk(float delta)
	{
		_moveClamp = Mathf.MoveTowards(
			_moveClamp,
			isCrouching ? crouchMoveClamp : moveClamp,
			(isCrouching ? moveClampDeAceleration : moveClampAceleration) * delta
		);


		if (controls.move != 0)
		{
			// Set horizontal move speed
			hMoveSpeed += controls.move * (hitBottom ? acceleration : accelerationAir) * delta;

			// clamped by max frame movement
			hMoveSpeed = Mathf.Clamp(hMoveSpeed, -_moveClamp * delta, _moveClamp * delta);
		}
		else
		{
			// No input. Let's slow the character down
			hMoveSpeed = Mathf.MoveTowards(hMoveSpeed, 0, (hitBottom ? deAcceleration : deAccelerationAir) * delta);
		}

		hSpeed = Mathf.MoveTowards(hSpeed, 0, (hitBottom ? deAccelerationEx : deAccelerationExAir) * delta);

		if (hMoveSpeed > 0 && hitRight || hMoveSpeed < 0 && hitLeft)
		{
			// Don't walk through walls
			hMoveSpeed = 0;
			hSpeed = 0;
		}
	}

	void CalcGravity(float delta)
	{
		if (hitTop)
		{
			if (vSpeed < 0)
			{
				// Move out of the celling
				vSpeed = bonkSpeed * delta;
			}
		}

		if (hitBottom)
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
		_minJumpSpeed = -Mathf.Sqrt(2 * (fallSpeed * delta) * minJumpHeight);
		_maxJumpSpeed = -Mathf.Sqrt(2 * (fallSpeed * delta) * maxJumpHeight);

		_jumpBuffer -= delta;
		if (controls.jump)
		{
			_jumpBuffer = jumpBufferLength;
		}
		controls.jump = false;

		if (_coyoteTimer > 0 && _jumpBuffer > 0)
		{
			_jumpBuffer = -1;
			vSpeed = isCrouching ? _minJumpSpeed : _maxJumpSpeed;
		}

		if (controls.jumpCancel && vSpeed < _minJumpSpeed)
		{
			vSpeed = _minJumpSpeed;
		}
		controls.jumpCancel = false;
	}

	#endregion Movement

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

		var realVSpeed = vSpeed / Time.tickDelta;
		var realHSpeed = hMoveSpeed / Time.tickDelta;

		var vSpeedScale = realVSpeed / fallClamp;
		var hSpeedScale = Mathf.Abs(realHSpeed / moveClamp);

		var fallingSpeedScale = Mathf.Clamp01(vSpeedScale);

		var hopWave = Mathf.Sin((float)Time.duration.TotalSeconds * Mathf.pi * hopsPerSecond) / 2 + 1;

		var offset = new Vector2(0, vTiltOffset * Mathf.Abs(hSpeedScale) + vFallingOffset * fallingSpeedScale);

		// Move the pivot behind when moving and hop
		var pivot = new Vector2(centerX * hSpeedScale * hopWave, centerY);

		// Stretch the player when falling
		var stretch = Mathf.Abs(vSpeedScale) * stretchFactor;
		var scale = new Vector2(1 - stretch, 1 + stretch);

		var normalTilt = hSpeedScale * normalTiltAngle;
		var fallingTilt = hSpeedScale * fallingTiltAngle;
		// Mix between normal tilt and falling tilt based on vertical speed
		var tilt = Mathf.Lerp(normalTilt, fallingTilt, fallingSpeedScale);

		if (isCrouching)
		{
			offset.y += vCrouchOffset;
			scale.y *= 0.67f;
			// tilt = 0;
		}

		var color = Color.white;
		var washed = false;

		if (timeSinceLastDamage < 0.1f)
		{
			color = RNG.shared.RandomBool() ? Color.white : Color.black;
			washed = true;
		}

		batch.Draw(sprite, offset, pivot, Mathf.Deg2Rad(tilt), scale, color, washed);

		var holdOffset = new Vector2(8, isCrouching ? 2 : 0);

		const float barWidth = 20;
		batch.Rect(new(-barWidth / 2, -18, barWidth, 2), Color.black.translucent);
		batch.Rect(new(-barWidth / 2, -18, (float)health / maxHealth * barWidth, 2), Color.green);

		// collider!.Render(batch);
	}
}
