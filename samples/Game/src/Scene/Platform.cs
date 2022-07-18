namespace Game;

public abstract class Platform : Body2D
{
	public IEnumerable<Actor> GetRiders()
	{
		return (IEnumerable<Actor>)Physics.All(this, scene!.bodies.Where(body => body is Actor), globalPosition + Vector2.up);
	}

	public abstract bool IsSolid(Vector2 position, Vector2 direction);
}
