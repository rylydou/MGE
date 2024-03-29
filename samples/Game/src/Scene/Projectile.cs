using System;

namespace Game;

public class Projectile : Body2D
{
	[Prop] public float speed;
	[Prop] public float lifetime;
	[Prop] public float deAcceleration;

	[Prop] public Damage damage;

	public Texture sprite = App.content.Get<Texture>("Nodes/Projectile/Sprite.ase");

	public List<Actor> hitActors = new();

	protected override void Ready()
	{
		base.Ready();

		collider = new HitboxCollider2D(new(2), new(-4)) { color = Color.red };
		collider.CenterOrigin();
	}

	protected override void Tick(float delta)
	{
		base.Tick(delta);

		var hitActor = false;
		var hitH = MoveF(speed * delta * scale.y, actor =>
		{
			hitActor = true;
			damage.DamageActor(actor, Vector2.right * scale.y);
			RemoveSelf();
			return false;
		});
		if (hitActor) return;

		if (hitH.HasValue)
		{
			RemoveSelf();
			return;
		}

		lifetime -= delta;
		if (lifetime < 0)
		{
			RemoveSelf();
			return;
		}

		speed = Mathf.MoveTowards(speed, 0, deAcceleration * delta);
	}

	public CollisionInfo? MoveF(float h, Func<Actor, bool> onHit)
	{
		var moveH = (int)System.Math.Round(h, MidpointRounding.ToEven);

		var target = position + right * moveH;
		var dir = Mathf.Sign(moveH);
		var move = 0;

		while (moveH != 0)
		{
			var step = position + right * dir;

			foreach (var actor in CollideAll<Actor>(step))
			{
				if (hitActors.Contains(actor)) continue;

				var shouldContinue = onHit(actor);
				if (!shouldContinue) return null;
				hitActors.Add(actor);
			}

			var platform = CollideFirst<Platform>(step);

			if (platform is not null && platform.IsSolid(position, right * dir))
			{
				return new CollisionInfo()
				{
					direction = right * (float)dir,
					moved = right * (float)move,
					targetPosition = target,
					hit = platform,
				};
			}

			move += dir;
			moveH -= dir;

			position = step;
		}

		return null;
	}

	protected override void Render(Batch2D batch)
	{
		batch.SetBox(new(0, 0, speed * Time.tickDelta * 4 * -scale.y, 1), Color.white, new(255, 0, 0, 0), new(255, 0, 0, 0), Color.white);

		batch.SetCircle(Vector2.zero, collider!.size.x, 8, new(0xFF0000FF));
		// batch.Draw(sprite, Vector2.zero, Color.white);
	}
}
