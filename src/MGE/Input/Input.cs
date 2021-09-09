using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MGE
{
	public static class Input
	{
		#region Mouse

		public static Vector2 mousePosition = Vector2.zero;

		public static bool mouseIsInWindow = false;

		static List<Buttons> _mouseButtons = new List<Buttons>();
		static List<Buttons> _oldMouseButtons = new List<Buttons>();

		public static int scroll = 0;
		public static int scrollRaw = 0;
		static int _mouseWheelAdditionPrev = 0;

		#endregion

		#region Keyboard

		public static string keyboardString = string.Empty;
		public static Keys keyboardKey = Keys.None;
		public static Keys keyboardLastKey = Keys.None;
		public static char keyboardLastChar = ' ';

		static StringBuilder _keyboardBuffer = new StringBuilder();
		static Keys _keyboardLastKeyBuffer = Keys.None;
		static List<Keys> _currentKeys = new List<Keys>();
		static List<Keys> _oldKeys = new List<Keys>();

		#endregion

		#region Gamepad

		public static float gamepadTriggersDeadzone = 0.5f;

		public static GamePadDeadZone gamepadDeadzoneType = GamePadDeadZone.Circular;

		static int _maxGamepadCount = 4;
		public static int maxGamepadCount
		{
			get => _maxGamepadCount;
			set
			{
				_maxGamepadCount = Math.Clamp(value, 0, GamePad.MaximumGamePadCount);
				GamepadInit();
			}
		}

		static List<Buttons>[] _gamepadButtons = new List<Buttons>[_maxGamepadCount];
		static List<Buttons>[] _oldGamepadButtons = new List<Buttons>[_maxGamepadCount];

		static GamePadState[] _gamepadState = new GamePadState[_maxGamepadCount];
		static GamePadState[] _oldGamepadState = new GamePadState[_maxGamepadCount];

		#endregion

		const int _keyboardMaxCode = 1000;
		const int _mouseMaxCode = 2000;
		const int _gamepadMaxCode = 3000;

		static bool _mouseCleared = false;
		static bool _keyboardCleared = false;
		static bool _gamepadCleared = false;

		public static void Update()
		{
			_mouseCleared = false;
			_keyboardCleared = false;
			_gamepadCleared = false;

			#region Mouse

			var mouseState = Mouse.GetState();

			// windowMousePosition = GUI.CanvasToWindow(mouseState.Position);
			// cameraMousePosition = Camera.WinToCam(mouseState.Position);

			// mouseIsInWindow =
			// windowMousePosition.x >= 0 && windowMousePosition.x < Window.renderSize.x &&
			// windowMousePosition.y >= 0 && windowMousePosition.y < Window.renderSize.y;

			mousePosition = mouseState.Position;

			_oldMouseButtons = _mouseButtons;
			_mouseButtons = new List<Buttons>();

			if (mouseState.LeftButton == ButtonState.Pressed)
				_mouseButtons.Add(Buttons.Mouse_Left);

			if (mouseState.RightButton == ButtonState.Pressed)
				_mouseButtons.Add(Buttons.Mouse_Right);

			if (mouseState.MiddleButton == ButtonState.Pressed)
				_mouseButtons.Add(Buttons.Mouse_Middle);

			scrollRaw = _mouseWheelAdditionPrev - mouseState.ScrollWheelValue;
			_mouseWheelAdditionPrev = mouseState.ScrollWheelValue;

			scroll = Math.Sign(scrollRaw);

			#endregion

			#region Keyboard

			keyboardString = _keyboardBuffer.ToString();
			_keyboardBuffer.Clear();

			if (keyboardString.Length > 0)
				keyboardLastChar = keyboardString[keyboardString.Length - 1];

			keyboardLastKey = _keyboardLastKeyBuffer;

			_oldKeys.Clear();
			_oldKeys.AddRange(_currentKeys);
			_currentKeys.Clear();
			_currentKeys.AddRange(Keyboard.GetState().GetPressedKeys());

			if (_currentKeys.Count > 0)
				keyboardKey = _currentKeys[_currentKeys.Count - 1];
			else
				keyboardKey = Keys.None;

			#endregion

			#region Gamepad

			for (var i = 0; i < maxGamepadCount; i += 1)
			{
				_oldGamepadState[i] = _gamepadState[i];

				_gamepadState[i] = GamePad.GetState(i, gamepadDeadzoneType);

				_oldGamepadButtons[i] = _gamepadButtons[i];
				_gamepadButtons[i] = new List<Buttons>();

				if (_gamepadState[i].DPad.Left == ButtonState.Pressed)
					_gamepadButtons[i].Add(Buttons.Gamepad_DPad_Left);
				if (_gamepadState[i].DPad.Right == ButtonState.Pressed)
					_gamepadButtons[i].Add(Buttons.Gamepad_DPad_Right);
				if (_gamepadState[i].DPad.Up == ButtonState.Pressed)
					_gamepadButtons[i].Add(Buttons.Gamepad_DPad_Up);
				if (_gamepadState[i].DPad.Down == ButtonState.Pressed)
					_gamepadButtons[i].Add(Buttons.Gamepad_DPad_Down);

				if (_gamepadState[i].Buttons.A == ButtonState.Pressed)
					_gamepadButtons[i].Add(Buttons.Gamepad_A);
				if (_gamepadState[i].Buttons.B == ButtonState.Pressed)
					_gamepadButtons[i].Add(Buttons.Gamepad_B);
				if (_gamepadState[i].Buttons.X == ButtonState.Pressed)
					_gamepadButtons[i].Add(Buttons.Gamepad_X);
				if (_gamepadState[i].Buttons.Y == ButtonState.Pressed)
					_gamepadButtons[i].Add(Buttons.Gamepad_Y);

				if (_gamepadState[i].Buttons.LeftShoulder == ButtonState.Pressed)
					_gamepadButtons[i].Add(Buttons.Gamepad_LB);
				if (_gamepadState[i].Buttons.RightShoulder == ButtonState.Pressed)
					_gamepadButtons[i].Add(Buttons.Gamepad_RB);

				if (_gamepadState[i].Triggers.Left > gamepadTriggersDeadzone)
					_gamepadButtons[i].Add(Buttons.Gamepad_LT);
				if (_gamepadState[i].Triggers.Right > gamepadTriggersDeadzone)
					_gamepadButtons[i].Add(Buttons.Gamepad_RT);

				if (_gamepadState[i].Buttons.LeftStick == ButtonState.Pressed)
					_gamepadButtons[i].Add(Buttons.Gamepad_LS);
				if (_gamepadState[i].Buttons.RightStick == ButtonState.Pressed)
					_gamepadButtons[i].Add(Buttons.Gamepad_RS);

				if (_gamepadState[i].Buttons.Start == ButtonState.Pressed)
					_gamepadButtons[i].Add(Buttons.Gamepad_Start);
				if (_gamepadState[i].Buttons.Back == ButtonState.Pressed)
					_gamepadButtons[i].Add(Buttons.Gamepad_Select);
			}

			#endregion
		}

		#region Button Checks

		public static bool IsButtonHeld(Buttons button, int index = 0)
		{
			var buttonCode = (int)button;

			return
				index <
				_gamepadButtons.Length && !_gamepadCleared && _gamepadButtons[index].Contains(button) ||
				buttonCode < _keyboardMaxCode && !_keyboardCleared && _currentKeys.Contains((Keys)button) ||
				buttonCode < _mouseMaxCode && !_mouseCleared && _mouseButtons.Contains(button);
		}

		public static bool IsButtonPressed(Buttons button, int index = 0)
		{
			var buttonCode = (int)button;

			return
				index <
				_gamepadButtons.Length && !_gamepadCleared && _gamepadButtons[index].Contains(button) && !_oldGamepadButtons[index].Contains(button) ||
				buttonCode < _keyboardMaxCode && !_keyboardCleared && _currentKeys.Contains((Keys)button) && !_oldKeys.Contains((Keys)button) ||
				buttonCode < _mouseMaxCode && !_mouseCleared && _mouseButtons.Contains(button) && !_oldMouseButtons.Contains(button);
		}

		public static bool IsButtonReleased(Buttons button, int index = 0)
		{
			var buttonCode = (int)button;

			return
			index <
				_gamepadButtons.Length && !_gamepadCleared && !_gamepadButtons[index].Contains(button) && _oldGamepadButtons[index].Contains(button) ||
				buttonCode < _keyboardMaxCode && !_keyboardCleared && !_currentKeys.Contains((Keys)button) && _oldKeys.Contains((Keys)button) ||
				buttonCode < _mouseMaxCode && !_mouseCleared && !_mouseButtons.Contains(button) && _oldMouseButtons.Contains(button);
		}

		#endregion

		#region Mouse

		public static void ClearMouseInput() => _mouseCleared = true;

		#endregion

		#region Keyboard

		public static bool IsKeyboardHeld() => !_keyboardCleared && _currentKeys.Count > 0;
		public static bool IsKeyboardPressed() => !_keyboardCleared && _currentKeys.Count > 0 && _oldKeys.Count == 0;
		public static bool IsKeyboardReleased() => !_keyboardCleared && _currentKeys.Count == 0 && _oldKeys.Count > 0;

		public static void ClearKeyboardInput() => _keyboardCleared = true;

		internal static void WhenTextInput(TextInputEventArgs args)
		{
			_keyboardBuffer.Append(args.Character);
			_keyboardLastKeyBuffer = args.Key;
		}

		#endregion

		#region Gamepad

		internal static void GamepadInit()
		{
			_gamepadButtons = new List<Buttons>[_maxGamepadCount];
			for (var i = 0; i < _gamepadButtons.Length; i += 1)
				_gamepadButtons[i] = new List<Buttons>();

			_oldGamepadButtons = new List<Buttons>[_maxGamepadCount];
			for (var i = 0; i < _oldGamepadButtons.Length; i += 1)
				_oldGamepadButtons[i] = new List<Buttons>();

			_gamepadState = new GamePadState[_maxGamepadCount];
			for (var i = 0; i < _gamepadState.Length; i += 1)
				_gamepadState[i] = new GamePadState();

			_oldGamepadState = _gamepadState;
		}

		public static bool GetGamepadConnected(int index = 0)
		{
			if (!_gamepadCleared && index < _gamepadState.Length)
				return _gamepadState[index].IsConnected;
			return false;
		}
		public static Vector2 GetGamepadLeftStick(int index = 0)
		{
			if (!_gamepadCleared && index < _gamepadState.Length)
				return _gamepadState[index].ThumbSticks.Left;
			return Vector2.zero;
		}
		public static Vector2 GetGamepadRightStick(int index = 0)
		{
			if (!_gamepadCleared && index < _gamepadState.Length)
				return _gamepadState[index].ThumbSticks.Right;
			return Vector2.zero;
		}

		// public static void SetGamepadVibration3D(Vector3 position, float strength, int index = 0)
		// {
		// 	position.Normalize();
		// 	var multiplier = 1.0f - position.Length();

		// 	GamePad.SetVibration(index, strength * (float)Math.Clamp01(multiplier - 0.25f), strength * (float)Math.Clamp01(multiplier + 0.25f));
		// }
		public static void SetGamepadVibrationRaw(float leftMotor, float rightMotor, int index = 0) =>
			GamePad.SetVibration(index, leftMotor, rightMotor);

		public static void ClearGamepadInput() => _gamepadCleared = true;

		#endregion

		public static void ClearAllInput()
		{
			ClearMouseInput();
			ClearKeyboardInput();
			ClearGamepadInput();
		}
	}
}