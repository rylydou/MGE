using MGE.Graphics;

namespace MGE.UI;

public class UIImage : UIWidget
{
	public Texture texture = Texture.pixelTexture;
	public RectInt source;
	public Color color = Color.white;

	protected override void Render()
	{
		GFX.DrawTextureRegionAtDest(texture, source, rect, color);
	}
}
