using MGE;

namespace Demo.Items;

public class Shotgun : Item
{
	Prefab projectile = App.content.Get<Prefab>("Scene/Items/Shotgun/Projectile.node");

	Node2D? shootPoint;

	protected override void Ready()
	{
		base.Ready();

		shootPoint = GetChild<Node2D>("Shoot point");
		sprite = App.content.Get<Texture>("Scene/Items/Shotgun/Sprite.ase");
	}

	public override void Use()
	{
		base.Use();

		if (holder.OnGround())
			holder.ApplyImpulseForce(right * -196);
		else
			holder.ApplyImpulseForce(new Vector2(-196 * right.x, -196));

		for (int i = 0; i < 3; i++)
		{
			var proj = projectile.CreateInstance<Projectile>();
			proj.speed *= RNG.shared.RandomFloat(0.75f, 1f);
			proj.hitPlayers.Add(holder);
			scene!.AddChild(proj);
			proj.transform = shootPoint!.GetGlobalTransform();
			proj.rotation = Math.Deg2Rad(RNG.shared.RandomFloat(-5, 5));
		}
	}
}
