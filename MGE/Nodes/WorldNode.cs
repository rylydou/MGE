using tainicom.Aether.Physics2D.Dynamics;

#nullable disable

namespace MGE;

public class WorldNode : Node
{
	public World world;
	public Vector2 gravity = new(0, -128f);

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
}
