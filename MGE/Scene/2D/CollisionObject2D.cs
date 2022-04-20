namespace MGE;

public class CollisionObject2D : Node2D
{
	[Prop] public CollisionLayer collisionLayer = CollisionLayer.Layer1;
	[Prop] public CollisionLayer collisionMask = CollisionLayer.Layer1;

	[Prop] public Collider2D? collider;
	[Prop] public bool collidable;

	// public CollisionObject2D? CollideFirst(Tag tag)
	// {
	// 	if (!isInScene) return null;
	// 	return Physics.First(this, scene.collisionObjects);
	// }

	// public CollisionObject2D? CollideFirst(Tag tag, Vector2 at)
	// {
	// 	if (!isInScene) return null;
	// 	return Physics.First(this, scene.collisionObjects, at);
	// }

	public CollisionObject2D? CollideFirst()
	{
		if (!isInScene) return null;
		return Physics.First(this, scene.collisionObjects);
	}

	public CollisionObject2D? CollideFirst(Vector2 at)
	{
		if (!isInScene) return null;
		return Physics.First(this, scene.collisionObjects, at);
	}
}
