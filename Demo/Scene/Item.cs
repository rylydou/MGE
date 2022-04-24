using System.Diagnostics.CodeAnalysis;
using MGE;

namespace Demo;

public abstract class Item : Actor
{
	[Prop] public float fallSpeed = 16;
	[Prop] public float fallClamp = 384;

	[Prop] public float deAcceration = 4;
	[Prop] public float deAccerationAir = 2;

	public Player? holder { get; private set; }
	public bool isHeld
	{
		[MemberNotNullWhen(true, nameof(holder))]
		get => holder is not null;
	}

	public Texture? sprite;

	public float hSpeed;
	public float vSpeed;

	protected override void Ready()
	{
		base.Ready();

		SetCollider(new HitboxCollider2D(new(10, 10), new(-5, -5)));

		globalPosition = new(320, -320);
	}

	protected override void Tick(float delta)
	{
		base.Tick(delta);

		if (isHeld)
		{
			Held_Tick(delta);
		}
		else
		{
			Dropped_Tick(delta);
		}
	}

	[MemberNotNull(nameof(holder))]
	public virtual void Use()
	{
		if (holder is null) throw new Exception("Use item", "Holder is null, how did this even happen?");
	}

	public void Pickup(Player player)
	{
		holder = player;
	}

	protected virtual void OnPickup()
	{
		vSpeed = 0;
		hSpeed = 0;
	}

	public void Drop()
	{
		holder = null;
	}

	protected virtual void OnDrop()
	{
		vSpeed = -96 * Time.tickDelta;
		hSpeed = 96 * Time.tickDelta * globalScale.x;
	}

	protected virtual void Held_Tick(float delta) { }

	protected virtual void Dropped_Tick(float delta)
	{
		// Gravity
		if (hitTop && vSpeed < 0) vSpeed = 0;

		if (hitBottom)
		{
			if (vSpeed > 0)
			{
				vSpeed = 0;
			}
		}
		else
		{
			vSpeed += fallSpeed * delta;

			if (vSpeed > fallClamp * delta)
				vSpeed = fallClamp * delta;
		}

		// Movement
		hSpeed = Math.MoveTowards(hSpeed, 0, (hitBottom ? deAcceration : deAccerationAir) * delta);

		if (hitLeft && hSpeed < 0) hSpeed = 0;
		if (hitRight && hSpeed > 0) hSpeed = 0;

		var hitV = MoveV(vSpeed);
		if (hitV.HasValue)
		{
			vSpeed = 0;
		}

		var hitH = MoveH(hSpeed);
		if (hitH.HasValue)
		{
			hSpeed = 0;
		}
	}

	protected override void Render(Batch2D batch)
	{
		if (sprite is not null)
		{
			batch.Draw(sprite, Vector2.zero, Color.white);
		}
	}
}
