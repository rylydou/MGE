namespace MGE.UI;

public class UIText : UIWidget
{
	public string text;

	public UIText(string text)
	{
		this.text = text;
	}

	public override void Draw()
	{
		// currentStyle.font.DrawText(text, box.position, currentStyle.foregroundColor);
	}
}
