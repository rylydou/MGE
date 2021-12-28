using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace MGE;

// public class ColorJsonConverter : JsonConverter<Color>
// {
// 	public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
// 	{
// 		var value = (string?)reader.Value;
// 		if (value is null) return Color.transparent;
// 		return value.StartsWith("#") ? new Color(value) : (Color)System.Drawing.Color.FromName(value);
// 	}

// 	public override void WriteJson(JsonWriter writer, Color color, JsonSerializer serializer)
// 	{
// 		writer.WriteValue(color.ToHex());
// 	}
// }

[Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Color : IEquatable<Color>
{
	#region Static

	#region Constants

	public static readonly Color red = new(1f, 0f, 0f);
	public static readonly Color green = new(0f, 1f, 0f);
	public static readonly Color blue = new(0f, 0f, 1f);

	public static readonly Color yellow = new(1f, 1f, 0f);
	public static readonly Color cyan = new(0f, 1f, 1f);
	public static readonly Color magenta = new(1f, 0f, 1f);

	public static readonly Color white = new(1f);
	public static readonly Color lightGray = new(0.75f);
	public static readonly Color gray = new(0.5f);
	public static readonly Color darkGray = new(0.25f);
	public static readonly Color black = new(0f);

	public static readonly Color transparent = new();

	#endregion Constants

	#region Interpolation

	public static Color Lerp(Color a, Color b, float t)
	{
		return new Color(
			a.r + (b.r - a.r) * t,
			a.g + (b.g - a.g) * t,
			a.b + (b.b - a.b) * t,
			a.a + (b.a - a.a) * t
		);
	}

	public static Color LerpClamped(Color a, Color b, float t)
	{
		t = (float)Math.Clamp01(t);

		return new Color(
			a.r + (b.r - a.r) * t,
			a.g + (b.g - a.g) * t,
			a.b + (b.b - a.b) * t,
			a.a + (b.a - a.a) * t
		);
	}

	#endregion Interpolation

	#endregion Static

	#region Instance

	public byte intR;
	public byte intG;
	public byte intB;
	public byte intA;

	public float r { get => (float)intR / 255; set => intR = (byte)(value * 255); }
	public float g { get => (float)intG / 255; set => intG = (byte)(value * 255); }
	public float b { get => (float)intB / 255; set => intB = (byte)(value * 255); }
	public float a { get => (float)intA / 255; set => intA = (byte)(value * 255); }

	[Prop] public string hex { get => ToHex(); set => this = new Color(value); }

	public Color(byte value) : this(value, value, value) { }
	public Color(byte intR, byte intG, byte intB) : this(intR, intG, intB, 255) { }

	public Color(byte value, byte a) : this(value, value, value, a) { }
	public Color(byte intR, byte intG, byte intB, byte intA)
	{
		this.intR = intR;
		this.intG = intG;
		this.intB = intB;
		this.intA = intA;
	}

	public Color(float value) : this(value, value, value) { }
	public Color(float r, float g, float b) : this((byte)(r * 255), (byte)(g * 255), (byte)(b * 255)) { }

	public Color(float value, float a) : this(value, value, value, a) { }
	public Color(float r, float g, float b, float a) : this((byte)(r * 255), (byte)(g / 255), (byte)(b * 255), (byte)(a * 255)) { }

	public Color(string hex)
	{
		if (hex.StartsWith('#'))
			hex = hex.Substring(1);

		switch (hex.Length)
		{
			case 3:
				intR = byte.Parse(hex.Substring(0, 1) + hex.Substring(0, 1), NumberStyles.AllowHexSpecifier);
				intG = byte.Parse(hex.Substring(1, 1) + hex.Substring(1, 1), NumberStyles.AllowHexSpecifier);
				intB = byte.Parse(hex.Substring(2, 1) + hex.Substring(2, 1), NumberStyles.AllowHexSpecifier);
				intA = 255;
				return;
			case 4:
				intR = byte.Parse(hex.Substring(0, 1) + hex.Substring(0, 1), NumberStyles.AllowHexSpecifier);
				intG = byte.Parse(hex.Substring(1, 1) + hex.Substring(1, 1), NumberStyles.AllowHexSpecifier);
				intB = byte.Parse(hex.Substring(2, 1) + hex.Substring(2, 1), NumberStyles.AllowHexSpecifier);
				intA = byte.Parse(hex.Substring(3, 1) + hex.Substring(3, 1), NumberStyles.AllowHexSpecifier);
				return;
			case 6:
				intR = byte.Parse(hex.Substring(0, 2), NumberStyles.AllowHexSpecifier);
				intG = byte.Parse(hex.Substring(2, 2), NumberStyles.AllowHexSpecifier);
				intB = byte.Parse(hex.Substring(4, 2), NumberStyles.AllowHexSpecifier);
				intA = 255;
				return;
			case 8:
				intR = byte.Parse(hex.Substring(0, 2), NumberStyles.AllowHexSpecifier);
				intG = byte.Parse(hex.Substring(2, 2), NumberStyles.AllowHexSpecifier);
				intB = byte.Parse(hex.Substring(4, 2), NumberStyles.AllowHexSpecifier);
				intA = byte.Parse(hex.Substring(6, 2), NumberStyles.AllowHexSpecifier);
				return;
		}

		throw new Exception();
	}

	public static Color FromNonPremultiplied(byte r, byte g, byte b, byte a) => new(r * a, g * a, b * a, a);

	public byte this[int index]
	{
		get
		{
			switch (index)
			{
				case 0: return intR;
				case 1: return intG;
				case 2: return intB;
				case 3: return intA;
				default: throw new IndexOutOfRangeException($"Invalid Color index of {index}!");
			}
		}

		set
		{
			switch (index)
			{
				case 0: intR = value; break;
				case 1: intG = value; break;
				case 2: intB = value; break;
				case 3: intA = value; break;
				default: throw new IndexOutOfRangeException($"Invalid Color index of {index}!");
			}
		}
	}

	#region Properties

	public float average { get => (r + g + b) / 3; }
	public float grayscale { get => r * 0.299f + g * 0.587f + b * 0.114f; }
	public Color inverted { get => new Color(1f - r, 1f - g, 1f - b, a); }
	public Color opaque { get => new Color(r, g, b); }

	#endregion

	#region Methods

	public string ToHex(int length = 8)
	{
		switch (length)
		{
			case 3: return $"#{intR / 15:X1}{intG / 15:X1}{intB / 15:X1}";
			case 4: return $"#{intR / 15:X1}{intG / 15:X1}{intB / 15:X1}{intA / 15:X1}";
			case 6: return $"#{intR:X2}{intG:X2}{intB:X2}";
			case 8: return $"#{intR:X2}{intG:X2}{intB:X2}{intA:X2}";
		}
		throw new ArgumentException($"Color '{this.ToString()}' can not be converted to hex string a length of {length}!", nameof(length));
	}

	public Color WithAlpha(float a) => new(r, g, b, a);

	public void Deconstruct(out byte r, out byte g, out byte b, out byte a)
	{
		r = intR;
		g = intG;
		b = intB;
		a = intA;
	}

	public void Deconstruct(out byte r, out byte g, out byte b)
	{
		r = intR;
		g = intG;
		b = intB;
	}

	#endregion

	#region Operators

	public static Color operator +(Color left, Color right) => new(left.intR + right.intR, left.intG + left.intG, left.intB + right.intB);
	public static Color operator -(Color left, Color right) => new(left.intR - right.intR, left.intG - left.intG, left.intB - right.intB);
	public static Color operator *(Color left, Color right) => new(left.intR * right.intR, left.intG * left.intG, left.intB * right.intB);
	public static Color operator /(Color left, Color right) => new(left.intR / right.intR, left.intG / left.intG, left.intB / right.intB);

	// public static Color operator -(Color color) => new(-color.intR, -color.intG, -color.intB, color.intA);

	public static Color operator *(Color left, float right) => new Color(left.r * right, left.g * right, left.b * right);
	public static Color operator /(Color left, float right) => new Color(left.r / right, left.g / right, left.b / right);

	public static bool operator ==(Color left, Color right) => left.intA == right.intA && left.intR == right.intR && left.intG == right.intG && left.intB == right.intB;
	public static bool operator !=(Color left, Color right) => !(left == right);

	#endregion Operators

	#region Conversion

	public static implicit operator (float, float, float, float)(Color color) => (color.r, color.g, color.b, color.a);
	public static implicit operator Color((float, float, float, float) color) => new(color.Item1, color.Item2, color.Item3, color.Item4);

	public static implicit operator (float, float, float)(Color color) => (color.r, color.g, color.b);
	public static implicit operator Color((float, float, float) color) => new(color.Item1, color.Item2, color.Item3);

	#region Thirdparty

	public static implicit operator System.Drawing.Color(Color color) => System.Drawing.Color.FromArgb(color.intA, color.intR, color.intG, color.intB);
	public static implicit operator Color(System.Drawing.Color color) => new(color.R, color.G, color.B, color.A);

	#endregion Thirdparty

	#endregion Conversion

	#region Overrides

	public override string ToString() => $"({intR} {intG} {intB} {intA})";
	public string ToString(string format)
	{
		if (format.StartsWith('#'))
			return string.Format(format, intR, intG, intB, intA);
		return string.Format(format, r, g, b, a);
	}

	public override int GetHashCode() => HashCode.Combine(r, g, b, a);

	public bool Equals(Color color) => intA == color.intA && intR == color.intR && intG == color.intG && intB == color.intB;
	public override bool Equals(object? other) => other is Color color && Equals(color);

	#endregion Overrides

	#endregion Instance
}
