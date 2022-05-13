namespace Demo;

public abstract class Platform : Body2D
{
	public IEnumerable<Actor> GetRiders()
	{
		return (IEnumerable<Actor>)Physics.All(this, scene!.bodies.Where(body => body is Actor), globalPosition + Vector2.up);
	}
}
