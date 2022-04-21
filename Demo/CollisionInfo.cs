using MGE;

namespace Demo;

public struct CollisionInfo
{
	public static readonly CollisionInfo Empty;

	public Vector2 Direction;
	public Vector2 Moved;
	public Vector2 TargetPosition;
	// public Platform Hit;
	// public Solid Pusher;
}
