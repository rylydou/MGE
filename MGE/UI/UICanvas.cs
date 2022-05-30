namespace MGE.UI;

public class UICanvas : UIBox
{
	public bool enableDebug = false;

	public UICanvas()
	{
		canvas = this;
	}

	public void RenderCanvas(Batch2D batch)
	{
		DoRender(batch);
	}
}
