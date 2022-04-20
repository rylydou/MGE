using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MGE;

[Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Vector2 : IEquatable<Vector2>
{
	#region Constants

	public static readonly Vector2 zero = new Vector2(0, 0);
	public static readonly Vector2 one = new Vector2(1, 1);

	public static readonly Vector2 down = new Vector2(0, 1);
	public static readonly Vector2 up = new Vector2(0, -1);
	public static readonly Vector2 left = new Vector2(-1, 0);
	public static readonly Vector2 right = new Vector2(1, 0);

	public static readonly Vector2 upLeft = new Vector2(-1, 1);
	public static readonly Vector2 upRight = new Vector2(1, 1);
	public static readonly Vector2 downLeft = new Vector2(-1, -1);
	public static readonly Vector2 downRight = new Vector2(1, -1);

	#endregion Constants

	#region Static

	#region Interpolation

	public static Vector2 Lerp(Vector2 current, Vector2 target, float time) => current + (target - current) * time;
	public static Vector2 LerpClamped(Vector2 current, Vector2 target, float time) => current + (target - current) * Math.Clamp01(time);

	public static Vector2 MoveTowards(Vector2 point, Vector2 target, float maxDistance)
	{
		var direction = target - point;
		var sqDist = direction.lengthSqr;

		if (sqDist == 0 || (maxDistance >= 0 && sqDist <= maxDistance * maxDistance)) return target;

		return point + direction / Math.Sqrt(sqDist) * maxDistance;
	}

	#endregion

	public static float Dot(Vector2 a, Vector2 b) => a.x * b.x + a.y * b.y;
	public static Vector2 PerpendicularDir(Vector2 inDir) => new(-inDir.y, inDir.x);
	public static Vector2 ReflectDir(Vector2 inDir, Vector2 inNormal)
	{
		var factor = -2 * Dot(inNormal, inDir);
		return new Vector2(factor * inNormal.x + inDir.x, factor * inNormal.y + inDir.y);
	}

	public static float DistanceSqr(Vector2 from, Vector2 to) => (from - to).lengthSqr;
	public static float Distance(Vector2 from, Vector2 to) => Math.Sqrt(DistanceSqr(from, to));

	public static bool DistanceEqualTo(Vector2 from, Vector2 to, float value) => Math.Approximately(DistanceSqr(from, to), value * value);
	public static bool DistanceLessThan(Vector2 from, Vector2 to, float value) => DistanceSqr(from, to) < value * value;
	public static bool DistanceGreaterThan(Vector2 from, Vector2 to, float value) => DistanceSqr(from, to) > value * value;

	public static Vector2 FromTo(Vector2 from, Vector2 to) => to - from;
	public static Vector2 Midpoint(Vector2 a, Vector2 b) => (a + b) / 2;
	public static Vector2 CenterDirection(Vector2 a, Vector2 b) => (a + b).normalized;
	public static Vector2 Direction(Vector2 point, Vector2 target) => (point - target).normalized;
	public static Vector2 FromAngle(float angle) => new Vector2(Math.Cos(angle), Math.Sin(angle));

	public static float AngleTo(Vector2 point, Vector2 target) => Direction(point, target).angle;
	public static float SignedAngle(Vector2 a, Vector2 b) => AngleBetween(a, b) * Math.Sign(Dot(a, b));
	public static float AngleBetween(Vector2 a, Vector2 b) => Math.Acos(Math.ClampUnit(Vector2.Dot(a.normalized, b.normalized)));
	public static float AngleFromToCW(Vector2 from, Vector2 to) => Dot(from, to) < 0f ? AngleBetween(from, to) : Math.tau - AngleBetween(from, to);
	public static float AngleFromToCCW(Vector2 from, Vector2 to) => Dot(from, to) > 0f ? AngleBetween(from, to) : Math.tau - AngleBetween(from, to);

	public static Vector2 RotateAroundPoint(Vector2 center, Vector2 point, float rotationRad)
	{
		var cosTheta = Math.Cos(rotationRad);
		var sinTheta = Math.Sin(rotationRad);

		return new(
			cosTheta * (point.x - center.x) - sinTheta * (point.y - center.y) + center.x,
			sinTheta * (point.x - center.x) + cosTheta * (point.y - center.y) + center.y
		);
	}

	public static Vector2 RotateAroundPoint(Vector2 point, float rotationRad)
	{
		var cosTheta = Math.Cos(rotationRad);
		var sinTheta = Math.Sin(rotationRad);

		return new(
			cosTheta * point.x - sinTheta * point.y,
			sinTheta * point.x + cosTheta * point.y
		);
	}

	public static Vector2 ClampLength(Vector2 vector, float length)
	{
		if (vector.lengthSqr > length * length)
			return vector.normalized * length;
		return vector;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Transform(Vector2 position, Transform2D transform) => new Vector2(
		(position.x * transform.x.x) + (position.y * transform.y.x) + transform.origin.x,
		(position.x * transform.x.y) + (position.y * transform.y.y) + transform.origin.y
	);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Transform(Vector2 position, Matrix3x2 matrix) => new Vector2(
		(position.x * matrix.M11) + (position.y * matrix.M21) + matrix.M31,
		(position.x * matrix.M12) + (position.y * matrix.M22) + matrix.M32
	);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Transform(Vector2 position, Matrix4x4 matrix) => new(
		position.x * matrix.M11 + position.y * matrix.M21 + matrix.M41,
		position.x * matrix.M12 + position.y * matrix.M22 + matrix.M42
	);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]

	public static Vector2 TransformNormal(Vector2 position, Matrix4x4 matrix) => new(
		position.x * matrix.M11 + position.y * matrix.M21,
		position.x * matrix.M12 + position.y * matrix.M22
	);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 TransformNormal(Vector2 normal, Matrix3x2 matrix) => new Vector2(
		(normal.x * matrix.M11) + (normal.y * matrix.M21),
		(normal.x * matrix.M12) + (normal.y * matrix.M22)
	);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 TransformNormal(Vector2 normal, Transform2D transform) => new Vector2(
		(normal.x * transform.x.x) + (normal.y * transform.y.x),
		(normal.x * transform.x.y) + (normal.y * transform.y.y)
	);

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
		get
		{
			switch (index)
			{
				case 0: return x;
				case 1: return y;
				default: throw new IndexOutOfRangeException($"Invalid Vector2 index of {index}!");
			}
		}

		set
		{
			switch (index)
			{
				case 0: x = value; break;
				case 1: y = value; break;
				default: throw new IndexOutOfRangeException($"Invalid Vector2 index of {index}!");
			}
		}
	}

	public Vector2 With(int index, float value) => index switch
	{
		0 => new(value, y),
		1 => new(x, value),
		_ => throw new ArgumentOutOfRangeException(nameof(index)),
	};

	public Vector2 normalized
	{
		get
		{
			var vector = new Vector2(x, y);
			vector.Normalize();
			return vector;
		}
	}

	public Vector2 sign => new Vector2(Math.Sign(x), Math.Sign(y));

	public Vector2 abs => new Vector2(Math.Abs(x), Math.Abs(y));

	// public Vector2 xVector => new Vector2(x, 0);
	// public Vector2 yVector => new Vector2(0, y);

	public float lengthSqr => x * x + y * y;
	public float length => Math.Sqrt(lengthSqr);

	public float angle => Math.Atan2(y, x);

	public Vector2 turnRight => new Vector2(-y, x);
	public Vector2 turnLeft => new Vector2(y, -x);

	public void Normalize()
	{
		var len = length;
		// if (len <= 1f) return;
		if (len > Math.epsilon)
			this = this / len;
		else
			this = zero;
	}

	public void Clamp(float max)
	{
		x = Math.Clamp(x, 0, max);
		y = Math.Clamp(y, 0, max);
	}

	public void Clamp(float min, float max)
	{
		x = Math.Clamp(x, min, max);
		y = Math.Clamp(y, min, max);
	}

	public void Clamp(float minX, float minY, float maxX, float maxY)
	{
		x = Math.Clamp(x, minX, maxX);
		y = Math.Clamp(y, minY, maxY);
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

	public Vector2 Snap()
	{
		return new(Math.Round(x), Math.Round(y));
	}

	#region Operators

	public static Vector2 operator +(Vector2 left, Vector2 right) => new(left.x + right.x, left.y + right.y);
	public static Vector2 operator -(Vector2 left, Vector2 right) => new(left.x - right.x, left.y - right.y);
	public static Vector2 operator *(Vector2 left, Vector2 right) => new(left.x * right.x, left.y * right.y);
	public static Vector2 operator /(Vector2 left, Vector2 right) => new(left.x / right.x, left.y / right.y);

	public static Vector2 operator -(Vector2 vector) => new(-vector.x, -vector.y);

	public static Vector2 operator +(Vector2 left, float right) => new(left.x + right, left.y + right);
	public static Vector2 operator -(Vector2 left, float right) => new(left.x - right, left.y - right);
	public static Vector2 operator *(Vector2 left, float right) => new(left.x * right, left.y * right);
	public static Vector2 operator *(float left, Vector2 right) => new(right.x * left, right.y * left);
	public static Vector2 operator /(Vector2 left, float right) => new(left.x / right, left.y / right);

	public static bool operator ==(Vector2 left, Vector2 right)
	{
		var diff_x = left.x - right.x;
		var diff_y = left.y - right.y;
		return Math.Abs(diff_x * diff_x + diff_y * diff_y) < Math.epsilonSqrt;
	}
	public static bool operator !=(Vector2 left, Vector2 right) => !(left == right);

	#endregion

	#region Conversion

	public static implicit operator Vector2(Vector2Int vector) => new(vector.x, vector.y);

	public static implicit operator (float, float)(Vector2 vector) => (vector.x, vector.y);
	public static implicit operator Vector2((float, float) vector) => new(vector.Item1, vector.Item2);

	#region Thirdparty

	public static implicit operator global::System.Numerics.Vector2(Vector2 vector) => new(vector.x, vector.y);
	public static implicit operator Vector2(global::System.Numerics.Vector2 vector) => new(vector.X, vector.Y);

	// public static implicit operator OpenTK.Mathematics.Vector2(Vector2 vector) => new(vector.x, vector.y);
	// public static implicit operator Vector2(OpenTK.Mathematics.Vector2 vector) => new(vector.X, vector.Y);

	// public static implicit operator OpenTK.Mathematics.Vector3(Vector2 vector) => new(vector.x, vector.y, 0);
	// public static implicit operator Vector2(OpenTK.Mathematics.Vector3 vector) => new(vector.X, vector.Y);

	// public static implicit operator tainicom.Aether.Physics2D.Common.Vector2(Vector2 vector) => new(vector.x, vector.y);
	// public static implicit operator Vector2(tainicom.Aether.Physics2D.Common.Vector2 vector) => new(vector.X, vector.Y);

	// public static implicit operator tainicom.Aether.Physics2D.Common.Vector3(Vector2 vector) => new(vector.x, vector.y, 0);
	// public static implicit operator Vector2(tainicom.Aether.Physics2D.Common.Vector3 vector) => new(vector.X, vector.Y);

	#endregion Thirdparty

	#endregion Conversion

	#region Overrides

	public override string ToString() => $"({x:N2}, {y:N2})";
	public string ToString(string format) => string.Format(format, x, y);

	public override int GetHashCode() => HashCode.Combine(x, y);

	public bool Equals(Vector2 other) => x == other.x && y == other.y;
	public override bool Equals(object? other) => other is Vector2 vector && Equals(vector);

	#endregion Overloads

	#endregion Instance
}
