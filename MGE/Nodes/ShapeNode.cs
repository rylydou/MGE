using tainicom.Aether.Physics2D.Dynamics;

#nullable disable

namespace MGE;

public class ShapeNode : TransformNode
{
	[Prop] public float density;

	public Fixture fixture;
}
