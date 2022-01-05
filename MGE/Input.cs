using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MGE;

public static class Input
{
	public static string textInput = "";

	static List<Button> _currentButtonsDown = new();
	static List<Button> _oldButtonsDown = new();

	static List<Button> _keysPressed = new();

	public static bool IsButtonDown(Button button, int controllerIndex = 0)
	{
		return _currentButtonsDown.Contains(button);
	}

	public static bool IsButtonPressed(Button button, int controllerIndex = 0)
	{
		return _currentButtonsDown.Contains(button) && !_currentButtonsDown.Contains(button);
	}

	public static bool IsButtonReleased(Button button, int controllerIndex = 0)
	{
		return !_currentButtonsDown.Contains(button) && _currentButtonsDown.Contains(button);
	}

	#region Keyboard

	internal static void ClearInputs()
	{
		_oldButtonsDown = _currentButtonsDown;
		_oldButtonsDown = new();
		_keysPressed.Clear();
	}

	internal static void UpdateKeyboard(KeyboardState state)
	{
		foreach (var key in Enum.GetValues<Keys>())
		{
			if (key < 0) continue;

			if (state.IsKeyDown(key))
			{
				_currentButtonsDown.Add((Button)key);
			}
		}
	}

	internal static void OnKeyDown(KeyboardKeyEventArgs e)
	{
		_keysPressed.Add((Button)e.Key);
	}

	internal static void OnKeyUp(KeyboardKeyEventArgs e) { }

	internal static void OnTextInput(TextInputEventArgs e)
	{
		textInput = e.AsString ?? "";
	}

	#endregion Keyboard

	#region Mouse

	internal static void UpdateMouse(MouseState state)
	{
		foreach (var button in Enum.GetValues<MouseButton>())
		{
			if (button < 0) continue;

			if (state.IsButtonDown(button))
			{
				_currentButtonsDown.Add((Button)button);
			}
		}
	}

	#endregion Mouse

	#region Joystick & Gamepad

	class JoyState
	{
		public string name = "Unknown";
		public int id;

		public List<float> axes = new();
		public List<int> buttons = new();
		public List<Hat> hats = new();
	}

	static List<JoyState> _currentJoystickStates = new();
	static List<JoyState> _oldGamepadStates = new();

	internal static void UpdateJoysticks(IReadOnlyList<JoystickState> states)
	{
		var gamepads = states.Where(gp => gp is not null);
		if (gamepads.Count() > 0)
		{
			Debug.LogList(gamepads);
		}

		_oldGamepadStates = _currentJoystickStates;
		_currentJoystickStates.Clear();

		foreach (var joystick in states)
		{
			var state = new JoyState();

			if (joystick is null)
			{
				_currentJoystickStates.Add(state);
				continue;
			}

			state.name = joystick.Name;
			state.id = joystick.Id;

			// Axis
			for (int i = 0; i < joystick.AxisCount; i++)
			{
				state.axes.Add(joystick.GetAxis(i));
			}

			// Buttons
			for (int i = 0; i < joystick.ButtonCount; i++)
			{
				if (joystick.IsButtonDown(i))
				{
					state.buttons.Add(i);
				}
			}

			// Hats
			for (int i = 0; i < joystick.HatCount; i++)
			{
				state.hats.Add(joystick.GetHat(i));
			}

			_currentJoystickStates.Add(state);
		}
	}

	internal static void OnJoystickConnected(JoystickEventArgs e)
	{
		if (e.IsConnected)
		{
			// Connected
			Debug.Log($"Joystick #{e.JoystickId} connected");

			return;
		}

		Debug.Log($"Joystick #{e.JoystickId} disconnected");

		// Disconnected
	}

	#endregion Joystick & Gamepad
}
