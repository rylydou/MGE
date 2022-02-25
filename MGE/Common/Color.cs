using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;

namespace MGE;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 4)]
public struct Color : IEquatable<Color>
{
	public static readonly Color transparent = new Color(0, 0, 0, 0);

	public static readonly Color white = new Color(0xffffff);
	public static readonly Color lightGray = new Color(0xc0c0c0);
	public static readonly Color gray = new Color(0x808080);
	public static readonly Color darkGray = new Color(0x404040);
	public static readonly Color black = new Color(0x000000);

	public static readonly Color red = new Color(0xff0000);
	public static readonly Color green = new Color(0x00ff00);
	public static readonly Color blue = new Color(0x0000ff);
	public static readonly Color yellow = new Color(0xffff00);
	public static readonly Color cyan = new Color(0x00ffff);
	public static readonly Color magenta = new Color(0xff00ff);

	/// <summary>
	/// The Color Value in a ABGR 32-bit unsigned integer
	/// </summary>
	public uint abgr;

	/// <summary>
	/// Gets the Color Value in a RGBA 32-bit unsigned integer
	/// </summary>
	public uint rgba => new Color(a, b, g, r).abgr;

	/// <summary>
	/// The Red Component
	/// </summary>
	public byte r
	{
		get => (byte)abgr;
		set => abgr = (abgr & 0xffffff00) | value;
	}

	/// <summary>
	/// The Green Component
	/// </summary>
	public byte g
	{
		get => (byte)(abgr >> 8);
		set => abgr = (abgr & 0xffff00ff) | ((uint)value << 8);
	}

	/// <summary>
	/// The Blue Component
	/// </summary>
	public byte b
	{
		get => (byte)(abgr >> 16);
		set => abgr = (abgr & 0xff00ffff) | ((uint)value << 16);
	}

	/// <summary>
	/// The Alpha Component
	/// </summary>
	public byte a
	{
		get => (byte)(abgr >> 24);
		set => abgr = (abgr & 0x00ffffff) | ((uint)value << 24);
	}

	public Color translucent => new(r, g, b, (byte)128);
	public Color opaque => new(r, g, b, (byte)255);

	/// <summary>
	/// Creates a color given the int32 RGB data
	/// </summary>
	public Color(int rgb, byte alpha = 255)
	{
		abgr = 0;

		r = (byte)(rgb >> 16);
		g = (byte)(rgb >> 08);
		b = (byte)(rgb);
		a = alpha;
	}

	public Color(int rgb, float alpha)
	{
		abgr = 0;

		r = (byte)((rgb >> 16) * alpha);
		g = (byte)((rgb >> 08) * alpha);
		b = (byte)(rgb * alpha);
		a = (byte)(255 * alpha);
	}

	/// <summary>
	/// Creates a color given the uint32 RGBA data
	/// </summary>
	public Color(uint rgba)
	{
		abgr = 0;

		r = (byte)(rgba >> 24);
		g = (byte)(rgba >> 16);
		b = (byte)(rgba >> 08);
		a = (byte)(rgba);
	}

	public Color(byte r, byte g, byte b, byte a)
	{
		abgr = 0;
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}

	public Color(int r, int g, int b, int a)
	{
		abgr = 0;
		this.r = (byte)r;
		this.g = (byte)g;
		this.b = (byte)b;
		this.a = (byte)a;
	}

	public Color(float r, float g, float b, float a)
	{
		abgr = 0;
		this.r = (byte)(r * 255);
		this.g = (byte)(g * 255);
		this.b = (byte)(b * 255);
		this.a = (byte)(a * 255);
	}

	/// <summary>
	/// Premultiplies the color value based on its Alpha component
	/// </summary>
	/// <returns></returns>
	public Color Premultiply()
	{
		var a = this.a;
		return new Color((byte)(r * a / 255), (byte)(g * a / 255), (byte)(b * a / 255), a);
	}

	public Color WithAlpha(byte alpha)
	{
		return new(r, g, b, alpha);
	}

	/// <summary>
	/// Converts the Color to a Vector4
	/// </summary>
	public Vector4 ToVector4() => new Vector4(r / 255f, g / 255f, b / 255f, a / 255f);

	/// <summary>
	/// Converts the Color to a Vector3
	/// </summary>
	public Vector3 ToVector3() => new Vector3(r / 255f, g / 255f, b / 255f);

	public override bool Equals(object? obj) => obj is Color color && Equals(color);
	public bool Equals(Color other) => this == other;

	public override int GetHashCode() => (int)abgr;

	public override string ToString() => ($"[{r}, {g}, {b}, {a}]");

	/// <summary>
	/// Returns a Hex String representation of the Color's given components
	/// </summary>
	/// <param name="components">The Components, in any order. ex. "RGBA" or "RGB" or "ARGB"</param>
	/// <returns></returns>
	public string ToHexString(string components)
	{
		const string HEX = "0123456789ABCDEF";
		Span<char> result = stackalloc char[components.Length * 2];

		for (int i = 0; i < components.Length; i++)
		{
			switch (components[i])
			{
				case 'R':
				case 'r':
					result[i * 2 + 0] = HEX[(r & 0xf0) >> 4];
					result[i * 2 + 1] = HEX[(r & 0x0f)];
					break;
				case 'G':
				case 'g':
					result[i * 2 + 0] = HEX[(g & 0xf0) >> 4];
					result[i * 2 + 1] = HEX[(g & 0x0f)];
					break;
				case 'B':
				case 'b':
					result[i * 2 + 0] = HEX[(b & 0xf0) >> 4];
					result[i * 2 + 1] = HEX[(b & 0x0f)];
					break;
				case 'A':
				case 'a':
					result[i * 2 + 0] = HEX[(a & 0xf0) >> 4];
					result[i * 2 + 1] = HEX[(a & 0x0f)];
					break;
			}
		}

		return new string(result);
	}

	/// <summary>
	/// Returns an RGB Hex string representation of the Color
	/// </summary>
	public string ToHexStringRGB() => ToHexString("RGB");

	/// <summary>
	/// Returns an RGBA Hex string representation of the Color
	/// </summary>
	public string ToHexStringRGBA() => ToHexString("RGBA");

	/// <summary>
	/// Creates a new Color with the given components from the given string value
	/// </summary>
	/// <param name="components">The components to parse in order, ex. "RGBA"</param>
	/// <param name="value">The Hex value to parse</param>
	/// <returns></returns>
	public static Color FromHexString(string components, ReadOnlySpan<char> value)
	{
		// skip past useless string data (ex. if the string was 0xffffff or #ffffff)
		if (value.Length > 0 && value[0] == '#')
			value = value.Slice(1);
		if (value.Length > 1 && value[0] == '0' && (value[1] == 'x' || value[1] == 'X'))
			value = value.Slice(2);

		var color = black;

		for (int i = 0; i < components.Length && i * 2 + 2 <= value.Length; i++)
		{
			switch (components[i])
			{
				case 'R':
				case 'r':
					if (byte.TryParse(value.Slice(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r))
						color.r = r;
					break;
				case 'G':
				case 'g':
					if (byte.TryParse(value.Slice(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g))
						color.g = g;
					break;
				case 'B':
				case 'b':
					if (byte.TryParse(value.Slice(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
						color.b = b;
					break;
				case 'A':
				case 'a':
					if (byte.TryParse(value.Slice(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var a))
						color.a = a;
					break;
			}
		}

		return color;
	}

	/// <summary>
	/// Creates a new Color from the given RGB Hex value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public static Color FromHexStringRGB(string value) => FromHexString("RGB", value);

	/// <summary>
	/// Creates a new Color from the given RGBA Hex value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public static Color FromHexStringRGBA(string value) => FromHexString("RGBA", value);

	/// <summary>
	/// Linearly interpolates between two colors
	/// </summary>
	/// <returns></returns>
	public static Color Lerp(Color a, Color b, float amount)
	{
		return new(
			(int)(a.r + (b.r - a.r) * amount),
			(int)(a.g + (b.g - a.g) * amount),
			(int)(a.b + (b.b - a.b) * amount),
			(int)(a.a + (b.a - a.a) * amount)
		);
	}
	/// <summary>
	/// Linearly interpolates between two colors
	/// </summary>
	/// <returns></returns>
	public static Color LerpClamped(Color a, Color b, float amount)
	{
		amount = Math.Clamp01(amount);

		return new(
			(int)(a.r + (b.r - a.r) * amount),
			(int)(a.g + (b.g - a.g) * amount),
			(int)(a.b + (b.b - a.b) * amount),
			(int)(a.a + (b.a - a.a) * amount)
		);
	}
	/// <summary>
	/// Implicitely converts an int32 to a Color, ex 0xffffff
	/// This does not include Alpha values
	/// </summary>
	/// <param name="color"></param>
	public static implicit operator Color(int color) => new Color(color);

	/// <summary>
	/// Implicitely converts an uint32 to a Color, ex 0xffffffff
	/// </summary>
	/// <param name="color"></param>
	public static implicit operator Color(uint color) => new Color(color);

	/// <summary>
	/// Multiplies a Color by a scaler
	/// </summary>
	public static Color operator *(Color value, float scaler)
	{
		return new Color(
			(int)(value.r * scaler),
			(int)(value.g * scaler),
			(int)(value.b * scaler),
			(int)(value.a * scaler)
		);
	}

	public static bool operator ==(Color a, Color b) => a.abgr == b.abgr;

	public static bool operator !=(Color a, Color b) => a.abgr != b.abgr;

	static public implicit operator Color(Vector4 vec) => new Color(vec.X, vec.Y, vec.Z, vec.W);

	static public implicit operator Color(Vector3 vec) => new Color(vec.x, vec.y, vec.z, 1.0f);

	static public implicit operator Vector4(Color col) => col.ToVector4();

	static public implicit operator Vector3(Color col) => col.ToVector3();
}
