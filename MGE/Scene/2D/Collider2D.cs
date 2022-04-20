namespace MGE;

public abstract class Collider2D
{
	public CollisionObject2D? owner { get; internal set; }
	public Vector2 position;
}
