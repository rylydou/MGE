using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MGE.GLFW;

internal class GLFW_Input : Input
{
	// we need to keep track of delegates because otherwise they can be garbage collected
	// and then the C++ GLFW stuff is calling garbage collected delegates...
	readonly Dictionary<IntPtr, List<Delegate>> delegateTracker = new Dictionary<IntPtr, List<Delegate>>();
	readonly Dictionary<Cursors, IntPtr> cursors = new Dictionary<Cursors, IntPtr>();
	readonly List<IntPtr> windows = new List<IntPtr>();
	string? clipboardText;

	// GLFW has this really weird bug where if you're holding the Mouse Down
	// while creating a new Window, it will trigger a Mouse Up event, along with a second
	// Mouse-Up when you actually release the mouse ... So when a new Window is created
	// we ignore the next mouse-up event, if a Mouse Button is held
	readonly bool[] ignoreMouseUp = new bool[3];

	GLFW.GamepadState gamepadState = new GLFW.GamepadState()
	{
		Buttons = new char[(int)GLFW_Enum.GAMEPAD_BUTTON_LAST + 1],
		Axes = new float[(int)GLFW_Enum.GAMEPAD_AXIS_LAST + 1]
	};

	internal void Init()
	{
		GLFW.UpdateGamepadMappings(Util.EmbeddedResourceText("gamecontrollerdb.csv"));
		var userGameControllerDB = Folder.data.GetFile("gamecontrollerdb.csv");
		if (userGameControllerDB.exists)
		{
			GLFW.UpdateGamepadMappings(userGameControllerDB.ReadText());
			Log.Info("Loaded user game controller database");
		}

		// find the already-connected joysticks
		for (int jid = 0; jid <= (int)GLFW_Enum.JOYSTICK_LAST; jid++)
		{
			if (GLFW.JoystickPresent(jid) != 0)
				OnJoystickCallback(jid, GLFW_Enum.CONNECTED);
		}
	}

	internal void Dispose()
	{
		foreach (var kv in delegateTracker)
			StopListening(kv.Key);
		delegateTracker.Clear();
	}

	internal void StartListening(IntPtr window)
	{
		windows.Add(window);

		ignoreMouseUp[0] = mouse.leftDown;
		ignoreMouseUp[1] = mouse.middleDown;
		ignoreMouseUp[2] = mouse.rightDown;

		GLFW.SetKeyCallback(window, TrackDelegate<GLFW.KeyFunc>(window, OnKeyCallback));
		GLFW.SetCharCallback(window, TrackDelegate<GLFW.CharFunc>(window, OnCharCallback));
		GLFW.SetMouseButtonCallback(window, TrackDelegate<GLFW.MouseButtonFunc>(window, OnMouseCallback));
		GLFW.SetScrollCallback(window, TrackDelegate<GLFW.ScrollFunc>(window, OnScrollCallback));
	}

	internal void StopListening(IntPtr window)
	{
		windows.Remove(window);

		GLFW.SetKeyCallback(window, null);
		GLFW.SetCharCallback(window, null);
		GLFW.SetMouseButtonCallback(window, null);
		delegateTracker.Remove(window);
	}

	T TrackDelegate<T>(IntPtr windowPtr, T method) where T : Delegate
	{
		if (!delegateTracker.TryGetValue(windowPtr, out var list))
			delegateTracker[windowPtr] = list = new List<Delegate>();

		list.Add(method);

		return method;
	}

	public override void SetMouseCursor(Cursors cursors)
	{
		var cursor = GetCursor(cursors);

		for (int i = 0; i < App.windowing.windows.Count; i++)
		{
			if (App.windowing.windows[i].implementation is GLFW_Window window)
				GLFW.SetCursor(window._pointer, cursor);
		}
	}

	public override string? GetClipboardString()
	{
		return clipboardText;
	}

	public override void SetClipboardString(string value)
	{
		clipboardText = value;

		if (App.windowing.windows[0].implementation is GLFW_Window window)
			GLFW.SetClipboardString(window._pointer, value);
	}

	IntPtr GetCursor(Cursors MGECursor)
	{
		if (!cursors.TryGetValue(MGECursor, out var ptr))
		{
			GLFW_Enum cursor;
			switch (MGECursor)
			{
				default:
				case Cursors.Default: cursor = GLFW_Enum.ARROW_CURSOR; break;
				case Cursors.IBeam: cursor = GLFW_Enum.IBEAM_CURSOR; break;
				case Cursors.Crosshair: cursor = GLFW_Enum.CROSSHAIR_CURSOR; break;
				case Cursors.Hand: cursor = GLFW_Enum.HAND_CURSOR; break;
				case Cursors.HorizontalResize: cursor = GLFW_Enum.HRESIZE_CURSOR; break;
				case Cursors.VerticalResize: cursor = GLFW_Enum.VRESIZE_CURSOR; break;
			}

			cursors.Add(MGECursor, ptr = GLFW.CreateStandardCursor((int)cursor));
		}

		return ptr;
	}

	void OnJoystickCallback(int jid, GLFW_Enum eventType)
	{
		if (eventType == GLFW_Enum.CONNECTED)
		{
			var name = GLFW.GetJoystickName(jid);
			var isGamepad = GLFW.JoystickIsGamepad(jid) != 0;

			GLFW.GetJoystickButtons(jid, out int buttonCount);
			GLFW.GetJoystickAxes(jid, out int axisCount);

			OnJoystickConnect((uint)jid, name, (uint)buttonCount, (uint)axisCount, isGamepad);
		}
		else if (eventType == GLFW_Enum.DISCONNECTED)
		{
			OnJoystickDisconnect((uint)jid);
		}
	}

	void OnMouseCallback(IntPtr window, int button, int action, int mods)
	{
		MouseButtons mb = MouseButtons.Unknown;
		if (button == 0)
			mb = MouseButtons.Left;
		else if (button == 1)
			mb = MouseButtons.Right;
		else if (button == 2)
			mb = MouseButtons.Middle;

		if (action == 1)
		{
			OnMouseDown(mb);
		}
		else if (action == 0)
		{
			if (!ignoreMouseUp[button])
				OnMouseUp(mb);
			ignoreMouseUp[button] = false;
		}
	}

	void OnScrollCallback(IntPtr window, double offsetX, double offsetY)
	{
		OnMouseWheel((float)offsetX, (float)offsetY);
	}

	void OnCharCallback(IntPtr window, uint codepoint)
	{
		OnText((char)codepoint);
	}

	void OnKeyCallback(IntPtr window, int key, int scancode, int action, int mods)
	{
		if (key >= 0 && key < Keyboard.MAX_KEYS)
		{
			if (action == 1)
			{
				OnKeyDown((Keys)key);
			}
			else if (action == 0)
			{
				OnKeyUp((Keys)key);
			}
		}
	}

	internal void BeforeUpdate()
	{
		// clipboard text
		if (windows.Count > 0)
		{
			var ptr = GLFW.GetClipboardString(windows[0]);
			if (ptr == IntPtr.Zero)
				clipboardText = null;
			else
				clipboardText = Marshal.PtrToStringUTF8(ptr);
		}
	}

	internal void AfterUpdate()
	{
		const float AXIS_EPSILON = 0.000001f;

		for (int jid = 0; jid <= (int)GLFW_Enum.JOYSTICK_LAST; jid++)
		{
			uint index = (uint)jid;

			if (GLFW.JoystickPresent(jid) != 0)
			{
				if (GLFW.JoystickIsGamepad(jid) != 0 && GLFW.GetGamepadState(jid, ref gamepadState) != 0)
				{
					for (int i = 0; i < gamepadState.Buttons.Length; i++)
					{
						var button = GamepadButtonToEnum((GLFW_Enum)i);
						var down = IsGamepadButtonDown(index, button);
						var state = gamepadState.Buttons[i];

						if (!down && state == 1)
						{
							OnGamepadButtonDown(index, button);
						}
						else if (down && state == 0)
						{
							OnGamepadButtonUp(index, button);
						}
					}

					for (int i = 0; i < gamepadState.Axes.Length; i++)
					{
						var axis = GamepadAxisToEnum((GLFW_Enum)i);
						var current = GetGamepadAxis(index, axis);
						var next = gamepadState.Axes[i];

						if (Mathf.Abs(current - next) > AXIS_EPSILON)
							OnGamepadAxis(index, axis, next);
					}
				}
				else
				{
					unsafe
					{
						char* buttons = (char*)GLFW.GetJoystickButtons(jid, out int buttonCount).ToPointer();
						for (int i = 0; i < buttonCount; i++)
						{
							var button = (uint)i;
							var down = IsJoystickButtonDown(index, button);
							var state = buttons[i];

							if (!down && state == 1)
							{
								OnJoystickButtonDown(index, button);
							}
							else if (down && state == 0)
							{
								OnJoystickButtonUp(index, button);
							}
						}

						float* axes = (float*)GLFW.GetJoystickAxes(jid, out int axesCount).ToPointer();
						for (int i = 0; i < axesCount; i++)
						{
							var axis = (uint)i;
							var current = GetJoystickAxis(index, axis);
							var next = axes[i];

							if (Mathf.Abs(current - next) > AXIS_EPSILON)
								OnJoystickAxis(index, axis, next);
						}
					}
				}
			}
		}
	}

	Buttons GamepadButtonToEnum(GLFW_Enum btn)
	{
		return btn switch
		{
			GLFW_Enum.GAMEPAD_BUTTON_A => Buttons.A,
			GLFW_Enum.GAMEPAD_BUTTON_B => Buttons.B,
			GLFW_Enum.GAMEPAD_BUTTON_X => Buttons.X,
			GLFW_Enum.GAMEPAD_BUTTON_Y => Buttons.Y,
			GLFW_Enum.GAMEPAD_BUTTON_LEFT_BUMPER => Buttons.LeftShoulder,
			GLFW_Enum.GAMEPAD_BUTTON_RIGHT_BUMPER => Buttons.RightShoulder,
			GLFW_Enum.GAMEPAD_BUTTON_BACK => Buttons.Back,
			GLFW_Enum.GAMEPAD_BUTTON_START => Buttons.Start,
			GLFW_Enum.GAMEPAD_BUTTON_GUIDE => Buttons.Select,
			GLFW_Enum.GAMEPAD_BUTTON_LEFT_THUMB => Buttons.LeftStick,
			GLFW_Enum.GAMEPAD_BUTTON_RIGHT_THUMB => Buttons.RightStick,
			GLFW_Enum.GAMEPAD_BUTTON_DPAD_UP => Buttons.Up,
			GLFW_Enum.GAMEPAD_BUTTON_DPAD_RIGHT => Buttons.Right,
			GLFW_Enum.GAMEPAD_BUTTON_DPAD_DOWN => Buttons.Down,
			GLFW_Enum.GAMEPAD_BUTTON_DPAD_LEFT => Buttons.Left,
			_ => Buttons.None,
		};
	}

	Axes GamepadAxisToEnum(GLFW_Enum axes)
	{
		return axes switch
		{
			GLFW_Enum.GAMEPAD_AXIS_LEFT_X => Axes.LeftX,
			GLFW_Enum.GAMEPAD_AXIS_LEFT_Y => Axes.LeftY,
			GLFW_Enum.GAMEPAD_AXIS_RIGHT_X => Axes.RightX,
			GLFW_Enum.GAMEPAD_AXIS_RIGHT_Y => Axes.RightY,
			GLFW_Enum.GAMEPAD_AXIS_LEFT_TRIGGER => Axes.LeftTrigger,
			GLFW_Enum.GAMEPAD_AXIS_RIGHT_TRIGGER => Axes.RightTrigger,
			_ => Axes.None,
		};
	}
}
