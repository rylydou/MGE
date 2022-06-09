using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MGE;

[Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Vector2 : IEquatable<Vector2>
{
	const MethodImplOptions INLINE = MethodImplOptions.AggressiveInlining;

	#region Static

	#region Constants

	public static readonly Vector2 zero = new Vector2(0, 0);
	public static readonly Vector2 one = new Vector2(1, 1);

	public static readonly Vector2 down = new Vector2(0, 1);
	public static readonly Vector2 up = new Vector2(0, -1);
	public static readonly Vector2 left = new Vector2(-1, 0);
	public static readonly Vector2 right = new Vector2(1, 0);

	public static readonly Vector2 upLeft = new Vector2(-1, -1);
	public static readonly Vector2 upRight = new Vector2(1, -1);
	public static readonly Vector2 downLeft = new Vector2(-1, 1);
	public static readonly Vector2 downRight = new Vector2(1, 1);

	#endregion Constants

	#region Interpolation

	[MethodImpl(INLINE)] public static Vector2 Lerp(Vector2 current, Vector2 target, float time) => current + (target - current) * time;
	[MethodImpl(INLINE)] public static Vector2 LerpClamped(Vector2 current, Vector2 target, float time) => current + (target - current) * Mathf.Clamp01(time);

	[MethodImpl(INLINE)]
	public static Vector2 MoveTowards(Vector2 point, Vector2 target, float maxDistance)
	{
		var direction = target - point;
		var sqDist = direction.lengthSqr;

		if (sqDist == 0 || (maxDistance >= 0 && sqDist <= maxDistance * maxDistance)) return target;

		return point + direction / Mathf.Sqrt(sqDist) * maxDistance;
	}

	#endregion

	#region Utilities

	[MethodImpl(INLINE)] public static float Dot(Vector2 a, Vector2 b) => a.x * b.x + a.y * b.y;
	[MethodImpl(INLINE)] public static Vector2 PerpendicularDir(Vector2 inDir) => new(-inDir.y, inDir.x);
	[MethodImpl(INLINE)]
	public static Vector2 ReflectDir(Vector2 inDir, Vector2 inNormal)
	{
		var factor = -2 * Dot(inNormal, inDir);
		return new Vector2(factor * inNormal.x + inDir.x, factor * inNormal.y + inDir.y);
	}

	[MethodImpl(INLINE)] public static float DistanceSqr(Vector2 from, Vector2 to) => (from - to).lengthSqr;
	[MethodImpl(INLINE)] public static float Distance(Vector2 from, Vector2 to) => Mathf.Sqrt(DistanceSqr(from, to));

	[MethodImpl(INLINE)] public static bool DistanceEqualTo(Vector2 from, Vector2 to, float value) => Mathf.Approximately(DistanceSqr(from, to), value * value);
	[MethodImpl(INLINE)] public static bool DistanceLessThan(Vector2 from, Vector2 to, float value) => DistanceSqr(from, to) < value * value;
	[MethodImpl(INLINE)] public static bool DistanceGreaterThan(Vector2 from, Vector2 to, float value) => DistanceSqr(from, to) > value * value;

	[MethodImpl(INLINE)] public static Vector2 FromTo(Vector2 from, Vector2 to) => to - from;
	[MethodImpl(INLINE)] public static Vector2 Midpoint(Vector2 a, Vector2 b) => (a + b) / 2;
	[MethodImpl(INLINE)] public static Vector2 CenterDirection(Vector2 a, Vector2 b) => (a + b).normalized;
	[MethodImpl(INLINE)] public static Vector2 Direction(Vector2 point, Vector2 target) => (point - target).normalized;
	[MethodImpl(INLINE)] public static Vector2 FromAngle(float angle) => new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

	[MethodImpl(INLINE)] public static float AngleTo(Vector2 point, Vector2 target) => Direction(point, target).angle;
	[MethodImpl(INLINE)] public static float SignedAngle(Vector2 a, Vector2 b) => AngleBetween(a, b) * Mathf.Sign(Dot(a, b));
	[MethodImpl(INLINE)] public static float AngleBetween(Vector2 a, Vector2 b) => Mathf.Acos(Mathf.ClampUnit(Vector2.Dot(a.normalized, b.normalized)));
	[MethodImpl(INLINE)] public static float AngleFromToCW(Vector2 from, Vector2 to) => Dot(from, to) < 0f ? AngleBetween(from, to) : Mathf.TAU - AngleBetween(from, to);
	[MethodImpl(INLINE)] public static float AngleFromToCCW(Vector2 from, Vector2 to) => Dot(from, to) > 0f ? AngleBetween(from, to) : Mathf.TAU - AngleBetween(from, to);

	[MethodImpl(INLINE)]
	public static Vector2 RotateAroundPoint(Vector2 center, Vector2 point, float rotationRad)
	{
		var cosTheta = Mathf.Cos(rotationRad);
		var sinTheta = Mathf.Sin(rotationRad);

		return new(
			cosTheta * (point.x - center.x) - sinTheta * (point.y - center.y) + center.x,
			sinTheta * (point.x - center.x) + cosTheta * (point.y - center.y) + center.y
		);
	}

	[MethodImpl(INLINE)]
	public static Vector2 RotateAroundPoint(Vector2 point, float rotationRad)
	{
		var cosTheta = Mathf.Cos(rotationRad);
		var sinTheta = Mathf.Sin(rotationRad);

		return new(
			cosTheta * point.x - sinTheta * point.y,
			sinTheta * point.x + cosTheta * point.y
		);
	}

	[MethodImpl(INLINE)]
	public static Vector2 ClampLength(Vector2 vector, float length)
	{
		if (vector.lengthSqr > length * length)
			return vector.normalized * length;
		return vector;
	}

	[MethodImpl(INLINE)]
	public static Vector2 Transform(Vector2 position, Transform2D transform) => new Vector2(
		(position.x * transform.x.x) + (position.y * transform.y.x) + transform.origin.x,
		(position.x * transform.x.y) + (position.y * transform.y.y) + transform.origin.y
	);

	[MethodImpl(INLINE)]
	public static Vector2 Transform(Vector2 position, Matrix3x2 matrix) => new Vector2(
		(position.x * matrix.M11) + (position.y * matrix.M21) + matrix.M31,
		(position.x * matrix.M12) + (position.y * matrix.M22) + matrix.M32
	);

	[MethodImpl(INLINE)]
	public static Vector2 Transform(Vector2 position, Matrix4x4 matrix) => new(
		position.x * matrix.M11 + position.y * matrix.M21 + matrix.M41,
		position.x * matrix.M12 + position.y * matrix.M22 + matrix.M42
	);

	[MethodImpl(INLINE)]
	public static Vector2 Transform(Vector2 value, Quaternion rotation)
	{
		var x2 = rotation.X + rotation.X;
		var y2 = rotation.Y + rotation.Y;
		var z2 = rotation.Z + rotation.Z;

		var wz2 = rotation.W * z2;
		var xx2 = rotation.X * x2;
		var xy2 = rotation.X * y2;
		var yy2 = rotation.Y * y2;
		var zz2 = rotation.Z * z2;

		return new Vector2(
			value.x * (1.0f - yy2 - zz2) + value.y * (xy2 - wz2),
			value.x * (xy2 + wz2) + value.y * (1.0f - xx2 - zz2)
		);
	}

	[MethodImpl(INLINE)]
	public static Vector2 TransformNormal(Vector2 position, Matrix4x4 matrix) => new(
		position.x * matrix.M11 + position.y * matrix.M21,
		position.x * matrix.M12 + position.y * matrix.M22
	);

	[MethodImpl(INLINE)]
	public static Vector2 TransformNormal(Vector2 normal, Matrix3x2 matrix) => new Vector2(
		(normal.x * matrix.M11) + (normal.y * matrix.M21),
		(normal.x * matrix.M12) + (normal.y * matrix.M22)
	);

	[MethodImpl(INLINE)]
	public static Vector2 TransformNormal(Vector2 normal, Transform2D transform) => new Vector2(
		(normal.x * transform.x.x) + (normal.y * transform.y.x),
		(normal.x * transform.x.y) + (normal.y * transform.y.y)
	);

	#endregion Utilities

	#region Conversion

	[MethodImpl(INLINE)] public static implicit operator Vector2(Vector2Int vector) => new(vector.x, vector.y);

	[MethodImpl(INLINE)] public static implicit operator (float, float)(Vector2 vector) => (vector.x, vector.y);
	[MethodImpl(INLINE)] public static implicit operator Vector2((float, float) vector) => new(vector.Item1, vector.Item2);

	#region Thirdparty

	[MethodImpl(INLINE)] public static implicit operator System.Numerics.Vector2(Vector2 vector) => new(vector.x, vector.y);
	[MethodImpl(INLINE)] public static implicit operator Vector2(System.Numerics.Vector2 vector) => new(vector.X, vector.Y);

	#endregion Thirdparty

	#endregion Conversion

	#region Operators

	[MethodImpl(INLINE)] public static Vector2 operator +(Vector2 left, Vector2 right) => new(left.x + right.x, left.y + right.y);
	[MethodImpl(INLINE)] public static Vector2 operator -(Vector2 left, Vector2 right) => new(left.x - right.x, left.y - right.y);
	[MethodImpl(INLINE)] public static Vector2 operator *(Vector2 left, Vector2 right) => new(left.x * right.x, left.y * right.y);
	[MethodImpl(INLINE)] public static Vector2 operator /(Vector2 left, Vector2 right) => new(left.x / right.x, left.y / right.y);

	[MethodImpl(INLINE)] public static Vector2 operator -(Vector2 vector) => new(-vector.x, -vector.y);

	[MethodImpl(INLINE)] public static Vector2 operator +(Vector2 left, float right) => new(left.x + right, left.y + right);
	[MethodImpl(INLINE)] public static Vector2 operator -(Vector2 left, float right) => new(left.x - right, left.y - right);
	[MethodImpl(INLINE)] public static Vector2 operator *(Vector2 left, float right) => new(left.x * right, left.y * right);
	[MethodImpl(INLINE)] public static Vector2 operator *(float left, Vector2 right) => new(right.x * left, right.y * left);
	[MethodImpl(INLINE)] public static Vector2 operator /(Vector2 left, float right) => new(left.x / right, left.y / right);

	[MethodImpl(INLINE)]
	public static bool operator ==(Vector2 left, Vector2 right)
	{
		var diff_x = left.x - right.x;
		var diff_y = left.y - right.y;
		return Mathf.Abs(diff_x * diff_x + diff_y * diff_y) < Mathf.EPSILON_SQRT;
	}
	[MethodImpl(INLINE)] public static bool operator !=(Vector2 left, Vector2 right) => !(left == right);

	#endregion

	#endregion Static

	#region Instance

	[Prop] public float x;
	[Prop] public float y;

	public Vector2(float value)
	{
		this.x = value;
		this.y = value;
	}

	public Vector2(float x, float y)
	{
		this.x = x;
		this.y = y;
	}

	public float this[int index]
	{
		[MethodImpl(INLINE)]
		get
		{
			switch (index)
			{
				case 0: return x;
				case 1: return y;
				default: throw new IndexOutOfRangeException($"Invalid {nameof(Vector2)} index of {index}!");
			}
		}
		[MethodImpl(INLINE)]
		set
		{
			switch (index)
			{
				case 0: x = value; break;
				case 1: y = value; break;
				default: throw new IndexOutOfRangeException($"Invalid {nameof(Vector2)} index of {index}!");
			}
		}
	}

	[MethodImpl(INLINE)]
	public Vector2 With(int index, float value) => index switch
	{
		0 => new(value, y),
		1 => new(x, value),
		_ => throw new ArgumentOutOfRangeException(nameof(index)),
	};

	public Vector2 normalized
	{
		[MethodImpl(INLINE)]
		get
		{
			var vector = new Vector2(x, y);
			vector.Normalize();
			return vector;
		}
	}

	public Vector2 sign
	{
		[MethodImpl(INLINE)]
		get
		{
			return new Vector2(Mathf.Sign(x), Mathf.Sign(y));
		}
	}

	public Vector2 abs
	{
		[MethodImpl(INLINE)]
		get
		{
			return new Vector2(Mathf.Abs(x), Mathf.Abs(y));
		}
	}

	public float lengthSqr
	{
		[MethodImpl(INLINE)]
		get
		{
			return x * x + y * y;
		}
	}

	public float length
	{
		[MethodImpl(INLINE)]
		get
		{
			return Mathf.Sqrt(lengthSqr);
		}
	}

	public float angle
	{
		[MethodImpl(INLINE)]
		get
		{
			return Mathf.Atan2(y, x);
		}
	}

	public Vector2 turnRight => new Vector2(-y, x);
	public Vector2 turnLeft => new Vector2(y, -x);

	#region Methods

	[MethodImpl(INLINE)]
	public void Normalize()
	{
		var len = length;
		if (len > Mathf.EPSILON)
			this = this / len;
		else
			this = zero;
	}

	[MethodImpl(INLINE)]
	public void Clamp(float max)
	{
		x = Mathf.Clamp(x, 0, max);
		y = Mathf.Clamp(y, 0, max);
	}

	[MethodImpl(INLINE)]
	public void Clamp(float min, float max)
	{
		x = Mathf.Clamp(x, min, max);
		y = Mathf.Clamp(y, min, max);
	}

	[MethodImpl(INLINE)]
	public void Clamp(float minX, float minY, float maxX, float maxY)
	{
		x = Mathf.Clamp(x, minX, maxX);
		y = Mathf.Clamp(y, minY, maxY);
	}

	[MethodImpl(INLINE)]
	public void Offset(float offsetX, float offsetY)
	{
		x += offsetX;
		y += offsetY;
	}

	[MethodImpl(INLINE)]
	public void Offset(Vector2 offset)
	{
		x += offset.x;
		y += offset.y;
	}

	[MethodImpl(INLINE)]
	public Vector2 Snap()
	{
		return new(Mathf.Round(x), Mathf.Round(y));
	}

	#endregion Methods

	#region Overrides

	[MethodImpl(INLINE)] public override string ToString() => $"({x:N2}, {y:N2})";
	[MethodImpl(INLINE)] public string ToString(string format) => string.Format(format, x, y);

	[MethodImpl(INLINE)] public override int GetHashCode() => HashCode.Combine(x, y);

	[MethodImpl(INLINE)] public bool Equals(Vector2 other) => x == other.x && y == other.y;
	[MethodImpl(INLINE)] public override bool Equals(object? other) => other is Vector2 vector && Equals(vector);

	#endregion Overloads

	#endregion Instance
}
