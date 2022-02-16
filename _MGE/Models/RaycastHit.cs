namespace MGE;

public class RaycastHit
{
	public ColliderNode collider;
	public Vector2 point;
	public Vector2 normal;

	internal RaycastHit(ColliderNode collider, Vector2 point, Vector2 normal)
	{
		this.collider = collider;
		this.point = point;
		this.normal = normal;
	}
}
