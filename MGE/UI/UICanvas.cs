namespace MGE.UI;

public class UICanvas : UIBox
{
	public bool enableDebug = true;

	public UICanvas()
	{
		canvas = this;
	}

	protected override void UpdateMeasure()
	{
		_rect.width = fixedWidth;
		_rect.height = fixedHeight;

		base.UpdateMeasure();
	}
}
