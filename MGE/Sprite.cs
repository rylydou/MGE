using MGE.Graphics;

namespace MGE;

public class Sprite
{
	public Texture texture;
	public RectInt source;

	public Sprite(Texture texture, RectInt source)
	{
		this.texture = texture;
		this.source = source;
	}
}
