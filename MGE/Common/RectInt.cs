using System;

namespace MGE;

// public class RectIntJsonConverter : JsonConverter<RectInt>
// {
// 	public override RectInt ReadJson(JsonReader reader, Type objectType, RectInt existingValue, bool hasExistingValue, JsonSerializer serializer)
// 	{
// 		var values = reader.ReadAsMultiDimensional<int>();

// 		if (values.Length == 4)
// 			return new RectInt(values[0], values[1], values[2], values[3]);

// 		throw new InvalidOperationException("Invalid Rect");
// 	}

// 	public override void WriteJson(JsonWriter writer, RectInt rect, JsonSerializer serializer)
// 	{
// 		writer.WriteValue($"{rect.x} {rect.y} {rect.width} {rect.height}");
// 	}
// }

[System.Serializable]
public struct RectInt : IEquatable<RectInt>
{
	#region Static

	#region Constants

	public static readonly RectInt zero = new(0, 0, 0, 0);

	#endregion Constants

	#region Methods

	public static Rect Intersect(Rect a, Rect b)
	{
		Intersect(ref a, ref b, out var result);
		return result;
	}

	public static void Intersect(ref Rect a, ref Rect b, out Rect result)
	{
		if (a.Overlaps(b))
		{
			var right = Math.Min(a.x + a.width, b.x + b.width);
			var left = Math.Max(a.x, b.x);
			var top = Math.Max(a.y, b.y);
			var bottom = Math.Min(a.y + a.height, b.y + b.height);
			result = new(left, top, right - left, bottom - top);
		}
		else
		{
			result = Rect.zero;
		}
	}

	public static RectInt OrderMinMax(RectInt rect)
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

	public static RectInt Fit(RectInt inner, RectInt outer)
	{
		float scaleRatio;
		if (outer.width / outer.height >= inner.width / inner.height)
		{
			scaleRatio = inner.width / outer.width;
		}
		else
		{
			scaleRatio = inner.height / outer.height;
		}

		var width = outer.width * scaleRatio;
		var height = outer.height * scaleRatio;

		var xCenter = inner.x + (inner.width / 2);
		var yCenter = inner.y + (inner.height / 2);

		var x = xCenter - (width / 2);
		var y = yCenter - (height / 2);

		return new((int)x, (int)y, (int)width, (int)height);
	}

	#endregion Methods

	#endregion Static

	#region Instance

	int _xMin;
	int _yMin;
	int _width;
	int _height;

	public int this[int index]
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

	#region Size & Position

	public RectInt(int x, int y, int width, int height)
	{
		_xMin = x;
		_yMin = y;
		_width = width;
		_height = height;
	}

	public RectInt(Vector2Int position, Vector2Int size)
	{
		_xMin = position.x;
		_yMin = position.y;
		_width = size.x;
		_height = size.y;
	}

	public RectInt(Vector2Int position, int width, int height)
	{
		_xMin = position.x;
		_yMin = position.y;
		_width = width;
		_height = height;
	}

	public RectInt(Vector2Int position, int size)
	{
		_xMin = position.x;
		_yMin = position.y;
		_width = size;
		_height = size;
	}

	#endregion Size & Position

	#region Size Only

	public RectInt(int width, int height)
	{
		_xMin = 0;
		_yMin = 0;
		_width = width;
		_height = height;
	}
	public RectInt(Vector2Int size)
	{
		_xMin = 0;
		_yMin = 0;
		_width = size.x;
		_height = size.y;
	}

	#endregion Size Only

	#region Properties

	public int xMin { get => _xMin; set { var oldxmax = xMax; _yMin = value; _width = oldxmax - _xMin; } }
	public int yMin { get => _yMin; set { var oldymax = yMax; _yMin = value; _height = oldymax - _yMin; } }
	public int xMax { get => _width + _xMin; set => _width = value - _xMin; }
	public int yMax { get => _height + _yMin; set => _height = value - _yMin; }

	[Prop] public int x { get => _xMin; set => _xMin = value; }
	[Prop] public int y { get => _yMin; set => _yMin = value; }
	[Prop] public int width { get => _width; set => _width = value; }
	[Prop] public int height { get => _height; set => _height = value; }

	public Vector2Int min { get => new(xMin, yMin); set { xMin = value.x; yMin = value.y; } }
	public Vector2Int max { get => new(xMax, yMax); set { xMax = value.x; yMax = value.y; } }

	public int top { get => yMin; set => yMin = top; }
	public int bottom { get => yMax; set => yMax = bottom; }
	public int left { get => xMin; set => xMin = left; }
	public int right { get => xMax; set => xMax = right; }

	public Vector2Int topLeft { get => new(xMin, yMin); set { xMin = value.x; yMin = value.y; } }
	public Vector2Int topRight { get => new(xMax, yMin); set { xMax = value.x; yMin = value.y; } }
	public Vector2Int bottomLeft { get => new(xMin, yMax); set { xMin = value.x; yMax = value.y; } }
	public Vector2Int bottomRight { get => new(xMax, yMax); set { xMax = value.x; yMax = value.y; } }

	public Vector2Int position { get => new(_xMin, _yMin); set { _xMin = value.x; _yMin = value.y; } }

	public Vector2Int size { get => new(_width, _height); set { _width = value.x; _height = value.y; } }

	public Vector2 center { get => new(x + _width / 2, y + _height / 2); }

	public bool isEmpty { get => width == 0 && height == 0; }

	#endregion Properties

	#region Methods

	public void Set(int x, int y, int width, int height)
	{
		_xMin = x;
		_yMin = y;
		_width = width;
		_height = height;
	}

	public bool Contains(Vector2Int point) => (point.x >= xMin) && (point.x < xMax) && (point.y >= yMin) && (point.y < yMax);
	public bool Contains(RectInt rect) => (rect.xMin >= xMin) && (rect.xMax < xMax) && (rect.yMin >= yMin) && (rect.yMax < yMax);

	public bool Overlaps(RectInt other) => other.xMax > xMin && other.xMin < xMax && other.yMax > yMin && other.yMin < yMax;
	public bool Overlaps(RectInt other, bool allowInverse)
	{
		var self = this;
		if (allowInverse)
		{
			self = OrderMinMax(self);
			other = OrderMinMax(other);
		}
		return self.Overlaps(other);
	}

	#endregion Methods

	#region Operators

	public static bool operator !=(RectInt lhs, RectInt rhs) => !(lhs == rhs);
	public static bool operator ==(RectInt lhs, RectInt rhs) => lhs.x == rhs.x && lhs.y == rhs.y && lhs.width == rhs.width && lhs.height == rhs.height;

	#endregion Operators

	#region Conversion

	public static implicit operator (int, int, int, int)(RectInt rect) => (rect.x, rect.y, rect.width, rect.height);
	public static implicit operator RectInt((int, int, int, int) rect) => new(rect.Item1, rect.Item2, rect.Item3, rect.Item4);

	#region Thirdparty

	public static implicit operator System.Drawing.Rectangle(RectInt rect) => new(rect.x, rect.y, rect.width, rect.height);
	public static implicit operator RectInt(System.Drawing.Rectangle rect) => new(rect.X, rect.Y, rect.Width, rect.Height);

	#endregion Thirdparty

	#endregion Conversion

	#region Overrides

	public override string ToString() => $"{{ {x}, {y}; {width} x {height} }}";
	public string ToString(string format) => string.Format(format, x, y, width, height);

	public override int GetHashCode() => HashCode.Combine(x, y, width, height);

	public bool Equals(RectInt other) => x.Equals(other.x) && y.Equals(other.y) && width.Equals(other.width) && height.Equals(other.height);
	public override bool Equals(object? other) => other is RectInt rect && Equals(rect);

	#endregion Overrides

	#endregion Instance
}
