using System;
using System.Linq;
using MathD = System.Math;
using MathF = System.MathF;

namespace MGE;

public static class Math
{
	#region Constants

	public const float e = MathF.E;
	public const float log10E = 0.4342945f;
	public const float log2E = 1.442695f;
	public const float sqrtOf2 = 1.41421356237f;
	public const float goldenRatio = 1.61803398875f;

	public const float pi = MathF.PI;
	public const float tau = pi * 2;

	public const float positiveInfinity = float.PositiveInfinity;
	public const float negativeInfinity = float.NegativeInfinity;

	public const float deg2Rad = tau / 360f;
	public const float rad2Deg = 360f / tau;

	public const float epsilon = float.Epsilon;
	public const float epsilonSqrt = float.Epsilon * float.Epsilon;

	#endregion

	#region Operations

	public static float Abs(float value) => MathF.Abs(value);
	public static int Abs(int value) => MathD.Abs(value);

	public static float Sqrt(float value) => MathF.Sqrt(value);
	public static float Pow(float @base, float power) => MathF.Pow(@base, power);
	public static float Exp(float power) => MathF.Exp(power);

	public static float Log(float value, float @base) => MathF.Log(value, @base);
	public static float Log(float value) => MathF.Log(value);
	public static float Log10(float value) => MathF.Log10(value);

	public static bool Approximately(float a, float b) => Abs(b - a) < Max(0.000001f * Max(Abs(a), Abs(b)), epsilon * 8);

	#endregion Operations

	#region Trig Functions

	public static float Sin(float value) => MathF.Sin(value);
	public static float Asin(float value) => MathF.Asin(value);
	public static float Cos(float value) => MathF.Cos(value);
	public static float Acos(float value) => MathF.Acos(value);
	public static float Tan(float value) => MathF.Tan(value);
	public static float Atan(float value) => MathF.Atan(value);
	public static float Atan2(float y, float x) => MathF.Atan2(y, x);
	public static float Csc(float x) => 1f / MathF.Sin(x);
	public static float Sec(float x) => 1f / MathF.Cos(x);
	public static float Cot(float x) => 1f / MathF.Tan(x);
	public static float Ver(float x) => 1 - MathF.Cos(x);
	public static float Cvs(float x) => 1 - MathF.Sin(x);
	public static float Crd(float x) => 2 * MathF.Sin(x / 2);

	#endregion Trig Functions

	#region Min Max

	public static float Min(float a, float b) => a < b ? a : b;
	public static float Min(float a, float b, float c) => Min(Min(a, b), c);
	public static float Min(float a, float b, float c, float d) => Min(Min(a, b), Min(c, d));

	public static float Max(float a, float b) => a > b ? a : b;
	public static float Max(float a, float b, float c) => Max(Max(a, b), c);
	public static float Max(float a, float b, float c, float d) => Max(Max(a, b), Max(c, d));

	public static int Min(int a, int b) => a < b ? a : b;
	public static int Min(int a, int b, int c) => Min(Min(a, b), c);

	public static int Max(int a, int b) => a > b ? a : b;
	public static int Max(int a, int b, int c) => Max(Max(a, b), c);

	public static float Min(params float[] values) => values.Min();
	public static float Max(params float[] values) => values.Max();

	public static int Min(params int[] values) => values.Min();
	public static int Max(params int[] values) => values.Max();

	#endregion Min Max

	#region Rounding

	public static float Sign(float value) => value >= 0f ? 1 : -1;
	public static int Sign(int value) => value >= 0 ? 1 : -1;

	public static int SignWithZero(int value) => value == 0 ? 0 : Sign(value);
	public static float SignWithZero(float value, float epsilon = 0.000001f) => Abs(value) < epsilon ? 0 : Sign(value);

	public static float Floor(float value) => MathF.Floor(value);
	public static int FloorToInt(float value) => (int)MathF.Floor(value);

	public static float Ceil(float value) => MathF.Ceiling(value);
	public static int CeilToInt(float value) => (int)MathF.Ceiling(value);

	public static float Round(float value) => MathF.Round(value);
	public static int RoundToInt(float value) => (int)MathF.Round(value);
	public static float Round(float value, float snapInterval) => Math.Round(value / snapInterval) * snapInterval;
	public static int RoundToInt(float value, float snapInterval) => (int)(Math.Round(value / snapInterval) * snapInterval);

	public static float Trunc(float value) => MathF.Truncate(value);
	public static int TruncToInt(float value) => (int)MathF.Truncate(value);

	public static float CeilToEven(float value) => MathF.Round(value / 2, MidpointRounding.AwayFromZero) * 2;
	public static int CeilToEvenInt(float value) => (int)CeilToEven(value);

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

	public static float Frac(float x) => x - Floor(x);
	public static float Repeat(float value, float length) => Clamp(value - Floor(value / length) * length, 0f, length);
	public static int Mod(int value, int length) => (value % length + length) % length;

	public static float PingPong(float t, float length) => length - Abs(Repeat(t, length * 2f) - length);

	public static int Wrap(int value, int max) => Wrap(value, 0, max);
	public static int Wrap(int value, int min, int max) => value < min ? max - (min - value) % (max - min) : min + (value - min) % (max - min);

	public static float Wrap(float value, float max) => Wrap(value, 0f, max);
	public static float Wrap(float value, float min, float max) => value < min ? max - (min - value) % (max - min) : min + (value - min) % (max - min);

	#endregion Repeating

	#region Smoothing

	public static float Smooth01(float x) => x * x * (3 - 2 * x);
	public static float Smoother01(float x) => x * x * x * (x * (x * 6 - 15) + 10);
	public static float SmoothCos01(float x) => Cos(x * pi) * -0.5f + 0.5f;

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

	public static int Clamp(int value, int max) => Clamp(value, 0, max);
	public static int Clamp(int value, int min, int max)
	{
		if (value < min) value = min;
		else if (value > max) value = max;
		return value;
	}

	public static float Clamp(float value, float max) => Clamp(value, 0, max);
	public static float Clamp(float value, float min, float max)
	{
		if (value < min) value = min;
		else if (value > max) value = max;
		return value;
	}

	public static float Clamp01(float value)
	{
		if (value < 0f) return 0f;
		else if (value > 1f) return 1f;
		else return value;
	}

	public static float ClampUnit(float value)
	{
		if (value < -1f) return -1f;
		else if (value > 1f) return 1f;
		else return value;
	}

	#endregion Clamping

	#region Interpolation

	public static float Lerp(float a, float b, float t) => (1f - t) * a + t * b;
	public static float LerpClamped(float a, float b, float t)
	{
		t = Clamp01(t);
		return (1f - t) * a + t * b;
	}

	public static float InverseLerp(float a, float b, float value) => (value - a) / (b - a);
	public static float InverseLerpClamped(float a, float b, float value) => Clamp01((value - a) / (b - a));

	public static float Eerp(float a, float b, float t) => Math.Pow(a, 1 - t) * Math.Pow(b, t);
	public static float InverseEerp(float a, float b, float v) => Math.Log(a / v) / Math.Log(a / b);

	public static float Remap(float iMin, float iMax, float oMin, float oMax, float value) => Lerp(oMin, oMax, InverseLerp(iMin, iMax, value));
	public static float RemapClamped(float iMin, float iMax, float oMin, float oMax, float value) => Lerp(oMin, oMax, InverseLerpClamped(iMin, iMax, value));

	public static float LerpSmooth(float a, float b, float t)
	{
		t = Smooth01(Clamp01(t));
		return (1f - t) * a + t * b;
	}
	public static float InverseLerpSmooth(float a, float b, float value) => Smooth01(Clamp01((value - a) / (b - a)));

	public static float MoveTowards(float current, float target, float maxDelta)
	{
		if (Math.Abs(target - current) <= maxDelta) return target;
		return current + Math.Sign(target - current) * maxDelta;
	}

	public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime, float deltaTime, float maxSpeed = float.PositiveInfinity)
	{
		// Based on Game Programming Gems 4 Chapter 1.10
		smoothTime = Math.Max(0.0001f, smoothTime);
		var omega = 2f / smoothTime;

		var x = omega * deltaTime;
		var exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
		var change = current - target;
		var originalTo = target;

		// Clamp maximum speed
		var maxChange = maxSpeed * smoothTime;
		change = Math.Clamp(change, -maxChange, maxChange);
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

	public static float LerpAngle(float aRad, float bRad, float t)
	{
		var delta = Repeat((bRad - aRad), tau);
		if (delta > pi)
			delta -= tau;
		return aRad + delta * Clamp01(t);
	}

	public static float InverseLerpAngle(float a, float b, float v)
	{
		var angBetween = DeltaAngle(a, b);
		b = a + angBetween; // removes any a->b discontinuity
		var h = a + angBetween * 0.5f; // halfway angle
		v = h + DeltaAngle(h, v); // get offset from h, and offset by h
		return InverseLerpClamped(a, b, v);
	}

	public static float DeltaAngle(float a, float b) => Repeat(b - a + pi, tau) - pi;

	static public float MoveTowardsAngle(float current, float target, float maxDelta)
	{
		var deltaAngle = DeltaAngle(current, target);
		if (-maxDelta < deltaAngle && deltaAngle < maxDelta) return target;
		target = current + deltaAngle;
		return MoveTowards(current, target, maxDelta);
	}

	public static float SmoothDampAngle(float current, float target, ref float currentVelocity, float smoothTime, float deltaTime, float maxSpeed = float.PositiveInfinity) =>
		SmoothDamp(current, current + DeltaAngle(current, target), ref currentVelocity, smoothTime, maxSpeed, deltaTime);

	public static float Deg2Rad(float deg) => deg * deg2Rad;
	public static float Rad2Deg(float rad) => rad * rad2Deg;

	#endregion Angles
}
