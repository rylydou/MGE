using System;
using System.IO;

namespace MGE;

/// <summary>
/// Stores a 2D Image
/// </summary>
public class Bitmap
{
	/// <summary>
	/// The Pixel array of the Bitmap
	/// </summary>
	public readonly Color[] pixels;

	/// <summary>
	/// The Width of the Bitmap, in Pixels
	/// </summary>
	public readonly int width;

	/// <summary>
	/// The Height of the Bitmap, in Pixels
	/// </summary>
	public readonly int height;

	public Bitmap(int width, int height) : this(width, height, new Color[width * height])
	{
	}

	public Bitmap(int width, int height, Color[] pixels)
	{
		if (width <= 0 || height <= 0)
			throw new Exception("Width and Height must be larger than 0");
		if (pixels.Length < width * height)
			throw new Exception("Pixels array doesn't fit the Bitmap size");

		this.pixels = pixels;
		this.width = width;
		this.height = height;
	}

	public Bitmap(Stream stream, string format)
	{
		if (Image.Read(stream, format, out width, out height, out var pixels) && pixels is not null)
			this.pixels = pixels;
		else
			throw new NotSupportedException("Stream is either an invalid or not supported image format");
	}

	public Bitmap(File file)
	{
		using var stream = file.OpenRead();

		if (Image.Read(stream, file.extension, out width, out height, out var pixels) && pixels is not null)
			this.pixels = pixels;
		else
			throw new NotSupportedException("Stream is either an invalid or not supported image format");

		stream.Close();
	}

	/// <summary>
	/// Premultiplies the Color data of the Bitmap
	/// </summary>
	public void Premultiply()
	{
		unsafe
		{
			fixed (void* ptr = pixels)
			{
				byte* rgba = (byte*)ptr;

				for (int i = 0, len = pixels.Length * 4; i < len; i += 4)
				{
					rgba[i + 0] = (byte)(rgba[i + 0] * rgba[i + 3] / 255);
					rgba[i + 1] = (byte)(rgba[i + 1] * rgba[i + 3] / 255);
					rgba[i + 2] = (byte)(rgba[i + 2] * rgba[i + 3] / 255);
				}
			}
		}
	}

	/// <summary>
	/// Sets the contents of the bitmap to the given data
	/// </summary>
	public void SetPixels(Memory<Color> source)
	{
		source.Span.CopyTo(pixels);
	}

	/// <summary>
	/// Sets the contents of the bitmap over the given Rect to the given data
	/// </summary>
	public void SetPixels(RectInt destination, Memory<Color> source)
	{
		// TODO  perform bounds checking?

		var src = source.Span;
		var dst = new Span<Color>(pixels);

		for (int y = 0; y < destination.height; y++)
		{
			var from = src.Slice(y * destination.width, destination.width);
			var to = dst.Slice(destination.x + (destination.y + y) * width, destination.width);

			from.CopyTo(to);
		}
	}

	public void GetPixels(Memory<Color> destination)
	{
		pixels.CopyTo(destination);
	}

	public void GetPixels(Memory<Color> dest, Vector2Int destPosition, Vector2Int destSize, RectInt sourceRect)
	{
		var src = new Span<Color>(pixels);
		var dst = dest.Span;

		// can't be outside of the source image
		if (sourceRect.left < 0) sourceRect.left = 0;
		if (sourceRect.top < 0) sourceRect.top = 0;
		if (sourceRect.right > width) sourceRect.right = width;
		if (sourceRect.bottom > height) sourceRect.bottom = height;

		// can't be larger than our destination
		if (sourceRect.width > destSize.x - destPosition.x)
			sourceRect.width = destSize.x - destPosition.x;
		if (sourceRect.height > destSize.y - destPosition.y)
			sourceRect.height = destSize.y - destPosition.y;

		for (int y = 0; y < sourceRect.height; y++)
		{
			var from = src.Slice(sourceRect.x + (sourceRect.y + y) * width, sourceRect.width);
			var to = dst.Slice(destPosition.x + (destPosition.y + y) * destSize.x, sourceRect.width);

			from.CopyTo(to);
		}
	}

	public Bitmap GetSubBitmap(RectInt source)
	{
		var bmp = new Bitmap(source.width, source.height);
		GetPixels(bmp.pixels, Vector2Int.zero, source.size, source);
		return bmp;
	}

	public void SavePng(File file)
	{
		using var stream = file.Create();
		Image.WritePNG(stream, width, height, pixels);
	}

	public void SavePng(Stream stream)
	{
		Image.WritePNG(stream, width, height, pixels);
	}

	public void SaveJpg(File file, int quality = 90)
	{
		using var stream = file.Create();
		Image.WriteJPG(stream, width, height, pixels, quality);
	}

	public void SaveJpg(Stream stream, int quality = 90)
	{
		Image.WriteJPG(stream, width, height, pixels, quality);
	}

	/// <summary>
	/// Clones the Bitmap
	/// </summary>
	public Bitmap Clone()
	{
		return new Bitmap(width, height, new Memory<Color>(pixels).ToArray());
	}
}
