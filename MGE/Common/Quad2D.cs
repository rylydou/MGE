using System;
using System.Numerics;

namespace MGE;

/// <summary>
/// A 2D Quad
/// </summary>
public struct Quad2D : IConvexShape2D
{
	Vector2 _a;
	Vector2 _b;
	Vector2 _c;
	Vector2 _d;
	Vector2 _normalAB;
	Vector2 _normalBC;
	Vector2 _normalCD;
	Vector2 _normalDA;
	bool _dirty;

	public Vector2 a
	{
		get => _a;
		set
		{
			if (_a != value)
			{
				_a = value;
				_dirty = true;
			}
		}
	}

	public Vector2 b
	{
		get => _b;
		set
		{
			if (_b != value)
			{
				_b = value;
				_dirty = true;
			}
		}
	}

	public Vector2 c
	{
		get => _c;
		set
		{
			if (_c != value)
			{
				_c = value;
				_dirty = true;
			}
		}
	}

	public Vector2 d
	{
		get => _d;
		set
		{
			if (_d != value)
			{
				_d = value;
				_dirty = true;
			}
		}
	}

	public Vector2 normalAB
	{
		get
		{
			if (_dirty)
				UpdateQuad();
			return _normalAB;
		}
	}

	public Vector2 normalBC
	{
		get
		{
			if (_dirty)
				UpdateQuad();
			return _normalBC;
		}
	}

	public Vector2 normalCD
	{
		get
		{
			if (_dirty)
				UpdateQuad();
			return _normalCD;
		}
	}

	public Vector2 normalDA
	{
		get
		{
			if (_dirty)
				UpdateQuad();
			return _normalDA;
		}
	}

	public Vector2 center => (_a + _b + _c + _d) / 4f;

	public Quad2D(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
	{
		this._a = a;
		this._b = b;
		this._c = c;
		this._d = d;
		_normalAB = _normalBC = _normalCD = _normalDA = Vector2.zero;
		_dirty = true;
	}

	void UpdateQuad()
	{
		_normalAB = (_b - _a).normalized;
		_normalAB = new Vector2(-_normalAB.y, _normalAB.x);
		_normalBC = (_c - _b).normalized;
		_normalBC = new Vector2(-_normalBC.y, _normalBC.x);
		_normalCD = (_d - _c).normalized;
		_normalCD = new Vector2(-_normalCD.y, _normalCD.x);
		_normalDA = (_a - _d).normalized;
		_normalDA = new Vector2(-_normalDA.y, _normalDA.x);

		_dirty = false;
	}

	public Quad2D Translate(in Vector2 amount)
	{
		a += amount;
		b += amount;
		c += amount;
		d += amount;
		return this;
	}

	public void Project(in Vector2 axis, out float min, out float max)
	{
		min = float.MaxValue;
		max = float.MinValue;

		var dot = Vector2.Dot(a, axis);
		min = Math.Min(dot, min);
		max = Math.Max(dot, max);
		dot = Vector2.Dot(b, axis);
		min = Math.Min(dot, min);
		max = Math.Max(dot, max);
		dot = Vector2.Dot(c, axis);
		min = Math.Min(dot, min);
		max = Math.Max(dot, max);
		dot = Vector2.Dot(d, axis);
		min = Math.Min(dot, min);
		max = Math.Max(dot, max);
	}

	public int points => 4;

	public Vector2 GetPoint(int index)
	{
		return index switch
		{
			0 => a,
			1 => b,
			2 => c,
			3 => d,
			_ => throw new IndexOutOfRangeException(),
		};
	}

	public int axis => 4;

	public Vector2 GetAxis(int index)
	{
		return index switch
		{
			0 => new Vector2(-_normalAB.y, _normalAB.x),
			1 => new Vector2(-_normalBC.y, _normalBC.x),
			2 => new Vector2(-_normalCD.y, _normalCD.x),
			3 => new Vector2(-_normalDA.y, _normalDA.x),
			_ => throw new IndexOutOfRangeException(),
		};
	}

	public Rect BoundingRect()
	{
		var bounds = new Rect();
		bounds.x = Math.Min(_a.x, Math.Min(_b.x, Math.Min(_c.x, _d.x)));
		bounds.y = Math.Min(_a.y, Math.Min(_b.y, Math.Min(_c.y, _d.y)));
		bounds.width = Math.Max(_a.x, Math.Max(_b.x, Math.Max(_c.x, _d.x))) - bounds.x;
		bounds.height = Math.Max(_a.y, Math.Max(_b.y, Math.Max(_c.y, _d.y))) - bounds.y;
		return bounds;
	}

	public override bool Equals(object? obj) => (obj is Quad2D other) && (this == other);

	public override int GetHashCode()
	{
		int hash = 17;
		hash = hash * 23 + a.GetHashCode();
		hash = hash * 23 + b.GetHashCode();
		hash = hash * 23 + c.GetHashCode();
		hash = hash * 23 + d.GetHashCode();
		return hash;
	}

	public static Quad2D Transform(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Matrix3x2 matrix)
	{
		return new Quad2D(
			Vector2.Transform(a, matrix),
			Vector2.Transform(b, matrix),
			Vector2.Transform(c, matrix),
			Vector2.Transform(d, matrix));
	}

	public static Quad2D Transform(Quad2D quad, Matrix3x2 matrix)
	{
		return new Quad2D(
			Vector2.Transform(quad._a, matrix),
			Vector2.Transform(quad._b, matrix),
			Vector2.Transform(quad._c, matrix),
			Vector2.Transform(quad._d, matrix));
	}

	public static bool operator ==(Quad2D a, Quad2D b) => a.a == b.a && a.b == b.b && a.c == b.c && a.d == b.d;

	public static bool operator !=(Quad2D a, Quad2D b) => a.a != b.a || a.b != b.b || a.c != b.c || a.d != b.d;
}
