using MGE.Graphics;

namespace MGE;

public class SpriteNode : TransformNode
{
	public Texture texture = Texture.pixelTexture;
	public Color color = Color.white;

	public SpriteNode() { }

	public SpriteNode(Texture texture)
	{
		this.texture = texture;
	}

	protected override void Draw()
	{
		if (texture is not null)
		{
			GFX.DrawTexture(texture, worldPosition, worldScale, worldRotation, color);
		}
		else
		{
			GFX.DrawBox(worldPosition, worldScale, worldRotation, color);
		}

		base.Draw();
	}
}
