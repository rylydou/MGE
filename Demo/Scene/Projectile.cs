using System;
using System.Collections.Generic;
using MGE;
using Math = MGE.Math;

namespace Demo;

public class Projectile : Body2D
{
	[Prop] public float speed;
	[Prop] public float lifetime;
	[Prop] public float deAcceleration;

	[Prop] public int damage;
	[Prop] public float knockback;

	public Texture sprite = App.content.Get<Texture>("Scene/Projectile/Sprite.ase");

	public List<Actor> hitActors = new();

	protected override void Ready()
	{
		base.Ready();

		collider = new HitboxCollider2D(new(8), new(-4));
	}

	protected override void Tick(float delta)
	{
		base.Tick(delta);

		var hitPlayer = false;
		var hitH = MoveH(speed * delta * scale.y, player =>
		{
			hitPlayer = true;
			player.Damage(damage, Vector2.right * knockback * scale.y);
			RemoveSelf();
			return false;
		});
		if (hitPlayer) return;

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

		speed = Math.MoveTowards(speed, 0, deAcceleration * delta);
	}

	public CollisionInfo? MoveH(float h, Func<Actor, bool> onHit)
	{
		var moveH = (int)System.Math.Round(h, MidpointRounding.ToEven);

		var target = position + right * moveH;
		var dir = Math.Sign(moveH);
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

			var solid = CollideFirst<Solid>(step);

			if (solid is not null)
			{
				return new CollisionInfo()
				{
					direction = right * (float)dir,
					moved = right * (float)move,
					targetPosition = target,
					hit = (Platform)solid,
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
		batch.Rect(new(0, 0, speed * Time.tickDelta * 4 * -scale.y, 1), Color.red, new(255, 0, 0, 0), Color.red, new(255, 0, 0, 0));
		batch.Draw(sprite, Vector2.zero, Color.white);
	}
}
