#nullable disable

namespace Game;

public abstract class Item : Actor
{
	[Prop] public float useCooldown = 0.5f;

	[Prop] public float fallSpeed = 12;
	[Prop] public float fallClamp = 360;

	[Prop] public float deAcceration = 16;
	[Prop] public float deAccerationAir = 8;

	public float useCooldownTimmer;

	public Player holder { get; private set; }

	public Texture sprite;

	protected override void Ready()
	{
		base.Ready();

		collider = new HitboxCollider2D(new(10, 10), new(-5, -5));
	}

	protected sealed override void Tick(float delta)
	{
		base.Tick(delta);

		useCooldownTimmer -= delta;

		if (holder is not null)
		{
			Held_Tick(delta);
		}
		else
		{
			Dropped_Tick(delta);
		}
	}

	public bool Use()
	{
		if (useCooldownTimmer > 0) return false;

		if (OnUse())
		{
			useCooldownTimmer = useCooldown;
			return true;
		}

		return false;
	}

	protected virtual bool OnUse() => false;

	public void Pickup(Player player)
	{
		holder = player;
		collidable = false;
	}

	protected virtual void OnPickup()
	{
		vSpeed = 0;
		hSpeed = 0;
	}

	public void DisownFromParent()
	{
		holder = null;
		collidable = true;
	}

	public void Drop()
	{
		DisownFromParent();

		OnDrop();
	}

	protected virtual void OnDrop()
	{
		ApplyImpulseForce(new(96 * globalScale.x, -96));
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
		hSpeed = Mathf.MoveTowards(hSpeed, 0, (hitBottom ? deAcceration : deAccerationAir) * delta);

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
			batch.Draw(sprite, Vector2.zero, Color.LerpClamped(Color.white, Color.black, useCooldownTimmer / useCooldown));
		}
	}
}
