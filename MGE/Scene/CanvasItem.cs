namespace MGE;

public abstract class CanvasItem : Node
{
	public virtual Transform2D globalTransform { get; set; }

	public bool visible;

	protected override void RegisterCallbacks()
	{
		base.RegisterCallbacks();

		onUpdate += (delta) =>
		 {
			 var batch = Batch2D.current;
			 batch.PushMatrix(Vector2.zero, globalTransform.scale, globalTransform.origin, globalTransform.rotation);
			 onDraw(batch);
			 batch.PopMatrix();
		 };
		onDraw += Draw;
	}

	protected override void Update(float delta) => base.Update(delta);

	public delegate void DrawDelegate(Batch2D batch);
	public DrawDelegate onDraw = (batch) => { };
	protected virtual void Draw(Batch2D batch) { }
}
