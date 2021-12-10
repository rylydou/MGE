using tainicom.Aether.Physics2D.Dynamics;

namespace MGE;

public class DynamicNode : PhysicsBodyNode
{
	protected override void Init()
	{
		body = new() { Position = worldPosition, Rotation = worldRotation, BodyType = BodyType.Dynamic, };

		body.Awake = true;
		body.IgnoreGravity = false;
		body.LinearVelocity = new(64, 16);

		base.Init();
	}
}
