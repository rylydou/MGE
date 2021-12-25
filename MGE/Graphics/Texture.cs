using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL;
using StbImageSharp;

namespace MGE.Graphics;

public class Texture : GraphicsResource
{
	public static readonly Texture pixelTexture;

	public readonly Vector2Int size;

	bool initialized;

	static Texture()
	{
		pixelTexture = new Texture(new(1, 1));
		pixelTexture.SetData(new[] { Color.white });
	}

	public Texture(Vector2Int size) : base(GL.GenTexture())
	{
		this.size = size;

		Bind();

		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
	}

	#region Loading

	public static Texture LoadFromFile(File imageFile)
	{
		using var stream = imageFile.OpenRead();

		var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

		var texture = new Texture(new(image.Width, image.Height));
		texture.SetData(image.Data);

		return texture;
	}

	public static OpenTK.Windowing.Common.Input.Image LoadImageFromFile(File imageFile)
	{
		using var stream = imageFile.OpenRead();

		var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
		return new(image.Width, image.Height, image.Data);
	}

	#endregion

	#region Reading and Writing

	public void SetData<T>(T[]? data) where T : struct
	{
		Bind();
		GL.TexImage2D<T>(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, size.x, size.y, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
		initialized = true;
	}

	public void SetData<T>(RectInt region, T[]? data) where T : struct
	{
		InitIfNecessary();
		Bind();
		GL.TexSubImage2D<T>(TextureTarget.Texture2D, 0, region.x, region.y, region.width, region.height, PixelFormat.Rgba, PixelType.UnsignedByte, data);
	}

	public void GetData<T>(T[]? data) where T : struct
	{
		InitIfNecessary();
		Bind();
		GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
	}

	public void GetData<T>(T[,]? data) where T : struct
	{
		InitIfNecessary();
		Bind();
		GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
	}

	#endregion

	#region Utils

	[MethodImpl(MethodImplOptions.AggressiveInlining)] public Vector2 GetNormalizedPoint(Vector2 position) => GetNormalizedPoint(position.x, position.y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public Vector2 GetNormalizedPoint(float x, float y) => new(x / size.x, y / size.y);

	#endregion

	internal void InitIfNecessary()
	{
		if (!initialized)
		{
			initialized = true;
			SetData<byte>(null);
		}
	}

	internal void Bind()
	{
		GL.BindTexture(TextureTarget.Texture2D, _handle);
		GFX.CheckError();
	}

	internal void Use(TextureUnit unit = TextureUnit.Texture0)
	{
		Bind();
		GL.ActiveTexture(unit);
		GFX.CheckError();
	}

	internal static void UseNone()
	{
		GL.BindTexture(TextureTarget.Texture2D, 0);
		GFX.CheckError();
	}

	protected override void Delete()
	{
		GL.DeleteTexture(_handle);
		GFX.CheckError();
	}
}
