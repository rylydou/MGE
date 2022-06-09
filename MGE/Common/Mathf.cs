using System;
using System.Linq;
using System.Runtime.CompilerServices;
using MathD = System.Math;
using MathF = System.MathF;

namespace MGE;

public static class Mathf
{
	const MethodImplOptions INLINE = MethodImplOptions.AggressiveInlining;

	#region Constants

	public const float E = MathF.E;
	public const float LOG_10_E = 0.4342945f;
	public const float LOG_2_E = 1.442695f;
	public const float SQRT_OF_2 = 1.41421356237f;
	public const float GOLDEN_RATIO = 1.61803398875f;

	public const float PI = MathF.PI;
	public const float TAU = PI * 2;

	public const float POSITIVE_INFINITY = float.PositiveInfinity;
	public const float NEGATIVE_INFINITY = float.NegativeInfinity;

	public const float DEG_2_RAD = TAU / 360f;
	public const float RAD_2_DEG = 360f / TAU;

	public const float EPSILON = float.Epsilon;
	public const float EPSILON_SQRT = float.Epsilon * float.Epsilon;

	#endregion

	#region Operations

	[MethodImpl(INLINE)] public static float Abs(float value) => MathF.Abs(value);
	[MethodImpl(INLINE)] public static int Abs(int value) => MathD.Abs(value);

	[MethodImpl(INLINE)] public static float Sqrt(float value) => MathF.Sqrt(value);
	/// <summary>Returns the cube root of the given value, properly handling negative values unlike Pow(v,1/3)</summary>
	[MethodImpl(INLINE)] public static float Cbrt(float value) => value < 0 ? -Pow(-value, 1f / 3f) : Pow(value, 1f / 3f);

	[MethodImpl(INLINE)] public static float Pow(float @base, float power) => MathF.Pow(@base, power);
	[MethodImpl(INLINE)] public static float Exp(float power) => MathF.Exp(power);

	[MethodImpl(INLINE)] public static float Log(float value, float @base) => MathF.Log(value, @base);
	[MethodImpl(INLINE)] public static float Log(float value) => MathF.Log(value);
	[MethodImpl(INLINE)] public static float Log10(float value) => MathF.Log10(value);

	[MethodImpl(INLINE)] public static bool Approximately(float a, float b) => Abs(b - a) < Max(0.000001f * Max(Abs(a), Abs(b)), EPSILON * 8);

	/// <summary>Returns the binomial coefficient n over k</summary>
	[MethodImpl(INLINE)]
	public static ulong BinomialCoef(uint n, uint k)
	{
		// source: https://blog.plover.com/math/choose.html
		ulong r = 1;
		if (k > n) return 0;
		for (ulong d = 1; d <= k; d++)
		{
			r *= n--;
			r /= d;
		}

		return r;
		// mathematically clean but extremely prone to overflow
		//return Factorial( n ) / ( Factorial( k ) * Factorial( n - k ) );
	}

	/// <summary>Returns the Factorial of a given value from 0 to 12</summary>
	/// <param name="value">A value between 0 and 12 (integers can't store the factorial of 13 or above)</param>
	[MethodImpl(INLINE)]
	public static int Factorial(uint value)
	{
		if (value <= 12)
			return factorialInt[value];
		if (value <= 20)
			throw new OverflowException($"The Factorial of {value} is too big for integer representation, please use {nameof(FactorialLong)} instead");
		throw new OverflowException($"The Factorial of {value} is too big for integer representation");
	}

	/// <summary>Returns the Factorial of a given value from 0 to 20</summary>
	/// <param name="value">A value between 0 and 20 (neither long nor ulong can store values large enough for the factorial of 21)</param>
	[MethodImpl(INLINE)]
	public static long FactorialLong(uint value)
	{
		if (value <= 20)
			return factorialLong[value];
		throw new OverflowException($"The Factorial of {value} is too big for integer representation, even unsigned longs, soooo, rip");
	}

	static readonly long[] factorialLong = {
		/*0*/ 1,
		/*1*/ 1,
		/*2*/ 2,
		/*3*/ 6,
		/*4*/ 24,
		/*5*/ 120,
		/*6*/ 720,
		/*7*/ 5040,
		/*8*/ 40320,
		/*9*/ 362880,
		/*10*/ 3628800,
		/*11*/ 39916800,
		/*12*/ 479001600,
		/*13*/ 6227020800,
		/*14*/ 87178291200,
		/*15*/ 1307674368000,
		/*16*/ 20922789888000,
		/*17*/ 355687428096000,
		/*18*/ 6402373705728000,
		/*19*/ 121645100408832000,
		/*20*/ 2432902008176640000,
	};

	static readonly int[] factorialInt = {
		/*0*/ 1,
		/*1*/ 1,
		/*2*/ 2,
		/*3*/ 6,
		/*4*/ 24,
		/*5*/ 120,
		/*6*/ 720,
		/*7*/ 5040,
		/*8*/ 40320,
		/*9*/ 362880,
		/*10*/ 3628800,
		/*11*/ 39916800,
		/*12*/ 479001600,
	};

	#endregion Operations

	#region Trig Functions

	[MethodImpl(INLINE)] public static float Sin(float value) => MathF.Sin(value);
	[MethodImpl(INLINE)] public static float Asin(float value) => MathF.Asin(value);
	[MethodImpl(INLINE)] public static float Cos(float value) => MathF.Cos(value);
	[MethodImpl(INLINE)] public static float Acos(float value) => MathF.Acos(value);
	[MethodImpl(INLINE)] public static float Tan(float value) => MathF.Tan(value);
	[MethodImpl(INLINE)] public static float Atan(float value) => MathF.Atan(value);
	[MethodImpl(INLINE)] public static float Atan2(float y, float x) => MathF.Atan2(y, x);
	[MethodImpl(INLINE)] public static float Csc(float x) => 1f / MathF.Sin(x);
	[MethodImpl(INLINE)] public static float Sec(float x) => 1f / MathF.Cos(x);
	[MethodImpl(INLINE)] public static float Cot(float x) => 1f / MathF.Tan(x);
	[MethodImpl(INLINE)] public static float Ver(float x) => 1 - MathF.Cos(x);
	[MethodImpl(INLINE)] public static float Cvs(float x) => 1 - MathF.Sin(x);
	[MethodImpl(INLINE)] public static float Crd(float x) => 2 * MathF.Sin(x / 2);

	/// <summary>Returns the hyperbolic arc cosine of the given value</summary>
	[MethodImpl(INLINE)] public static float Acosh(float x) => (float)Math.Log(x + Mathf.Sqrt(x * x - 1));

	/// <summary>Returns the hyperbolic arc sine of the given value</summary>
	[MethodImpl(INLINE)] public static float Asinh(float x) => (float)Math.Log(x + Mathf.Sqrt(x * x + 1));

	/// <summary>Returns the hyperbolic arc tangent of the given value</summary>
	[MethodImpl(INLINE)] public static float Atanh(float x) => (float)(0.5 * Math.Log((1 + x) / (1 - x)));

	#endregion Trig Functions

	#region Min Max

	[MethodImpl(INLINE)] public static float Min(float a, float b) => a < b ? a : b;
	[MethodImpl(INLINE)] public static float Min(float a, float b, float c) => Min(Min(a, b), c);
	[MethodImpl(INLINE)] public static float Min(float a, float b, float c, float d) => Min(Min(a, b), Min(c, d));

	[MethodImpl(INLINE)] public static float Max(float a, float b) => a > b ? a : b;
	[MethodImpl(INLINE)] public static float Max(float a, float b, float c) => Max(Max(a, b), c);
	[MethodImpl(INLINE)] public static float Max(float a, float b, float c, float d) => Max(Max(a, b), Max(c, d));

	[MethodImpl(INLINE)] public static int Min(int a, int b) => a < b ? a : b;
	[MethodImpl(INLINE)] public static int Min(int a, int b, int c) => Min(Min(a, b), c);

	[MethodImpl(INLINE)] public static int Max(int a, int b) => a > b ? a : b;
	[MethodImpl(INLINE)] public static int Max(int a, int b, int c) => Max(Max(a, b), c);

	[MethodImpl(INLINE)] public static float Min(params float[] values) => values.Min();
	[MethodImpl(INLINE)] public static float Max(params float[] values) => values.Max();

	[MethodImpl(INLINE)] public static int Min(params int[] values) => values.Min();
	[MethodImpl(INLINE)] public static int Max(params int[] values) => values.Max();

	#endregion Min Max

	#region Rounding

	[MethodImpl(INLINE)] public static float Sign(float value) => value >= 0f ? 1 : -1;
	[MethodImpl(INLINE)] public static int Sign(int value) => value >= 0 ? 1 : -1;

	[MethodImpl(INLINE)] public static int SignWithZero(int value) => value == 0 ? 0 : Sign(value);
	[MethodImpl(INLINE)] public static float SignWithZero(float value, float epsilon = 0.000001f) => Abs(value) < epsilon ? 0 : Sign(value);

	[MethodImpl(INLINE)] public static float Floor(float value) => MathF.Floor(value);
	[MethodImpl(INLINE)] public static int FloorToInt(float value) => (int)MathF.Floor(value);

	[MethodImpl(INLINE)] public static float Ceil(float value) => MathF.Ceiling(value);
	[MethodImpl(INLINE)] public static int CeilToInt(float value) => (int)MathF.Ceiling(value);

	[MethodImpl(INLINE)] public static float Round(float value) => MathF.Round(value);
	[MethodImpl(INLINE)] public static int RoundToInt(float value) => (int)MathF.Round(value);
	[MethodImpl(INLINE)] public static float Round(float value, float snapInterval) => Mathf.Round(value / snapInterval) * snapInterval;
	[MethodImpl(INLINE)] public static int RoundToInt(float value, float snapInterval) => (int)(Mathf.Round(value / snapInterval) * snapInterval);

	[MethodImpl(INLINE)] public static float Trunc(float value) => MathF.Truncate(value);
	[MethodImpl(INLINE)] public static int TruncToInt(float value) => (int)MathF.Truncate(value);

	[MethodImpl(INLINE)] public static float CeilToEven(float value) => MathF.Round(value / 2, MidpointRounding.AwayFromZero) * 2;
	[MethodImpl(INLINE)] public static int CeilToEvenInt(float value) => (int)CeilToEven(value);

	[MethodImpl(INLINE)]
	public static uint CeilToPowerOf2(uint value)
	{
		value--;
		value |= value >> 1;
		value |= value >> 2;
		value |= value >> 4;
		value |= value >> 8;
		value |= value >> 16;
		value++;
		return value;
	}

	#endregion Rounding

	#region Repeating

	/// <summary>Returns the fractional part of the value. Equivalent to <c>x - floor(x)</c></summary>
	[MethodImpl(INLINE)] public static float Frac(float x) => x - Floor(x);

	/// <summary>Repeats the given value in the interval specified by length</summary>
	[MethodImpl(INLINE)] public static float Repeat(float value, float length) => Clamp(value - Floor(value / length) * length, 0f, length);

	/// <summary>Modulo, but, behaves the way you want with negative values, for stuff like array[(n+1)%length] etc.</summary>
	[MethodImpl(INLINE)] public static int Mod(int value, int length) => (value % length + length) % length;

	/// <summary>Repeats a value within a range, going back and forth</summary>
	[MethodImpl(INLINE)] public static float PingPong(float t, float length) => length - Abs(Repeat(t, length * 2f) - length);

	/// <summary>Returns the height of in a triangle wave at time <c>t</c> going from <c>0</c> to <c>1</c> and back to <c>0</c> within the the given <c>period</c></summary>
	[MethodImpl(INLINE)]
	public static float TriangleWave(float t, float period = 1f)
	{
		float x = t / period;
		return 1f - Abs(2 * (x - Floor(x)) - 1);
	}

	[MethodImpl(INLINE)] public static int Wrap(int value, int max) => Wrap(value, 0, max);
	[MethodImpl(INLINE)] public static int Wrap(int value, int min, int max) => value < min ? max - (min - value) % (max - min) : min + (value - min) % (max - min);

	[MethodImpl(INLINE)] public static float Wrap(float value, float max) => Wrap(value, 0f, max);
	[MethodImpl(INLINE)] public static float Wrap(float value, float min, float max) => value < min ? max - (min - value) % (max - min) : min + (value - min) % (max - min);

	#endregion Repeating

	#region Smoothing

	[MethodImpl(INLINE)] public static float Smooth01(float x) => x * x * (3 - 2 * x);

	[MethodImpl(INLINE)] public static float Smoother01(float x) => x * x * x * (x * (x * 6 - 15) + 10);

	[MethodImpl(INLINE)] public static float SmoothCos01(float x) => Cos(x * PI) * -0.5f + 0.5f;

	[MethodImpl(INLINE)]
	public static float Gamma(float value, float absmax, float gamma)
	{
		var negative = value < 0f;
		var absval = Abs(value);
		if (absval > absmax) return negative ? -absval : absval;

		var result = Pow(absval / absmax, gamma) * absmax;
		return negative ? -result : result;
	}

	#endregion Smoothing

	#region Clamping

	[MethodImpl(INLINE)] public static int Clamp(int value, int max) => Clamp(value, 0, max);
	[MethodImpl(INLINE)]
	public static int Clamp(int value, int min, int max)
	{
		if (value < min) value = min;
		else if (value > max) value = max;
		return value;
	}

	[MethodImpl(INLINE)] public static float Clamp(float value, float max) => Clamp(value, 0, max);
	[MethodImpl(INLINE)]
	public static float Clamp(float value, float min, float max)
	{
		if (value < min) value = min;
		else if (value > max) value = max;
		return value;
	}

	[MethodImpl(INLINE)]
	public static float Clamp01(float value)
	{
		if (value < 0f) return 0f;
		else if (value > 1f) return 1f;
		else return value;
	}

	[MethodImpl(INLINE)]
	public static float ClampUnit(float value)
	{
		if (value < -1f) return -1f;
		else if (value > 1f) return 1f;
		else return value;
	}

	#endregion Clamping

	#region Interpolation

	[MethodImpl(INLINE)] public static float Lerp(float a, float b, float t) => (1f - t) * a + t * b;
	[MethodImpl(INLINE)]
	public static float LerpClamped(float a, float b, float t)
	{
		t = Clamp01(t);
		return (1f - t) * a + t * b;
	}

	[MethodImpl(INLINE)] public static float InverseLerp(float a, float b, float value) => (value - a) / (b - a);
	[MethodImpl(INLINE)] public static float InverseLerpClamped(float a, float b, float value) => Clamp01((value - a) / (b - a));

	[MethodImpl(INLINE)] public static float Eerp(float a, float b, float t) => Mathf.Pow(a, 1 - t) * Mathf.Pow(b, t);
	[MethodImpl(INLINE)] public static float InverseEerp(float a, float b, float v) => Mathf.Log(a / v) / Mathf.Log(a / b);

	[MethodImpl(INLINE)] public static float Remap(float iMin, float iMax, float oMin, float oMax, float value) => Lerp(oMin, oMax, InverseLerp(iMin, iMax, value));
	[MethodImpl(INLINE)] public static float RemapClamped(float iMin, float iMax, float oMin, float oMax, float value) => Lerp(oMin, oMax, InverseLerpClamped(iMin, iMax, value));

	[MethodImpl(INLINE)]
	public static float LerpSmooth(float a, float b, float t)
	{
		t = Smooth01(Clamp01(t));
		return (1f - t) * a + t * b;
	}
	[MethodImpl(INLINE)] public static float InverseLerpSmooth(float a, float b, float value) => Smooth01(Clamp01((value - a) / (b - a)));

	[MethodImpl(INLINE)]
	public static float MoveTowards(float current, float target, float maxDelta)
	{
		if (Mathf.Abs(target - current) <= maxDelta) return target;
		return current + Mathf.Sign(target - current) * maxDelta;
	}

	[MethodImpl(INLINE)]
	public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime, float deltaTime, float maxSpeed = float.PositiveInfinity)
	{
		// Based on Game Programming Gems 4 Chapter 1.10
		smoothTime = Mathf.Max(0.0001f, smoothTime);
		var omega = 2f / smoothTime;

		var x = omega * deltaTime;
		var exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
		var change = current - target;
		var originalTo = target;

		// Clamp maximum speed
		var maxChange = maxSpeed * smoothTime;
		change = Mathf.Clamp(change, -maxChange, maxChange);
		target = current - change;

		var temp = (currentVelocity + omega * change) * deltaTime;
		currentVelocity = (currentVelocity - omega * temp) * exp;
		var output = target + (change + temp) * exp;

		// Prevent overshooting
		if (originalTo - current > 0f == output > originalTo)
		{
			output = originalTo;
			currentVelocity = (output - originalTo) / deltaTime;
		}

		return output;
	}

	#endregion Interpolation

	#region Angles

	[MethodImpl(INLINE)]
	public static float LerpAngle(float aRad, float bRad, float t)
	{
		var delta = Repeat((bRad - aRad), TAU);
		if (delta > PI)
			delta -= TAU;
		return aRad + delta * Clamp01(t);
	}

	[MethodImpl(INLINE)]
	public static float InverseLerpAngle(float a, float b, float v)
	{
		var angBetween = DeltaAngle(a, b);
		b = a + angBetween; // removes any a->b discontinuity
		var h = a + angBetween * 0.5f; // halfway angle
		v = h + DeltaAngle(h, v); // get offset from h, and offset by h
		return InverseLerpClamped(a, b, v);
	}

	[MethodImpl(INLINE)] public static float DeltaAngle(float a, float b) => Repeat(b - a + PI, TAU) - PI;

	[MethodImpl(INLINE)]
	public static float MoveTowardsAngle(float current, float target, float maxDelta)
	{
		var deltaAngle = DeltaAngle(current, target);
		if (-maxDelta < deltaAngle && deltaAngle < maxDelta) return target;
		target = current + deltaAngle;
		return MoveTowards(current, target, maxDelta);
	}

	[MethodImpl(INLINE)]
	public static float SmoothDampAngle(float current, float target, ref float currentVelocity, float smoothTime, float deltaTime, float maxSpeed = float.PositiveInfinity)
	{
		return SmoothDamp(current, current + DeltaAngle(current, target), ref currentVelocity, smoothTime, maxSpeed, deltaTime);
	}

	[MethodImpl(INLINE)] public static float Deg2Rad(float deg) => deg * DEG_2_RAD;
	[MethodImpl(INLINE)] public static float Rad2Deg(float rad) => rad * RAD_2_DEG;

	#endregion Angles
}
