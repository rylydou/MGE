

namespace MGE
{
	public class SpriteRenderer : Transform
	{
		public Texture texture;
		[Prop] public string texturePath;
		[Prop] public Color color = Color.white;
		[Prop] public float depth;

		public SpriteRenderer() { }

		public SpriteRenderer(Texture texture, Color? color = null, float depth = 0)
		{
			this.texture = texture;
			if (color.HasValue) this.color = color.Value;
			this.depth = depth;
		}

		public SpriteRenderer(string path, Color? color = null, float depth = 0)
		{
			texturePath = path;

			if (color.HasValue) this.color = color.Value;
			this.depth = depth;
		}

		protected override void Init()
		{
			if (texture is null) texture = Assets.GetAsset<Texture>(texturePath);

			base.Init();
		}

		protected override void Draw()
		{
			if (color.intA > 0 && texture?.texture is not null)
				GFX.Draw(texture, absolutePosition, null, color, absoluteRotation, Vector2.zero, absoluteScale, depth);
			// GFX.Draw(texture, absolutePosition, null, color, absoluteRotation, texture.size / 2f, absoluteScale, depth);

			base.Draw();
		}
	}
}
