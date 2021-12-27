using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MGE
{
	public static class Input
	{
		class JoyState
		{
			public string name = "Unknown";
			public int id;

			public List<float> axes = new();
			public List<int> buttons = new();
			public List<Hat> hats = new();
		}

		static List<Keys> _currentKeys = new();
		static List<Keys> _oldKeys = new();

		static List<JoyState> _currentJoystickStates = new();
		static List<JoyState> _oldJoystickStates = new();

		internal static void UpdateInputs(KeyboardState keyboardState, MouseState mouseState, IReadOnlyList<JoystickState> joystickStates)
		{
			#region Keyboard

			_oldKeys = _currentKeys;
			_currentKeys.Clear();

			foreach (var key in Enum.GetValues<Keys>())
			{
				if (key < 0) continue;

				if (keyboardState.IsKeyPressed(key))
				{
					_currentKeys.Add(key);
				}
			}

			#endregion

			#region Gamepads

			_oldJoystickStates = _currentJoystickStates;
			_currentJoystickStates.Clear();

			foreach (var joystick in joystickStates)
			{
				var joystickState = new JoyState();

				if (joystick is null)
				{
					_currentJoystickStates.Add(joystickState);
					continue;
				}

				joystickState.name = joystick.Name;
				joystickState.id = joystick.Id;

				// Axis
				for (int i = 0; i < joystick.AxisCount; i++)
				{
					joystickState.axes.Add(joystick.GetAxis(i));
				}

				// Buttons
				for (int i = 0; i < joystick.ButtonCount; i++)
				{
					if (joystick.IsButtonDown(i))
					{
						joystickState.buttons.Add(i);
					}
				}

				// Hats
				for (int i = 0; i < joystick.HatCount; i++)
				{
					joystickState.hats.Add(joystick.GetHat(i));
				}

				_currentJoystickStates.Add(joystickState);
			}

			#endregion
		}

		internal static void OnTextInput(TextInputEventArgs e)
		{
			// TODO
		}

		internal static void OnJoystickConnected(JoystickEventArgs e)
		{
			if (e.IsConnected)
			{
				// Connected
				Debug.Log($"Gamepad #{e.JoystickId} connected");

				return;
			}

			// Disconnected
			Debug.Log($"Gamepad #{e.JoystickId} disconnected");
		}
	}
}
