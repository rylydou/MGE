namespace MGE;

public class CanvasItem : Node
{
	public bool visible;

	public override void _Update(float deltaTime)
	{
		base._Update(deltaTime);

		_Draw();
	}

	public virtual void _Draw()
	{
	}
}
