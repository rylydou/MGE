using System;

namespace MGE;

// public class RectJsonConverter : JsonConverter<Rect>
// {
// 	public override Rect ReadJson(JsonReader reader, Type objectType, Rect existingValue, bool hasExistingValue, JsonSerializer serializer)
// 	{
// 		var values = reader.ReadAsMultiDimensional<float>();

// 		if (values.Length == 4)
// 			return new Rect(values[0], values[1], values[2], values[3]);

// 		throw new InvalidOperationException("Invalid Rect");
// 	}

// 	public override void WriteJson(JsonWriter writer, Rect rect, JsonSerializer serializer)
// 	{
// 		writer.WriteValue($"{rect.x} {rect.y} {rect.width} {rect.height}");
// 	}
// }

[System.Serializable]
public struct Rect : IEquatable<Rect>
{
	static Rect _zero = new Rect(0, 0, 0, 0);
	public static Rect zero { get => _zero; }

	////////////////////////////////////////////////////////////

	public static Rect OrderMinMax(Rect rect)
	{
		if (rect.xMin > rect.xMax)
		{
			var temp = rect.xMin;
			rect.xMin = rect.xMax;
			rect.xMax = temp;
		}

		if (rect.yMin > rect.yMax)
		{
			var temp = rect.yMin;
			rect.yMin = rect.yMax;
			rect.yMax = temp;
		}

		return rect;
	}

	public static Vector2 NormalizedToPoint(Rect rectangle, Vector2 normalized)
	{
		return new Vector2(
			Math.Lerp(rectangle.x, rectangle.xMax, normalized.x),
			Math.Lerp(rectangle.y, rectangle.yMax, normalized.y)
		);
	}

	public static Vector2 PointToNormalized(Rect rectangle, Vector2 point)
	{
		return new Vector2(
			Math.InverseLerp(rectangle.x, rectangle.xMax, point.x),
			Math.InverseLerp(rectangle.y, rectangle.yMax, point.y)
		);
	}

	public static Rect FitRect(Rect inner, Rect outer)
	{
		float scaleRatio;
		if (outer.width / outer.height >= inner.width / inner.height)
			scaleRatio = inner.width / outer.width;
		else
			scaleRatio = inner.height / outer.height;

		var width = outer.width * scaleRatio;
		var height = outer.height * scaleRatio;

		var xCenter = inner.x + (inner.width / 2);
		var yCenter = inner.y + (inner.height / 2);

		var x = xCenter - (width / 2);
		var y = yCenter - (height / 2);

		return new Rect(x, y, width, height);
	}

	////////////////////////////////////////////////////////////

	float _xMin;
	float _yMin;
	float _width;
	float _height;

	public Rect(float x, float y, float width, float height)
	{
		_xMin = x;
		_yMin = y;
		_width = width;
		_height = height;
	}

	public Rect(float x, float y, float size)
	{
		_xMin = x;
		_yMin = y;
		_width = size;
		_height = size;
	}

	public Rect(Vector2 position, float size)
	{
		_xMin = position.x;
		_yMin = position.y;
		_width = size;
		_height = size;
	}

	public Rect(Vector2 position, float width, float height)
	{
		_xMin = position.x;
		_yMin = position.y;
		_width = width;
		_height = height;
	}

	public Rect(float x, float y, Vector2 size)
	{
		_xMin = x;
		_yMin = y;
		_width = size.x;
		_height = size.y;
	}

	public Rect(Vector2 position, Vector2 size)
	{
		_xMin = position.x;
		_yMin = position.y;
		_width = size.x;
		_height = size.y;
	}

	public float this[int index]
	{
		get
		{
			switch (index)
			{
				case 0: return _xMin;
				case 1: return _yMin;
				case 2: return _width;
				case 3: return _height;
				default: throw new IndexOutOfRangeException($"Invalid Rect index of {index}!");
			}
		}

		set
		{
			switch (index)
			{
				case 0: _xMin = value; break;
				case 1: _yMin = value; break;
				case 2: _width = value; break;
				case 3: _height = value; break;
				default: throw new IndexOutOfRangeException($"Invalid Rect index of {index}!");
			}
		}
	}

	////////////////////////////////////////////////////////////

	public float xMin { get => _xMin; set { var oldxmax = xMax; _yMin = value; _width = oldxmax - _xMin; } }
	public float yMin { get => _yMin; set { var oldymax = yMax; _yMin = value; _height = oldymax - _yMin; } }
	public float xMax { get => _width + _xMin; set => _width = value - _xMin; }
	public float yMax { get => _height + _yMin; set => _height = value - _yMin; }

	[Prop] public float x { get => _xMin; set => _xMin = value; }
	[Prop] public float y { get => _yMin; set => _yMin = value; }
	[Prop] public float width { get => _width; set => _width = value; }
	[Prop] public float height { get => _height; set => _height = value; }

	public Vector2 min { get => new Vector2(xMin, yMin); set { xMin = value.x; yMin = value.y; } }
	public Vector2 max { get => new Vector2(xMax, yMax); set { xMax = value.x; yMax = value.y; } }

	public float top { get => yMin; set => yMin = top; }
	public float bottom { get => yMax; set => yMax = bottom; }
	public float left { get => xMin; set => xMin = left; }
	public float right { get => xMax; set => xMax = right; }

	public Vector2 topLeft { get => new Vector2(xMin, yMin); set { xMin = value.x; yMin = value.y; } }
	public Vector2 topRight { get => new Vector2(xMax, yMin); set { xMax = value.x; yMin = value.y; } }
	public Vector2 bottomLeft { get => new Vector2(xMin, yMax); set { xMin = value.x; yMax = value.y; } }
	public Vector2 bottomRight { get => new Vector2(xMax, yMax); set { xMax = value.x; yMax = value.y; } }

	public Vector2 position
	{
		get => new Vector2(_xMin, _yMin);
		set { _xMin = value.x; _yMin = value.y; }
	}

	public Vector2 size { get => new Vector2(_width, _height); set { _width = value.x; _height = value.y; } }

	public Vector2 center
	{
		get => new Vector2(x + _width / 2f, y + _height / 2f);
		set { _xMin = value.x - _width / 2f; _yMin = value.y - _height / 2f; }
	}

	////////////////////////////////////////////////////////////

	public bool Contains(Vector2 point) => (point.x >= xMin) && (point.x < xMax) && (point.y >= yMin) && (point.y < yMax);
	public bool Contains(Rect rect) => (rect.xMin >= xMin) && (rect.xMax < xMax) && (rect.yMin >= yMin) && (rect.yMax < yMax);

	public bool Overlaps(Rect other) => other.xMax > xMin && other.xMin < xMax && other.yMax > yMin && other.yMin < yMax;
	public bool Overlaps(Rect other, bool allowInverse)
	{
		var self = this;
		if (allowInverse)
		{
			self = OrderMinMax(self);
			other = OrderMinMax(other);
		}
		return self.Overlaps(other);
	}

	public void Expand(float amount)
	{
		amount = amount / 2;
		top -= amount;
		left -= amount;
		bottom += amount;
		right += amount;
	}

	////////////////////////////////////////////////////////////

	public static bool operator !=(Rect lhs, Rect rhs) => !(lhs == rhs);
	public static bool operator ==(Rect lhs, Rect rhs) => lhs.x == rhs.x && lhs.y == rhs.y && lhs.width == rhs.width && lhs.height == rhs.height;

	////////////////////////////////////////////////////////////

	public static implicit operator Rect(RectInt rect) => new Rect(rect.x, rect.y, rect.width, rect.height);
	public static implicit operator RectInt(Rect rect) => new RectInt((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);

	public static implicit operator (float, float, float, float)(Rect rect) => (rect.x, rect.y, rect.width, rect.height);
	public static implicit operator Rect((float, float, float, float) rect) => new(rect.Item1, rect.Item2, rect.Item3, rect.Item4);

	public static implicit operator tainicom.Aether.Physics2D.Collision.AABB(Rect rect) => new(rect.center, rect.width, rect.height);
	public static implicit operator Rect(tainicom.Aether.Physics2D.Collision.AABB aabb) => new(aabb.LowerBound, aabb.Width, aabb.Height);

	////////////////////////////////////////////////////////////

	public override string ToString() => $"{{ {x:F2} {y:F2} {width:F2} x {height:F2} }}";
	public string ToString(string format) => string.Format(format, x, y, width, height);

	public override int GetHashCode() => HashCode.Combine(x, y, width, height);

	public bool Equals(Rect other) => x.Equals(other.x) && y.Equals(other.y) && width.Equals(other.width) && height.Equals(other.height);
	public override bool Equals(object? other) => other is Rect rect && Equals(rect);
}
