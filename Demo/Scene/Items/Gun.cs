#nullable disable

namespace Demo.Items;

public partial class Gun : Item
{
	[Prop] public float spread;
	[Prop] public float minSpeedVariation;
	[Prop] public float maxSpeedVariation;
	[Prop] public float numberOfBullets;
	[Prop] public float recoilForce;

	[Prop] public Projectile projectile;

	Prefab _projPrefab;
	Node2D _shootPoint;

	protected override void Ready()
	{
		base.Ready();

		_projPrefab = projectile.CreatePrefab();

		_shootPoint = GetChild<Node2D>("Shoot point");
		sprite = App.content.Get<Texture>("Scene/Items/Shotgun/Sprite.ase");
	}

	protected override bool OnUse()
	{
		if (holder.OnGround())
			holder.ApplyImpulseForce(right * -recoilForce);
		else
			holder.ApplyImpulseForce(one * -recoilForce);

		for (int i = 0; i < numberOfBullets; i++)
		{
			var proj = _projPrefab.CreateInstance<Projectile>();
			proj.transform = _shootPoint.GetGlobalTransform();
			proj.rotation = Mathf.Deg2Rad(RNG.shared.RandomFloat(-spread / 2, spread / 2));
			proj.speed *= RNG.shared.RandomFloat(minSpeedVariation, maxSpeedVariation);
			proj.hitActors.Add(holder);

			scene.AddChild(proj);
		}

		return true;
	}
}
