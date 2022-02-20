using System;
using SysMath = System.Math;

namespace MGE;

/// <summary>
/// Time values
/// </summary>
public static class Time
{
	/// <summary>
	/// The Target Framerate of a Fixed Timestep update
	/// </summary>
	public static int fixedStepTarget = 60;

	/// <summary>
	/// The Maximum elapsed time a fixed update can take before skipping update calls
	/// </summary>
	public static TimeSpan fixedMaxElapsedTime = TimeSpan.FromMilliseconds(500);

	/// <summary>
	/// The time since the start of the Application
	/// </summary>
	public static TimeSpan duration { get; internal set; }

	/// <summary>
	/// The total fixed-update duration since the start of the Application
	/// </summary>
	public static TimeSpan fixedDuration { get; internal set; }

	/// <summary>
	/// Multiplies the Delta Time per frame by the scale value
	/// </summary>
	public static float deltaScale = 1.0f;

	/// <summary>
	/// The Delta Time from the last frame. Fixed or Variable depending on the current Update method.
	/// Note that outside of Update Methods, this will return the last Variable Delta Time
	/// </summary>
	public static float delta { get; internal set; }

	/// <summary>
	/// The Delta Time from the last frame, not scaled by DeltaScale. Fixed or Variable depending on the current Update method.
	/// Note that outside of Update Methods, this will return the last Raw Variable Delta Time
	/// </summary>
	public static float rawDelta { get; internal set; }

	/// <summary>
	/// The last Fixed Delta Time.
	/// </summary>
	public static float fixedDelta { get; internal set; }

	/// <summary>
	/// The last Fixed Delta Time, not scaled by DeltaScale
	/// </summary>
	public static float rawFixedDelta { get; internal set; }

	/// <summary>
	/// The last Variable Delta Time.
	/// </summary>
	public static float variableDelta { get; internal set; }

	/// <summary>
	/// The last Variable Delta Time, not scaled by DeltaScale
	/// </summary>
	public static float rawVariableDelta { get; internal set; }

	/// <summary>
	/// A rough estimate of the current Frames Per Second
	/// </summary>
	public static int fps { get; internal set; }

	/// <summary>
	/// Returns true when the elapsed time passes a given interval based on the delta time
	/// </summary>
	public static bool OnInterval(double time, double delta, double interval, double offset)
	{
		return SysMath.Floor((time - offset - delta) / interval) < SysMath.Floor((time - offset) / interval);
	}

	/// <summary>
	/// Returns true when the elapsed time passes a given interval based on the delta time
	/// </summary>
	public static bool OnInterval(double delta, double interval, double offset)
	{
		return OnInterval(duration.TotalSeconds, delta, interval, offset);
	}

	/// <summary>
	/// Returns true when the elapsed time passes a given interval based on the delta time
	/// </summary>
	public static bool OnInterval(double interval, double offset = 0.0)
	{
		return OnInterval(duration.TotalSeconds, delta, interval, offset);
	}

	/// <summary>
	/// Returns true when the elapsed time is between the given interval. Ex: an interval of 0.1 will be false for 0.1 seconds, then true for 0.1 seconds, and then repeat.
	/// </summary>
	public static bool BetweenInterval(double time, double interval, double offset)
	{
		return (time - offset) % (interval * 2) >= interval;
	}

	/// <summary>
	/// Returns true when the elapsed time is between the given interval. Ex: an interval of 0.1 will be false for 0.1 seconds, then true for 0.1 seconds, and then repeat.
	/// </summary>
	public static bool BetweenInterval(double interval, double offset = 0.0)
	{
		return BetweenInterval(duration.TotalSeconds, interval, offset);
	}

	/// <summary>
	/// Sine-wave a value between `from` and `to` with a period of `duration`.
	/// You can use `offsetPercent` to offset the sine wave.
	/// </summary>
	public static float SineWave(float from, float to, float duration, float offsetPercent)
	{
		var total = (float)Time.duration.TotalSeconds;
		var range = (to - from) * 0.5f;
		return from + range + MathF.Sin(((total + duration * offsetPercent) / duration) * MathF.Tau) * range;
	}
}
