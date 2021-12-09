namespace MGE;

public class PhysicsBodyNode : CollisionNode
{
	protected override void Tick(float deltaTime)
	{
		localPosition = body.Position;
		localRotation = body.Rotation;

		base.Tick(deltaTime);

		body.SetTransform(localPosition, worldRotation);
	}
}
