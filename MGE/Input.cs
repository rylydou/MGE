using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MGE;

public static class Input
{
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

	internal static void Init()
	{
		GLFW.UpdateGamepadMappings(Folder.assets.GetFile("Mappings.csv").ReadText());
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

	class ControllerState
	{
		public int id;
		public string name;

		// public List<Button> buttons = new();

		// public Vector2 leftStick;
		// public Vector2 rightStick;

		public bool[] buttons = new bool[15];
		public float[] axes = new float[6];

		public ControllerState(int id, string name)
		{
			this.id = id;
			this.name = name;
		}
	}

	static ControllerState[] _joyStates = new ControllerState[16];

	internal static void UpdateJoysticks(IReadOnlyList<JoystickState> joyStates)
	{
		foreach (var joyState in joyStates)
		{
			if (joyState is null) continue;
			if (!GLFW.GetGamepadState(joyState.Id, out var gamepadState)) return;

			var conState = new ControllerState(joyState.Id, joyState.Name);

			unsafe
			{
				var buttons = Create<byte>(gamepadState.Buttons, 15);
				var axes = Create<float>(gamepadState.Axes, 6);

				conState.buttons = buttons.Select(b => b == 1).ToArray();
				conState.axes = axes;
			}

			_joyStates[joyState.Id] = conState;
		}
	}

	internal static void DrawGamepadInput()
	{
		var offset = 2;
		foreach (var joyState in _joyStates)
		{
			if (joyState is null) continue;

			Font.monospace.DrawString(
				joyState.id + ". " + joyState.name,
				new(-GameWindow.current.Size.X / 2 + 8, GameWindow.current.Size.Y / 2 - 8 - offset++ * 18),
				Color.white.translucent
			);

			Font.monospace.DrawString(
				$"Buttons: {string.Join(' ', joyState.buttons.Select(x => x ? '1' : '0'))}",
				new(-GameWindow.current.Size.X / 2 + 8, GameWindow.current.Size.Y / 2 - 8 - offset++ * 18),
				Color.white.translucent
			);
			Font.monospace.DrawString(
				$"   Axes: {string.Join(' ', joyState.axes.Select(x => x.ToString("F2")))}",
				new(-GameWindow.current.Size.X / 2 + 8, GameWindow.current.Size.Y / 2 - 8 - offset++ * 18),
				Color.white.translucent
			);
		}
	}

	internal static void OnJoystickConnected(JoystickEventArgs e)
	{
		var name = GLFW.GetJoystickName(e.JoystickId);
		var uuid = GLFW.GetJoystickGUID(e.JoystickId);

		if (e.IsConnected)
		{
			var hasMapping = GLFW.JoystickIsGamepad(e.JoystickId);

			Debug.Log($"\"{name}\"(#{uuid}) connected at {e.JoystickId} with{(hasMapping ? "" : "out")} a mapping");

			return;
		}

		Debug.Log($" \"{name}\"(#{uuid}) disconnected at {e.JoystickId}");
	}

	internal static void RegisterJoystick(int jid)
	{
	}

	public unsafe static T[] Create<T>(void* ptr, int length) where T : struct
	{
		var type = typeof(T);
		var sizeInBytes = Marshal.SizeOf(typeof(T));

		var output = new T[length];

		if (type.IsPrimitive)
		{
			// Make sure the array won't be moved around by the GC
			var handle = GCHandle.Alloc(output, GCHandleType.Pinned);

			var destination = (byte*)handle.AddrOfPinnedObject().ToPointer();
			var byteLength = length * sizeInBytes;

			// There are faster ways to do this, particularly by using wider types or by
			// handling special lengths.
			for (int i = 0; i < byteLength; i++)
				destination[i] = ((byte*)ptr)[i];

			handle.Free();
		}
		else if (type.IsValueType)
		{
			if (!type.IsLayoutSequential && !type.IsExplicitLayout)
			{
				throw new InvalidOperationException(string.Format("{0} does not define a StructLayout attribute", type));
			}

			IntPtr sourcePtr = new IntPtr(ptr);

			for (int i = 0; i < length; i++)
			{
				IntPtr p = new IntPtr((byte*)ptr + i * sizeInBytes);

				output[i] = (T)System.Runtime.InteropServices.Marshal.PtrToStructure(p, typeof(T))!;
			}
		}
		else
		{
			throw new InvalidOperationException(string.Format("{0} is not supported", type));
		}

		return output;
	}

	#endregion Controller
}
