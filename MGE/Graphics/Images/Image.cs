using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using StbImageSharp;
using StbImageWriteSharp;

namespace MGE;

public static class Image
{
	public delegate bool IsValidDelegate();
	public delegate bool ReadDelegate(Stream stream, out int width, out int height, out Color[]? pixels);
	public delegate bool WriteDelegate(Stream stream, in int width, in int height, in Color[] pixels);

	public class Format
	{
		public readonly string[] extentions;
		public readonly ReadDelegate read;
		public readonly WriteDelegate write;

		public Format(string[] extentions, ReadDelegate read, WriteDelegate write)
		{
			this.extentions = extentions;
			this.read = read;
			this.write = write;
		}
	}

	static readonly Dictionary<string, Format> extention_2_format = new();

	static Image()
	{
		RegisterImageFormat(ReadSTB, WriteBMP, "bmp");
		RegisterImageFormat(ReadSTB, WritePNG, "png");
		RegisterImageFormat(ReadSTB, WriteTGA, "tga");
		RegisterImageFormat(ReadSTB, (Stream stream, in int width, in int height, in Color[] pixels) => WriteJPG(stream, width, height, pixels), "jpg");
	}

	public static void RegisterImageFormat(ReadDelegate read, WriteDelegate write, params string[] extentions)
	{
		var format = new Format(extentions, read, write);
		foreach (var extention in extentions)
		{
			extention_2_format.Set(extention, format);
		}
	}

	public static bool Read(Stream stream, string extention, out int width, out int height, out Color[]? pixels)
	{
		var format = extention_2_format[extention];
		return format.read.Invoke(stream, out width, out height, out pixels);
	}

	static bool ReadSTB(Stream stream, out int width, out int height, out Color[]? pixels)
	{
		var result = ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);

		width = result.Width;
		height = result.Height;
		pixels = result.Data.ToRGBA();

		return true;
	}

	static readonly ImageWriter imageWriter = new();

	public static bool WriteBMP(Stream stream, in int width, in int height, in Color[] pixels)
	{
		var data = pixels.ToRGBA();
		imageWriter.WriteBmp(data, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
		return true;
	}

	public static bool WritePNG(Stream stream, in int width, in int height, in Color[] pixels)
	{
		var data = pixels.ToRGBA();
		imageWriter.WritePng(data, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
		return true;
	}

	public static bool WriteTGA(Stream stream, in int width, in int height, in Color[] pixels)
	{
		var data = pixels.ToRGBA();
		imageWriter.WriteTga(data, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
		return true;
	}

	public static bool WriteJPG(Stream stream, in int width, in int height, in Color[] pixels, int quality = 90)
	{
		var data = pixels.ToRGBA();
		imageWriter.WriteJpg(data, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream, quality);
		return true;
	}
}
