namespace MGE;

public class PhysicsBodyNode : BodyNode
{
	public Vector2 velocity { get => body.LinearVelocity; set => body.LinearVelocity = value; }
	public float angularVelocity { get => body.AngularVelocity; set => body.AngularVelocity = value; }

	protected override void Tick(float deltaTime)
	{
		localPosition = body.Position;
		localRotation = body.Rotation;

		base.Tick(deltaTime);

		// body.SetTransform(localPosition, worldRotation);
	}
}
