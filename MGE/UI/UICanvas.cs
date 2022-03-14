namespace MGE.UI;

public class UICanvas : UIBox
{
	public bool enableDebug = true;

	public UICanvas()
	{
		canvas = this;
	}

	internal override void PropertiesChanged()
	{
		_rect.size = fixedSize;

		base.PropertiesChanged();
	}

	public void Draw(Batch2D batch)
	{
		DoRender(batch);
	}
}
