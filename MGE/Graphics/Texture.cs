using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MGE.Graphics;

public class Texture : GraphicsResource
{
	public readonly Vector2Int size;

	public Texture(Vector2Int size) : base(GL.GenTexture())
	{
		this.size = size;

		GL.ActiveTexture(TextureUnit.Texture0);
		GL.BindTexture(TextureTarget.Texture2D, handle);

		GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, size.x, size.y, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

		GL.Enable(EnableCap.Blend);

		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
	}

	public Texture(Vector2Int size, Color[] pixels) : base(GL.GenTexture())
	{
		this.size = size;

		GL.ActiveTexture(TextureUnit.Texture0);
		GL.BindTexture(TextureTarget.Texture2D, handle);

		GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, size.x, size.y, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

		GL.Enable(EnableCap.Blend);

		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
	}

	public static Texture LoadFromFile(string path)
	{
		using (var image = Image.Load<Rgba32>($"{Environment.CurrentDirectory}/Assets/{path}"))
		{
			image.Mutate(x => x.Flip(FlipMode.Vertical));

			var pixels = new List<Color>(image.Width * image.Height);

			for (int y = 0; y < image.Height; y++)
			{
				var row = image.GetPixelRowSpan(y);

				for (int x = 0; x < image.Width; x++)
				{
					pixels.Add(Color.FromBytes(row[x].R, row[x].G, row[x].B, row[x].A));
				}
			}

			return new Texture(new Vector2Int(image.Width, image.Height), pixels.ToArray());
		}
	}

	public static OpenTK.Windowing.Common.Input.Image LoadIconData(string path)
	{
		using (var image = Image.Load<Rgba32>($"{Environment.CurrentDirectory}/Assets/{path}"))
		{
			var pixels = new List<byte>(image.Width * image.Height * 4);

			for (int y = 0; y < image.Height; y++)
			{
				var row = image.GetPixelRowSpan(y);

				for (int x = 0; x < image.Width; x++)
				{
					pixels.Add(row[x].R);
					pixels.Add(row[x].G);
					pixels.Add(row[x].B);
					pixels.Add(row[x].A);
				}
			}

			return new OpenTK.Windowing.Common.Input.Image(image.Width, image.Height, pixels.ToArray());
		}
	}

	public Vector2 GetTextureCoord(Vector2 position) => GetTextureCoord(position.x, position.y);
	public Vector2 GetTextureCoord(float x, float y) => new Vector2(x / size.x, y / size.y);

	public void Use(byte unit)
	{
		if (unit > 31) throw new ArgumentOutOfRangeException("unit", unit, "Intended range 0 - 31");
		Use((TextureUnit)(33984 + unit));
	}
	public void Use(TextureUnit unit = TextureUnit.Texture0)
	{
		GL.ActiveTexture(unit);
		GL.BindTexture(TextureTarget.Texture2D, handle);
	}

	protected override void Dispose(bool manual)
	{
		if (!manual) return;
		GL.DeleteTexture(handle);
	}
}
