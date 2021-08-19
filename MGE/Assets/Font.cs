namespace MGE
{
	public sealed class FontLoader : AssetLoader<Font>
	{
		protected override Font LoadAsset(string path)
		{
			var font = new Font(new Texture(path));

			font.charSize = new Vector2Int(10, 17);
			font.spacing = new Vector2(2, 6);

			return font;
		}
	}

	public class Font : System.IDisposable
	{
		Texture texture;

		public int offset;

		public Vector2Int charSize;
		public Vector2 spacing;

		public Vector2 fullCharSize { get => (Vector2)charSize + spacing; }

		public Font() { }

		public Font(Texture texture)
		{
			this.texture = texture;
			charSize.y = texture.size.y;
		}

		public void Draw(string text, Vector2 position, Color? color = null, float scale = 1)
		{
			for (int i = 0; i < text.Length; i++)
			{
				var source = new RectInt(charSize.x * (text[i] - offset), 0, charSize.x, texture.texture.Height);
				var destination = new Rect(position.x + fullCharSize.x * i * scale, position.y, charSize * scale);

				GFX.Draw(texture, destination, source, color);
			}
		}

		public void Dispose() => texture.Dispose();
	}
}