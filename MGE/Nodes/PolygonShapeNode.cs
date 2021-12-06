#nullable disable

using System.Collections.Generic;
using System.Linq;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Dynamics;

namespace MGE;

public class PolygonShapeNode : ShapeNode
{
	public List<Vector2> vertices = new();

	protected override void Init()
	{
		fixture = new Fixture(new PolygonShape(new(vertices.Select(x => (tainicom.Aether.Physics2D.Common.Vector2)x)), density));

		base.Init();
	}
}
