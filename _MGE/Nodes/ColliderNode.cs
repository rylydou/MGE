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
		var color = Color.yellow.WithAlpha(0.5f);

		fixture.GetAABB(out var aabb, 0);
		GFX.DrawRect(new(fixture.Body.Position.X + aabb.LowerBound.X, fixture.Body.Position.Y + aabb.LowerBound.Y, aabb.Width, aabb.Height), color);

		var shape = fixture.Shape;
		color = fixture.Body.Awake ? Color.green : Color.green.WithAlpha(0.5f);

		if (shape is PolygonShape polygon)
		{
			GFX.DrawPolyline(polygon.Vertices.Select(x => (Vector2)(fixture.Body.Position + x)).ToArray(), color);
		}
		else if (shape is CircleShape circle)
		{
			GFX.DrawCircle(fixture.Body.Position + circle.Position, circle.Radius, color);
		}

		base.Draw();
	}
}
