namespace MGE.UI;

public class UIButton : UIBin
{
	public UIButton(string text)
	{
		AddWidget(new UIText(text));
	}
}
