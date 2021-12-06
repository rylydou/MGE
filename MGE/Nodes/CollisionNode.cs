using System.Collections.Generic;
using tainicom.Aether.Physics2D.Dynamics;

#nullable disable

namespace MGE;

public class CollisionNode : TransformNode
{
	public List<ShapeNode> collisionShapes { get; private set; } = new();
	public int collisionLayer = 1;
	public int collisionMask = 1;

	public Body body { get; private set; }

	public WorldNode world { get; private set; }

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
		if (child is not ShapeNode shape) return true;

		collisionShapes.Add(shape);

		return true;
	}

	protected override bool WhenChildDetached(Node child)
	{
		if (child is not ShapeNode shape) return true;

		collisionShapes.Remove(shape);

		return true;
	}
}
