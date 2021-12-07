using tainicom.Aether.Physics2D.Dynamics;

#nullable disable

namespace MGE;

public class CollisionShapeNode : TransformNode
{
	[Prop] public float density;

	internal Fixture fixture;

	protected override void Init()
	{
		fixture.Tag = this;

		base.Init();
	}
}
