using System.Collections.Generic;
using tainicom.Aether.Physics2D.Dynamics;

#nullable disable

namespace MGE;

public class WorldNode : Node
{
	[Prop] Vector2 _gravity = new(0, -32f);
	public Vector2 gravity { get => _gravity; set { _gravity = value; world.Gravity = value; } }

	internal World world;

	protected override void Init()
	{
		world = new(gravity);

		base.Init();
	}

	protected override void Tick(float deltaTime)
	{
		base.Tick(deltaTime);

		world.Step(deltaTime);
	}

	public ColliderNode OverlapPoint(Vector2 point)
	{
		var fixture = world.TestPoint(point);
		if (fixture is null) return null;
		return (ColliderNode)fixture.Tag;
	}

	public RaycastHit Raycast(Vector2 from, Vector2 to)
	{
		RaycastHit hit = null;
		world.RayCast((fixture, point, normal, fraction) => { hit = new((ColliderNode)fixture.Tag, point, normal); return 0; /* Stop the raycast */ }, from, to);
		return hit;
	}

	public RaycastHit[] RaycastAll(Vector2 from, Vector2 to)
	{
		var hits = new List<RaycastHit>();
		world.RayCast((fixture, point, normal, fraction) => { hits.Add(new((ColliderNode)fixture.Tag, point, normal)); return 1; /* Continue the raycast, I think */ }, from, to);
		return hits.ToArray();
	}
}
