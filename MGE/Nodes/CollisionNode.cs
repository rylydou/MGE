using System.Collections.Generic;
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
