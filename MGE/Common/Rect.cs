using System;

namespace MGE;

public struct Rect : IEquatable<Rect>
{
	#region Static

	#region Constants

	public static readonly Rect zero = new(0, 0, 0, 0);

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
			var right = Mathf.Min(a.x + a.width, b.x + b.width);
			var left = Mathf.Max(a.x, b.x);
			var top = Mathf.Max(a.y, b.y);
			var bottom = Mathf.Min(a.y + a.height, b.y + b.height);
			result = new(left, top, right - left, bottom - top);
		}
		else
		{
			result = Rect.zero;
		}
	}

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

	public static Rect Fit(Vector2 inner, Rect outer)
	{
		var innerAspectRatio = inner.x / inner.y;
		var outerAspectRatio = outer.width / outer.height;

		var resizeFactor = (innerAspectRatio >= outerAspectRatio) ?
		(outer.width / (float)inner.x) :
		(outer.height / (float)inner.y);

		var newWidth = inner.x * resizeFactor;
		var newHeight = inner.y * resizeFactor;
		var newLeft = outer.left + (outer.width - newWidth) / 2f;
		var newTop = outer.top + (outer.height - newHeight) / 2f;

		return new(newLeft, newTop, newWidth, newHeight);
	}

	#endregion Methods

	#endregion Static

	#region Instance

	float _xMin;
	float _yMin;
	float _width;
	float _height;

	public float this[float index]
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

	public Rect(float x, float y, float width, float height)
	{
		_xMin = x;
		_yMin = y;
		_width = width;
		_height = height;
	}

	public Rect(Vector2 position, Vector2 size)
	{
		_xMin = position.x;
		_yMin = position.y;
		_width = size.x;
		_height = size.y;
	}

	public Rect(Vector2 position, float width, float height)
	{
		_xMin = position.x;
		_yMin = position.y;
		_width = width;
		_height = height;
	}

	public Rect(Vector2 position, float size)
	{
		_xMin = position.x;
		_yMin = position.y;
		_width = size;
		_height = size;
	}

	#endregion Size & Position

	#region Size Only

	public Rect(float width, float height)
	{
		_xMin = 0;
		_yMin = 0;
		_width = width;
		_height = height;
	}
	public Rect(Vector2 size)
	{
		_xMin = 0;
		_yMin = 0;
		_width = size.x;
		_height = size.y;
	}

	#endregion Size Only

	#region Properties

	public float xMin { get => _xMin; set { var oldxmax = xMax; _yMin = value; _width = oldxmax - _xMin; } }
	public float yMin { get => _yMin; set { var oldymax = yMax; _yMin = value; _height = oldymax - _yMin; } }
	public float xMax { get => _width + _xMin; set => _width = value - _xMin; }
	public float yMax { get => _height + _yMin; set => _height = value - _yMin; }

	[Prop] public float x { get => _xMin; set => _xMin = value; }
	[Prop] public float y { get => _yMin; set => _yMin = value; }
	[Prop] public float width { get => _width; set => _width = value; }
	[Prop] public float height { get => _height; set => _height = value; }

	public Vector2 min { get => new(xMin, yMin); set { xMin = value.x; yMin = value.y; } }
	public Vector2 max { get => new(xMax, yMax); set { xMax = value.x; yMax = value.y; } }

	public float top { get => yMin; set => yMin = top; }
	public float bottom { get => yMax; set => yMax = bottom; }
	public float left { get => xMin; set => xMin = left; }
	public float right { get => xMax; set => xMax = right; }

	public Vector2 topLeft { get => new(xMin, yMin); set { xMin = value.x; yMin = value.y; } }
	public Vector2 topRight { get => new(xMax, yMin); set { xMax = value.x; yMin = value.y; } }
	public Vector2 bottomLeft { get => new(xMin, yMax); set { xMin = value.x; yMax = value.y; } }
	public Vector2 bottomRight { get => new(xMax, yMax); set { xMax = value.x; yMax = value.y; } }

	public Vector2 position { get => new(_xMin, _yMin); set { _xMin = value.x; _yMin = value.y; } }

	public Vector2 size { get => new(_width, _height); set { _width = value.x; _height = value.y; } }

	public Vector2 center { get => new(x + _width / 2, y + _height / 2); }

	public bool isEmpty { get => width == 0 || height == 0; }

	#endregion Properties

	#region Methods

	public void Set(float x, float y, float width, float height)
	{
		_xMin = x;
		_yMin = y;
		_width = width;
		_height = height;
	}

	public void Offset(float offsetX, float offsetY)
	{
		x += offsetX;
		y += offsetY;
	}

	public void Offset(Vector2 offset)
	{
		x += offset.x;
		y += offset.y;
	}

	public void Expand(float amount)
	{
		_xMin -= amount;
		_yMin -= amount;
		_width += amount * 2;
		_height += amount * 2;
	}

	public Rect Encapsulate(Vector2 point)
	{
		Rect expanded = this;

		Vector2 begin = expanded.position;
		Vector2 end = expanded.position + expanded.size;

		if (point.x < begin.x)
		{
			begin.x = point.x;
		}
		if (point.y < begin.y)
		{
			begin.y = point.y;
		}

		if (point.x > end.x)
		{
			end.x = point.x;
		}
		if (point.y > end.y)
		{
			end.y = point.y;
		}

		expanded.position = begin;
		expanded.size = end - begin;

		return expanded;
	}

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

	#endregion Methods

	#region Operators

	public static bool operator !=(Rect a, Rect b) => !(a == b);
	public static bool operator ==(Rect a, Rect b) => a.x == b.x && a.y == b.y && a.width == b.width && a.height == b.height;

	public static Rect operator +(Rect a, Vector2 b) => new Rect(a.x + b.x, a.y + b.y, a.width, a.height);
	public static Rect operator -(Rect a, Vector2 b) => new Rect(a.x - b.x, a.y - b.y, a.width, a.height);

	#endregion Operators

	#region Conversion

	public static implicit operator RectInt(Rect rect) => new((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
	public static implicit operator Rect(RectInt rect) => new(rect.x, rect.y, rect.width, rect.height);

	public static implicit operator (float, float, float, float)(Rect rect) => (rect.x, rect.y, rect.width, rect.height);
	public static implicit operator Rect((float, float, float, float) rect) => new(rect.Item1, rect.Item2, rect.Item3, rect.Item4);

	#region Thirdparty

	public static implicit operator global::System.Drawing.Rectangle(Rect rect) => new((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
	public static implicit operator Rect(global::System.Drawing.Rectangle rect) => new(rect.X, rect.Y, rect.Width, rect.Height);

	#endregion Thirdparty

	#endregion Conversion

	#region Overrides

	public override string ToString() => $"({x:N2}, {y:N2}; {width:N2}, {height:N2})";
	public string ToString(string format) => string.Format(format, x, y, width, height);

	public override int GetHashCode() => HashCode.Combine(x, y, width, height);

	public bool Equals(Rect other) => x.Equals(other.x) && y.Equals(other.y) && width.Equals(other.width) && height.Equals(other.height);
	public override bool Equals(object? other) => other is Rect rect && Equals(rect);

	#endregion Overrides

	#endregion Instance
}
