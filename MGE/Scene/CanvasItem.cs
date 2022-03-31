namespace MGE;

public abstract class CanvasItem : Node
{
	public bool visible;

	protected override void RegisterCallbacks()
	{
		base.RegisterCallbacks();

		onUpdate += (delta) => Draw(Batch2D.current);
	}

	protected override void Update(float delta) => base.Update(delta);

	protected virtual void Draw(Batch2D batch) { }
}
