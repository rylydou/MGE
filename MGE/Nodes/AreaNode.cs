#nullable disable

using tainicom.Aether.Physics2D.Dynamics;

namespace MGE;

public class AreaNode : CollisionNode
{
	protected override void Init()
	{
		body = new() { Position = worldPosition, Rotation = worldRotation, BodyType = BodyType.Static, };

		base.Init();
	}
}
