using tainicom.Aether.Physics2D.Collision.Shapes;

namespace MGE.Nodes;

public class CircleColliderNode : ColliderNode
{
	[Prop] public float radius = 16;

	protected override void Init()
	{
		fixture = new(new CircleShape(radius, 1));

		base.Init();
	}
}
