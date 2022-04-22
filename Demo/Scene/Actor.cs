using System;
using MGE;
using Math = MGE.Math;

namespace Demo;

public class Actor : Body2D
{
	Vector2 movementCounter;
	public Vector2 positionRemainder => movementCounter;
	public Vector2 exactPosition => globalPosition + movementCounter;

	public void ZeroRemainderX() => this.movementCounter.x = 0.0f;
	public void ZeroRemainderY() => this.movementCounter.y = 0.0f;

	public bool treatNaive;
	public bool ignoreJumpThrus;
	public bool allowPushing = true;
	public float liftSpeedGraceTime = 0.16f;

	protected override void RegisterCallbacks()
	{
		base.RegisterCallbacks();

		onSquish += OnSquish;
	}

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
							if (!CollideCheck<Solid>(globalPosition + vec))
							{
								globalPosition = globalPosition + vec;
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
							if (!CollideCheck<Solid>(info.targetPosition + vec))
							{
								globalPosition = info.targetPosition + vec;
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

	public virtual bool IsRiding(Semisolid semisolid) => !ignoreJumpThrus && CollideCheckOutside(semisolid, globalPosition + Vector2.down);
	public virtual bool IsRiding(Solid solid) => CollideCheck(solid, globalPosition + Vector2.down);

	public bool OnGround(int downCheck = 1)
	{
		if (CollideCheck<Solid>(globalPosition + Vector2.down * downCheck))
			return true;
		return !ignoreJumpThrus && CollideCheckOutside<Semisolid>(globalPosition + Vector2.down * downCheck);
	}

	public bool OnGround(Vector2 at, int downCheck = 1)
	{
		Vector2 position = globalPosition;
		globalPosition = at;
		var hit = OnGround(downCheck);
		globalPosition = position;
		return hit;
	}

	public CollisionInfo? MoveH(float moveH, Solid? pusher = null)
	{
		movementCounter.x += moveH;
		var moveH1 = (int)System.Math.Round(movementCounter.x, MidpointRounding.ToEven);
		if (moveH1 == 0)
			return null;
		movementCounter.x -= moveH1;
		return MoveHExact(moveH1, pusher);
	}

	public CollisionInfo? MoveV(float moveV, Solid? pusher = null)
	{
		this.movementCounter.y += moveV;
		var moveV1 = (int)System.Math.Round(movementCounter.y, MidpointRounding.ToEven);
		if (moveV1 == 0)
			return null;
		movementCounter.y -= moveV1;
		return MoveVExact(moveV1, pusher);
	}

	public CollisionInfo? MoveHExact(int moveH, Solid? pusher = null)
	{
		var target = globalPosition + Vector2.right * moveH;
		var dir = Math.Sign(moveH);
		var move = 0;

		while (moveH != 0)
		{
			var solid = CollideFirst<Solid>(globalPosition + Vector2.right * dir);

			if (solid is not null)
			{
				movementCounter.x = 0.0f;

				return new CollisionInfo()
				{
					direction = Vector2.right * (float)dir,
					moved = Vector2.right * (float)move,
					targetPosition = target,
					hit = (Platform)solid,
					pusher = pusher
				};
			}

			move += dir;
			moveH -= dir;

			globalPosition = new(globalPosition.x + dir, globalPosition.y);
		}

		return null;
	}

	public CollisionInfo? MoveVExact(int moveV, Solid? pusher = null)
	{
		var target = globalPosition + Vector2.down * (float)moveV;
		var dir = Math.Sign(moveV);
		var move = 0;

		while (moveV != 0)
		{
			var solid = CollideFirst<Solid>(globalPosition + Vector2.down * dir);
			if (solid is not null)
			{
				movementCounter.y = 0.0f;

				return new CollisionInfo()
				{
					direction = Vector2.down * dir,
					moved = Vector2.down * move,
					targetPosition = target,
					hit = solid,
					pusher = pusher
				};
			}

			if (moveV > 0 && !ignoreJumpThrus)
			{
				var semisolid = this.CollideFirstOutside<Semisolid>(globalPosition + Vector2.down * dir);

				if (semisolid is not null)
				{
					movementCounter.y = 0.0f;

					return new CollisionInfo()
					{
						direction = Vector2.down * dir,
						moved = Vector2.down * move,
						targetPosition = target,
						hit = semisolid,
						pusher = pusher
					};
				}
			}

			move += dir;
			moveV -= dir;

			globalPosition = new(globalPosition.x, globalPosition.y + dir);
		}

		return null;
	}

	public CollisionInfo? MoveTowardsX(float targetX, float maxAmount) => MoveToX(Calc.Approach(exactPosition.x, targetX, maxAmount));
	public CollisionInfo? MoveTowardsY(float targetY, float maxAmount) => MoveToY(Calc.Approach(exactPosition.y, targetY, maxAmount));

	public CollisionInfo? MoveToX(float toX) => MoveH(toX - exactPosition.x);
	public CollisionInfo? MoveToY(float toY) => MoveV(toY - exactPosition.y);

	public void NaiveMove(Vector2 amount)
	{
		movementCounter += amount;
		var x = Math.RoundToInt(this.movementCounter.x);
		var y = Math.RoundToInt(this.movementCounter.y);
		globalPosition = globalPosition + new Vector2(x, y);
		movementCounter -= new Vector2(x, y);
	}
}
