namespace MGE.UI;

public class UILabel : UIWidget
{
	public string text;

	public UILabel()
	{
		this.text = "Hello World";
	}

	public UILabel(string text)
	{
		this.text = text;
	}

	public override void InternalRender(UIRenderContext context)
	{
		base.InternalRender(context);

		Font.normal.DrawString(text, bounds.position, Color.white);
	}
}
