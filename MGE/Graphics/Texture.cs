using System;
using System.IO;
using OpenTK.Graphics.OpenGL;
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

	public static Texture LoadTexture(string path)
	{
		using (var stream = File.OpenRead($"{Environment.CurrentDirectory}/Assets/{path}"))
		{
			var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
			return new(new(image.Width, image.Height), image.Data);
		}
	}

	public static OpenTK.Windowing.Common.Input.Image LoadImageData(string path)
	{
		using (var stream = File.OpenRead($"{Environment.CurrentDirectory}/Assets/{path}"))
		{
			var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
			return new(image.Width, image.Height, image.Data);
		}
	}

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
