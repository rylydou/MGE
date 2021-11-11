using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Graphics.OpenGL;
// using SixLabors.ImageSharp;
// using SixLabors.ImageSharp.PixelFormats;
// using SixLabors.ImageSharp.Processing;
using StbImageSharp;

namespace MGE.Graphics;

public class Texture : GraphicsResource, IUseable
{
	public readonly Vector2Int size;

	public Texture(int handle) : base(handle)
	{
		GL.GetTexParameter(TextureTarget.Texture2D, GetTextureParameter.TextureWidth, out int width);
		size.x = width;
		GL.GetTexParameter(TextureTarget.Texture2D, GetTextureParameter.TextureHeight, out int height);
		size.y = height;
	}

	public Texture(Vector2Int size, Color[]? pixels) : base(GL.GenTexture())
	{
		this.size = size;

		Use();

		GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, size.x, size.y, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

		GL.Enable(EnableCap.Blend);

		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
	}

	public Texture(Vector2Int size, byte[]? pixels = null) : base(GL.GenTexture())
	{
		this.size = size;

		Use();

		GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, size.x, size.y, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

		GL.Enable(EnableCap.Blend);

		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
	}

	public static Texture LoadFromFile(string path)
	{
		using (var stream = File.OpenRead($"{Environment.CurrentDirectory}/Assets/{path}"))
		{
			var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
			return new(new(image.Width, image.Height), image.Data);
		}
	}

	// TODO
	// public static OpenTK.Windowing.Common.Input.Image LoadIconData(string path)
	// {
	// 	using (var image = Image.Load<Rgba32>($"{Environment.CurrentDirectory}/Assets/{path}"))
	// 	{
	// 		var pixels = new List<byte>(image.Width * image.Height * 4);

	// 		for (int y = 0; y < image.Height; y++)
	// 		{
	// 			var row = image.GetPixelRowSpan(y);

	// 			for (int x = 0; x < image.Width; x++)
	// 			{
	// 				pixels.Add(row[x].R);
	// 				pixels.Add(row[x].G);
	// 				pixels.Add(row[x].B);
	// 				pixels.Add(row[x].A);
	// 			}
	// 		}

	// 		return new OpenTK.Windowing.Common.Input.Image(image.Width, image.Height, pixels.ToArray());
	// 	}
	// }

	public Vector2 GetTextureCoord(Vector2 position) => GetTextureCoord(position.x, position.y);
	public Vector2 GetTextureCoord(float x, float y) => new Vector2(x / size.x, y / size.y);

	public void Use() => Use(TextureUnit.Texture0);
	public void Use(TextureUnit unit)
	{
		GL.ActiveTexture(unit);
		GL.BindTexture(TextureTarget.Texture2D, handle);
	}

	public void StopUse() => GL.BindTexture(TextureTarget.Texture2D, 0);

	protected override void Delete()
	{
		GL.DeleteTexture(handle);
	}
}
