using System;
using System.Globalization;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace MGE
{
	public class ColorJsonConverter : JsonConverter<Color>
	{
		public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			var value = (string)reader.Value;
			return value.StartsWith("#") ? new Color(value) : (Color)System.Drawing.Color.FromName(value);
		}

		public override void WriteJson(JsonWriter writer, Color color, JsonSerializer serializer)
		{
			writer.WriteValue(color.ToHex());
		}
	}

	[Serializable, DataContract]
	public struct Color : IEquatable<Color>
	{
		public static readonly Color red = new Color(1f, 0f, 0f);
		public static readonly Color yellow = new Color(1f, 1f, 0f);
		public static readonly Color green = new Color(0f, 1f, 0f);
		public static readonly Color cyan = new Color(0f, 1f, 1f);
		public static readonly Color blue = new Color(0f, 0f, 1f);
		public static readonly Color magenta = new Color(1f, 0f, 1f);

		public static readonly Color black = new Color(0f);
		public static readonly Color darkGray = new Color(0.25f);
		public static readonly Color gray = new Color(0.5f);
		public static readonly Color lightGray = new Color(0.75f);
		public static readonly Color white = new Color(1f);

		////////////////////////////////////////////////////////////

		public static Color Lerp(Color a, Color b, float t)
		{
			t = (float)Math.Clamp01(t);

			return new Color(
				a.r + (b.r - a.r) * t,
				a.g + (b.g - a.g) * t,
				a.b + (b.b - a.b) * t,
				a.a + (b.a - a.a) * t
			);
		}

		public static Color LerpUnclamped(Color a, Color b, float t)
		{
			return new Color(
				a.r + (b.r - a.r) * t,
				a.g + (b.g - a.g) * t,
				a.b + (b.b - a.b) * t,
				a.a + (b.a - a.a) * t
			);
		}

		////////////////////////////////////////////////////////////

		public byte intR;
		public byte intG;
		public byte intB;
		public byte intA;

		public float r { get => (float)intR / 255; set => intR = (byte)(value * 255); }
		public float g { get => (float)intG / 255; set => intG = (byte)(value * 255); }
		public float b { get => (float)intB / 255; set => intB = (byte)(value * 255); }
		public float a { get => (float)intA / 255; set => intA = (byte)(value * 255); }

		[DataMember] public string hex { get => ToHex(); set => this = new Color(value); }

		public Color(float r, float g, float b, float a = 1.0f) : this()
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		public Color(float value, float a = 1.0f) : this()
		{
			this.r = value;
			this.g = value;
			this.b = value;
			this.a = a;
		}

		public Color(string hex)
		{
			intR = 255;
			intG = 0;
			intB = 255;
			intA = 255;

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
		}

		public static Color FromBytes(byte intR, byte intG, byte intB, byte intA = 255)
		{
			var color = new Color();
			color.intR = intR;
			color.intG = intG;
			color.intB = intB;
			color.intA = intA;
			return color;
		}

		public static Color FromBytes(int intR, int intG, int intB, int intA = 255)
		{
			var color = new Color();
			color.intR = (byte)intR;
			color.intG = (byte)intG;
			color.intB = (byte)intB;
			color.intA = (byte)intA;
			return color;
		}

		public static Color FromBytes(byte intValue, byte intA = 255) => FromBytes(intValue, intValue, intValue, intA);

		public static Color FromNonPremultiplied(byte r, byte g, byte b, byte a)
		{
			return Color.FromBytes(
				(r * a / 255),
				(g * a / 255),
				(b * a / 255),
				a
			);
		}

		public float this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return r;
					case 1: return g;
					case 2: return b;
					case 3: return a;
					default: throw new IndexOutOfRangeException($"Invalid Color index of {index}!");
				}
			}

			set
			{
				switch (index)
				{
					case 0: r = value; break;
					case 1: g = value; break;
					case 2: b = value; break;
					case 3: a = value; break;
					default: throw new IndexOutOfRangeException($"Invalid Color index of {index}!");
				}
			}
		}

		////////////////////////////////////////////////////////////

		public float average { get => (r + g + b) / 3; }
		public float grayscale { get => 0.299f * r + 0.587f * g + 0.114f * b; }
		public Color inverted { get => new Color(1.0f - r, 1.0f - g, 1.0f - b, a); }
		public Color opaque { get => new Color(r, g, b); }

		////////////////////////////////////////////////////////////

		public string ToHex(int? length = null)
		{
			if (!length.HasValue)
				length = intA == byte.MaxValue ? 6 : 8;

			switch (length)
			{
				case 3: return string.Format("#{0:X1}{1:X1}{2:X1}", intR / 15, intG / 15, intB / 15);
				case 4: return string.Format("#{0:X1}{1:X1}{2:X1}{3:X1}", intR / 15, intG / 15, intB / 15, intA / 15);
				case 6: return string.Format("#{0:X2}{1:X2}{2:X2}", intR, intG, intB);
				case 8: return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", intR, intG, intB, intA);
			}

			Logger.LogError($"Color '{this.ToString()}' can not be convered to hex string a length of {length}!");
			return string.Empty;
		}

		public Color WithAlpha(float a) => new Color(r, g, b, a);

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

		////////////////////////////////////////////////////////////

		public static Color operator +(Color left, Color right) => Color.FromBytes(left.intR + right.intR, left.intG + left.intG, left.intB + right.intB);
		public static Color operator -(Color left, Color right) => Color.FromBytes(left.intR - right.intR, left.intG - left.intG, left.intB - right.intB);
		public static Color operator *(Color left, Color right) => Color.FromBytes(left.intR * right.intR, left.intG * left.intG, left.intB * right.intB);
		public static Color operator /(Color left, Color right) => Color.FromBytes(left.intR / right.intR, left.intG / left.intG, left.intB / right.intB);

		public static Color operator -(Color color) => Color.FromBytes(-color.intR, -color.intG, -color.intB, color.intA);

		public static Color operator *(Color left, float right) => new Color(left.r * right, left.g * right, left.b * right);
		public static Color operator /(Color left, float right) => new Color(left.r / right, left.g / right, left.b / right);

		public static bool operator ==(Color left, Color right) => left.intA == right.intA && left.intR == right.intR && left.intG == right.intG && left.intB == right.intB;

		public static bool operator !=(Color left, Color right) => !(left == right);

		////////////////////////////////////////////////////////////

		public static implicit operator Microsoft.Xna.Framework.Color(Color color) => new Microsoft.Xna.Framework.Color(color.intR, color.intG, color.intB, color.intA);
		public static implicit operator Color(Microsoft.Xna.Framework.Color color) => Color.FromBytes(color.R, color.G, color.B, color.A);

		public static implicit operator System.Drawing.Color(Color color) => System.Drawing.Color.FromArgb(color.intA, color.intR, color.intG, color.intB);
		public static implicit operator Color(System.Drawing.Color color) => Color.FromBytes(color.R, color.G, color.B, color.A);

		////////////////////////////////////////////////////////////

		public override string ToString() => $"({intR} {intG} {intB} {intA})";
		public string ToString(string format)
		{
			if (format.StartsWith('#'))
				return string.Format(format, intR, intG, intB, intA);
			return string.Format(format, r, g, b, a);
		}

		public override int GetHashCode() => HashCode.Combine(r, g, b, a);

		public bool Equals(Color color) => intA == color.intA && intR == color.intR && intG == color.intG && intB == color.intB;
		public override bool Equals(object other)
		{
			if (other is Color color)
				return Equals(color);
			return false;
		}
	}
}