using MGE.Graphics;
using tainicom.Aether.Physics2D.Dynamics;

#nullable disable

namespace MGE;

public class ColliderNode : TransformNode
{
	[Prop] public float density;

	internal Fixture fixture;

	protected override void Init()
	{
		fixture.Tag = this;

		base.Init();
	}

	protected override void Draw()
	{
		fixture.GetAABB(out var aabb, 0);
		GFX.DrawBoxOutline(aabb, Color.yellow);

		base.Draw();
	}
}
