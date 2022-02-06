namespace MGE.UI;

public class UICanvas : UIBox
{
	protected override void UpdateMeasure()
	{
		_rect.width = fixedWidth;
		_rect.height = fixedHeight;

		base.UpdateMeasure();
	}
}
