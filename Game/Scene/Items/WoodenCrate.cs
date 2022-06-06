#nullable disable

namespace Game.Items;

public class WoodenCrate : Item
{
	public Prefab item = App.content.Get<Prefab>("Scene/Items/Shotgun/Shotgun.node");

	public WoodenCrate()
	{
		sprite = App.content.Get<Texture>("Scene/Items/Wooden Crate/Sprite.ase");
	}

	protected override void Ready()
	{
		base.Ready();

		globalPosition = new(RNG.shared.RandomInt(Main.screenSize.x), 0);
	}

	protected override bool OnUse()
	{
		holder.DisownHelditem();

		ApplyImpulseForce(new(256 * globalScale.x, -96));

		return false;
	}

	protected override void OnDeath()
	{
		var itemInstance = item.CreateInstance<Item>();
		itemInstance.position = position;
		parent.AddChild(itemInstance);

		base.OnDeath();
	}
}
