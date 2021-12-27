using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace MGE
{
	public sealed class TextureLoader : AssetLoader<Texture>
	{
		protected override Texture LoadAsset(string path) => new Texture(path);
	}

	public class Texture : System.IDisposable
	{
		public Texture2D texture { get; private set; }
		public Vector2Int size => new Vector2Int(texture.Width, texture.Height);

		static Texture _pixel;
		public static Texture pixel
		{
			get
			{
				if (_pixel is null)
				{
					_pixel = new Texture(Vector2Int.one);
				}
				return _pixel;
			}
		}

		public Texture() { }

		public Texture(string path)
		{
			texture = Texture2D.FromFile(GFX.gfxManager.GraphicsDevice, path);
		}

		public Texture(Vector2Int size)
		{
			texture = new Texture2D(GFX.gfxManager.GraphicsDevice, size.x, size.y, false, SurfaceFormat.Color, 1);

			var pixels = new List<Color>();

			for (int i = 0; i < size.x * size.y; i++)
			{
				pixels.Add(Color.white);
			}

			texture.SetData<Microsoft.Xna.Framework.Color>(pixels.Select(c => (Microsoft.Xna.Framework.Color)c).ToArray());
		}

		public static implicit operator Texture2D(Texture texture) => texture?.texture;

		public void Dispose() => texture.Dispose();
	}
}