using System.Text;

namespace MGE.UI;

public class UILabel : UIWidget
{
	public StringBuilder text = new();
	public Color textColor = Color.white;

	public Vector2<UIAlignment> textAlignment = new(UIAlignment.Start, UIAlignment.Center);

	public UILabel(string text)
	{
		this.text = new(text);
	}

	public UILabel(StringBuilder text)
	{
		this.text = text;
	}

	protected override void Render(Batch2D batch)
	{
		// var textSize = (Vector2Int)Font.normal.MeasureString(text);
		// var position = rect.position;

		// switch (textAlignment.horizontal)
		// {
		// 	case UIAlignment.Center: position.x += (rect.width - textSize.x) / 2; break;
		// 	case UIAlignment.End: position.x += (rect.width - textSize.x); break;
		// }

		// switch (textAlignment.vertical)
		// {
		// 	case UIAlignment.Center: position.y += (rect.height - textSize.y) / 2; break;
		// 	case UIAlignment.End: position.y += (rect.height - textSize.y); break;
		// }

		// Font.normal.DrawString(text, position, textColor);
	}
}
