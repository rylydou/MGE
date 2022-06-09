using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;

namespace MGE;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 4)]
public struct Color : IEquatable<Color>
{
	#region Static

	#region Constants

	public static readonly Color clear = new Color(0, 0, 0, 0);

	public static readonly Color white = new Color(0xFFFFFFFF);
	public static readonly Color lightGray = new Color(0xC0C0C0FF);
	public static readonly Color gray = new Color(0x808080FF);
	public static readonly Color darkGray = new Color(0x404040FF);
	public static readonly Color black = new Color(0x000000FF);

	public static readonly Color red = new Color(0xFF0000FF);
	public static readonly Color green = new Color(0x00FF00FF);
	public static readonly Color blue = new Color(0x0000FFFF);
	public static readonly Color yellow = new Color(0xFFFF00FF);
	public static readonly Color cyan = new Color(0x00FFFFFF);
	public static readonly Color magenta = new Color(0xFF00FFFF);

	#endregion Constants

	#region Methods

	private static void MinMaxRgb(out int min, out int max, int r, int g, int b)
	{
		if (r > g)
		{
			max = r;
			min = g;
		}
		else
		{
			max = g;
			min = r;
		}
		if (b > max)
		{
			max = b;
		}
		else if (b < min)
		{
			min = b;
		}
	}

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

	#endregion Methods

	#region Interpolation

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
		amount = Mathf.Clamp01(amount);

		return new(
			(int)(a.r + (b.r - a.r) * amount),
			(int)(a.g + (b.g - a.g) * amount),
			(int)(a.b + (b.b - a.b) * amount),
			(int)(a.a + (b.a - a.a) * amount)
		);
	}

	#endregion Interpolation

	#region Convertion

	// public static implicit operator Color(uint color) => new Color(color);
	// public static implicit operator Color(uint color) => new Color(color);

	public static implicit operator Vector4(Color col) => col.ToVector4();
	public static implicit operator Color(Vector4 vec) => new Color(vec.X, vec.Y, vec.Z, vec.W);

	public static implicit operator Vector3(Color col) => col.ToVector3();
	public static implicit operator Color(Vector3 vec) => new Color(vec.x, vec.y, vec.z, 1.0f);

	public static implicit operator System.Drawing.Color(Color color) => System.Drawing.Color.FromArgb(color.a, color.r, color.g, color.b);
	public static implicit operator Color(System.Drawing.Color color) => new(color.R, color.G, color.B, color.A);

	#endregion Convertion

	#region Operators

	public static bool operator ==(Color a, Color b) => a.abgr == b.abgr;
	public static bool operator !=(Color a, Color b) => a.abgr != b.abgr;

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

	#endregion Operators

	#endregion Static

	#region Instance

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
		set => abgr = (abgr & 0xFFFFFF00) | value;
	}

	/// <summary>
	/// The Green Component
	/// </summary>
	public byte g
	{
		get => (byte)(abgr >> 8);
		set => abgr = (abgr & 0xFFFF00FF) | ((uint)value << 8);
	}

	/// <summary>
	/// The Blue Component
	/// </summary>
	public byte b
	{
		get => (byte)(abgr >> 16);
		set => abgr = (abgr & 0xFF00FFFF) | ((uint)value << 16);
	}

	/// <summary>
	/// The Alpha Component
	/// </summary>
	public byte a
	{
		get => (byte)(abgr >> 24);
		set => abgr = (abgr & 0x00FFFFFF) | ((uint)value << 24);
	}

	public Color translucent => new(r, g, b, (byte)128);
	public Color opaque => new(r, g, b, (byte)255);

	// /// <summary>
	// /// Creates a color given the int32 RGB data
	// /// </summary>
	// public Color(uint rgb, byte alpha = 255)
	// {
	// 	abgr = 0;

	// 	r = (byte)(rgb >> 16);
	// 	g = (byte)(rgb >> 08);
	// 	b = (byte)(rgb);
	// 	a = alpha;
	// }

	// public Color(uint rgb, float alpha)
	// {
	// 	abgr = 0;

	// 	r = (byte)((rgb >> 16) * alpha);
	// 	g = (byte)((rgb >> 08) * alpha);
	// 	b = (byte)(rgb * alpha);
	// 	a = (byte)(255 * alpha);
	// }

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

	#region Methods

	public Color WithAlpha(byte alpha)
	{
		return new(r, g, b, alpha);
	}

	public Color WithAlpha(int alpha)
	{
		return new(r, g, b, alpha);
	}

	public Color WithAlpha(float alpha)
	{
		return new(r, g, b, (byte)(alpha * 255));
	}

	public float GetBrightness()
	{
		MinMaxRgb(out int min, out int max, r, g, b);

		return (max + min) / (byte.MaxValue * 2f);
	}

	public float GetHue()
	{
		if (r == g && g == b)
			return 0f;

		MinMaxRgb(out int min, out int max, r, g, b);

		float delta = max - min;
		float hue;

		if (r == max)
			hue = (g - b) / delta;
		else if (g == max)
			hue = (b - r) / delta + 2f;
		else
			hue = (r - g) / delta + 4f;

		hue *= 60f;
		if (hue < 0f)
			hue += 360f;

		return hue;
	}

	public float GetSaturation()
	{
		if (r == g && g == b)
			return 0f;

		MinMaxRgb(out int min, out int max, r, g, b);

		int div = max + min;
		if (div > byte.MaxValue)
			div = byte.MaxValue * 2 - max - min;

		return (max - min) / (float)div;
	}

	#endregion Methods

	#region Convertion

	/// <summary>
	/// Converts the Color to a Vector4
	/// </summary>
	public Vector4 ToVector4() => new Vector4(r / 255f, g / 255f, b / 255f, a / 255f);

	/// <summary>
	/// Converts the Color to a Vector3
	/// </summary>
	public Vector3 ToVector3() => new Vector3(r / 255f, g / 255f, b / 255f);

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

	#endregion Convertion

	#region Overrides

	public override bool Equals(object? obj) => obj is Color color && Equals(color);
	public bool Equals(Color other) => this == other;

	public override int GetHashCode() => (int)abgr;

	public override string ToString() => ($"[{r}, {g}, {b}, {a}]");

	#endregion Overrides

	#endregion Instance
}
