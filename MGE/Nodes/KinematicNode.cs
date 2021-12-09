using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Contacts;

#nullable disable

namespace MGE;

public class KinematicNode : PhysicsBodyNode
{
	protected override void Init()
	{
		body = new() { Position = worldPosition, Rotation = worldRotation, BodyType = BodyType.Static, };

		body.OnCollision += (sender, other, contact) =>
		{
			OnCollision(other, contact);
			return true; // DOCS https://github.com/tainicom/Aether.Physics2D/blob/master/Physics2D/Dynamics/Contacts/Contact.cs#L317
		};

		base.Init();
	}

	protected virtual void OnCollision(Fixture other, Contact contact) { }
}
