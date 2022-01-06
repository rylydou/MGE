using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MGE;

public static class Input
{
	class ControllerMapping
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryLoad(string data, [MaybeNullWhen(false)] out ControllerMapping mapping)
		{
			if (data.EndsWith("iOS,") || data.EndsWith("Android,"))
			{
				mapping = null;
				return false;
			}

			var segments = data.Split(',', System.StringSplitOptions.RemoveEmptyEntries);

			mapping = new(segments[0], segments[1], segments.Last().Remove(0, 9));

			for (int i = 0; i < segments.Length - 2; i++)
			{
				var pair = segments[i + 2].Split(':');
				mapping.mappings.Add(pair[0], pair[1]);
			}

			return true;
		}

		public string uuid;
		public string name;
		public string platform;

		public Dictionary<string, string> mappings = new();

		public ControllerMapping(string uuid, string name, string platform)
		{
			this.uuid = uuid;
			this.name = name;
			this.platform = platform;
		}
	}


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
		return _currentButtonsDown.Contains(button) && !_oldButtonsDown.Contains(button);
	}

	public static bool IsButtonReleased(Button button, int controllerIndex = 0)
	{
		return !_currentButtonsDown.Contains(button) && _oldButtonsDown.Contains(button);
	}

	internal static void LoadInput()
	{
		Debug.LogList(_currentButtonsDown);
	}

	#region Keyboard

	internal static void ClearInputs()
	{
		_oldButtonsDown = _currentButtonsDown;
		_currentButtonsDown = new();
		_keysPressed.Clear();
	}

	internal static void UpdateKeyboard(KeyboardState state)
	{
		foreach (var key in Enum.GetValues<Keys>())
		{
			if (key < 0) continue;

			if (state.IsKeyDown(key))
			{
				_currentButtonsDown.Add((Button)(int)key);
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

	#region Controllers

	static Dictionary<string, Dictionary<string, ControllerMapping>> _controllerMappingDatabase = new();

	public static void InitControllers()
	{
		Debug.StartStopwatch("Load controller mappings");

		using var mappingData = Folder.assets.GetFile("Mappings.csv").OpenReadText();

		_controllerMappingDatabase.Clear();
		_controllerMappingDatabase.Add("Windows", new());
		_controllerMappingDatabase.Add("Mac OS X", new());
		_controllerMappingDatabase.Add("Linux", new());

		while (true)
		{
			if (mappingData.EndOfStream) break;

			var line = mappingData.ReadLine();

			if (string.IsNullOrWhiteSpace(line)) continue; // Ignore empty lines
			if (line.StartsWith("#")) continue; // Ignore comments

			if (ControllerMapping.TryLoad(line, out var mapping))
			{
				_controllerMappingDatabase[mapping.platform].Add(mapping.uuid, mapping);
			}
		}

		Debug.EndStopwatch();
	}

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
			var uuid = GLFW.GetJoystickGUID(e.JoystickId);
			var mapping = _controllerMappingDatabase[Engine.operatingSystemName][uuid];
			// Connected
			Debug.Log($"Joystick #{e.JoystickId} connected {mapping.name}#{uuid}");

			return;
		}

		Debug.Log($"Joystick #{e.JoystickId} disconnected");

		// Disconnected
	}

	#endregion Controller
}
