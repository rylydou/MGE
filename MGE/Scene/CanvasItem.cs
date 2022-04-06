namespace MGE;

public abstract class CanvasItem : Node
{
	public bool visible;

	protected override void RegisterCallbacks()
	{
		base.RegisterCallbacks();

		onUpdate += (delta) => onDraw(Batch2D.current);
		onDraw += Draw;
	}

	protected override void Update(float delta) => base.Update(delta);

	public delegate void DrawDelegate(Batch2D batch);
	public DrawDelegate onDraw = (batch) => { };
	protected virtual void Draw(Batch2D batch) { }
}
