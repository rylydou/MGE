using System.Linq;
using tainicom.Aether.Physics2D.Collision.Shapes;
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
		GFX.DrawBoxOutline(new(body.worldPosition.x + aabb.LowerBound.X, body.worldPosition.y + aabb.LowerBound.Y, aabb.Width, aabb.Height), Color.yellow);

		var shape = fixture.Shape;
		var color = fixture.Body.Awake ? Color.green : Color.green.WithAlpha(1f / 2);

		if (shape is PolygonShape polygon)
		{
			GFX.DrawPolygonOutline(polygon.Vertices.Select(x => (Vector2)(body.body.Position + x)).ToArray(), color);
		}
		else if (shape is CircleShape circle)
		{
			GFX.DrawCircleOutline(body.body.Position + circle.Position, circle.Radius, color);
		}

		base.Draw();
	}
}
