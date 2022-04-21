using MGE;

namespace Demo;

public struct CollisionInfo
{
	public static readonly CollisionInfo empty;

	public Vector2 direction;
	public Vector2 moved;
	public Vector2 targetPosition;
	public Platform hit;
	public Solid? pusher;
}
