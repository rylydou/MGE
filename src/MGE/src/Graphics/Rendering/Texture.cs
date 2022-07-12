using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MGE;

/// <summary>
/// Texture Filter
/// </summary>
public enum TextureFilter
{
	Linear = 0,
	Nearest = 1
}

/// <summary>
/// Texture Format
/// </summary>
public enum TextureFormat
{
	/// <summary>
	/// No Texture Format
	/// </summary>
	None,

	/// <summary>
	/// Single 8-bit component stored in the Red channel
	/// </summary>
	Red,

	/// <summary>
	/// Two 8-bit components stored in the Red, Green channels
	/// </summary>
	RG,

	/// <summary>
	/// Three 8-bit components stored in the Red, Green, Blue channels
	/// </summary>
	RGB,

	/// <summary>
	/// ARGB uint32 Color
	/// </summary>
	Color,

	/// <summary>
	/// Depth 24-bit Stencil 8-bit
	/// </summary>
	DepthStencil,
}

public static class TextureFormatExt
{
	public static bool IsTextureColorFormat(this TextureFormat format)
	{
		return
			format == TextureFormat.Color ||
			format == TextureFormat.Red ||
			format == TextureFormat.RG ||
			format == TextureFormat.RGB;
	}

	public static bool IsDepthStencilFormat(this TextureFormat format)
	{
		return format == TextureFormat.DepthStencil;
	}
}

/// <summary>
/// Texture Wrap
/// </summary>
public enum TextureWrap
{
	Wrap = 0,
	Clamp = 1
}

/// <summary>
/// A 2D Texture used for Rendering
/// </summary>
public class Texture : IDisposable
{
	/// <summary>
	/// Internal Implementation of the Texture
	/// </summary>
	public abstract class Platform
	{
		protected internal abstract void Init(Texture texture);
		protected internal abstract void Resize(int width, int height);
		protected internal abstract void SetFilter(TextureFilter filter);
		protected internal abstract void SetWrap(TextureWrap x, TextureWrap y);
		protected internal abstract void SetData<T>(RectInt rect, T[] pixels) where T : struct;
		protected internal abstract void SetData<T>(ReadOnlyMemory<T> buffer);
		protected internal abstract void GetData<T>(Memory<T> buffer);
		protected internal abstract bool IsFrameBuffer();
		protected internal abstract void Dispose();
	}

	/// <summary>
	/// Default Texture Filter used for all Textures
	/// </summary>
	public static TextureFilter defaultTextureFilter = TextureFilter.Nearest;

	/// <summary>
	/// A reference to the internal platform implementation of the Texture
	/// </summary>
	public readonly Platform implementation;

	/// <summary>
	/// The Texture Data Format
	/// </summary>
	public readonly TextureFormat format;

	/// <summary>
	/// Gets the Width of the Texture
	/// </summary>
	public int width { get; set; }

	/// <summary>
	/// Gets the Height of the Texture
	/// </summary>
	public int height { get; set; }

	/// <summary>
	/// Whether the Texture is part of a FrameBuffer
	/// </summary>
	public bool isFrameBuffer => implementation.IsFrameBuffer();

	/// <summary>
	/// The Size of the Texture, in bytes
	/// </summary>
	public int size => width * height * (format switch
	{
		TextureFormat.Color => 4,
		TextureFormat.Red => 1,
		TextureFormat.RG => 2,
		TextureFormat.RGB => 3,
		TextureFormat.DepthStencil => 4,
		_ => throw new MGException("Invalid Texture Format")
	});

	/// <summary>
	/// The Texture Filter to be used while drawing
	/// </summary>
	public TextureFilter filter
	{
		get => _filter;
		set => implementation.SetFilter(_filter = value);
	}

	/// <summary>
	/// The Horizontal Wrapping mode
	/// </summary>
	public TextureWrap wrapX
	{
		get => _wrapX;
		set => implementation.SetWrap(_wrapX = value, _wrapY);
	}

	/// <summary>
	/// The Vertical Wrapping mode
	/// </summary>
	public TextureWrap wrapY
	{
		get => _wrapY;
		set => implementation.SetWrap(_wrapX, _wrapY = value);
	}

	readonly Graphics _graphics;
	TextureFilter _filter = TextureFilter.Linear;
	TextureWrap _wrapX = TextureWrap.Clamp;
	TextureWrap _wrapY = TextureWrap.Clamp;

	public Texture(Graphics graphics, int width, int height, TextureFormat format = TextureFormat.Color)
	{
		if (format == TextureFormat.None) throw new MGException("Invalid Texture Format");

		if (width <= 0 || height <= 0) throw new MGException("Texture must have a size larger than 0");

		this._graphics = graphics;
		this.width = width;
		this.height = height;
		this.format = format;

		implementation = graphics.CreateTexture(this.width, this.height, this.format);
		implementation.Init(this);

		filter = defaultTextureFilter;
	}

	public Texture(int width, int height, TextureFormat format = TextureFormat.Color) : this(App.graphics, width, height, format) { }

	public Texture(Bitmap bitmap) : this(App.graphics, bitmap.width, bitmap.height, TextureFormat.Color)
	{
		implementation.SetData<Color>(bitmap.pixels);
	}

	public Texture(File file) : this(new Bitmap(file)) { }

	public Texture(Stream stream, string format) : this(new Bitmap(stream, format)) { }

	public void Resize(int width, int height)
	{
		if (width <= 0 || height <= 0)
			throw new MGException("Texture must have a size larger than 0");

		if (this.width != width || this.height != height)
		{
			this.width = width;
			this.height = height;

			implementation.Resize(width, height);
		}
	}

	/// <summary>
	/// Creates a Bitmap with the Texture Color data
	/// </summary>
	public Bitmap AsBitmap()
	{
		var bitmap = new Bitmap(width, height);
		GetData<Color>(new Memory<Color>(bitmap.pixels));
		return bitmap;
	}

	/// <summary>
	/// Sets the Texture Color data from the given buffer
	/// </summary>
	public void SetColor(ReadOnlyMemory<Color> buffer) => SetData<Color>(buffer);

	/// <summary>
	/// Writes the Texture Color data to the given buffer
	/// </summary>
	public void GetColor(Memory<Color> buffer) => GetData<Color>(buffer);

	public void SetData<T>(RectInt rect, T[] pixles) where T : struct
	{
		implementation.SetData<T>(rect, pixles);
	}

	/// <summary>
	/// Sets the Texture data from the given buffer
	/// </summary>
	public void SetData<T>(ReadOnlyMemory<T> buffer)
	{
		if (Marshal.SizeOf<T>() * buffer.Length < size) throw new MGException("Buffer is smaller than the Size of the Texture");

		implementation.SetData(buffer);
	}

	/// <summary>
	/// Writes the Texture data to the given buffer
	/// </summary>
	public void GetData<T>(Memory<T> buffer)
	{
		if (Marshal.SizeOf<T>() * buffer.Length < size) throw new MGException("Buffer is smaller than the Size of the Texture");

		implementation.GetData(buffer);
	}

	public void SavePng(File file)
	{
		using var stream = file.OpenWrite();
		SavePng(stream);
	}

	public void SavePng(Stream stream)
	{
		var color = new Color[width * height];

		if (format == TextureFormat.Color || format == TextureFormat.DepthStencil)
		{
			GetData<Color>(color);
		}
		else
		{
			// TODO do this inline with a single buffer

			var buffer = new byte[size];
			GetData<byte>(buffer);

			if (format == TextureFormat.Red)
			{
				for (int i = 0; i < buffer.Length; i++)
				{
					color[i].r = buffer[i];
					color[i].a = 255;
				}
			}
			else if (format == TextureFormat.RG)
			{
				for (int i = 0; i < buffer.Length; i += 2)
				{
					color[i].r = buffer[i + 0];
					color[i].g = buffer[i + 1];
					color[i].a = 255;
				}
			}
			else if (format == TextureFormat.RGB)
			{
				for (int i = 0; i < buffer.Length; i += 3)
				{
					color[i].r = buffer[i + 0];
					color[i].g = buffer[i + 1];
					color[i].b = buffer[i + 2];
					color[i].a = 255;
				}
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		// We may need to flip our buffer.
		// This is due to some rendering APIs drawing from the bottom left (OpenGL).
		if (isFrameBuffer && _graphics.originBottomLeft)
		{
			for (int y = 0; y < height / 2; y++)
			{
				var a = y * width;
				var b = (height - y - 1) * width;

				for (int x = 0; x < width; x++, a++, b++)
				{
					var temp = color[a];
					color[a] = color[b];
					color[b] = temp;
				}
			}
		}

		Image.WritePNG(stream, width, height, color);
	}

	public void SaveJpg(string path)
	{
		throw new NotImplementedException();
	}

	public void SaveJpg(Stream stream)
	{
		throw new NotImplementedException();
	}

	public void Dispose()
	{
		implementation.Dispose();
	}
}
