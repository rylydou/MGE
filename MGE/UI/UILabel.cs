namespace MGE.UI;

public class UILabel : UIWidget
{
	[Prop] public string text;
	[Prop] public float fontSize = 16;

	[Prop] public TextAlignment textAlignment;

	public IFont font = App.content.Get<SDFFont>("Fonts/Regular.json");

	public UILabel(string text)
	{
		this.text = new(text);
	}

	protected override void Render(Batch2D batch)
	{
		batch.DrawString(font, text, absoluteRect, textAlignment, foregroundColor, fontSize);
	}
}
