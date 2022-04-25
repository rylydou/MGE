// TODO  Implement better crouching physics

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
	[Prop] public float crouchSpeedMult;

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

	[Prop] public float pickupRange;

	public Texture sprite = App.content.Get<Texture>("Scene/Player/Red.ase");
	Texture inputErrorSprite = App.content.Get<Texture>("Input Error.ase");

	Node2D? holdPoint;

	public Controls controls = new Controls(-1);
	public Item? heldItem;

	public float hMoveSpeed;

	public bool isCrouching = false;

	HitboxCollider2D _normalHitbox = new HitboxCollider2D(new(12, 12), new(-6, -6));
	HitboxCollider2D _crouchingHitbox = new HitboxCollider2D(new(12, 6), new(-6, 0));

	protected override void Ready()
	{
		base.Ready();

		holdPoint = GetChild<Node2D>("Hold point");

		// if (holdPoint!.parent != this) throw new System.Exception("Hold point's parent is not me, it is " + holdPoint!.parent!.name);

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

		if (controls.action)
		{
			if (controls.crouch)
			{
				// Pickup/Drop item
				if (heldItem is not null)
				{
					// Drop held item
					DropHeldItem();
				}
				else
				{
					// Pickup item from ground
					var item = CollideFirst<Item>();
					if (item is not null)
					{
						Pickup(item);
					}
				}
			}
			else
			{
				if (heldItem is not null)
				{
					// Use item
					heldItem.Use();
				}
				else
				{
					// TODO  Punch
				}
			}
		}
		controls.action = false;

		_coyoteTimer -= delta;
		if (hitBottom) _coyoteTimer = coyoteTime;

		CalcCrouch(delta);
		CalcWalk(delta);
		CalcGravity(delta);
		CalcJump(delta);

		var hitV = MoveV(vSpeed);
		var hitX = MoveH(hMoveSpeed + hSpeed);

		if (position.y > Game.screenSize.y + 16) OnDeath();
	}

	protected override void OnDeath()
	{
		Respawn();
	}

	void Respawn()
	{
		position = new(Game.screenSize.x / 2, 0);
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
		if (controls.move != 0)
		{
			// Set horizontal move speed
			hMoveSpeed += controls.move * (hitBottom ? acceleration : accelerationAir) * delta;

			// clamped by max frame movement
			hMoveSpeed = Math.Clamp(hMoveSpeed, -moveClamp * delta, moveClamp * delta);

			if (isCrouching)
			{
				hMoveSpeed *= crouchSpeedMult;
			}
		}
		else
		{
			// No input. Let's slow the character down
			hMoveSpeed = Math.MoveTowards(hMoveSpeed, 0, (hitBottom ? deAcceleration : deAccelerationAir) * delta);
		}

		hSpeed = Math.MoveTowards(hSpeed, 0, (hitBottom ? deAccelerationEx : deAccelerationExAir) * delta);

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
		_minJumpSpeed = -Math.Sqrt(2 * (fallSpeed * delta) * minJumpHeight);
		_maxJumpSpeed = -Math.Sqrt(2 * (fallSpeed * delta) * maxJumpHeight);

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
		var hSpeedScale = Math.Abs(realHSpeed / moveClamp);

		var fallingSpeedScale = Math.Clamp01(vSpeedScale);

		var hopWave = Math.Sin((float)Time.duration.TotalSeconds * Math.pi * hopsPerSecond) / 2 + 1;

		var offset = new Vector2(0, vTiltOffset * Math.Abs(hSpeedScale) + vFallingOffset * fallingSpeedScale);

		// Move the pivot behind when moving and hop
		var pivot = new Vector2(centerX * hSpeedScale * hopWave, centerY);

		// Stretch the player when falling
		var stretch = Math.Abs(vSpeedScale) * stretchFactor;
		var scale = new Vector2(1 - stretch, 1 + stretch);

		var normalTilt = hSpeedScale * normalTiltAngle;
		var fallingTilt = hSpeedScale * fallingTiltAngle;
		// Mix between normal tilt and falling tilt based on vertical speed
		var tilt = Math.Lerp(normalTilt, fallingTilt, fallingSpeedScale);

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
			color = RNG.shared.RandomBool() ? Color.white : Color.red;
			washed = true;
		}

		batch.Draw(sprite, offset, pivot, Math.Deg2Rad(tilt), scale, color, washed);

		if (!controls.isPresent || controls.hasError)
		{
			batch.Draw(inputErrorSprite, new(0, -24), new(0, -24), Time.SineWave(-Math.pi / 6, Math.pi / 8, 1f, 0f), Vector2.one, Color.white);
		}

		var holdOffset = new Vector2(8, isCrouching ? 2 : 0);

		const float barWidth = 20;
		batch.Rect(new(-barWidth / 2, -18, barWidth, 2), Color.black.translucent);
		batch.Rect(new(-barWidth / 2, -18, (float)health / maxHealth * barWidth, 2), Color.green);

		// collider!.Render(batch);
	}
}
