using System.Collections.Generic;
using tainicom.Aether.Physics2D.Dynamics;

#nullable disable

namespace MGE;

public abstract class BodyNode : TransformNode
{
	[Prop] public bool autoSleep = false;
	[Prop] public bool isAwake = true;

	[Prop] public bool enableSweeping = false;

	[Prop] public int collisionLayer = 1;
	[Prop] public int collisionMask = 1;

	internal List<ColliderNode> collisionShapes = new();
	internal Body body;

	public WorldNode world { get; private set; }

	protected override void Init()
	{
		body.Tag = this;

		body.SleepingAllowed = autoSleep;
		body.Awake = isAwake;

		body.IsBullet = enableSweeping;

		base.Init();
	}

	protected override void WhenAttached()
	{
		world = GetParent<WorldNode>();

		world.world.Add(body);

		// Debug.Log($"Added {this} to {world}");

		base.WhenAttached();
	}

	protected override void WhenDetached()
	{
		world.world.Remove(body);

		world = null;

		base.WhenDetached();
	}

	protected virtual void ConnectCollider(ColliderNode collider)
	{
		// Debug.Log($"Added {collider} to {this}");
	}

	protected override bool WhenChildAttached(Node child)
	{
		if (child is not ColliderNode collider) return true;

		collider.body = this;
		collisionShapes.Add(collider);
		body.Add(collider.fixture);

		ConnectCollider(collider);

		return false;
	}

	protected override bool WhenChildDetached(Node child)
	{
		if (child is not ColliderNode shape) return true;

		collisionShapes.Remove(shape);

		return false;
	}
}
