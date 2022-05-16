#nullable disable

namespace Demo.Items;

public class WoodenCrate : Item
{
	public Prefab item = App.content.Get<Prefab>("Scene/Items/Shotgun/Shotgun.node");

	public WoodenCrate()
	{
		sprite = App.content.Get<Texture>("Scene/Items/Wooden Crate/Sprite.ase");
	}

	protected override void OnDeath()
	{
		parent.AddChild(item.CreateInstance());

		base.OnDeath();
	}
}
