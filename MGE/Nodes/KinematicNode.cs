using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Contacts;

#nullable disable

namespace MGE;

public class KinematicNode : PhysicsBody
{
	protected override void Init()
	{
		body = new() { Position = worldPosition, Rotation = worldRotation, BodyType = BodyType.Static, };

		body.OnCollision += (sender, other, contact) =>
		{
			OnCollision(other, contact);
			return true; // TODO Figure out what the return is for
		};

		base.Init();
	}

	protected virtual void OnCollision(Fixture other, Contact contact) { }
}
