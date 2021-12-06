using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Contacts;

#nullable disable

namespace MGE;

public class KinematicNode : PhysicsBody
{
	protected override void Init()
	{
		body = world.world.CreateBody(worldPosition, worldRotation, BodyType.Kinematic);

		body.OnCollision += (sender, other, contact) =>
		{
			OnCollision(other, contact);
			return true; // TODO Figure out what the return is for
		};

		base.Init();
	}

	protected override void Tick(float deltaTime)
	{
		localPosition = body.Position;
		localRotation = body.Rotation;

		base.Tick(deltaTime);
	}

	protected virtual void OnCollision(Fixture other, Contact contact) { }
}
