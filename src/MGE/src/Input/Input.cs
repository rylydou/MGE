using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MGE;

/// <summary>
/// The Input Manager that stores the current Input State
/// </summary>
public abstract class Input
{
	/// <summary>
	/// The Current Input State
	/// </summary>
	public readonly InputState state;

	/// <summary>
	/// The Input State of the previous frame
	/// </summary>
	public readonly InputState lastState;

	/// <summary>
	/// The Input State of the next frame
	/// </summary>
	readonly InputState nextState;

	/// <summary>
	/// The Keyboard of the current State
	/// </summary>
	public Keyboard keyboard => state.keyboard;

	/// <summary>
	/// The Mouse of the Current State
	/// </summary>
	public Mouse mouse => state.mouse;

	/// <summary>
	/// The Controllers of the Current State
	/// </summary>
	public ReadOnlyCollection<Controller> controllers => state.controllers;

	/// <summary>
	/// Default delay before a key or button starts repeating
	/// </summary>
	public float repeatDelay = 0.150f;

	/// <summary>
	/// Default interval that the repeat is triggered, in seconds
	/// </summary>
	public float repeatInterval = 1f / 30;

	internal List<WeakReference<VirtualButton>> virtualButtons = new List<WeakReference<VirtualButton>>();

	protected Input()
	{
		state = new InputState(this);
		lastState = new InputState(this);
		nextState = new InputState(this);
	}

	internal void Step()
	{
		lastState.Copy(state);
		state.Copy(nextState);
		nextState.Step();

		for (int i = virtualButtons.Count - 1; i >= 0; i--)
		{
			var button = virtualButtons[i];
			if (button.TryGetTarget(out var target))
				target.Update();
			else
				virtualButtons.RemoveAt(i);
		}
	}

	/// <summary>
	/// Sets the Mouse Cursor
	/// </summary>
	public abstract void SetMouseCursor(Cursors cursors);

	/// <summary>
	/// Sets the Mouse Cursor Mode
	/// </summary>
	public abstract void SetMouseCursorMode(CursorModes modes);

	/// <summary>
	/// Gets the Clipboard String, if it is a String
	/// </summary>
	public abstract string? GetClipboardString();

	/// <summary>
	/// Sets the Clipboard to the given String
	/// </summary>
	public abstract void SetClipboardString(string value);

	public delegate void textInputHandler(char value);

	public event textInputHandler? onTextEvent;

	protected void OnText(char value)
	{
		onTextEvent?.Invoke(value);
		nextState.keyboard.text.Append(value);
	}

	protected void OnKeyDown(Keys key)
	{
		var id = (uint)key;
		if (id >= Keyboard.MAX_KEYS)
			throw new ArgumentOutOfRangeException(nameof(key), "Value is out of Range for supported keys");

		nextState.keyboard._down[id] = true;
		nextState.keyboard._pressed[id] = true;
		nextState.keyboard._timestamp[id] = Time.duration.Ticks;
	}

	protected void OnKeyUp(Keys key)
	{
		var id = (uint)key;
		if (id >= Keyboard.MAX_KEYS)
			throw new ArgumentOutOfRangeException(nameof(key), "Value is out of Range for supported keys");

		nextState.keyboard._down[id] = false;
		nextState.keyboard._released[id] = true;
	}

	protected void OnMouseDown(MouseButtons button)
	{
		nextState.mouse._down[(int)button] = true;
		nextState.mouse._pressed[(int)button] = true;
		nextState.mouse._timestamp[(int)button] = Time.duration.Ticks;
	}

	protected void OnMouseUp(MouseButtons button)
	{
		nextState.mouse._down[(int)button] = false;
		nextState.mouse._released[(int)button] = true;
	}

	protected void OnMouseWheel(float offsetX, float offsetY)
	{
		nextState.mouse._wheelValue = new Vector2(offsetX, offsetY);
	}

	protected void OnJoystickConnect(uint index, string name, uint buttonCount, uint axisCount, bool isGamepad)
	{
		if (index < InputState.maxControllers)
			nextState.controllers[(int)index].Connect(name, buttonCount, axisCount, isGamepad);
	}

	protected void OnJoystickDisconnect(uint index)
	{
		if (index < InputState.maxControllers)
			nextState.controllers[(int)index].Disconnect();
	}

	protected void OnJoystickButtonDown(uint index, uint button)
	{
		if (index < InputState.maxControllers && button < Controller.MAX_BUTTONS)
		{
			nextState.controllers[(int)index]._down[button] = true;
			nextState.controllers[(int)index]._pressed[button] = true;
			nextState.controllers[(int)index]._timestamp[button] = Time.duration.Ticks;
		}
	}

	protected void OnJoystickButtonUp(uint index, uint button)
	{
		if (index < InputState.maxControllers && button < Controller.MAX_BUTTONS)
		{
			nextState.controllers[(int)index]._down[button] = false;
			nextState.controllers[(int)index]._released[button] = true;
		}
	}

	protected void OnGamepadButtonDown(uint index, Buttons button)
	{
		if (index < InputState.maxControllers && button != Buttons.None)
		{
			nextState.controllers[(int)index]._down[(int)button] = true;
			nextState.controllers[(int)index]._pressed[(int)button] = true;
			nextState.controllers[(int)index]._timestamp[(int)button] = Time.duration.Ticks;
		}
	}

	protected void OnGamepadButtonUp(uint index, Buttons button)
	{
		if (index < InputState.maxControllers && button != Buttons.None)
		{
			nextState.controllers[(int)index]._down[(int)button] = false;
			nextState.controllers[(int)index]._released[(int)button] = true;
		}
	}

	protected bool IsJoystickButtonDown(uint index, uint button)
	{
		return (index < InputState.maxControllers && button < Controller.MAX_BUTTONS && nextState.controllers[(int)index]._down[button]);
	}

	protected bool IsGamepadButtonDown(uint index, Buttons button)
	{
		return (index < InputState.maxControllers && button != Buttons.None && nextState.controllers[(int)index]._down[(int)button]);
	}

	protected void OnJoystickAxis(uint index, uint axis, float value)
	{
		if (index < InputState.maxControllers && axis < Controller.MAX_AXIS)
		{
			nextState.controllers[(int)index]._axis[axis] = value;
			nextState.controllers[(int)index]._axisTimestamp[axis] = Time.duration.Ticks;
		}
	}

	protected float GetJoystickAxis(uint index, uint axis)
	{
		if (index < InputState.maxControllers && axis < Controller.MAX_AXIS)
			return nextState.controllers[(int)index]._axis[axis];
		return 0;
	}

	protected void OnGamepadAxis(uint index, Axes axis, float value)
	{
		if (index < InputState.maxControllers && axis != Axes.None)
		{
			nextState.controllers[(int)index]._axis[(int)axis] = value;
			nextState.controllers[(int)index]._axisTimestamp[(int)axis] = Time.duration.Ticks;
		}
	}

	protected float GetGamepadAxis(uint index, Axes axis)
	{
		if (index < InputState.maxControllers && axis != Axes.None)
			return nextState.controllers[(int)index]._axis[(int)axis];
		return 0;
	}
}

/// <summary>
/// Stores an Input State
/// </summary>
public class InputState
{
	/// <summary>
	/// The Maximum number of Controllers
	/// </summary>
	public const int maxControllers = 32;

	/// <summary>
	/// Our Input Module
	/// </summary>
	public readonly Input input;

	/// <summary>
	/// The Keyboard State
	/// </summary>
	public readonly Keyboard keyboard;

	/// <summary>
	/// The Mouse State
	/// </summary>
	public readonly Mouse mouse;

	/// <summary>
	/// A list of all the Controllers
	/// </summary>
	readonly Controller[] _controllers;

	/// <summary>
	/// A Read-Only Collection of the Controllers
	/// Note that they aren't necessarily connected
	/// </summary>
	public readonly ReadOnlyCollection<Controller> controllers;

	public InputState(Input input)
	{
		this.input = input;

		_controllers = new Controller[maxControllers];
		for (int i = 0; i < _controllers.Length; i++)
			_controllers[i] = new Controller(input);

		controllers = new ReadOnlyCollection<Controller>(_controllers);
		keyboard = new Keyboard(input);
		mouse = new Mouse();
	}

	internal void Step()
	{
		for (int i = 0; i < controllers.Count; i++)
		{
			if (controllers[i].isConnected)
				controllers[i].Step();
		}
		keyboard.Step();
		mouse.Step();
	}

	internal void Copy(InputState other)
	{
		for (int i = 0; i < controllers.Count; i++)
		{
			if (other.controllers[i].isConnected || (controllers[i].isConnected != other.controllers[i].isConnected))
				controllers[i].Copy(other.controllers[i]);
		}

		keyboard.Copy(other.keyboard);
		mouse.Copy(other.mouse);
	}
}
