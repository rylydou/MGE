using tainicom.Aether.Physics2D.Dynamics;

#nullable disable

namespace MGE;

public class ColliderNode : TransformNode
{
	internal Fixture fixture;
	public BodyNode body;

	protected override void Draw()
	{
		fixture.GetAABB(out var aabb, 0);
		GFX.DrawBoxOutline(new(body.worldPosition.x + aabb.LowerBound.X, body.worldPosition.y + aabb.LowerBound.Y, aabb.Width, aabb.Height), new Color(1, 1, 0, 1f / 3));

		base.Draw();
	}
}
