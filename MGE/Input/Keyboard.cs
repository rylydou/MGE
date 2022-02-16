using System;
using System.Text;

namespace MGE;

/// <summary>
/// List of Keys
/// (currently copied from GLFW)
/// </summary>
public enum Keys
{
	Unknown = 0,

	Space = 32,
	Apostrophe = 39,
	BackQuote = 40,
	Comma = 44,
	Minus = 45,
	Period = 46,
	Slash = 47,

	D0 = 48,
	D1 = 49,
	D2 = 50,
	D3 = 51,
	D4 = 52,
	D5 = 53,
	D6 = 54,
	D7 = 55,
	D8 = 56,
	D9 = 57,

	Semicolon = 59,
	Equal = 61,

	A = 65,
	B = 66,
	C = 67,
	D = 68,
	E = 69,
	F = 70,
	G = 71,
	H = 72,
	I = 73,
	J = 74,
	K = 75,
	L = 76,
	M = 77,
	N = 78,
	O = 79,
	P = 80,
	Q = 81,
	R = 82,
	S = 83,
	T = 84,
	U = 85,
	V = 86,
	W = 87,
	X = 88,
	Y = 89,
	Z = 90,

	LeftBracket = 91,
	BackSlash = 92,
	RightBracket = 93,
	GraveAccent = 96,
	World1 = 161,
	World2 = 162,
	Escape = 256,
	Enter = 257,
	Tab = 258,
	Backspace = 259,
	Insert = 260,
	Delete = 261,
	Right = 262,
	Left = 263,
	Down = 264,
	Up = 265,
	PageUp = 266,
	PageDown = 267,
	Home = 268,
	End = 269,
	CapsLock = 280,
	ScrollLock = 281,
	NumLock = 282,
	PrintScreen = 283,
	Pause = 284,

	F1 = 290,
	F2 = 291,
	F3 = 292,
	F4 = 293,
	F5 = 294,
	F6 = 295,
	F7 = 296,
	F8 = 297,
	F9 = 298,
	F10 = 299,
	F11 = 300,
	F12 = 301,
	F13 = 302,
	F14 = 303,
	F15 = 304,
	F16 = 305,
	F17 = 306,
	F18 = 307,
	F19 = 308,
	F20 = 309,
	F21 = 310,
	F22 = 311,
	F23 = 312,
	F24 = 313,
	F25 = 314,

	KP_0 = 320,
	KP_1 = 321,
	KP_2 = 322,
	KP_3 = 323,
	KP_4 = 324,
	KP_5 = 325,
	KP_6 = 326,
	KP_7 = 327,
	KP_8 = 328,
	KP_9 = 329,

	KP_Decimal = 330,
	KP_Divide = 331,
	KP_Multiply = 332,
	KP_Subtract = 333,
	KP_Add = 334,
	KP_Enter = 335,
	KP_Equal = 336,

	LeftShift = 340,
	LeftControl = 341,
	LeftAlt = 342,
	LeftSuper = 343,

	RightShift = 344,
	RightControl = 345,
	RightAlt = 346,
	RightSuper = 347,

	Menu = 348
}

/// <summary>
/// Stores a Keyboard State
/// </summary>
public class Keyboard
{

	public const int MaxKeys = 400;

	internal readonly bool[] pressed = new bool[MaxKeys];
	internal readonly bool[] down = new bool[MaxKeys];
	internal readonly bool[] released = new bool[MaxKeys];
	internal readonly long[] timestamp = new long[MaxKeys];

	/// <summary>
	/// The Input Module this Keyboard belong to
	/// </summary>
	public readonly Input Input;

	/// <summary>
	/// Any Text that was typed over the last frame
	/// </summary>
	public readonly StringBuilder Text = new StringBuilder();

	internal Keyboard(Input input)
	{
		Input = input;
	}

	/// <summary>
	/// Checks if the given key was pressed
	/// </summary>
	public bool Pressed(Keys key) => pressed[(int)key];

	/// <summary>
	/// Checks if any of the given keys were pressed
	/// </summary>
	public bool Pressed(Keys key1, Keys key2) => pressed[(int)key1] || pressed[(int)key2];

	/// <summary>
	/// Checks if any of the given keys were pressed
	/// </summary>
	public bool Pressed(Keys key1, Keys key2, Keys key3) => pressed[(int)key1] || pressed[(int)key2] || pressed[(int)key3];

	/// <summary>
	/// Checks if the given key is held
	/// </summary>
	public bool Down(Keys key) => down[(int)key];

	/// <summary>
	/// Checks if any of the given keys were down
	/// </summary>
	public bool Down(Keys key1, Keys key2) => down[(int)key1] || down[(int)key2];

	/// <summary>
	/// Checks if any of the given keys were down
	/// </summary>
	public bool Down(Keys key1, Keys key2, Keys key3) => down[(int)key1] || down[(int)key2] || down[(int)key3];

	/// <summary>
	/// Checks if the given key was released
	/// </summary>
	public bool Released(Keys key) => released[(int)key];

	/// <summary>
	/// Checks if any of the given keys were released
	/// </summary>
	public bool Released(Keys key1, Keys key2) => released[(int)key1] || released[(int)key2];

	/// <summary>
	/// Checks if any of the given keys were released
	/// </summary>
	public bool Released(Keys key1, Keys key2, Keys key3) => released[(int)key1] || released[(int)key2] || released[(int)key3];

	/// <summary>
	/// Checks if any of the given keys were pressed
	/// </summary>
	public bool Pressed(ReadOnlySpan<Keys> keys)
	{
		for (int i = 0; i < keys.Length; i++)
			if (pressed[(int)keys[i]])
				return true;

		return false;
	}

	/// <summary>
	/// Checks if any of the given keys are held
	/// </summary>
	public bool Down(ReadOnlySpan<Keys> keys)
	{
		for (int i = 0; i < keys.Length; i++)
			if (down[(int)keys[i]])
				return true;

		return false;
	}

	/// <summary>
	/// Checks if any of the given keys were released
	/// </summary>
	public bool Released(ReadOnlySpan<Keys> keys)
	{
		for (int i = 0; i < keys.Length; i++)
			if (released[(int)keys[i]])
				return true;

		return false;
	}

	/// <summary>
	/// Checks if the given key was Repeated
	/// </summary>
	public bool Repeated(Keys key)
	{
		return Repeated(key, Input.RepeatDelay, Input.RepeatInterval);
	}

	/// <summary>
	/// Checks if the given key was Repeated, given the delay and interval
	/// </summary>
	public bool Repeated(Keys key, float delay, float interval)
	{
		if (Pressed(key))
			return true;

		var time = timestamp[(int)key] / (float)TimeSpan.TicksPerSecond;

		return Down(key) && (Time.duration.TotalSeconds - time) > delay && Time.OnInterval(interval, time);
	}

	/// <summary>
	/// Gets the Timestamp of when the given key was last pressed, in Ticks
	/// </summary>
	public long Timestamp(Keys key)
	{
		return timestamp[(int)key];
	}

	/// <summary>
	/// Returns True if the Left or Right Control keys are held
	/// </summary>
	public bool Ctrl => Down(Keys.LeftControl, Keys.RightControl);

	/// <summary>
	/// Returns True if the Left or Right Control keys are held (or Command on MacOS)
	/// </summary>
	public bool CtrlOrCommand
	{
		get
		{
			if (OperatingSystem.IsMacOS())
				return Down(Keys.LeftSuper, Keys.RightSuper);
			else
				return Down(Keys.LeftControl, Keys.RightControl);
		}
	}

	/// <summary>
	/// Returns True if the Left or Right Alt keys are held
	/// </summary>
	public bool Alt => Down(Keys.LeftAlt, Keys.RightAlt);

	/// <summary>
	/// Returns True of the Left or Right Shift keys are held
	/// </summary>
	public bool Shift => Down(Keys.LeftShift, Keys.RightShift);

	public void Clear()
	{
		Array.Clear(pressed, 0, MaxKeys);
		Array.Clear(down, 0, MaxKeys);
		Array.Clear(released, 0, MaxKeys);
		Array.Clear(timestamp, 0, MaxKeys);
		Text.Clear();
	}

	public void Clear(Keys key)
	{
		pressed[(int)key] = false;
		down[(int)key] = false;
		released[(int)key] = false;
		timestamp[(int)key] = 0;
	}

	internal void Copy(Keyboard other)
	{
		Array.Copy(other.pressed, 0, pressed, 0, MaxKeys);
		Array.Copy(other.down, 0, down, 0, MaxKeys);
		Array.Copy(other.released, 0, released, 0, MaxKeys);
		Array.Copy(other.timestamp, 0, timestamp, 0, MaxKeys);

		Text.Clear();
		Text.Append(other.Text);
	}

	internal void Step()
	{
		Array.Fill(pressed, false);
		Array.Fill(released, false);

		Text.Clear();
	}
}
