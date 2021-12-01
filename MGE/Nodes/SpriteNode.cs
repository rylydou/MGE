using MGE.Graphics;

namespace MGE
{
	public class SpriteNode : TransformNode
	{
		public Texture? texture;
		public Color color;

		protected override void Draw()
		{
			if (texture is not null)
			{
				GFX.DrawTexture(texture, position, scale, rotation, color);
			}
			else
			{
				GFX.DrawSquare(position, scale, rotation, color);
			}

			base.Draw();
		}
	}
}
