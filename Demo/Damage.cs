using MGE;

namespace Demo;

public struct Damage
{
	[Prop] public int damage;
	[Prop] public float knockback;

	public void DamageActor(Actor actor, Vector2 direction)
	{
		actor.Damage(damage, direction * knockback);
	}
}
