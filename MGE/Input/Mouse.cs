using System;
using System.Collections.Generic;

namespace MGE;

/// <summary>
/// Mouse Buttons
/// </summary>
public enum MouseButtons
{
	None = 0,
	Unknown = 1,
	Left = 2,
	Middle = 3,
	Right = 4
}

public static class MouseButtonsExt
{
	public static IEnumerable<MouseButtons> all
	{
		get
		{
			yield return MouseButtons.Left;
			yield return MouseButtons.Middle;
			yield return MouseButtons.Right;
		}
	}
}

/// <summary>
/// Mouse Cursor Styles
/// </summary>
public enum Cursors
{
	Default,
	IBeam,
	Crosshair,
	Hand,
	HorizontalResize,
	VerticalResize,
}

/// <summary>
/// Stores a Mouse State
/// </summary>
public class Mouse
{
	public const int MAX_BUTTONS = 5;

	internal readonly bool[] _pressed = new bool[MAX_BUTTONS];
	internal readonly bool[] _down = new bool[MAX_BUTTONS];
	internal readonly bool[] _released = new bool[MAX_BUTTONS];
	internal readonly long[] _timestamp = new long[MAX_BUTTONS];
	internal Vector2 _wheelValue;

	public bool Pressed(MouseButtons button) => _pressed[(int)button];
	public bool Down(MouseButtons button) => _down[(int)button];
	public bool Released(MouseButtons button) => _released[(int)button];

	public long Timestamp(MouseButtons button)
	{
		return _timestamp[(int)button];
	}

	public bool Repeated(MouseButtons button, float delay, float interval)
	{
		if (Pressed(button)) return true;

		var time = _timestamp[(int)button] / (float)TimeSpan.TicksPerSecond;

		return Down(button) && (Time.duration.TotalSeconds - time) > delay && Time.OnInterval(interval, time);
	}

	public bool leftPressed => _pressed[(int)MouseButtons.Left];
	public bool leftDown => _down[(int)MouseButtons.Left];
	public bool leftReleased => _released[(int)MouseButtons.Left];

	public bool rightPressed => _pressed[(int)MouseButtons.Right];
	public bool rightDown => _down[(int)MouseButtons.Right];
	public bool rightReleased => _released[(int)MouseButtons.Right];

	public bool middlePressed => _pressed[(int)MouseButtons.Middle];
	public bool middleDown => _down[(int)MouseButtons.Middle];
	public bool middleReleased => _released[(int)MouseButtons.Middle];

	public Vector2 wheel => _wheelValue;

	internal void Copy(Mouse other)
	{
		Array.Copy(other._pressed, 0, _pressed, 0, MAX_BUTTONS);
		Array.Copy(other._down, 0, _down, 0, MAX_BUTTONS);
		Array.Copy(other._released, 0, _released, 0, MAX_BUTTONS);
		Array.Copy(other._timestamp, 0, _timestamp, 0, MAX_BUTTONS);

		_wheelValue = other._wheelValue;
	}

	internal void Step()
	{
		Array.Fill(_pressed, false);
		Array.Fill(_released, false);
		_wheelValue = Vector2.zero;
	}
}
