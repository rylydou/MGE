namespace Game;

public interface IPhysicalTileBehavior
{
	bool IsSolid(Vector2Int tilePos, Vector2 origin, Vector2 direction);
}

public class SolidTile : IPhysicalTileBehavior
{
	public virtual bool IsSolid(Vector2Int tilePos, Vector2 origin, Vector2 direction)
	{
		return true;
	}
}

public class PlatformTile : IPhysicalTileBehavior
{
	public virtual bool IsSolid(Vector2Int tilePos, Vector2 origin, Vector2 direction)
	{
		return origin.y <= tilePos.y && direction.y > 0;
	}
}

public abstract class AreaTile : IPhysicalTileBehavior
{
	public virtual bool IsSolid(Vector2Int tilePos, Vector2 origin, Vector2 direction)
	{
		return false;
	}

	public virtual void OnActorEnter(Actor actor) { }
	public virtual void OnActorExit(Actor actor) { }
	public virtual void WhileActorInside(Actor actor) { }
}
