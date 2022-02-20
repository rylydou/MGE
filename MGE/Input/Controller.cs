using System;

namespace MGE;

/// <summary>
/// Gamepad Axes
/// </summary>
public enum Axes
{
	None = -1,
	LeftX = 0,
	LeftY = 1,
	RightX = 2,
	RightY = 3,
	LeftTrigger = 4,
	RightTrigger = 5
}

/// <summary>
/// Gamepad Buttons
/// </summary>
public enum Buttons
{
	None = -1,
	A = 0,
	B = 1,
	X = 2,
	Y = 3,
	Back = 4,
	Select = 5,
	Start = 6,
	LeftStick = 7,
	RightStick = 8,
	LeftShoulder = 9,
	RightShoulder = 10,
	Up = 11,
	Down = 12,
	Left = 13,
	Right = 14
}

/// <summary>
/// Represents a Gamepad or Joystick
/// </summary>
public class Controller
{
	public const int MAX_BUTTONS = 64;
	public const int MAX_AXIS = 64;

	public readonly Input input;

	public string name { get; set; } = "Unknown";
	public bool connected { get; set; } = false;
	public bool isGamepad { get; set; } = false;
	public int buttons { get; set; } = 0;
	public int axes { get; set; } = 0;

	internal readonly bool[] _pressed = new bool[MAX_BUTTONS];
	internal readonly bool[] _down = new bool[MAX_BUTTONS];
	internal readonly bool[] _released = new bool[MAX_BUTTONS];
	internal readonly long[] _timestamp = new long[MAX_BUTTONS];
	internal readonly float[] _axis = new float[MAX_AXIS];
	internal readonly long[] _axisTimestamp = new long[MAX_AXIS];

	internal void Connect(string name, uint buttonCount, uint axisCount, bool isGamepad)
	{
		this.name = name;
		buttons = (int)Math.Min(buttonCount, MAX_BUTTONS);
		axes = (int)Math.Min(axisCount, MAX_AXIS);
		this.isGamepad = isGamepad;
		connected = true;
	}

	internal void Disconnect()
	{
		name = "Unknown";
		connected = false;
		isGamepad = false;
		buttons = 0;
		axes = 0;

		Array.Fill(_pressed, false);
		Array.Fill(_down, false);
		Array.Fill(_released, false);
		Array.Fill(_timestamp, 0L);
		Array.Fill(_axis, 0);
		Array.Fill(_axisTimestamp, 0L);
	}

	internal void Step()
	{
		Array.Fill(_pressed, false);
		Array.Fill(_released, false);
	}

	internal void Copy(Controller other)
	{
		name = other.name;
		connected = other.connected;
		isGamepad = other.isGamepad;
		buttons = other.buttons;
		axes = other.axes;

		Array.Copy(other._pressed, 0, _pressed, 0, _pressed.Length);
		Array.Copy(other._down, 0, _down, 0, _pressed.Length);
		Array.Copy(other._released, 0, _released, 0, _pressed.Length);
		Array.Copy(other._timestamp, 0, _timestamp, 0, _pressed.Length);
		Array.Copy(other._axis, 0, _axis, 0, _axis.Length);
		Array.Copy(other._axisTimestamp, 0, _axisTimestamp, 0, _axis.Length);
	}

	public bool Pressed(int buttonIndex) => buttonIndex >= 0 && buttonIndex < MAX_BUTTONS && _pressed[buttonIndex];
	public bool Pressed(Buttons button) => Pressed((int)button);

	public long Timestamp(int buttonIndex) => buttonIndex >= 0 && buttonIndex < MAX_BUTTONS ? _timestamp[buttonIndex] : 0;
	public long Timestamp(Buttons button) => Timestamp((int)button);
	public long Timestamp(Axes axis) => _axisTimestamp[(int)axis];

	public bool Down(int buttonIndex) => buttonIndex >= 0 && buttonIndex < MAX_BUTTONS && _down[buttonIndex];
	public bool Down(Buttons button) => Down((int)button);

	public bool Released(int buttonIndex) => buttonIndex >= 0 && buttonIndex < MAX_BUTTONS && _released[buttonIndex];
	public bool Released(Buttons button) => Released((int)button);

	public float Axis(int axisIndex) => (axisIndex >= 0 && axisIndex < MAX_AXIS) ? _axis[axisIndex] : 0f;
	public float Axis(Axes axis) => Axis((int)axis);

	public Vector2 Axis(int axisX, int axisY) => new Vector2(Axis(axisX), Axis(axisY));
	public Vector2 Axis(Axes axisX, Axes axisY) => new Vector2(Axis(axisX), Axis(axisY));

	public Vector2 LeftStick => Axis(MGE.Axes.LeftX, MGE.Axes.LeftY);
	public Vector2 RightStick => Axis(MGE.Axes.RightX, MGE.Axes.RightY);

	public bool Repeated(Buttons button)
	{
		return Repeated(button, input.repeatDelay, input.repeatInterval);
	}

	public bool Repeated(Buttons button, float delay, float interval)
	{
		if (Pressed(button))
			return true;

		if (Down(button))
		{
			var time = Timestamp(button) / 1000.0;
			return (Time.duration.TotalSeconds - time) > delay && Time.OnInterval(interval, time);
		}

		return false;
	}

	public Controller(Input input)
	{
		this.input = input;
	}
}
