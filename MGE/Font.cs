using System.Text;
using FontStashSharp;
using FontStashSharp.Interfaces;
using MGE.Graphics;

namespace MGE;

public class Font
{
	class Texture2DManager : ITexture2DManager
	{
		public Texture? texture;

		public object CreateTexture(int width, int height)
		{
			texture = new Texture(new(width, height));
			return texture;
		}

		public System.Drawing.Point GetTextureSize(object obj)
		{
			var texture = (Texture)obj;
			return texture.size;
		}

		public void SetTextureData(object obj, System.Drawing.Rectangle bounds, byte[] data)
		{
			var texture = (Texture)obj;
			texture.SetData(new(bounds.X, bounds.Y, bounds.Width, bounds.Height), data);
		}
	}

	class FontStashRenderer : IFontStashRenderer
	{
		ITexture2DManager textureManager = new Texture2DManager();
		public ITexture2DManager TextureManager => textureManager;

		public void Draw(object obj, System.Numerics.Vector2 pos, System.Drawing.Rectangle? src, System.Drawing.Color color, float rotation, System.Numerics.Vector2 origin, System.Numerics.Vector2 scale, float depth)
		{
			if (rotation != 0) throw new MGEException();
			if (!src.HasValue) throw new MGEException();

			var texture = (Texture)obj;

			var realPos = new Vector2(pos.X - origin.X * scale.X, pos.Y - (src.Value.Height - origin.Y) * scale.Y);
			var destRect = new Rect(realPos, src.Value.Width * scale.X, src.Value.Height * scale.Y);

			// GFX.DrawRect(destRect, ((Color)color).inverted.WithAlpha(1f / 3));

			GFX.DrawTextureRegionAtDest(texture, src.Value, destRect, color);
		}
	}

	public static Font normal = new(Folder.assets.GetFile("Fonts/Inter/Regular.ttf"));
	public static Font monospace = new(Folder.assets.GetFile("Fonts/Iosevka/Regular.ttf"));

	internal Texture? atlas => ((Texture2DManager)renderer.TextureManager).texture;

	FontSystem fontSystem;
	FontStashRenderer renderer = new();

	public Font(File fontFile)
	{
		fontSystem = new(new() { PremultiplyAlpha = false, /* FontResolutionFactor = 2, KernelWidth = 2, KernelHeight = 2, */ });
		fontSystem.AddFont(fontFile.ReadBytes());
		fontSystem.CurrentAtlasFull += (sender, args) => throw new MGEException("Ran out of space in the font texture atlas");
	}

	public void DrawString(string text, Vector2 position, Color color, int fontSize = 18) => fontSystem.GetFont(fontSize).DrawText(renderer, text, position, color);
	public void DrawString(StringBuilder sb, Vector2 position, Color color, int fontSize = 18) => fontSystem.GetFont(fontSize).DrawText(renderer, sb, position, color);

	public Vector2 MeasureString(string text, int fontSize)
	{
		var font = fontSystem.GetFont(fontSize);
		return font.MeasureString(text);
	}
}
