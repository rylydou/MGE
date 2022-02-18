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
		double Timestamp { get; }
	}

	public class KeyNode : INode
	{
		public Input Input;
		public Keys Key;
		public bool Positive;

		public float Value(bool deadzone) => (Input.Keyboard.Down(Key) ? (Positive ? 1 : -1) : 0);
		public double Timestamp => Input.Keyboard.Timestamp(Key);

		public KeyNode(Input input, Keys key, bool positive)
		{
			Input = input;
			Key = key;
			Positive = positive;
		}
	}

	public class ButtonNode : INode
	{
		public Input Input;
		public int Index;
		public Buttons Button;
		public bool Positive;

		public float Value(bool deadzone) => (Input.Controllers[Index].Down(Button) ? (Positive ? 1 : -1) : 0);
		public double Timestamp => Input.Controllers[Index].Timestamp(Button);

		public ButtonNode(Input input, int controller, Buttons button, bool positive)
		{
			Input = input;
			Index = controller;
			Button = button;
			Positive = positive;
		}
	}

	public class AxisNode : INode
	{
		public Input Input;
		public int Index;
		public Axes Axis;
		public bool Positive;
		public float Deadzone;

		public float Value(bool deadzone)
		{
			if (!deadzone || Math.Abs(Input.Controllers[Index].Axis(Axis)) >= Deadzone)
				return Input.Controllers[Index].Axis(Axis) * (Positive ? 1 : -1);
			return 0f;
		}

		public double Timestamp
		{
			get
			{
				if (Math.Abs(Input.Controllers[Index].Axis(Axis)) < Deadzone)
					return 0;
				return Input.Controllers[Index].Timestamp(Axis);
			}
		}

		public AxisNode(Input input, int controller, Axes axis, float deadzone, bool positive)
		{
			Input = input;
			Index = controller;
			Axis = axis;
			Deadzone = deadzone;
			Positive = positive;
		}
	}

	public float Value => GetValue(true);
	public float ValueNoDeadzone => GetValue(false);

	public int IntValue => SysMath.Sign(Value);
	public int IntValueNoDeadzone => SysMath.Sign(ValueNoDeadzone);

	public readonly Input Input;
	public readonly List<INode> Nodes = new List<INode>();
	public Overlaps OverlapBehavior = Overlaps.CancelOut;

	const float EPSILON = 0.00001f;

	public VirtualAxis(Input input)
	{
		Input = input;
	}

	public VirtualAxis(Input input, Overlaps overlapBehavior)
	{
		Input = input;
		OverlapBehavior = overlapBehavior;
	}

	float GetValue(bool deadzone)
	{
		var value = 0f;

		if (OverlapBehavior == Overlaps.CancelOut)
		{
			foreach (var input in Nodes)
				value += input.Value(deadzone);
			value = Math.ClampUnit(value);
		}
		else if (OverlapBehavior == Overlaps.TakeNewer)
		{
			var timestamp = 0d;
			for (int i = 0; i < Nodes.Count; i++)
			{
				var time = Nodes[i].Timestamp;
				var val = Nodes[i].Value(deadzone);

				if (time > 0 && Math.Abs(val) > EPSILON && time > timestamp)
				{
					value = val;
					timestamp = time;
				}
			}
		}
		else if (OverlapBehavior == Overlaps.TakeOlder)
		{
			var timestamp = double.MaxValue;
			for (int i = 0; i < Nodes.Count; i++)
			{
				var time = Nodes[i].Timestamp;
				var val = Nodes[i].Value(deadzone);

				if (time > 0 && Math.Abs(val) > EPSILON && time < timestamp)
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
		Nodes.Add(new KeyNode(Input, negative, false));
		Nodes.Add(new KeyNode(Input, positive, true));
		return this;
	}

	public VirtualAxis Add(Keys key, bool isPositive)
	{
		Nodes.Add(new KeyNode(Input, key, isPositive));
		return this;
	}

	public VirtualAxis Add(int controller, Buttons negative, Buttons positive)
	{
		Nodes.Add(new ButtonNode(Input, controller, negative, false));
		Nodes.Add(new ButtonNode(Input, controller, positive, true));
		return this;
	}

	public VirtualAxis Add(int controller, Buttons button, bool isPositive)
	{
		Nodes.Add(new ButtonNode(Input, controller, button, isPositive));
		return this;
	}

	public VirtualAxis Add(int controller, Axes axis, float deadzone = 0f)
	{
		Nodes.Add(new AxisNode(Input, controller, axis, deadzone, true));
		return this;
	}

	public VirtualAxis Add(int controller, Axes axis, bool inverse, float deadzone = 0f)
	{
		Nodes.Add(new AxisNode(Input, controller, axis, deadzone, !inverse));
		return this;
	}

	public void Clear()
	{
		Nodes.Clear();
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
		bool Down { get; }
		bool Released { get; }
		bool Repeated(float delay, float interval);
		void Update();
	}

	public class KeyNode : INode
	{
		public Input Input;
		public Keys Key;

		public bool Pressed(float buffer, long lastBufferConsumedTime)
		{
			if (Input.Keyboard.Pressed(Key))
				return true;

			var timestamp = Input.Keyboard.Timestamp(Key);
			var time = Time.duration.Ticks;

			if (time - timestamp <= TimeSpan.FromSeconds(buffer).Ticks && timestamp > lastBufferConsumedTime)
				return true;

			return false;
		}

		public bool Down => Input.Keyboard.Down(Key);
		public bool Released => Input.Keyboard.Released(Key);
		public bool Repeated(float delay, float interval) => Input.Keyboard.Repeated(Key, delay, interval);
		public void Update() { }

		public KeyNode(Input input, Keys key)
		{
			Input = input;
			Key = key;
		}
	}

	public class MouseButtonNode : INode
	{
		public Input Input;
		public MouseButtons MouseButton;

		public bool Pressed(float buffer, long lastBufferConsumedTime)
		{
			if (Input.Mouse.Pressed(MouseButton))
				return true;

			var timestamp = Input.Mouse.Timestamp(MouseButton);
			var time = Time.duration.Ticks;

			if (time - timestamp <= TimeSpan.FromSeconds(buffer).Ticks && timestamp > lastBufferConsumedTime)
				return true;

			return false;
		}

		public bool Down => Input.Mouse.Down(MouseButton);
		public bool Released => Input.Mouse.Released(MouseButton);
		public bool Repeated(float delay, float interval) => Input.Mouse.Repeated(MouseButton, delay, interval);
		public void Update() { }

		public MouseButtonNode(Input input, MouseButtons mouseButton)
		{
			Input = input;
			MouseButton = mouseButton;
		}
	}

	public class ButtonNode : INode
	{
		public Input Input;
		public int Index;
		public Buttons Button;

		public bool Pressed(float buffer, long lastBufferConsumedTime)
		{
			if (Input.Controllers[Index].Pressed(Button))
				return true;

			var timestamp = Input.Controllers[Index].Timestamp(Button);
			var time = Time.duration.Ticks;

			if (time - timestamp <= TimeSpan.FromSeconds(buffer).Ticks && timestamp > lastBufferConsumedTime)
				return true;

			return false;
		}

		public bool Down => Input.Controllers[Index].Down(Button);
		public bool Released => Input.Controllers[Index].Released(Button);
		public bool Repeated(float delay, float interval) => Input.Controllers[Index].Repeated(Button, delay, interval);
		public void Update() { }

		public ButtonNode(Input input, int controller, Buttons button)
		{
			Input = input;
			Index = controller;
			Button = button;
		}
	}

	public class AxisNode : INode
	{
		public Input Input;
		public int Index;
		public Axes Axis;
		public float Threshold;

		float pressedTimestamp;
		const float AXIS_EPSILON = 0.00001f;

		public bool Pressed(float buffer, long lastBufferConsumedTime)
		{
			if (Pressed())
				return true;

			var time = Time.duration.Ticks;

			if (time - pressedTimestamp <= TimeSpan.FromSeconds(buffer).Ticks && pressedTimestamp > lastBufferConsumedTime)
				return true;

			return false;
		}

		public bool Down
		{
			get
			{
				if (Math.Abs(Threshold) <= AXIS_EPSILON)
					return Math.Abs(Input.Controllers[Index].Axis(Axis)) > AXIS_EPSILON;

				if (Threshold < 0)
					return Input.Controllers[Index].Axis(Axis) <= Threshold;

				return Input.Controllers[Index].Axis(Axis) >= Threshold;
			}
		}

		public bool Released
		{
			get
			{
				if (Math.Abs(Threshold) <= AXIS_EPSILON)
					return Math.Abs(Input.LastState.Controllers[Index].Axis(Axis)) > AXIS_EPSILON && Math.Abs(Input.Controllers[Index].Axis(Axis)) < AXIS_EPSILON;

				if (Threshold < 0)
					return Input.LastState.Controllers[Index].Axis(Axis) <= Threshold && Input.Controllers[Index].Axis(Axis) > Threshold;

				return Input.LastState.Controllers[Index].Axis(Axis) >= Threshold && Input.Controllers[Index].Axis(Axis) < Threshold;
			}
		}

		public bool Repeated(float delay, float interval)
		{
			throw new NotImplementedException();
		}

		bool Pressed()
		{
			if (Math.Abs(Threshold) <= AXIS_EPSILON)
				return (Math.Abs(Input.LastState.Controllers[Index].Axis(Axis)) < AXIS_EPSILON && Math.Abs(Input.Controllers[Index].Axis(Axis)) > AXIS_EPSILON);

			if (Threshold < 0)
				return (Input.LastState.Controllers[Index].Axis(Axis) > Threshold && Input.Controllers[Index].Axis(Axis) <= Threshold);

			return (Input.LastState.Controllers[Index].Axis(Axis) < Threshold && Input.Controllers[Index].Axis(Axis) >= Threshold);
		}

		public void Update()
		{
			if (Pressed())
				pressedTimestamp = Input.Controllers[Index].Timestamp(Axis);
		}

		public AxisNode(Input input, int controller, Axes axis, float threshold)
		{
			Input = input;
			Index = controller;
			Axis = axis;
			Threshold = threshold;
		}
	}

	public readonly Input Input;
	public readonly List<INode> Nodes = new List<INode>();
	public float RepeatDelay;
	public float RepeatInterval;
	public float Buffer;

	long lastBufferConsumeTime;

	public bool Pressed
	{
		get
		{
			for (int i = 0; i < Nodes.Count; i++)
				if (Nodes[i].Pressed(Buffer, lastBufferConsumeTime))
					return true;

			return false;
		}
	}

	public bool Down
	{
		get
		{
			for (int i = 0; i < Nodes.Count; i++)
				if (Nodes[i].Down)
					return true;

			return false;
		}
	}

	public bool Released
	{
		get
		{
			for (int i = 0; i < Nodes.Count; i++)
				if (Nodes[i].Released)
					return true;

			return false;
		}
	}

	public bool Repeated
	{
		get
		{
			for (int i = 0; i < Nodes.Count; i++)
				if (Nodes[i].Pressed(Buffer, lastBufferConsumeTime) || Nodes[i].Repeated(RepeatDelay, RepeatInterval))
					return true;

			return false;
		}
	}

	public VirtualButton(Input input, float buffer = 0f)
	{
		Input = input;

		// Using a Weak Reference to subscribe this object to Updates
		// This way it's automatically collected if the user is no longer
		// using it, and we don't require the user to call a Dispose or
		// Unsubscribe callback
		Input.virtualButtons.Add(new WeakReference<VirtualButton>(this));

		RepeatDelay = Input.RepeatDelay;
		RepeatInterval = Input.RepeatInterval;
		Buffer = buffer;
	}

	public void ConsumeBuffer()
	{
		lastBufferConsumeTime = Time.duration.Ticks;
	}

	public VirtualButton Add(params Keys[] keys)
	{
		foreach (var key in keys)
			Nodes.Add(new KeyNode(Input, key));
		return this;
	}

	public VirtualButton Add(params MouseButtons[] buttons)
	{
		foreach (var button in buttons)
			Nodes.Add(new MouseButtonNode(Input, button));
		return this;
	}

	public VirtualButton Add(int controller, params Buttons[] buttons)
	{
		foreach (var button in buttons)
			Nodes.Add(new ButtonNode(Input, controller, button));
		return this;
	}

	public VirtualButton Add(int controller, Axes axis, float threshold)
	{
		Nodes.Add(new AxisNode(Input, controller, axis, threshold));
		return this;
	}

	public void Clear()
	{
		Nodes.Clear();
	}

	internal void Update()
	{
		for (int i = 0; i < Nodes.Count; i++)
			Nodes[i].Update();
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
	public VirtualAxis Horizontal;

	/// <summary>
	/// The Vertical Axis
	/// </summary>
	public VirtualAxis Vertical;

	/// <summary>
	/// This Deadzone is applied to the Length of the combined Horizontal and Vertical axis values
	/// </summary>
	public float CircularDeadzone = 0f;

	/// <summary>
	/// Gets the current Virtual Stick value
	/// </summary>
	public Vector2 Value
	{
		get
		{
			var result = new Vector2(Horizontal.Value, Vertical.Value);
			if (CircularDeadzone != 0 && result.length < CircularDeadzone)
				return Vector2.zero;
			return result;
		}
	}

	/// <summary>
	/// Gets the current Virtual Stick value, ignoring Deadzones
	/// </summary>
	public Vector2 ValueNoDeadzone => new Vector2(Horizontal.ValueNoDeadzone, Vertical.ValueNoDeadzone);

	/// <summary>
	/// Gets the current Virtual Stick value, clamping to Integer Values
	/// </summary>
	public Vector2Int IntValue
	{
		get
		{
			var result = Value;
			return new Vector2Int(MathF.Sign(result.x), MathF.Sign(result.y));
		}
	}

	/// <summary>
	/// Gets the current Virtual Stick value, clamping to Integer values and Ignoring Deadzones
	/// </summary>
	public Vector2Int IntValueNoDeadzone => new Vector2Int(Horizontal.IntValueNoDeadzone, Vertical.IntValueNoDeadzone);

	public VirtualStick(Input input, float circularDeadzone = 0f)
	{
		Horizontal = new VirtualAxis(input);
		Vertical = new VirtualAxis(input);
		CircularDeadzone = circularDeadzone;
	}

	public VirtualStick(Input input, VirtualAxis.Overlaps overlapBehavior, float circularDeadzone = 0f)
	{
		Horizontal = new VirtualAxis(input, overlapBehavior);
		Vertical = new VirtualAxis(input, overlapBehavior);
		CircularDeadzone = circularDeadzone;
	}

	public VirtualStick Add(Keys left, Keys right, Keys up, Keys down)
	{
		Horizontal.Add(left, right);
		Vertical.Add(up, down);
		return this;
	}

	public VirtualStick Add(int controller, Buttons left, Buttons right, Buttons up, Buttons down)
	{
		Horizontal.Add(controller, left, right);
		Vertical.Add(controller, up, down);
		return this;
	}

	public VirtualStick Add(int controller, Axes horizontal, Axes vertical, float deadzoneHorizontal = 0f, float deadzoneVertical = 0f)
	{
		Horizontal.Add(controller, horizontal, deadzoneHorizontal);
		Vertical.Add(controller, vertical, deadzoneVertical);
		return this;
	}

	public VirtualStick AddLeftJoystick(int controller, float deadzoneHorizontal = 0, float deadzoneVertical = 0)
	{
		Horizontal.Add(controller, Axes.LeftX, deadzoneHorizontal);
		Vertical.Add(controller, Axes.LeftY, deadzoneVertical);
		return this;
	}

	public VirtualStick AddRightJoystick(int controller, float deadzoneHorizontal = 0, float deadzoneVertical = 0)
	{
		Horizontal.Add(controller, Axes.RightX, deadzoneHorizontal);
		Vertical.Add(controller, Axes.RightY, deadzoneVertical);
		return this;
	}

	public void Clear()
	{
		Horizontal.Clear();
		Vertical.Clear();
	}
}
