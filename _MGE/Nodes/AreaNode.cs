#nullable disable

using tainicom.Aether.Physics2D.Dynamics;

namespace MGE;

public class AreaNode : BodyNode
{
	protected override void Init()
	{
		body = new() { Position = worldPosition, Rotation = worldRotation, BodyType = BodyType.Static, };

		base.Init();
	}

	protected override void ConnectCollider(ColliderNode collider)
	{
		collider.fixture.IsSensor = true;

		collider.fixture.BeforeCollision += (sender, other) => { OnAreaEnter((ColliderNode)other.Tag); return true; };
		collider.fixture.OnCollision += (sender, other, contact) => { OnAreaEnter((ColliderNode)other.Tag); return true; };
		collider.fixture.AfterCollision += (sender, other, contact, impulse) => OnAreaEnter((ColliderNode)other.Tag);

		base.ConnectCollider(collider);
	}

	protected virtual void OnAreaEnter(ColliderNode other) { }
}
