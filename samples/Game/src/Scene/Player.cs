#nullable disable

namespace Game;

public class Player : Actor
{
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

	[Prop] public float coyoteTime;
	float _coyoteTimer;

	public PlayerData data { get; init; }

	Node2D holdPoint;
	Area2D pickupArea;
	Area2D punchArea;

	public Item heldItem;

	public float hMoveSpeed;

	public bool isCrouching = false;

	HitboxCollider2D _normalHitbox = new HitboxCollider2D(new(12, 12), new(-6, -6));
	HitboxCollider2D _crouchingHitbox = new HitboxCollider2D(new(12, 6), new(-6, 0));

	public Player(PlayerData data)
	{
		this.data = data;
	}

	protected override void Ready()
	{
		base.Ready();

		holdPoint = GetChild<Node2D>("Holder");
		pickupArea = GetChild<Area2D>("Pickup area");
		punchArea = GetChild<Area2D>("Punch area");

		collider = _normalHitbox;
		collider.color = data.color;
		collider.CenterOrigin();

		Respawn();
	}

	protected override void Update(float delta)
	{
		var kb = App.input.keyboard;

		if (data.controls.move.intValue != 0)
		{
			scale = new(data.controls.move.intValue, 1);
		}
	}

	protected override void Tick(float delta)
	{
		base.Tick(delta);

		_punchCooldownTimmer -= delta;

		if (data.controls.action.pressed)
		{
			if (data.controls.crouch.down)
			{
				data.controls.action.ConsumeBuffer();

				// Pickup/Drop item
				if (heldItem is not null)
				{
					// Drop held item
					DropHeldItem();
				}
				else
				{
					// Pickup item from ground
					var item = pickupArea.CollideFirst<Item>();
					if (item is not null)
					{
						Pickup(item);
					}
				}
			}
			else
			{
				// Use/Punch
				if (heldItem is not null)
				{
					// Use item
					var usedItem = heldItem.Use();

					// If used item then reset the use buffer
					if (usedItem)
					{
						data.controls.action.ConsumeBuffer();
					}
				}
				else
				{
					// Can punch?
					if (_punchCooldownTimmer < 0)
					{
						_punchCooldownTimmer = punchCooldown;
						data.controls.action.ConsumeBuffer();

						foreach (var actor in punchArea.CollideAll<Actor>().ToArray())
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
		}

		_coyoteTimer -= delta;
		if (hitBottom) _coyoteTimer = coyoteTime;

		CalcCrouch(delta);
		CalcWalk(delta);
		CalcGravity(delta);
		CalcJump(delta);

		MoveV(vSpeed + (hitBottom ? 1 : 0));
		MoveH(hMoveSpeed + hSpeed - (hitLeft ? 1 : 0) + (hitRight ? 1 : 0));

		if (position.y > Main.screenSize.y + 16) OnDeath();
	}

	protected override void OnDeath()
	{
		Respawn();
	}

	void Respawn()
	{
		position = new(Main.screenSize.x / 2, 0);
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
		holdPoint.AddChild(heldItem);
		heldItem.position = Vector2.zero;
		heldItem.Pickup(this);
	}

	public void DisownHelditem()
	{
		if (heldItem is null) throw new System.Exception();

		heldItem.DisownFromParent();
		heldItem.RemoveSelf();
		parent.AddChild(heldItem);
		heldItem.position = position;
		heldItem = null;
	}

	void DropHeldItem()
	{
		if (heldItem is null) throw new System.Exception();

		var item = heldItem;

		DisownHelditem();
		item.Drop();
	}

	#region Movement

	void CalcCrouch(float delta)
	{
		if (isCrouching == data.controls.crouch.down) return;

		if (data.controls.crouch.down)
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

			var isBlocked = CollideCheck<Platform>();

			// If you can't then crouch again
			if (isBlocked)
			{
				isCrouching = true;
				collider = _crouchingHitbox;
			}
		}

		holdPoint.position = new(8, isCrouching ? 2 : 0);
	}

	void CalcWalk(float delta)
	{
		_moveClamp = Mathf.MoveTowards(
			_moveClamp,
			isCrouching ? crouchMoveClamp : moveClamp,
			(isCrouching ? moveClampDeAceleration : moveClampAceleration) * delta
		);

		if (data.controls.move.intValue != 0)
		{
			// Set horizontal move speed
			hMoveSpeed += data.controls.move.intValue * (hitBottom ? acceleration : accelerationAir) * delta;

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

		if (_coyoteTimer > 0 && data.controls.jump.pressed)
		{
			data.controls.jump.ConsumeBuffer();
			vSpeed = isCrouching ? _minJumpSpeed : _maxJumpSpeed;
		}

		if (data.controls.jump.released && vSpeed < _minJumpSpeed)
		{
			data.controls.jump.ConsumeBuffer();
			vSpeed = _minJumpSpeed;
		}
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

		var hopWave = Mathf.Sin((float)Time.duration.TotalSeconds * Mathf.PI * hopsPerSecond) / 2 + 1;

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

		var sprite = data.skin.spriteSheet.atlas["0"]!.GetClipSubtexture(data.skin.spriteSheet.slices[0].rect);

		for (int y = -1; y <= 1; y++)
		{
			for (int x = -1; x <= 1; x++)
			{
				batch.Draw(sprite, offset + new Vector2(x, y), pivot, Mathf.Deg2Rad(tilt), scale, data.color, true);
			}
		}

		batch.Draw(sprite, offset, pivot, Mathf.Deg2Rad(tilt), scale, color, washed);

		// batch.SetCircle(offset, 4, 8, Color.black);
		// batch.SetCircle(offset, 2, 8, data.color);

		var holdOffset = new Vector2(8, isCrouching ? 2 : 0);

		const float barWidth = 20;

		var bgRect = new Rect(-barWidth / 2, -18, barWidth, 2);
		var fillRect = new Rect(-barWidth / 2, -18, (float)health / maxHealth * barWidth, 2);

		if (health <= 0)
		{
			fillRect.width = 0;
		}
		else if (fillRect.width < 1 && health > 0)
		{
			fillRect.width = 1;
		}

		batch.SetBox(bgRect, new(0, 0, 0, 256 - 64));
		batch.SetBox(fillRect, data.color);
		// batch.SetRect(bgRect, 1, data.color);

		// collider.Render(batch);
	}
}
