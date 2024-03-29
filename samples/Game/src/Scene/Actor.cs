#nullable disable

namespace Game;

// Based on https://maddythorson.medium.com/celeste-and-towerfall-physics-d24bd2ae0fc5
public class Actor : Body2D
{
	[Prop] public int maxHealth = 100;
	public int health;

	public float hSpeed;
	public float vSpeed;

	public bool hitTop;
	public bool hitBottom;
	public bool hitLeft;
	public bool hitRight;

	Vector2 _movementCounter;
	public Vector2 positionRemainder => _movementCounter;
	public Vector2 exactPosition => position + _movementCounter;

	public void ZeroRemainderX() => _movementCounter.x = 0f;
	public void ZeroRemainderY() => _movementCounter.y = 0f;

	public bool treatNaive;
	// public bool ignoreJumpThrus;
	public bool allowPushing = true;

	public float timeSinceLastDamage = float.PositiveInfinity;

	protected override void Ready()
	{
		base.Ready();

		health = maxHealth;
	}

	protected override void RegisterCallbacks()
	{
		base.RegisterCallbacks();

		onSquish += OnSquish;
	}

	protected override void Tick(float delta)
	{
		timeSinceLastDamage += delta;
	}

	public void Damage(int damage, Vector2 knockback)
	{
		health -= damage;
		ApplyImpulseForce(knockback);

		if (health <= 0)
		{
			OnDeath();
			return;
		}

		timeSinceLastDamage = 0;
	}

	public void ApplyImpulseForce(Vector2 force)
	{
		hSpeed += force.x * Time.tickDelta;
		vSpeed = force.y * Time.tickDelta;
	}

	protected virtual void OnDeath()
	{
		RemoveSelf();
	}

	#region Movement

	public Action<CollisionInfo> onSquish = info => { };
	public virtual void OnSquish(CollisionInfo info)
	{
		if (TrySquishWiggle(info))
			return;
		RemoveSelf();
	}

	protected bool TrySquishWiggle(CollisionInfo info, int wiggleX = 3, int wiggleY = 3)
	{
		if (info.pusher is not null)
			info.pusher.collidable = true;

		for (int x = 0; x <= wiggleX; ++x)
		{
			for (int y = 0; y <= wiggleY; ++y)
			{
				if (x != 0 || y != 0)
				{
					for (int i = 1; i >= -1; i -= 2)
					{
						for (int j = 1; j >= -1; j -= 2)
						{
							var vec = new Vector2(x * i, y * j);
							if (!CollideCheck<Platform>(position + vec))
							{
								position = position + vec;
								if (info.pusher is not null)
									info.pusher.collidable = false;
								return true;
							}
						}
					}
				}
			}
		}

		for (int x = 0; x <= wiggleX; ++x)
		{
			for (int y = 0; y <= wiggleY; ++y)
			{
				if (x != 0 || y != 0)
				{
					for (int i = 1; i >= -1; i -= 2)
					{
						for (int j = 1; j >= -1; j -= 2)
						{
							var vec = new Vector2(x * i, y * j);
							if (!CollideCheck<Platform>(info.targetPosition + vec))
							{
								position = info.targetPosition + vec;
								if (info.pusher is not null)
									info.pusher.collidable = false;
								return true;
							}
						}
					}
				}
			}
		}

		if (info.pusher is not null)
			info.pusher.collidable = false;

		return false;
	}

	// public virtual bool IsRiding(Semisolid semisolid) => !ignoreJumpThrus && CollideCheckOutside(semisolid, position + Vector2.down);
	public virtual bool IsRiding(Platform solid) => CollideCheck(solid, position + Vector2.down);

	public bool OnGround(int downCheck = 1)
	{
		return CollideCheck<Platform>(position + Vector2.down * downCheck);
		// return !ignoreJumpThrus && CollideCheckOutside<Semisolid>(position + Vector2.down * downCheck);
	}

	public bool OnGround(Vector2 at, int downCheck = 1)
	{
		Vector2 lastPosition = position;
		position = at;
		var hit = OnGround(downCheck);
		position = lastPosition;
		return hit;
	}

	public CollisionInfo? MoveH(float moveH, Platform pusher = null)
	{
		_movementCounter.x += moveH;
		var moveH1 = (int)System.Math.Round(_movementCounter.x, MidpointRounding.ToEven);
		if (moveH1 == 0)
		{
			hitLeft = false;
			hitRight = false;
			return null;
		}
		_movementCounter.x -= moveH1;
		return MoveHExact(moveH1, pusher);
	}

	public CollisionInfo? MoveV(float moveV, Platform pusher = null)
	{
		_movementCounter.y += moveV;
		var moveV1 = (int)System.Math.Round(_movementCounter.y, MidpointRounding.ToEven);
		if (moveV1 == 0)
		{
			hitTop = false;
			hitBottom = false;
			return null;
		}
		_movementCounter.y -= moveV1;
		return MoveVExact(moveV1, pusher);
	}

	public CollisionInfo? MoveHExact(int moveH, Platform pusher = null)
	{
		hitLeft = false;
		hitRight = false;

		var target = position + Vector2.right * moveH;
		var dir = Mathf.Sign(moveH);
		var move = 0;

		while (moveH != 0)
		{
			var step = position + Vector2.right * dir;
			var platform = CollideFirst<Platform>(step);

			if (platform is not null && platform.IsSolid(position, Vector2.right * dir))
			{
				_movementCounter.x = 0f;
				hitRight = dir > 0;
				hitLeft = dir < 0;

				return new CollisionInfo()
				{
					direction = Vector2.right * (float)dir,
					moved = Vector2.right * (float)move,
					targetPosition = target,
					hit = (Platform)platform,
					pusher = pusher
				};
			}

			move += dir;
			moveH -= dir;

			position = step;
		}

		return null;
	}

	public CollisionInfo? MoveVExact(int moveV, Platform pusher = null)
	{
		hitTop = false;
		hitBottom = false;

		var target = position + Vector2.down * (float)moveV;
		var dir = Mathf.Sign(moveV);
		var move = 0;

		while (moveV != 0)
		{
			var step = position + Vector2.down * dir;
			var platform = CollideFirst<Platform>(step);

			if (platform is not null && platform.IsSolid(position, Vector2.down * dir))
			{
				_movementCounter.y = 0f;
				hitBottom = dir >= 0;
				hitTop = dir < 0;

				return new CollisionInfo()
				{
					direction = Vector2.down * dir,
					moved = Vector2.down * move,
					targetPosition = target,
					hit = platform,
					pusher = pusher
				};
			}

			move += dir;
			moveV -= dir;

			position = step;
		}

		return null;
	}

	public CollisionInfo? MoveTowardsX(float targetX, float maxAmount) => MoveToX(Calc.Approach(exactPosition.x, targetX, maxAmount));
	public CollisionInfo? MoveTowardsY(float targetY, float maxAmount) => MoveToY(Calc.Approach(exactPosition.y, targetY, maxAmount));

	public CollisionInfo? MoveToX(float toX) => MoveH(toX - exactPosition.x);
	public CollisionInfo? MoveToY(float toY) => MoveV(toY - exactPosition.y);

	public void NaiveMove(Vector2 amount)
	{
		_movementCounter += amount;
		var x = Mathf.RoundToInt(this._movementCounter.x);
		var y = Mathf.RoundToInt(this._movementCounter.y);
		position = position + new Vector2(x, y);
		_movementCounter -= new Vector2(x, y);
	}

	#endregion Movement
}
