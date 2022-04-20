namespace MGE;

public class CollisionObject2D : Node2D
{
	public CollisionLayer collisionLayer = CollisionLayer.Layer1;
	public CollisionLayer collisionMask = CollisionLayer.Layer1;

	public Vector2 size = Vector2.one;
}
