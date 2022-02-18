using System;
using System.Numerics;

namespace MGE;

/// <summary>
/// A 2D Quad
/// </summary>
public struct Quad2D : IConvexShape2D
{
	Vector2 a;
	Vector2 b;
	Vector2 c;
	Vector2 d;
	Vector2 normalAB;
	Vector2 normalBC;
	Vector2 normalCD;
	Vector2 normalDA;
	bool dirty;

	public Vector2 A
	{
		get => a;
		set
		{
			if (a != value)
			{
				a = value;
				dirty = true;
			}
		}
	}

	public Vector2 B
	{
		get => b;
		set
		{
			if (b != value)
			{
				b = value;
				dirty = true;
			}
		}
	}

	public Vector2 C
	{
		get => c;
		set
		{
			if (c != value)
			{
				c = value;
				dirty = true;
			}
		}
	}

	public Vector2 D
	{
		get => d;
		set
		{
			if (d != value)
			{
				d = value;
				dirty = true;
			}
		}
	}

	public Vector2 NormalAB
	{
		get
		{
			if (dirty)
				UpdateQuad();
			return normalAB;
		}
	}

	public Vector2 NormalBC
	{
		get
		{
			if (dirty)
				UpdateQuad();
			return normalBC;
		}
	}

	public Vector2 NormalCD
	{
		get
		{
			if (dirty)
				UpdateQuad();
			return normalCD;
		}
	}

	public Vector2 NormalDA
	{
		get
		{
			if (dirty)
				UpdateQuad();
			return normalDA;
		}
	}

	public Vector2 Center => (a + b + c + d) / 4f;


	public Quad2D(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
	{
		this.a = a;
		this.b = b;
		this.c = c;
		this.d = d;
		normalAB = normalBC = normalCD = normalDA = Vector2.zero;
		dirty = true;
	}

	void UpdateQuad()
	{
		normalAB = (b - a).normalized;
		normalAB = new Vector2(-normalAB.y, normalAB.x);
		normalBC = (c - b).normalized;
		normalBC = new Vector2(-normalBC.y, normalBC.x);
		normalCD = (d - c).normalized;
		normalCD = new Vector2(-normalCD.y, normalCD.x);
		normalDA = (a - d).normalized;
		normalDA = new Vector2(-normalDA.y, normalDA.x);

		dirty = false;
	}

	public Quad2D Translate(in Vector2 amount)
	{
		A += amount;
		B += amount;
		C += amount;
		D += amount;
		return this;
	}

	public void Project(in Vector2 axis, out float min, out float max)
	{
		min = float.MaxValue;
		max = float.MinValue;

		var dot = Vector2.Dot(A, axis);
		min = Math.Min(dot, min);
		max = Math.Max(dot, max);
		dot = Vector2.Dot(B, axis);
		min = Math.Min(dot, min);
		max = Math.Max(dot, max);
		dot = Vector2.Dot(C, axis);
		min = Math.Min(dot, min);
		max = Math.Max(dot, max);
		dot = Vector2.Dot(D, axis);
		min = Math.Min(dot, min);
		max = Math.Max(dot, max);
	}

	public int Points => 4;

	public Vector2 GetPoint(int index)
	{
		return index switch
		{
			0 => A,
			1 => B,
			2 => C,
			3 => D,
			_ => throw new IndexOutOfRangeException(),
		};
	}

	public int Axis => 4;

	public Vector2 GetAxis(int index)
	{
		return index switch
		{
			0 => new Vector2(-normalAB.y, normalAB.x),
			1 => new Vector2(-normalBC.y, normalBC.x),
			2 => new Vector2(-normalCD.y, normalCD.x),
			3 => new Vector2(-normalDA.y, normalDA.x),
			_ => throw new IndexOutOfRangeException(),
		};
	}

	public Rect BoundingRect()
	{
		var bounds = new Rect();
		bounds.x = Math.Min(a.x, Math.Min(b.x, Math.Min(c.x, d.x)));
		bounds.y = Math.Min(a.y, Math.Min(b.y, Math.Min(c.y, d.y)));
		bounds.width = Math.Max(a.x, Math.Max(b.x, Math.Max(c.x, d.x))) - bounds.x;
		bounds.height = Math.Max(a.y, Math.Max(b.y, Math.Max(c.y, d.y))) - bounds.y;
		return bounds;
	}

	public override bool Equals(object? obj) => (obj is Quad2D other) && (this == other);

	public override int GetHashCode()
	{
		int hash = 17;
		hash = hash * 23 + A.GetHashCode();
		hash = hash * 23 + B.GetHashCode();
		hash = hash * 23 + C.GetHashCode();
		hash = hash * 23 + D.GetHashCode();
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
			Vector2.Transform(quad.a, matrix),
			Vector2.Transform(quad.b, matrix),
			Vector2.Transform(quad.c, matrix),
			Vector2.Transform(quad.d, matrix));
	}

	public static bool operator ==(Quad2D a, Quad2D b) => a.A == b.A && a.B == b.B && a.C == b.C && a.D == b.D;

	public static bool operator !=(Quad2D a, Quad2D b) => a.A != b.A || a.B != b.B || a.C != b.C || a.D != b.D;
}
