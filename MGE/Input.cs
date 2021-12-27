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

		static List<JoyState> _currentGamepadStates = new();
		static List<JoyState> _oldGamepadStates = new();

		public static Vector2 mousePosition { get; private set; }
		public static Vector2 mousePositionDelta { get; private set; }

		public static Vector2 mouseScroll { get; set; }
		public static Vector2 mouseScrollClamped { get; set; }
		public static Vector2 mouseScrollPosition { get; set; }

		public static string textInput { get; private set; } = "";

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

			#region Mouse

			mouseState.IsButtonDown(MouseButton.Left);

			mousePosition = mouseState.Position;
			mousePositionDelta = mouseState.Delta;

			mouseScroll = mouseState.ScrollDelta;
			mouseScrollClamped = new(Math.Clamp01(mouseState.ScrollDelta.X), Math.Clamp01(mouseState.ScrollDelta.Y));
			mouseScrollPosition = mouseState.Scroll;

			#endregion

			#region Gamepads

			var gamepads = joystickStates.Where(gp => gp is not null);
			if (gamepads.Count() > 0)
			{
				Debug.LogList(gamepads);
			}

			_oldGamepadStates = _currentGamepadStates;
			_currentGamepadStates.Clear();

			foreach (var gamepad in joystickStates)
			{
				var state = new JoyState();

				if (gamepad is null)
				{
					_currentGamepadStates.Add(state);
					continue;
				}

				state.name = gamepad.Name;
				state.id = gamepad.Id;

				// Axis
				for (int i = 0; i < gamepad.AxisCount; i++)
				{
					state.axes.Add(gamepad.GetAxis(i));
				}

				// Buttons
				for (int i = 0; i < gamepad.ButtonCount; i++)
				{
					if (gamepad.IsButtonDown(i))
					{
						state.buttons.Add(i);
					}
				}

				// Hats
				for (int i = 0; i < gamepad.HatCount; i++)
				{
					state.hats.Add(gamepad.GetHat(i));
				}

				_currentGamepadStates.Add(state);
			}

			#endregion
		}

		public static float GetGamepadAxis(int axis, int index = 0)
		{
			return _currentGamepadStates[index].axes[axis];
		}

		internal static void OnTextInput(TextInputEventArgs e)
		{
			textInput = e.AsString;
		}

		internal static void OnGamepadConnected(JoystickEventArgs e)
		{
			if (e.IsConnected)
			{
				// Connected
				Debug.Log($"Gamepad #{e.JoystickId} connected");

				return;
			}

			Debug.Log($"Gamepad #{e.JoystickId} disconnected");

			// Disconnected
		}
	}
}
