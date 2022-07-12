namespace MGE;

public struct Vector3
{
	#region Static

	public static readonly Vector3 zero = new Vector3(0f, 0f, 0f);
	public static readonly Vector3 one = new Vector3(1f, 1f, 1f);

	public static readonly Vector3 up = new Vector3(0f, 1f, 0f);
	public static readonly Vector3 down = new Vector3(0f, -1f, 0f);
	public static readonly Vector3 right = new Vector3(1f, 0f, 0f);
	public static readonly Vector3 left = new Vector3(-1f, 0f, 0f);

	public static readonly Vector3 forward = new Vector3(0f, 0f, -1f);
	public static readonly Vector3 backward = new Vector3(0f, 0f, 1f);

	public static float DistanceSquared(Vector3 value1, Vector3 value2)
	{
		return
			(value1.x - value2.x) * (value1.x - value2.x) +
			(value1.y - value2.y) * (value1.y - value2.y) +
			(value1.z - value2.z) * (value1.z - value2.z);
	}
	public static void DistanceSquared(ref Vector3 value1, ref Vector3 value2, out float result)
	{
		result =
			(value1.x - value2.x) * (value1.x - value2.x) +
			(value1.y - value2.y) * (value1.y - value2.y) +
			(value1.z - value2.z) * (value1.z - value2.z);
	}

	public static float Distance(Vector3 value1, Vector3 value2)
	{
		float result;
		DistanceSquared(ref value1, ref value2, out result);
		return Mathf.Sqrt(result);
	}

	public static void Distance(ref Vector3 value1, ref Vector3 value2, out float result)
	{
		DistanceSquared(ref value1, ref value2, out result);
		result = Mathf.Sqrt(result);
	}

	public static Vector3 Cross(Vector3 vector1, Vector3 vector2)
	{
		Cross(ref vector1, ref vector2, out vector1);
		return vector1;
	}

	public static void Cross(ref Vector3 vector1, ref Vector3 vector2, out Vector3 result)
	{
		var x = vector1.y * vector2.z - vector2.y * vector1.z;
		var y = -(vector1.x * vector2.z - vector2.x * vector1.z);
		var z = vector1.x * vector2.y - vector2.x * vector1.y;
		result.x = x;
		result.y = y;
		result.z = z;
	}

	public static float Dot(Vector3 value1, Vector3 value2) => value1.x * value2.x + value1.y * value2.y + value1.z * value2.z;
	public static void Dot(ref Vector3 value1, ref Vector3 value2, out float result) => result = value1.x * value2.x + value1.y * value2.y + value1.z * value2.z;

	public static Vector3 Normalize(Vector3 value)
	{
		float factor = Mathf.Sqrt(value.x * value.x + value.y * value.y + value.z * value.z);
		factor = 1f / factor;
		return new Vector3(value.x * factor, value.y * factor, value.z * factor);
	}

	public static void Normalize(ref Vector3 value, out Vector3 result)
	{
		var factor = Mathf.Sqrt(value.x * value.x + value.y * value.y + value.z * value.z);
		factor = 1f / factor;
		result.x = value.x * factor;
		result.y = value.y * factor;
		result.z = value.z * factor;
	}

	#endregion

	public float x;
	public float y;
	public float z;

	public Vector3(float x, float y, float z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public float sqrMagnitude { get => x * x + y * y + z * z; }
	public float magnitude { get => Mathf.Sqrt(sqrMagnitude); }

	public void Normalize()
	{
		float factor = Mathf.Sqrt(x * x + y * y + z * z);
		factor = 1f / factor;
		x *= factor;
		y *= factor;
		z *= factor;
	}

	public static Vector3 operator -(Vector3 value)
	{
		value = new Vector3(-value.x, -value.y, -value.z);
		return value;
	}

	public static Vector3 operator -(Vector3 value1, Vector3 value2)
	{
		value1.x -= value2.x;
		value1.y -= value2.y;
		value1.z -= value2.z;
		return value1;
	}
}
