using System;

namespace MGE;

[Serializable]
public struct Vector2Int : IEquatable<Vector2Int>
{
	public static readonly Vector2Int zero = new Vector2Int(0, 0);
	public static readonly Vector2Int one = new Vector2Int(1, 1);

	public static readonly Vector2Int up = new Vector2Int(0, -1);
	public static readonly Vector2Int down = new Vector2Int(0, 1);
	public static readonly Vector2Int left = new Vector2Int(-1, 0);
	public static readonly Vector2Int right = new Vector2Int(1, 0);

	////////////////////////////////////////////////////////////

	public static Vector2Int Scale(Vector2Int left, Vector2Int right) => new Vector2Int(left.x * right.x, left.y * right.y);

	public static Vector2Int Perpendicular(Vector2Int inDirection) => new Vector2Int(-inDirection.y, inDirection.x);

	public static int Dot(Vector2Int left, Vector2Int right) => left.x * right.x + left.y * right.y;

	public static int Distance(Vector2Int from, Vector2Int to)
	{
		var diff_x = from.x - to.x;
		var diff_y = from.y - to.y;
		return (int)Mathf.Sqrt(diff_x * diff_x + diff_y * diff_y);
	}

	public static Vector2Int Min(Vector2Int a, Vector2Int b) => new Vector2Int(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));
	public static Vector2Int Max(Vector2Int a, Vector2Int b) => new Vector2Int(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));

	////////////////////////////////////////////////////////////

	[Prop] public int x;
	[Prop] public int y;

	public Vector2Int(int value)
	{
		this.x = value;
		this.y = value;
	}

	public Vector2Int(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public int this[int index]
	{
		get
		{
			switch (index)
			{
				case 0: return x;
				case 1: return y;
				default: throw new IndexOutOfRangeException($"Invalid Vector2Int index of {index}");
			}
		}

		set
		{
			switch (index)
			{
				case 0: x = value; break;
				case 1: y = value; break;
				default: throw new IndexOutOfRangeException($"Invalid Vector2Int index of {index}");
			}
		}
	}

	public Vector2Int With(int index, int value) => index switch
	{
		0 => new(value, y),
		1 => new(x, value),
		_ => throw new ArgumentOutOfRangeException(nameof(index)),
	};

	////////////////////////////////////////////////////////////

	public Vector2Int sign { get => new Vector2Int(Mathf.Sign(x), Mathf.Sign(y)); }

	public int sqrMagnitude { get => x * x + y * y; }
	public float magnitude { get => Mathf.Sqrt(sqrMagnitude); }

	public int max { get => Mathf.Max(x, y); }
	public int min { get => Mathf.Min(x, y); }

	////////////////////////////////////////////////////////////

	public void Clamp(int max)
	{
		x = Mathf.Clamp(x, 0, max);
		y = Mathf.Clamp(y, 0, max);
	}

	public void Clamp(int min, int max)
	{
		x = Mathf.Clamp(x, min, max);
		y = Mathf.Clamp(y, min, max);
	}

	public void Clamp(int minX, int minY, int maxX, int maxY)
	{
		x = Mathf.Clamp(x, minX, maxX);
		y = Mathf.Clamp(y, minY, maxY);
	}

	public void Offset(int offsetX, int offsetY)
	{
		x += offsetX;
		y += offsetY;
	}

	public void Offset(Vector2Int offset)
	{
		x += offset.x;
		y += offset.y;
	}

	////////////////////////////////////////////////////////////

	public static Vector2Int operator +(Vector2Int left, Vector2Int right) => new(left.x + right.x, left.y + right.y);
	public static Vector2Int operator -(Vector2Int left, Vector2Int right) => new(left.x - right.x, left.y - right.y);
	public static Vector2Int operator *(Vector2Int left, Vector2Int right) => new(left.x * right.x, left.y * right.y);
	public static Vector2 operator /(Vector2Int left, Vector2Int right) => new((float)left.x / right.x, (float)left.y / right.y);

	public static Vector2Int operator -(Vector2Int vector) => new(-vector.x, -vector.y);

	public static Vector2Int operator *(Vector2Int left, int right) => new(left.x * right, left.y * right);
	public static Vector2Int operator *(int left, Vector2Int right) => new(right.x * left, right.y * left);
	public static Vector2 operator /(Vector2Int left, int right) => new((float)left.x / right, (float)left.y / right);

	public static Vector2 operator *(Vector2Int left, float right) => new(left.x * right, left.y * right);
	public static Vector2 operator *(float left, Vector2Int right) => new(right.x * left, right.y * left);
	public static Vector2 operator /(Vector2Int left, float right) => new(left.x / right, left.y / right);

	public static bool operator ==(Vector2Int left, Vector2Int right) => left.x == right.x && left.y == right.y;
	public static bool operator !=(Vector2Int lhs, Vector2Int rhs) => !(lhs == rhs);

	public static implicit operator Vector2Int(Vector2 vector) => new((int)vector.x, (int)vector.y);

	public static implicit operator System.Drawing.Point(Vector2Int vector) => new(vector.x, vector.y);
	public static implicit operator Vector2Int(System.Drawing.Point vector) => new(vector.X, vector.Y);

	public static implicit operator (int, int)(Vector2Int vector) => (vector.x, vector.y);
	public static implicit operator Vector2Int((int, int) vector) => new(vector.Item1, vector.Item2);

	////////////////////////////////////////////////////////////

	public override string ToString() => $"({x}, {y})";
	public string ToString(string format) => string.Format(format, x, y);

	public override int GetHashCode() => HashCode.Combine(x, y);

	public bool Equals(Vector2Int other) => x == other.x && y == other.y;
	public override bool Equals(object? other) => other is Vector2Int vector && Equals(vector);
}
