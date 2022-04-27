using MGE;

namespace Demo.Items;

public class Gun : Item
{
	[Prop] public float spread;
	[Prop] public float minSpeedVariation;
	[Prop] public float maxSpeedVariation;
	[Prop] public float numberOfBullets;
	[Prop] public float recoilForce;

	[Prop] public Projectile? projectile;

	Node2D? shootPoint;

	protected override void Ready()
	{
		base.Ready();

		shootPoint = GetChild<Node2D>("Shoot point");
		sprite = App.content.Get<Texture>("Scene/Items/Shotgun/Sprite.ase");
	}

	protected override void OnUse()
	{
		if (holder!.OnGround())
			holder.ApplyImpulseForce(right * -recoilForce);
		else
			holder.ApplyImpulseForce(one * -recoilForce);

		for (int i = 0; i < numberOfBullets; i++)
		{
			var proj = projectile!.CreateNewInstance<Projectile>();
			proj.transform = shootPoint!.GetGlobalTransform();
			proj.rotation = Mathf.Deg2Rad(RNG.shared.RandomFloat(-spread / 2, spread / 2));
			proj.speed *= RNG.shared.RandomFloat(minSpeedVariation, maxSpeedVariation);
			proj.hitActors.Add(holder);

			scene!.AddChild(proj);
		}
	}
}
