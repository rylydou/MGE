using tainicom.Aether.Physics2D.Dynamics;

#nullable disable

namespace MGE;

public class ColliderNode : TransformNode
{
	internal Fixture fixture;

	protected override void Draw()
	{
		fixture.GetAABB(out var aabb, 0);
		GFX.DrawBoxOutline(aabb, new Color(1, 1, 0, 1f / 3));

		base.Draw();
	}
}
