namespace MGE.UI;

public class UICanvas : UIBox
{
	public bool enableDebug = false;

	public UICanvas()
	{
		canvas = this;
	}

	internal override void PropertiesChanged()
	{
		_rect.size = fixedSize;

		base.PropertiesChanged();
	}

	public void RenderCanvas(Batch2D batch)
	{
		DoRender(batch);
	}
}
