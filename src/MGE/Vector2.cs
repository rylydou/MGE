using System;

namespace MGE
{
	// public class Vector2JsonConverter : JsonConverter<Vector2>
	// {
	// 	public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
	// 	{
	// 		var values = reader.ReadAsMultiDimensional<float>();
	// 		if (values.Length == 2) return new Vector2(values[0], values[1]);
	// 		throw new InvalidOperationException("Invalid Vector2");
	// 	}

	// 	public override void WriteJson(JsonWriter writer, Vector2 vector, JsonSerializer serializer)
	// 	{
	// 		writer.WriteValue($"{vector.x} {vector.y}");
	// 	}
	// }

	[Serializable]
	public struct Vector2 : IEquatable<Vector2>
	{
		public static readonly Vector2 zero = new Vector2(0, 0);
		public static readonly Vector2 one = new Vector2(1, 1);
		public static readonly Vector2 up = new Vector2(0, 1);
		public static readonly Vector2 down = new Vector2(0, -1);
		public static readonly Vector2 left = new Vector2(-1, 0);
		public static readonly Vector2 right = new Vector2(1, 0);
		public static readonly Vector2 positiveInfinity = new Vector2(float.PositiveInfinity);
		public static readonly Vector2 negativeInfinity = new Vector2(float.NegativeInfinity);

		////////////////////////////////////////////////////////////

		public static Vector2 FromAngle(float angle) => new Vector2(Math.Cos(angle), Math.Sin(angle));

		public static Vector2 Lerp(Vector2 current, Vector2 target, float time)
		{
			time = Math.Clamp01(time);
			return new Vector2(
				current.x + (target.x - current.x) * time,
				current.y + (target.y - current.y) * time
			);
		}

		public static Vector2 LerpUnclamped(Vector2 current, Vector2 target, float time)
		{
			return new Vector2(
				current.x + (target.x - current.x) * time,
				current.y + (target.y - current.y) * time
			);
		}

		public static Vector2 MoveTowards(Vector2 from, Vector2 to, float maxDistanceDelta)
		{
			var toVector_x = to.x - from.x;
			var toVector_y = to.y - from.y;

			var sqDist = toVector_x * toVector_x + toVector_y * toVector_y;

			if (sqDist == 0 || (maxDistanceDelta >= 0 && sqDist <= maxDistanceDelta * maxDistanceDelta)) return to;

			var dist = Math.Sqrt(sqDist);

			return new Vector2(
				from.x + toVector_x / dist * maxDistanceDelta,
				from.y + toVector_y / dist * maxDistanceDelta
			);
		}

		public static Vector2 Reflect(Vector2 inDirection, Vector2 inNormal)
		{
			var factor = -2f * Dot(inNormal, inDirection);
			return new Vector2(factor * inNormal.x + inDirection.x, factor * inNormal.y + inDirection.y);
		}

		public static Vector2 Perpendicular(Vector2 inDirection) => new Vector2(-inDirection.y, inDirection.x);

		public static float Dot(Vector2 left, Vector2 right) => left.x * right.x + left.y * right.y;

		public static float Angle(Vector2 from, Vector2 to)
		{
			var denominator = Math.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
			if (denominator < Math.epsilonSqrt)
				return 0;

			var dot = Math.Clamp(Dot(from, to) / denominator, -1.0f, 1.0f);
			return Math.Acos(dot) * Math.rad2Deg;
		}

		public static float SignedAngle(Vector2 from, Vector2 to)
		{
			var uangle = Angle(from, to);
			var sign = Math.Sign(from.x * to.y - from.y * to.x);
			return uangle * sign;
		}

		public static float DistanceSqr(Vector2 from, Vector2 to) => (from - to).sqrMagnitude;
		public static float Distance(Vector2 from, Vector2 to) => Math.Sqrt(DistanceSqr(from, to));

		public static bool DistanceEqualTo(Vector2 from, Vector2 to, float value) => Math.Approximately(DistanceSqr(from, to), value * value);
		public static bool DistanceLessThan(Vector2 from, Vector2 to, float value) => DistanceSqr(from, to) < value * value;
		public static bool DistanceGreaterThan(Vector2 from, Vector2 to, float value) => DistanceSqr(from, to) > value * value;

		public static Vector2 Clamp(Vector2 vector, float length) => new Vector2(Math.Clamp(vector.x, -length, length), Math.Clamp(vector.y, -length, length));
		public static Vector2 Clamp(Vector2 vector, Vector2 size) => new Vector2(Math.Clamp(vector.x, -size.x, size.x), Math.Clamp(vector.y, -size.y, size.y));

		public static Vector2 ClampMagnitude(Vector2 vector, float maxLength)
		{
			if (vector.sqrMagnitude > maxLength * maxLength)
				return vector.normalized * maxLength;
			return vector;
		}

		public static Vector2 GetDirection(Vector2 start, Vector2 end) => (start - end).normalized;

		public static Vector2 Min(Vector2 a, Vector2 b) => new Vector2(Math.Min(a.x, b.x), Math.Min(a.y, b.y));
		public static Vector2 Max(Vector2 a, Vector2 b) => new Vector2(Math.Max(a.x, b.x), Math.Max(a.y, b.y));

		////////////////////////////////////////////////////////////

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

		////////////////////////////////////////////////////////////

		public Vector2 normalized
		{
			get
			{
				var vector = new Vector2(x, y);
				vector.Normalize();
				return vector;
			}
		}

		public Vector2 sign { get => new Vector2(Math.Sign(x), Math.Sign(y)); }

		public Vector2 abs { get => new Vector2(Math.Abs(x), Math.Abs(y)); }

		public Vector2 isolateX { get => new Vector2(x, 0); }
		public Vector2 isolateY { get => new Vector2(0, y); }

		public float sqrMagnitude { get => x * x + y * y; }
		public float magnitude { get => Math.Sqrt(sqrMagnitude); }

		public float max { get => Math.Max(x, y); }
		public float min { get => Math.Min(x, y); }

		public float angle { get => Math.Atan2(y, x); }

		////////////////////////////////////////////////////////////

		public void Normalize()
		{
			var mag = magnitude;
			// if (mag <= 1.0f) return;
			if (mag > Math.epsilon)
				this = this / mag;
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

		////////////////////////////////////////////////////////////

		public static Vector2 operator +(Vector2 left, Vector2 right) => new Vector2(left.x + right.x, left.y + right.y);
		public static Vector2 operator -(Vector2 left, Vector2 right) => new Vector2(left.x - right.x, left.y - right.y);
		public static Vector2 operator *(Vector2 left, Vector2 right) => new Vector2(left.x * right.x, left.y * right.y);
		public static Vector2 operator /(Vector2 left, Vector2 right) => new Vector2(left.x / right.x, left.y / right.y);
		public static Vector2 operator -(Vector2 vector) => new Vector2(-vector.x, -vector.y);

		public static Vector2 operator +(Vector2 left, float right) => new Vector2(left.x + right, left.y + right);
		public static Vector2 operator -(Vector2 left, float right) => new Vector2(left.x - right, left.y - right);
		public static Vector2 operator *(Vector2 left, float right) => new Vector2(left.x * right, left.y * right);
		public static Vector2 operator *(float left, Vector2 right) => new Vector2(right.x * left, right.y * left);
		public static Vector2 operator /(Vector2 left, float right) => new Vector2(left.x / right, left.y / right);

		public static bool operator ==(Vector2 left, Vector2 right)
		{
			var diff_x = left.x - right.x;
			var diff_y = left.y - right.y;
			return Math.Abs(diff_x * diff_x + diff_y * diff_y) < Math.epsilonSqrt;
		}
		public static bool operator !=(Vector2 left, Vector2 right) => !(left == right);

		////////////////////////////////////////////////////////////

		public static implicit operator Vector2Int(Vector2 vector) => new Vector2Int((int)vector.x, (int)vector.y);
		public static implicit operator Vector2(Vector2Int vector) => new Vector2(vector.x, vector.y);

		public static implicit operator OpenTK.Mathematics.Vector2(Vector2 vector) => new OpenTK.Mathematics.Vector2(vector.x, vector.y);
		public static implicit operator Vector2(OpenTK.Mathematics.Vector2 vector) => new Vector2(vector.X, vector.Y);

		public static implicit operator OpenTK.Mathematics.Vector3(Vector2 vector) => new OpenTK.Mathematics.Vector3(vector.x, vector.y, 0);
		public static implicit operator Vector2(OpenTK.Mathematics.Vector3 vector) => new Vector2(vector.X, vector.Y);

		// public static implicit operator Vector2(Microsoft.Xna.Framework.Point vector) => new Vector2(vector.X, vector.Y);
		// public static implicit operator Microsoft.Xna.Framework.Point(Vector2 vector) => new Microsoft.Xna.Framework.Point((int)vector.x, (int)vector.y);

		////////////////////////////////////////////////////////////

		public override string ToString() => $"({x.ToString("F3")}, {y.ToString("F3")})";
		public string ToString(string format) => string.Format(format, x, y);

		public override int GetHashCode() => HashCode.Combine(x, y);

		public bool Equals(Vector2 other) => x == other.x && y == other.y;
		public override bool Equals(object? other) => other is Vector2 vector && Equals(vector);
	}
}
