using System.Collections.Generic;
using System.Linq;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Dynamics;

#nullable disable

namespace MGE;

public abstract class CollisionNode : TransformNode
{
	[Prop] public int collisionLayer = 1;
	[Prop] public int collisionMask = 1;

	internal List<ColliderNode> collisionShapes = new();
	internal Body body;

	public WorldNode world { get; private set; }

	protected override void Init()
	{
		body.Tag = this;

		base.Init();
	}

	protected override void Draw()
	{
		foreach (var fixture in body.FixtureList)
		{
			var shape = fixture.Shape;
			var color = fixture.Body.Awake ? Color.green : new Color(0, 1, 0, 1f / 3);

			if (shape is PolygonShape polygon)
			{
				GFX.DrawPolygonOutline(polygon.Vertices.Select(x => (Vector2)(body.Position + x)).ToArray(), color);
			}
			else if (shape is CircleShape circle)
			{
				GFX.DrawCircleOutline(circle.Position, circle.Radius, color);
			}
		}

		base.Draw();
	}

	protected override void WhenAttached()
	{
		world = GetParent<WorldNode>();

		world.world.Add(body);

		base.WhenAttached();
	}

	protected override void WhenDetached()
	{
		world.world.Remove(body);

		world = null;

		base.WhenDetached();
	}

	protected override bool WhenChildAttached(Node child)
	{
		if (child is not ColliderNode shape) return true;

		collisionShapes.Add(shape);

		body.Add(shape.fixture);

		return true;
	}

	protected override bool WhenChildDetached(Node child)
	{
		if (child is not ColliderNode shape) return true;

		collisionShapes.Remove(shape);

		return true;
	}
}
