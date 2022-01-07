using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

	static List<Button> _currentButtonsDown = new();
	static List<Button> _oldButtonsDown = new();

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

	static List<Button> _buttonsEntered = new();

	public static bool IsButtonEntered(Button button, int controllerIndex = 0)
	{
		return _buttonsEntered.Contains(button);
	}

	#region Keyboard

	public static string textInput = "";

	internal static void ClearInputs()
	{
		_oldButtonsDown = _currentButtonsDown;
		_currentButtonsDown = new();
		_buttonsEntered.Clear();
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
		_buttonsEntered.Add((Button)e.Key);
	}

	internal static void OnKeyUp(KeyboardKeyEventArgs e) { }

	internal static void OnTextInput(TextInputEventArgs e)
	{
		textInput = e.AsString ?? "";
	}

	#endregion Keyboard

	#region Mouse

	public static Vector2 mousePosition;
	public static Vector2 mousePositionDelta;

	public static Vector2 mouseScroll;
	public static Vector2 mouseScrollRaw;

	internal static void UpdateMouse(MouseState state)
	{
		mousePosition = state.Position;
		mousePositionDelta = state.Position - state.PreviousPosition;

		mouseScroll = state.ScrollDelta;
		mouseScrollRaw = state.Scroll;

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

	class JoyState
	{
		public List<Button> buttons = new();

		public Vector2 leftStick;
		public Vector2 rightStick;
	}

	static JoyState[] _joyStates = new JoyState[16];

	internal static void UpdateJoysticks(IReadOnlyList<JoystickState> states)
	{
		foreach (var joyState in states)
		{
			if (joyState is null) continue;

			if (!GLFW.GetGamepadState(joyState.Id, out var gamepadState))
			{
				Debug.Log($"{joyState.Name} at {joyState.Id} does not have a mapping");
				return;
			}

			unsafe
			{
				var axes = Create<float>(gamepadState.Axes, 6);

				Debug.Log(axes);
			}
		}
	}

	internal static void OnJoystickConnected(JoystickEventArgs e)
	{
		var name = GLFW.GetJoystickName(e.JoystickId);
		var uuid = GLFW.GetJoystickGUID(e.JoystickId);

		if (e.IsConnected)
		{
			var hasMapping = _controllerMappingDatabase[Engine.operatingSystemName].TryGetValue(uuid, out var mapping);

			Debug.Log($"\"{name}\"(#{uuid}) connected at {e.JoystickId} with{(hasMapping ? "" : "out")} a mapping");

			return;
		}

		Debug.Log($"\"{name}\"(#{uuid}) disconnected at {e.JoystickId}");
	}

	internal static void RegisterJoystick(int jid)
	{
	}

	public unsafe static T[] Create<T>(void* ptr, int length) where T : struct
	{
		T[] array = new T[length];

		for (int i = 0; i < length; i++)
		{
			array[i] = (T)Marshal.PtrToStructure(new IntPtr(ptr), typeof(T))!;
		}

		return array;
	}

	#endregion Controller
}
