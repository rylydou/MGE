using System.Collections.Generic;
using System.Linq;
using tainicom.Aether.Physics2D.Collision.Shapes;

#nullable disable

namespace MGE;

public class PolygonColliderNode : ColliderNode
{
	[Prop] public List<Vector2> vertices = new();

	protected override void Init()
	{
		fixture = new(new PolygonShape(new(vertices.Select(x => (tainicom.Aether.Physics2D.Common.Vector2)x)), 1));

		base.Init();
	}
}
