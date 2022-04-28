// TODO  Fix font rendering on diffrent scales

using System.Text;
using FontStashSharp;
using FontStashSharp.Interfaces;

namespace MGE;

public class Font
{
	class Texture2DManager : ITexture2DManager
	{
		public Texture? texture;

		public object CreateTexture(int width, int height)
		{
			texture = new Texture(App.graphics, width, height);
			return texture;
		}

		public System.Drawing.Point GetTextureSize(object obj)
		{
			var texture = (Texture)obj;
			return new(texture.width, texture.height);
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

		public Batch2D? batch;

		public void Draw(object obj, System.Numerics.Vector2 pos, System.Drawing.Rectangle? src, global::System.Drawing.Color color, float rotation, System.Numerics.Vector2 origin, System.Numerics.Vector2 scale, float depth)
		{
			if (!src.HasValue) throw new Exception();

			var texture = (Texture)obj;

			batch!.Image(texture, src.Value, pos, scale, origin, rotation, color);
		}
	}

	internal Texture? atlas => ((Texture2DManager)renderer.TextureManager).texture;

	FontSystem fontSystem;
	FontStashRenderer renderer;

	public Font(Graphics graphics, File fontFile, float fontResolutionFactor = 1f, int kernelSize = 1)
	{
		renderer = new();

		fontSystem = new(new()
		{
			PremultiplyAlpha = true,
			FontResolutionFactor = fontResolutionFactor,
			KernelWidth = kernelSize,
			KernelHeight = kernelSize,
		});
		fontSystem.AddFont(fontFile.OpenRead());
		fontSystem.CurrentAtlasFull += (sender, args) => throw new Exception("Ran out of space in the font texture atlas");
	}

	public void DrawString(Batch2D batch, string text, Vector2 position, Color color, int fontSize = 24)
	{
		renderer.batch = batch;
		fontSystem.GetFont(fontSize).DrawText(renderer, text, position, color);
	}

	public Vector2 MeasureString(string text, int fontSize = 24)
	{
		return fontSystem.GetFont(fontSize).MeasureString(text);
	}

	public Vector2 MeasureString(StringBuilder text, int fontSize = 24)
	{
		return fontSystem.GetFont(fontSize).MeasureString(text);
	}
}
