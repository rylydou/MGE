using System;
using System.Collections.Generic;

using SysMath = System.Math;

namespace MGE;

/// <summary>
/// A Virtual Input Axis that can be mapped to different keyboards and gamepads
/// </summary>
public class VirtualAxis
{
	public enum Overlaps
	{
		CancelOut,
		TakeOlder,
		TakeNewer
	};

	public interface INode
	{
		float Value(bool deadzone);
		double timestamp { get; }
	}

	public class KeyNode : INode
	{
		public Input input;
		public Keys key;
		public bool positive;

		public float Value(bool deadzone) => (input.keyboard.Down(key) ? (positive ? 1 : -1) : 0);
		public double timestamp => input.keyboard.Timestamp(key);

		public KeyNode(Input input, Keys key, bool positive)
		{
			this.input = input;
			this.key = key;
			this.positive = positive;
		}
	}

	public class ButtonNode : INode
	{
		public Input input;
		public int index;
		public Buttons button;
		public bool positive;

		public float Value(bool deadzone) => (input.controllers[index].Down(button) ? (positive ? 1 : -1) : 0);
		public double timestamp => input.controllers[index].Timestamp(button);

		public ButtonNode(Input input, int controller, Buttons button, bool positive)
		{
			this.input = input;
			index = controller;
			this.button = button;
			this.positive = positive;
		}
	}

	public class AxisNode : INode
	{
		public Input input;
		public int index;
		public Axes axis;
		public bool positive;
		public float deadzone;

		public float Value(bool deadzone)
		{
			if (!deadzone || Mathf.Abs(input.controllers[index].Axis(axis)) >= this.deadzone)
				return input.controllers[index].Axis(axis) * (positive ? 1 : -1);
			return 0f;
		}

		public double timestamp
		{
			get
			{
				if (Mathf.Abs(input.controllers[index].Axis(axis)) < deadzone)
					return 0;
				return input.controllers[index].Timestamp(axis);
			}
		}

		public AxisNode(Input input, int controller, Axes axis, float deadzone, bool positive)
		{
			this.input = input;
			index = controller;
			this.axis = axis;
			this.deadzone = deadzone;
			this.positive = positive;
		}
	}

	public float value => GetValue(true);
	public float valueNoDeadzone => GetValue(false);

	public int intValue => SysMath.Sign(value);
	public int intValueNoDeadzone => SysMath.Sign(valueNoDeadzone);

	public readonly Input input;
	public readonly List<INode> nodes = new List<INode>();
	public Overlaps overlapBehavior = Overlaps.CancelOut;

	const float EPSILON = 0.00001f;

	public VirtualAxis(Input input)
	{
		this.input = input;
	}

	public VirtualAxis(Input input, Overlaps overlapBehavior)
	{
		this.input = input;
		this.overlapBehavior = overlapBehavior;
	}

	float GetValue(bool deadzone)
	{
		var value = 0f;

		if (overlapBehavior == Overlaps.CancelOut)
		{
			foreach (var input in nodes)
				value += input.Value(deadzone);
			value = Mathf.ClampUnit(value);
		}
		else if (overlapBehavior == Overlaps.TakeNewer)
		{
			var timestamp = 0d;
			for (int i = 0; i < nodes.Count; i++)
			{
				var time = nodes[i].timestamp;
				var val = nodes[i].Value(deadzone);

				if (time > 0 && Mathf.Abs(val) > EPSILON && time > timestamp)
				{
					value = val;
					timestamp = time;
				}
			}
		}
		else if (overlapBehavior == Overlaps.TakeOlder)
		{
			var timestamp = double.MaxValue;
			for (int i = 0; i < nodes.Count; i++)
			{
				var time = nodes[i].timestamp;
				var val = nodes[i].Value(deadzone);

				if (time > 0 && Mathf.Abs(val) > EPSILON && time < timestamp)
				{
					value = val;
					timestamp = time;
				}
			}
		}

		return value;
	}

	public VirtualAxis Add(Keys negative, Keys positive)
	{
		nodes.Add(new KeyNode(input, negative, false));
		nodes.Add(new KeyNode(input, positive, true));
		return this;
	}

	public VirtualAxis Add(Keys key, bool isPositive)
	{
		nodes.Add(new KeyNode(input, key, isPositive));
		return this;
	}

	public VirtualAxis Add(int controller, Buttons negative, Buttons positive)
	{
		nodes.Add(new ButtonNode(input, controller, negative, false));
		nodes.Add(new ButtonNode(input, controller, positive, true));
		return this;
	}

	public VirtualAxis Add(int controller, Buttons button, bool isPositive)
	{
		nodes.Add(new ButtonNode(input, controller, button, isPositive));
		return this;
	}

	public VirtualAxis Add(int controller, Axes axis, float deadzone = 0f)
	{
		nodes.Add(new AxisNode(input, controller, axis, deadzone, true));
		return this;
	}

	public VirtualAxis Add(int controller, Axes axis, bool inverse, float deadzone = 0f)
	{
		nodes.Add(new AxisNode(input, controller, axis, deadzone, !inverse));
		return this;
	}

	public void Clear()
	{
		nodes.Clear();
	}
}

/// <summary>
/// A Virtual Input Button that can be mapped to different keyboards and gamepads
/// </summary>
public class VirtualButton
{
	public interface INode
	{
		bool Pressed(float buffer, long lastBufferConsumedTime);
		bool down { get; }
		bool released { get; }
		bool Repeated(float delay, float interval);
		void Update();
	}

	public class KeyNode : INode
	{
		public Input input;
		public Keys key;

		public bool Pressed(float buffer, long lastBufferConsumedTime)
		{
			if (input.keyboard.Pressed(key))
				return true;

			var timestamp = input.keyboard.Timestamp(key);
			var time = Time.duration.Ticks;

			if (time - timestamp <= TimeSpan.FromSeconds(buffer).Ticks && timestamp > lastBufferConsumedTime)
				return true;

			return false;
		}

		public bool down => input.keyboard.Down(key);
		public bool released => input.keyboard.Released(key);
		public bool Repeated(float delay, float interval) => input.keyboard.Repeated(key, delay, interval);
		public void Update() { }

		public KeyNode(Input input, Keys key)
		{
			this.input = input;
			this.key = key;
		}
	}

	public class MouseButtonNode : INode
	{
		public Input input;
		public MouseButtons mouseButton;

		public bool Pressed(float buffer, long lastBufferConsumedTime)
		{
			if (input.mouse.Pressed(mouseButton))
				return true;

			var timestamp = input.mouse.Timestamp(mouseButton);
			var time = Time.duration.Ticks;

			if (time - timestamp <= TimeSpan.FromSeconds(buffer).Ticks && timestamp > lastBufferConsumedTime)
				return true;

			return false;
		}

		public bool down => input.mouse.Down(mouseButton);
		public bool released => input.mouse.Released(mouseButton);
		public bool Repeated(float delay, float interval) => input.mouse.Repeated(mouseButton, delay, interval);
		public void Update() { }

		public MouseButtonNode(Input input, MouseButtons mouseButton)
		{
			this.input = input;
			this.mouseButton = mouseButton;
		}
	}

	public class ButtonNode : INode
	{
		public Input input;
		public int index;
		public Buttons button;

		public bool Pressed(float buffer, long lastBufferConsumedTime)
		{
			if (input.controllers[index].Pressed(button))
				return true;

			var timestamp = input.controllers[index].Timestamp(button);
			var time = Time.duration.Ticks;

			if (time - timestamp <= TimeSpan.FromSeconds(buffer).Ticks && timestamp > lastBufferConsumedTime)
				return true;

			return false;
		}

		public bool down => input.controllers[index].Down(button);
		public bool released => input.controllers[index].Released(button);
		public bool Repeated(float delay, float interval) => input.controllers[index].Repeated(button, delay, interval);
		public void Update() { }

		public ButtonNode(Input input, int controller, Buttons button)
		{
			this.input = input;
			index = controller;
			this.button = button;
		}
	}

	public class AxisNode : INode
	{
		public Input input;
		public int index;
		public Axes axis;
		public float threshold;

		long _pressedTimestamp;
		const float AXIS_EPSILON = 0.00001f;
		// const float AXIS_EPSILON = 0.15f;

		public bool Pressed(float buffer, long lastBufferConsumedTime)
		{
			if (Pressed())
				return true;

			var time = Time.duration.Ticks;

			if (time - _pressedTimestamp <= TimeSpan.FromSeconds(buffer).Ticks && _pressedTimestamp > lastBufferConsumedTime)
				return true;

			return false;
		}

		public bool down
		{
			get
			{
				if (Mathf.Abs(threshold) <= AXIS_EPSILON)
					return Mathf.Abs(input.controllers[index].Axis(axis)) > AXIS_EPSILON;

				if (threshold < 0)
					return input.controllers[index].Axis(axis) <= threshold;

				return input.controllers[index].Axis(axis) >= threshold;
			}
		}

		public bool released
		{
			get
			{
				if (Mathf.Abs(threshold) <= AXIS_EPSILON)
					return Mathf.Abs(input.lastState.controllers[index].Axis(axis)) > AXIS_EPSILON && Mathf.Abs(input.controllers[index].Axis(axis)) < AXIS_EPSILON;

				if (threshold < 0)
					return input.lastState.controllers[index].Axis(axis) <= threshold && input.controllers[index].Axis(axis) > threshold;

				return input.lastState.controllers[index].Axis(axis) >= threshold && input.controllers[index].Axis(axis) < threshold;
			}
		}

		// TODO Implement repeating
		public bool Repeated(float delay, float interval)
		{
			if (Pressed())
				return true;

			if (down)
			{
				// Log.Trace(_pressedTimestamp.ToString());
				var time = _pressedTimestamp / 10000000.0;
				// Log.Trace($"{time}");

				// Map the input value to 0 - 1 to account for the amopunt lost from the threshold
				var value = Mathf.Abs(input.lastState.controllers[index].Axis(axis));
				var thresh = Mathf.Abs(threshold);
				var t = (value - thresh) / (1 - thresh);
				// Log.Trace(t.ToString());

				// Speed up the further the stick is pushed
				interval = Mathf.Lerp(1f / 4, 1f / 15, t);
				// Log.Trace($"{Time.duration.TotalSeconds - time}");
				return (Time.duration.TotalSeconds - time) > 0.33 && Time.OnInterval(interval, time);
			}

			return false;
			// return Pressed();
			// throw new NotImplementedException();
		}

		bool Pressed()
		{
			if (Mathf.Abs(threshold) <= AXIS_EPSILON)
				return (Mathf.Abs(input.lastState.controllers[index].Axis(axis)) < AXIS_EPSILON && Mathf.Abs(input.controllers[index].Axis(axis)) > AXIS_EPSILON);

			if (threshold < 0)
				return (input.lastState.controllers[index].Axis(axis) > threshold && input.controllers[index].Axis(axis) <= threshold);

			return (input.lastState.controllers[index].Axis(axis) < threshold && input.controllers[index].Axis(axis) >= threshold);
		}

		public void Update()
		{
			if (Pressed())
				_pressedTimestamp = input.controllers[index].Timestamp(axis);
		}

		public AxisNode(Input input, int controller, Axes axis, float threshold)
		{
			this.input = input;
			index = controller;
			this.axis = axis;
			this.threshold = threshold;
		}
	}

	public readonly Input input;
	public readonly List<INode> nodes = new List<INode>();
	public float repeatDelay;
	public float repeatInterval;
	public float buffer;

	long _lastBufferConsumeTime;

	public bool pressed
	{
		get
		{
			for (int i = 0; i < nodes.Count; i++)
				if (nodes[i].Pressed(buffer, _lastBufferConsumeTime))
					return true;

			return false;
		}
	}

	public bool down
	{
		get
		{
			for (int i = 0; i < nodes.Count; i++)
				if (nodes[i].down)
					return true;

			return false;
		}
	}

	public bool released
	{
		get
		{
			for (int i = 0; i < nodes.Count; i++)
				if (nodes[i].released)
					return true;

			return false;
		}
	}

	public bool repeated
	{
		get
		{
			for (int i = 0; i < nodes.Count; i++)
				if (nodes[i].Pressed(buffer, _lastBufferConsumeTime) || nodes[i].Repeated(repeatDelay, repeatInterval))
					return true;

			return false;
		}
	}

	public VirtualButton(Input input, float buffer = 0f)
	{
		this.input = input;

		// Using a Weak Reference to subscribe this object to Updates
		// This way it's automatically collected if the user is no longer
		// using it, and we don't require the user to call a Dispose or
		// Unsubscribe callback
		this.input.virtualButtons.Add(new WeakReference<VirtualButton>(this));

		repeatDelay = this.input.repeatDelay;
		repeatInterval = this.input.repeatInterval;
		this.buffer = buffer;
	}

	public void ConsumeBuffer()
	{
		_lastBufferConsumeTime = Time.duration.Ticks;
	}

	public VirtualButton Add(params Keys[] keys)
	{
		foreach (var key in keys)
			nodes.Add(new KeyNode(input, key));
		return this;
	}

	public VirtualButton Add(params MouseButtons[] buttons)
	{
		foreach (var button in buttons)
			nodes.Add(new MouseButtonNode(input, button));
		return this;
	}

	public VirtualButton Add(int controller, params Buttons[] buttons)
	{
		foreach (var button in buttons)
			nodes.Add(new ButtonNode(input, controller, button));
		return this;
	}

	public VirtualButton Add(int controller, Axes axis, float threshold)
	{
		nodes.Add(new AxisNode(input, controller, axis, threshold));
		return this;
	}

	public void Clear()
	{
		nodes.Clear();
	}

	internal void Update()
	{
		for (int i = 0; i < nodes.Count; i++)
			nodes[i].Update();
	}
}

/// <summary>
/// A Virtual Input Stick that can be mapped to different keyboards and gamepads
/// </summary>
public class VirtualStick
{
	/// <summary>
	/// The Horizontal Axis
	/// </summary>
	public VirtualAxis horizontal;

	/// <summary>
	/// The Vertical Axis
	/// </summary>
	public VirtualAxis vertical;

	/// <summary>
	/// This Deadzone is applied to the Length of the combined Horizontal and Vertical axis values
	/// </summary>
	public float circularDeadzone = 0f;

	/// <summary>
	/// Gets the current Virtual Stick value
	/// </summary>
	public Vector2 value
	{
		get
		{
			var result = new Vector2(horizontal.value, vertical.value);
			if (circularDeadzone != 0 && result.length < circularDeadzone)
				return Vector2.zero;
			return result;
		}
	}

	/// <summary>
	/// Gets the current Virtual Stick value, ignoring Deadzones
	/// </summary>
	public Vector2 valueNoDeadzone => new Vector2(horizontal.valueNoDeadzone, vertical.valueNoDeadzone);

	/// <summary>
	/// Gets the current Virtual Stick value, clamping to Integer Values
	/// </summary>
	public Vector2Int intValue
	{
		get
		{
			var result = value;
			return new Vector2Int(MathF.Sign(result.x), MathF.Sign(result.y));
		}
	}

	/// <summary>
	/// Gets the current Virtual Stick value, clamping to Integer values and Ignoring Deadzones
	/// </summary>
	public Vector2Int intValueNoDeadzone => new Vector2Int(horizontal.intValueNoDeadzone, vertical.intValueNoDeadzone);

	public VirtualStick(Input input, float circularDeadzone = 0f)
	{
		horizontal = new VirtualAxis(input);
		vertical = new VirtualAxis(input);
		this.circularDeadzone = circularDeadzone;
	}

	public VirtualStick(Input input, VirtualAxis.Overlaps overlapBehavior, float circularDeadzone = 0f)
	{
		horizontal = new VirtualAxis(input, overlapBehavior);
		vertical = new VirtualAxis(input, overlapBehavior);
		this.circularDeadzone = circularDeadzone;
	}

	public VirtualStick Add(Keys left, Keys right, Keys up, Keys down)
	{
		horizontal.Add(left, right);
		vertical.Add(up, down);
		return this;
	}

	public VirtualStick Add(int controller, Buttons left, Buttons right, Buttons up, Buttons down)
	{
		horizontal.Add(controller, left, right);
		vertical.Add(controller, up, down);
		return this;
	}

	public VirtualStick Add(int controller, Axes horizontal, Axes vertical, float deadzoneHorizontal = 0f, float deadzoneVertical = 0f)
	{
		this.horizontal.Add(controller, horizontal, deadzoneHorizontal);
		this.vertical.Add(controller, vertical, deadzoneVertical);
		return this;
	}

	public VirtualStick AddLeftJoystick(int controller, float deadzoneHorizontal = 0, float deadzoneVertical = 0)
	{
		horizontal.Add(controller, Axes.LeftX, deadzoneHorizontal);
		vertical.Add(controller, Axes.LeftY, deadzoneVertical);
		return this;
	}

	public VirtualStick AddRightJoystick(int controller, float deadzoneHorizontal = 0, float deadzoneVertical = 0)
	{
		horizontal.Add(controller, Axes.RightX, deadzoneHorizontal);
		vertical.Add(controller, Axes.RightY, deadzoneVertical);
		return this;
	}

	public void Clear()
	{
		horizontal.Clear();
		vertical.Clear();
	}
}
