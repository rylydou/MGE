using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace MGE.UI;

public struct MouseInfo
{
	public Vector2Int Position;
	public bool IsLeftButtonDown;
	public bool IsMiddleButtonDown;
	public bool IsRightButtonDown;
	public float Wheel;
}

public class UIDesktop
{
	public const int DoubleClickIntervalInMs = 500;
	public const int DoubleClickRadius = 2;

	private UIRenderContext? _renderContext;

	private bool _layoutDirty = true;
	private bool _widgetsDirty = true;
	private UIWidget? _focusedKeyboardWidget, _mouseInsideWidget;
	private readonly List<UIWidget> _widgetsCopy = new List<UIWidget>();
	private DateTime _lastTouchDown;
	private DateTime? _lastKeyDown;
	private int _keyDownCount = 0;
	private MouseInfo _lastMouseInfo;
	private readonly bool[] _downKeys = new bool[0xff], _lastDownKeys = new bool[0xff];
	private UIWidget? _previousKeyboardFocus;
	private TouchCollection _oldTouchState;
	private bool _isTouchDown;
	private Vector2Int _previousMousePosition, _mousePosition, _previousTouchPosition, _touchPosition;
	private bool _contextMenuShown = false;
	private bool _keyboardFocusSet = false;
	public bool HasExternalTextInput = false;

	/// <summary>
	/// Root UIWidget
	/// </summary>
	public UIWidget? Root
	{
		get
		{
			if (Widgets.Count == 0) return null;
			return Widgets[0];
		}

		set
		{
			if (Root == value) return;

			HideContextMenu();
			Widgets.Clear();

			if (value is not null)
			{
				Widgets.Add(value);
			}
		}
	}

	public bool[] DownKeys { get => _downKeys; }

	public Vector2Int PreviousMousePosition { get => _previousMousePosition; }

	public Vector2Int MousePosition
	{
		get => _mousePosition;
		private set
		{
			if (value == _mousePosition) return;

			_previousMousePosition = _mousePosition;
			_mousePosition = value;
			MouseMoved?.Invoke();

			ChildrenCopy.ProcessMouseMovement();

			if (IsTouchDown)
			{
				TouchPosition = MousePosition;
			}
		}
	}

	public Vector2Int TouchPosition
	{
		get => _touchPosition;

		private set
		{
			_previousTouchPosition = _touchPosition;

			if (value == _touchPosition) return;

			_touchPosition = value;
			TouchMoved?.Invoke();

			ChildrenCopy.ProcessTouchMovement();
		}
	}

	public UIHorizontalMenu MenuBar { get; set; }

	public Func<MouseInfo> MouseInfoGetter { get; set; }

	public Action<bool[]> DownKeysGetter { get; set; }

	internal List<UIWidget> ChildrenCopy
	{
		get
		{
			UpdateWidgetsCopy();
			return _widgetsCopy;
		}
	}

	public ObservableCollection<UIWidget> Widgets { get; } = new ObservableCollection<UIWidget>();

	public Func<RectInt> BoundsFetcher = DefaultBoundsFetcher;

	internal RectInt InternalBounds { get; private set; }

	public UIWidget? ContextMenu { get; private set; }

	/// <summary>
	/// UIWidget having keyboard focus
	/// </summary>
	public UIWidget? FocusedKeyboardWidget
	{
		get => _focusedKeyboardWidget;
		set
		{
			if (value is not null) _keyboardFocusSet = true;

			if (value == _focusedKeyboardWidget) return;

			var oldValue = _focusedKeyboardWidget;
			if (oldValue is not null)
			{
				if (WidgetLosingKeyboardFocus is not null)
				{
					var args = new CancellableEventArgs<UIWidget>(oldValue);

					WidgetLosingKeyboardFocus(null, args);

					if (oldValue.IsPlaced && args.Cancel) return;
				}
			}

			_focusedKeyboardWidget = value;
			if (oldValue is not null)
			{
				oldValue.OnLostKeyboardFocus();
			}

			if (_focusedKeyboardWidget is not null)
			{
				_focusedKeyboardWidget.OnGotKeyboardFocus();
				WidgetGotKeyboardFocus?.Invoke(_focusedKeyboardWidget);
			}
		}
	}

	public UIWidget? MouseInsideWidget
	{
		get => _mouseInsideWidget;
		set
		{
			if (value == _mouseInsideWidget)
			{
				return;
			}

			_mouseInsideWidget = value;
			MouseInsideWidgetChanged?.Invoke(this);
		}
	}

	private UIRenderContext UIRenderContext
	{
		get
		{
			EnsureRenderContext();
			return _renderContext;
		}
	}

	public float Opacity { get; set; }

	public bool IsMouseOverGUI { get => IsPointOverGUI(MousePosition); }

	public bool IsTouchOverGUI { get => IsPointOverGUI(TouchPosition); }

	internal bool IsShiftDown { get => IsKeyDown(Keys.LeftShift) || IsKeyDown(Keys.RightShift); }

	internal bool IsControlDown { get => IsKeyDown(Keys.LeftCtrl) || IsKeyDown(Keys.RightCtrl); }

	internal bool IsAltDown { get => IsKeyDown(Keys.LeftAlt) || IsKeyDown(Keys.RightAlt); }

	public bool IsTouchDown
	{
		get => _isTouchDown;
		set
		{
			if (_isTouchDown == value) return;
			_isTouchDown = value;
			if (_isTouchDown)
			{
				InputOnTouchDown();
				TouchDown?.Invoke();
			}
			else
			{
				InputOnTouchUp();
				TouchUp?.Invoke();
			}
		}
	}

	public int RepeatKeyDownStartInMs { get; set; } = 500;

	public int RepeatKeyDownInternalInMs { get; set; } = 50;

	public bool HasModalWidget
	{
		get
		{
			for (var i = ChildrenCopy.Count - 1; i >= 0; --i)
			{
				var w = ChildrenCopy[i];
				if (w.Visible && w.Enabled && w.IsModal) return true;
			}

			return false;
		}
	}

	private bool IsMenuBarActive { get => MenuBar is not null && (MenuBar.OpenMenuItem is not null || IsAltDown); }

	public Action<Keys>? KeyDownHandler;

	public event EventHandler? MouseMoved;

	public event EventHandler? TouchMoved;
	public event EventHandler? TouchDown;
	public event EventHandler? TouchUp;
	public event EventHandler? TouchDoubleClick;

	public event EventHandler<GenericEventArgs<float>>? MouseWheelChanged;

	public event EventHandler<GenericEventArgs<Keys>>? KeyUp;
	public event EventHandler<GenericEventArgs<Keys>>? KeyDown;
	public event EventHandler<GenericEventArgs<char>>? Char;

	public event EventHandler<CancellableEventArgs<UIWidget>>? ContextMenuClosing;
	public event EventHandler<GenericEventArgs<UIWidget>>? ContextMenuClosed;

	public event EventHandler<CancellableEventArgs<UIWidget>>? WidgetLosingKeyboardFocus;
	public event EventHandler<GenericEventArgs<UIWidget>>? WidgetGotKeyboardFocus;

	public event EventHandler? MouseInsideWidgetChanged;

	public UIDesktop()
	{
		Opacity = 1.0f;
		Widgets.CollectionChanged += WidgetsOnCollectionChanged;

		MouseInfoGetter = DefaultMouseInfoGetter;
		DownKeysGetter = DefaultDownKeysGetter;

		KeyDownHandler = OnKeyDown;

#if FNA
			TextInputEXT.TextInput += c =>
			{
				OnChar(c);
			};
#endif
	}

	public bool IsKeyDown(Keys keys)
	{
		return _downKeys[(int)keys];
	}

	public MouseInfo DefaultMouseInfoGetter()
	{
#if MONOGAME || FNA
			var state = Mouse.GetState();

			return new MouseInfo
			{
				Position = new Vector2Int(state.x, state.y),
				IsLeftButtonDown = state.LeftButton == ButtonState.Pressed,
				IsMiddleButtonDown = state.MiddleButton == ButtonState.Pressed,
				IsRightButtonDown = state.RightButton == ButtonState.Pressed,
				Wheel = state.ScrollWheelValue
			};
#elif STRIDE
			var input = MyraEnvironment.Game.Input;

			var v = input.AbsoluteMousePosition;

			return new MouseInfo
			{
				Position = new Vector2Int((int)v.x, (int)v.y),
				IsLeftButtonDown = input.IsMouseButtonDown(MouseButton.Left),
				IsMiddleButtonDown = input.IsMouseButtonDown(MouseButton.Middle),
				IsRightButtonDown = input.IsMouseButtonDown(MouseButton.Right),
				Wheel = input.MouseWheelDelta
			};
#else
		return MyraEnvironment.Platform.GetMouseInfo();
#endif
	}

	public void DefaultDownKeysGetter(bool[] keys)
	{
#if MONOGAME || FNA
			var state = Keyboard.GetState();
			for (var i = 0; i < keys.Length; ++i)
			{
				keys[i] = state.IsKeyDown((Keys)i);
			}
#elif STRIDE
			var input = MyraEnvironment.Game.Input;
			for (var i = 0; i < keys.Length; ++i)
			{
				keys[i] = input.IsKeyDown((Keys)i);
			}
#else
		MyraEnvironment.Platform.SetKeysDown(keys);
#endif
	}

	public UIWidget GetChild(int index) => ChildrenCopy[index];

	private void HandleDoubleClick()
	{
		if ((DateTime.Now - _lastTouchDown).TotalMilliseconds < DoubleClickIntervalInMs && Math.Abs(_touchPosition.x - _previousTouchPosition.x) <= DoubleClickRadius && Math.Abs(_touchPosition.y - _previousTouchPosition.y) <= DoubleClickRadius)
		{
			TouchDoubleClick?.Invoke();

			ChildrenCopy.ProcessTouchDoubleClick();

			_lastTouchDown = DateTime.MinValue;
		}
		else
		{
			_lastTouchDown = DateTime.Now;
		}
	}

	private void ContextMenuOnTouchDown()
	{
		if (ContextMenu is null || ContextMenu.Bounds.Contains(TouchPosition)) return;

		var ev = ContextMenuClosing;
		if (ev is not null)
		{
			var args = new CancellableEventArgs<UIWidget>(ContextMenu);
			ev(null, args);

			if (args.Cancel) return;
		}

		HideContextMenu();
	}

	private void InputOnTouchDown()
	{
		_contextMenuShown = false;
		_keyboardFocusSet = false;

		ChildrenCopy.ProcessTouchDown();

		if (!_keyboardFocusSet && FocusedKeyboardWidget is not null)
		{
			// Nullify keyboard focus
			FocusedKeyboardWidget = null;
		}

		if (!_contextMenuShown)
		{
			ContextMenuOnTouchDown();
		}
	}

	private void InputOnTouchUp()
	{
		ChildrenCopy.ProcessTouchUp();
	}

	public void ShowContextMenu(UIWidget menu, Vector2Int position)
	{
		HideContextMenu();

		ContextMenu = menu;
		if (ContextMenu is null) return;

		ContextMenu.HorizontalAlignment = UIAlignment.Start;
		ContextMenu.VerticalAlignment = UIAlignment.Start;

		var measure = ContextMenu.Measure(InternalBounds.size);

		if (position.x + measure.x > InternalBounds.right)
		{
			position.x = InternalBounds.right - measure.x;
		}

		if (position.y + measure.y > InternalBounds.bottom)
		{
			position.y = InternalBounds.bottom - measure.y;
		}

		ContextMenu.Left = position.x;
		ContextMenu.Top = position.y;

		ContextMenu.Visible = true;

		Widgets.Add(ContextMenu);

		if (ContextMenu.AcceptsKeyboardFocus)
		{
			_previousKeyboardFocus = FocusedKeyboardWidget;
			FocusedKeyboardWidget = ContextMenu;
		}

		_contextMenuShown = true;
	}

	public void HideContextMenu()
	{
		if (ContextMenu is null) return;

		Widgets.Remove(ContextMenu);
		ContextMenu.Visible = false;

		ContextMenuClosed?.Invoke(ContextMenu);
		ContextMenu = null;

		if (_previousKeyboardFocus is not null)
		{
			FocusedKeyboardWidget = _previousKeyboardFocus;
			_previousKeyboardFocus = null;
		}
	}

	private void WidgetsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
	{
		if (args.Action == NotifyCollectionChangedAction.Add)
		{
			foreach (UIWidget w in args.NewItems!)
			{
				w.Desktop = this;
			}
		}
		else if (args.Action == NotifyCollectionChangedAction.Remove)
		{
			foreach (UIWidget w in args.OldItems!)
			{
				w.Desktop = null;
			}
		}
		else if (args.Action == NotifyCollectionChangedAction.Reset)
		{
			foreach (UIWidget w in ChildrenCopy)
			{
				w.Desktop = null;
			}
		}

		InvalidateLayout();
		_widgetsDirty = true;
	}

	[MemberNotNull(nameof(_renderContext))]
	private void EnsureRenderContext()
	{
		if (_renderContext is null)
		{
			_renderContext = new UIRenderContext();

			// if (MyraEnvironment.LayoutScale.HasValue)
			// {
			// #if MONOGAME || FNA
			// _renderContext.Transform = Matrix.CreateScale(MyraEnvironment.LayoutScale.Value);
			// #elif STRIDE
			// _renderContext.Transform = Matrix.Scaling(MyraEnvironment.LayoutScale.Value);
			// #else
			// _renderContext.Transform = Matrix3x2.CreateScale(MyraEnvironment.LayoutScale.Value);
			// #endif
			// }
		}
	}

	public void RenderVisual()
	{
		EnsureRenderContext();

		var oldScissorRectangle = _renderContext.scissor;

		// _renderContext.Begin();

		// Disable transform during setting the scissor rectangle for the Desktop
		// var transform = _renderContext.Transform;
		// _renderContext.Transform = null;
		_renderContext.scissor = InternalBounds;
		// _renderContext.Transform = transform;

		// _renderContext.View = InternalBounds;
		_renderContext.opacity = Opacity;

		// if (Stylesheet.Current.DesktopStyle is not null && Stylesheet.Current.DesktopStyle.Background is not null)
		// {
		// 	Stylesheet.Current.DesktopStyle.Background.Draw(_renderContext, InternalBounds);
		// }

		foreach (var widget in ChildrenCopy)
		{
			if (widget.Visible)
			{
				widget.Render(_renderContext);
			}
		}

		// _renderContext.End();

		_renderContext.scissor = oldScissorRectangle;
	}

	public void Render()
	{
		UpdateInput();
		UpdateLayout();
		RenderVisual();
	}

	public void InvalidateLayout()
	{
		_layoutDirty = true;
	}

	public void UpdateLayout()
	{
		var newBounds = BoundsFetcher();
		if (InternalBounds != newBounds)
		{
			InvalidateLayout();
		}

		InternalBounds = newBounds;

		if (InternalBounds.isEmpty) return;
		if (!_layoutDirty) return;

		foreach (var i in ChildrenCopy)
		{
			if (i.Visible)
			{
				i.Layout(InternalBounds);
			}
		}

		// Rest processing
		MenuBar = null;
		var active = true;
		for (var i = ChildrenCopy.Count - 1; i >= 0; --i)
		{
			var w = ChildrenCopy[i];
			if (!w.Visible) continue;

			w.ProcessWidgets(widget =>
			{
				widget.Active = active;

				if (MenuBar is null && widget is UIHorizontalMenu)
				{
					// Found MenuBar
					MenuBar = (UIHorizontalMenu)widget;
				}

				// Continue
				return true;
			});

			// Everything after first modal widget is not active
			if (w.IsModal)
			{
				active = false;
			}
		}

		UpdateRecursiveLayout(ChildrenCopy);

		// Fire Mouse Movement without actual mouse movement in order to update UIWidget.IsMouseInside
		_previousMousePosition = _mousePosition;
		ChildrenCopy.ProcessMouseMovement();

		_layoutDirty = false;
	}

	internal void ProcessWidgets(Func<UIWidget, bool> operation)
	{
		for (var i = ChildrenCopy.Count - 1; i >= 0; --i)
		{
			var w = ChildrenCopy[i];
			var result = w.ProcessWidgets(operation);
			if (!result) return;
		}
	}

	private void UpdateRecursiveLayout(IEnumerable<UIWidget> widgets)
	{
		foreach (var i in widgets)
		{
			// if (!i.Layout2d.Nullable)
			// {
			// 	ExpressionParser.Parse(i, ChildrenCopy);
			// }

			var c = i as UIContainer;
			if (c is not null)
			{
				UpdateRecursiveLayout(c.ChildrenCopy);
			}
		}
	}

	private UIWidget? GetWidgetBy(UIWidget root, Func<UIWidget, bool> filter)
	{
		if (filter(root)) return root;

		var asContainer = root as UIContainer;
		if (asContainer is null) return null;

		for (var i = 0; i < asContainer.ChildrenCount; ++i)
		{
			var w = asContainer.GetChild(i);
			var result = GetWidgetBy(w, filter);
			if (result is not null) return result;
		}

		return null;
	}

	public UIWidget? GetWidgetBy(Func<UIWidget, bool> filter)
	{
		foreach (var w in ChildrenCopy)
		{
			var result = GetWidgetBy(w, filter);
			if (result is not null) return result;
		}

		return null;
	}

	public UIWidget? GetWidgetByID(string ID) => GetWidgetBy(w => w.Id == ID);

	public int CalculateTotalWidgets(bool visibleOnly)
	{
		var result = 0;
		foreach (var w in Widgets)
		{
			if (visibleOnly && !w.Visible) continue;

			++result;

			var asContainer = w as UIContainer;
			if (asContainer is not null)
			{
				result += asContainer.CalculateTotalChildCount(visibleOnly);
			}
		}

		return result;
	}

	private UIWidget? GetTopWidget()
	{
		for (var i = ChildrenCopy.Count - 1; i >= 0; --i)
		{
			var w = ChildrenCopy[i];
			if (w.Visible && w.Enabled && w.Active) return w;
		}

		return null;
	}

	public void HandleButton(bool isDown, bool wasDown, MouseButtons buttons)
	{
		if (isDown && !wasDown)
		{
			TouchPosition = MousePosition;
			IsTouchDown = true;
			HandleDoubleClick();
		}
		else if (!isDown && wasDown)
		{
			IsTouchDown = false;
		}
	}

	public void UpdateMouseInput()
	{
		if (MouseInfoGetter is null) return;

		var mouseInfo = MouseInfoGetter();
		var mousePosition = mouseInfo.Position;

		EnsureRenderContext();
		// if (_renderContext.Transform is not null)
		// {
		// 	// Apply transform
		// 	var t = Vector2.Transform(new Vector2(mousePosition.x, mousePosition.y), _renderContext.InverseTransform);

		// 	mousePosition = new Vector2Int((int)t.x, (int)t.y);
		// }

		MousePosition = mousePosition;

		HandleButton(mouseInfo.IsLeftButtonDown, _lastMouseInfo.IsLeftButtonDown, MouseButtons.Left);
		HandleButton(mouseInfo.IsMiddleButtonDown, _lastMouseInfo.IsMiddleButtonDown, MouseButtons.Middle);
		HandleButton(mouseInfo.IsRightButtonDown, _lastMouseInfo.IsRightButtonDown, MouseButtons.Right);
#if STRIDE
				var handleWheel = mouseInfo.Wheel != 0;
#else
		var handleWheel = mouseInfo.Wheel != _lastMouseInfo.Wheel;
#endif

		if (handleWheel)
		{
			var delta = mouseInfo.Wheel;
#if !STRIDE
			delta -= _lastMouseInfo.Wheel;
#endif
			MouseWheelChanged?.Invoke(delta);

			UIWidget? mouseWheelFocusedWidget = null;
			if (FocusedKeyboardWidget is not null && FocusedKeyboardWidget.MouseWheelFocusType == UIMouseWheelFocusType.Focus)
			{
				mouseWheelFocusedWidget = FocusedKeyboardWidget;
			}
			else
			{
				// Go through the parents chain in order to find first widget that accepts mouse wheel events
				var widget = MouseInsideWidget;
				while (widget is not null)
				{
					if (widget.MouseWheelFocusType == UIMouseWheelFocusType.Hover)
					{
						mouseWheelFocusedWidget = widget;
						break;
					}

					widget = widget.Parent;
				}
			}

			if (mouseWheelFocusedWidget is not null)
			{
				mouseWheelFocusedWidget.OnMouseWheel(delta);
			}
		}

		_lastMouseInfo = mouseInfo;
	}

	public void UpdateKeyboardInput()
	{
		if (DownKeysGetter is null) return;

		DownKeysGetter(_downKeys);

		var now = DateTime.Now;
		for (var i = 0; i < _downKeys.Length; ++i)
		{
			var key = (Keys)i;
			if (_downKeys[i] && !_lastDownKeys[i])
			{
				if (key == Keys.Tab)
				{
					FocusNextWidget();
				}

				KeyDownHandler?.Invoke(key);

				_lastKeyDown = now;
				_keyDownCount = 0;
			}
			else if (!_downKeys[i] && _lastDownKeys[i])
			{
				// Key had been released
				KeyUp?.Invoke(key);
				if (_focusedKeyboardWidget is not null && _focusedKeyboardWidget.Active)
				{
					_focusedKeyboardWidget.OnKeyUp(key);
				}

				_lastKeyDown = null;
				_keyDownCount = 0;
			}
			else if (_downKeys[i] && _lastDownKeys[i])
			{
				if (
					_lastKeyDown is not null &&
					((_keyDownCount == 0 && (now - _lastKeyDown.Value).TotalMilliseconds > RepeatKeyDownStartInMs) ||
					(_keyDownCount > 0 && (now - _lastKeyDown.Value).TotalMilliseconds > RepeatKeyDownInternalInMs))
				)
				{
					KeyDownHandler?.Invoke(key);

					_lastKeyDown = now;
					++_keyDownCount;
				}
			}
		}

		Array.Copy(_downKeys, _lastDownKeys, _downKeys.Length);
	}

	private void FocusNextWidget()
	{
		if (Widgets.Count == 0) return;

		var isNull = FocusedKeyboardWidget is null;
		var focusChanged = false;
		ProcessWidgets(w =>
		{
			if (isNull)
			{
				if (CanFocusWidget(w))
				{
					w.SetKeyboardFocus();
					focusChanged = true;
					return false;
				}
			}
			else
			{
				if (w == FocusedKeyboardWidget)
				{
					isNull = true;
					// Next widget will be focused
				}
			}

			return true;
		});

		// Either new focus had been set or there are no focusable widgets
		if (focusChanged || FocusedKeyboardWidget is null) return;

		// Next run - try to focus first widget before focused one
		ProcessWidgets(w =>
		{
			if (CanFocusWidget(w))
			{
				w.SetKeyboardFocus();
				return false;
			}

			return true;
		});
	}

	private static bool CanFocusWidget(UIWidget widget) =>
		widget is not null && widget.Visible && widget.Active &&
		widget.Enabled && widget.AcceptsKeyboardFocus;

	public void UpdateInput()
	{
		UpdateMouseInput();
		UpdateKeyboardInput();
	}

	public void OnKeyDown(Keys key)
	{
		KeyDown?.Invoke(key);

		if (IsMenuBarActive)
		{
			MenuBar.OnKeyDown(key);
		}
		else
		{
			if (_focusedKeyboardWidget is not null && _focusedKeyboardWidget.Active)
			{
				_focusedKeyboardWidget.OnKeyDown(key);

				if (!HasExternalTextInput && !IsControlDown && !IsAltDown)
				{
					var c = key.ToChar(IsShiftDown);
					if (c is not null)
					{
						OnChar(c.Value);
					}
				}
			}
		}

		if (key == Keys.Escape && ContextMenu is not null)
		{
			HideContextMenu();
		}
	}

	public void OnChar(char c)
	{
		// Don't accept chars if menubar is open
		if (IsMenuBarActive) return;

		if (_focusedKeyboardWidget is not null && _focusedKeyboardWidget.Active)
		{
			_focusedKeyboardWidget.OnChar(c);
		}

		Char?.Invoke(c);
	}

	private void UpdateWidgetsCopy()
	{
		if (!_widgetsDirty) return;

		_widgetsCopy.Clear();
		_widgetsCopy.AddRange(Widgets);

		_widgetsCopy.SortWidgetsByZIndex();

		_widgetsDirty = false;
	}

	private bool InternalIsPointOverGUI(Vector2Int p, UIWidget w)
	{
		if (!w.Visible || !w.BorderBounds.Contains(p)) return false;
		if (!w.FallsThrough(p)) return true;

		// If widget fell through, then it is UIContainer for sure
		var asContainer = (UIContainer)w;

		// Or if any child is solid
		foreach (var ch in asContainer.ChildrenCopy)
		{
			if (InternalIsPointOverGUI(p, ch)) return true;
		}
		return false;
	}

	public bool IsPointOverGUI(Vector2Int p)
	{
		foreach (var widget in ChildrenCopy)
		{
			if (InternalIsPointOverGUI(p, widget)) return true;
		}
		return false;
	}

	public static RectInt DefaultBoundsFetcher()
	{
		var size = CrossEngineStuff.ViewSize;
		return new(0, 0, size.x, size.y);
	}
}
