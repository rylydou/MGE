using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MGE.UI;

public class UIDesktop
{
	public const int DoubleClickIntervalInMs = 500;
	public const int DoubleClickRadius = 2;

	public UIRenderContext renderContext = new();

	bool _layoutDirty = true;
	bool _widgetsDirty = true;
	UIWidget? _focusedWidget, _mouseInsideWidget;
	DateTime _lastTouchDown;
	UIWidget? _previousFocus;
	bool _isTouchDown;
	Vector2Int _previousMousePosition, _mousePosition, _previousTouchPosition, _touchPosition;
	bool _contextMenuShown = false;
	bool _focusSet = false;
	public bool hasExternalTextInput = false;

	/// <summary>
	/// Root UIWidget
	/// </summary>
	public UIWidget? root
	{
		get
		{
			if (widgets.Count == 0) return null;
			return widgets[0];
		}

		set
		{
			if (root == value) return;

			HideContextMenu();
			widgets.Clear();

			if (value is not null)
			{
				widgets.Add(value);
			}
		}
	}

	public Vector2Int previousMousePosition { get => _previousMousePosition; }

	public Vector2Int mousePosition
	{
		get => _mousePosition;
		set
		{
			if (value == _mousePosition) return;

			_previousMousePosition = _mousePosition;
			_mousePosition = value;
			// MouseMoved.Invoke();

			childrenCopy.ProcessMouseMovement();

			if (isTouchDown)
			{
				touchPosition = mousePosition;
			}
		}
	}

	public Vector2Int touchPosition
	{
		get => _touchPosition;

		set
		{
			_previousTouchPosition = _touchPosition;

			if (value == _touchPosition) return;

			_touchPosition = value;
			// TouchMoved.Invoke();

			childrenCopy.ProcessTouchMovement();
		}
	}

	readonly List<UIWidget> _widgetsCopy = new();
	internal List<UIWidget> childrenCopy
	{
		get
		{
			UpdateWidgetsCopy();
			return _widgetsCopy;
		}
	}

	public ObservableCollection<UIWidget> widgets { get; } = new ObservableCollection<UIWidget>();

	internal RectInt internalBounds { get; set; }

	public UIWidget? contextMenu { get; set; }

	/// <summary>
	/// UIWidget having keyboard focus
	/// </summary>
	public UIWidget? focusedWidget
	{
		get => _focusedWidget;
		set
		{
			if (value is not null) _focusSet = true;

			if (value == _focusedWidget) return;

			var oldValue = _focusedWidget;
			if (oldValue is not null)
			{
				// if (WidgetLosingKeyboardFocus is not null)
				// {
				// 	var cancel = WidgetLosingKeyboardFocus(oldValue);
				// 	if (oldValue.IsPlaced && cancel) return;
				// }
			}

			_focusedWidget = value;
			if (oldValue is not null)
			{
				oldValue.OnLostKeyboardFocus();
			}

			if (_focusedWidget is not null)
			{
				_focusedWidget.OnGotKeyboardFocus();
				// WidgetGotKeyboardFocus.Invoke(_focusedWidget);
			}
		}
	}

	public UIWidget? mouseInsideWidget
	{
		get => _mouseInsideWidget;
		set
		{
			if (value == _mouseInsideWidget)
			{
				return;
			}

			_mouseInsideWidget = value;
			// MouseInsideWidgetChanged(mouseInsideWidget);
		}
	}

	public float opacity { get; set; }

	public bool isMouseOverGUI { get => IsPointOverGUI(mousePosition); }

	public bool isTouchOverGUI { get => IsPointOverGUI(touchPosition); }

	internal bool isShiftDown { get => IsKeyDown(Button.KB_LeftShift) || IsKeyDown(Button.KB_RightShift); }

	internal bool isControlDown { get => IsKeyDown(Button.KB_LeftControl) || IsKeyDown(Button.KB_RightControl); }

	internal bool isAltDown { get => IsKeyDown(Button.KB_LeftAlt) || IsKeyDown(Button.KB_RightAlt); }

	public bool isTouchDown
	{
		get => _isTouchDown;
		set
		{
			if (_isTouchDown == value) return;
			_isTouchDown = value;
			if (_isTouchDown)
			{
				InputOnTouchDown();
				// TouchDown.Invoke();
			}
			else
			{
				InputOnTouchUp();
				// TouchUp.Invoke();
			}
		}
	}

	public bool hasModalWidget
	{
		get
		{
			for (var i = childrenCopy.Count - 1; i >= 0; --i)
			{
				var w = childrenCopy[i];
				if (w.Visible && w.Enabled && w.IsModal) return true;
			}

			return false;
		}
	}

	// public Action<Button> KeyDownHandler = (keys) => { };

	// public Action MouseMoved = () => { };

	// public Action TouchMoved = () => { };
	// public Action TouchDown = () => { };
	// public Action TouchUp = () => { };
	// public Action TouchDoubleClick = () => { };

	// public Action<float> MouseWheelChanged = (delta) => { };

	// public Action<Button> KeyUp = (keys) => { };
	// public Action<Button> KeyDown = (keys) => { };
	// public Action<Button> Char = (keys) => { };

	// public Func<UIWidget, bool> ContextMenuClosing = (widget) => false /* Do not cancel */;
	// public Action<UIWidget> ContextMenuClosed = (widget) => { };

	// public Func<UIWidget, bool> WidgetLosingKeyboardFocus = (widget) => false /* Do not cancel */;
	// public Action<UIWidget> WidgetGotKeyboardFocus = (widget) => { };

	// public Action<UIWidget?> MouseInsideWidgetChanged = (widget) => { };

	public UIDesktop()
	{
		opacity = 1.0f;
		widgets.CollectionChanged += WidgetsOnCollectionChanged;
	}

	public bool IsKeyDown(Button keys)
	{
		return _downKeys[(int)keys];
	}

	public UIWidget GetChild(int index) => childrenCopy[index];

	void HandleDoubleClick()
	{
		if ((DateTime.Now - _lastTouchDown).TotalMilliseconds < DoubleClickIntervalInMs && Vector2.DistanceLessThan(_touchPosition, _previousTouchPosition, DoubleClickRadius))
		{
			// TouchDoubleClick();

			childrenCopy.ProcessTouchDoubleClick();

			_lastTouchDown = DateTime.MinValue;
		}
		else
		{
			_lastTouchDown = DateTime.Now;
		}
	}

	void ContextMenuOnTouchDown()
	{
		if (contextMenu is null || contextMenu.Bounds.Contains(touchPosition)) return;

		var ev = ContextMenuClosing;
		var cancel = ev(contextMenu);

		if (cancel) return;

		HideContextMenu();
	}

	void InputOnTouchDown()
	{
		_contextMenuShown = false;
		_focusSet = false;

		childrenCopy.ProcessTouchDown();

		if (!_focusSet && focusedWidget is not null)
		{
			// Nullify keyboard focus
			focusedWidget = null;
		}

		if (!_contextMenuShown)
		{
			ContextMenuOnTouchDown();
		}
	}

	void InputOnTouchUp()
	{
		childrenCopy.ProcessTouchUp();
	}

	public void ShowContextMenu(UIWidget menu, Vector2Int position)
	{
		HideContextMenu();

		contextMenu = menu;
		if (contextMenu is null) return;

		contextMenu.HorizontalAlignment = UIAlignment.Start;
		contextMenu.VerticalAlignment = UIAlignment.Start;

		var measure = contextMenu.Measure(internalBounds.size);

		if (position.x + measure.x > internalBounds.right)
		{
			position.x = internalBounds.right - measure.x;
		}

		if (position.y + measure.y > internalBounds.bottom)
		{
			position.y = internalBounds.bottom - measure.y;
		}

		contextMenu.Left = position.x;
		contextMenu.Top = position.y;

		contextMenu.Visible = true;

		widgets.Add(contextMenu);

		if (contextMenu.AcceptsKeyboardFocus)
		{
			_previousFocus = focusedWidget;
			focusedWidget = contextMenu;
		}

		_contextMenuShown = true;
	}

	public void HideContextMenu()
	{
		if (contextMenu is null) return;

		widgets.Remove(contextMenu);
		contextMenu.Visible = false;

		// ContextMenuClosed(contextMenu);
		contextMenu = null;

		if (_previousFocus is not null)
		{
			focusedWidget = _previousFocus;
			_previousFocus = null;
		}
	}

	void WidgetsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
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
			foreach (UIWidget w in childrenCopy)
			{
				w.Desktop = null;
			}
		}

		InvalidateLayout();
		_widgetsDirty = true;
	}

	public void RenderVisual()
	{
		var oldScissorRectangle = renderContext.scissor;

		// _renderContext.Begin();

		// Disable transform during setting the scissor rectangle for the Desktop
		// var transform = _renderContext.Transform;
		// _renderContext.Transform = null;
		renderContext.scissor = internalBounds;
		// _renderContext.Transform = transform;

		// _renderContext.View = InternalBounds;
		renderContext.opacity = opacity;

		// if (Stylesheet.Current.DesktopStyle is not null && Stylesheet.Current.DesktopStyle.Background is not null)
		// {
		// 	Stylesheet.Current.DesktopStyle.Background.Draw(_renderContext, InternalBounds);
		// }

		foreach (var widget in childrenCopy)
		{
			if (widget.Visible)
			{
				widget.Render(renderContext);
			}
		}

		// _renderContext.End();

		renderContext.scissor = oldScissorRectangle;
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
		var newBounds = new(GameWindow.current.Size);
		if (internalBounds != newBounds)
		{
			InvalidateLayout();
		}

		internalBounds = newBounds;

		if (internalBounds.isEmpty) return;
		if (!_layoutDirty) return;

		foreach (var i in childrenCopy)
		{
			if (i.Visible)
			{
				i.Layout(internalBounds);
			}
		}

		// Rest processing
		var active = true;
		for (var i = childrenCopy.Count - 1; i >= 0; --i)
		{
			var w = childrenCopy[i];
			if (!w.Visible) continue;

			w.ProcessWidgets(widget =>
			{
				widget.Active = active;

				// Continue
				return true;
			});

			// Everything after first modal widget is not active
			if (w.IsModal)
			{
				active = false;
			}
		}

		UpdateRecursiveLayout(childrenCopy);

		// Fire Mouse Movement without actual mouse movement in order to update UIWidget.IsMouseInside
		_previousMousePosition = _mousePosition;
		childrenCopy.ProcessMouseMovement();

		_layoutDirty = false;
	}

	internal void ProcessWidgets(Func<UIWidget, bool> operation)
	{
		for (var i = childrenCopy.Count - 1; i >= 0; --i)
		{
			var w = childrenCopy[i];
			var result = w.ProcessWidgets(operation);
			if (!result) return;
		}
	}

	void UpdateRecursiveLayout(IEnumerable<UIWidget> widgets)
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

	UIWidget? GetWidgetBy(UIWidget root, Func<UIWidget, bool> filter)
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
		foreach (var w in childrenCopy)
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
		foreach (var w in widgets)
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

	UIWidget? GetTopWidget()
	{
		for (var i = childrenCopy.Count - 1; i >= 0; --i)
		{
			var w = childrenCopy[i];
			if (w.Visible && w.Enabled && w.Active) return w;
		}

		return null;
	}

	public void HandleButton(bool isDown, bool wasDown, Button buttons)
	{
		if (isDown && !wasDown)
		{
			touchPosition = mousePosition;
			isTouchDown = true;
			HandleDoubleClick();
		}
		else if (!isDown && wasDown)
		{
			isTouchDown = false;
		}
	}

	public void UpdateMouseInput()
	{
		mousePosition = mouseInfo.Position;

		// if (_renderContext.Transform is not null)
		// {
		// 	// Apply transform
		// 	var t = Vector2.Transform(new Vector2(mousePosition.x, mousePosition.y), _renderContext.InverseTransform);

		// 	mousePosition = new Vector2Int((int)t.x, (int)t.y);
		// }

		HandleButton(mouseInfo.IsLeftButtonDown, _lastMouseInfo.IsLeftButtonDown, Button.Mouse_Left);
		HandleButton(mouseInfo.IsMiddleButtonDown, _lastMouseInfo.IsMiddleButtonDown, Button.Mouse_Middle);
		HandleButton(mouseInfo.IsRightButtonDown, _lastMouseInfo.IsRightButtonDown, Button.Mouse_Right);
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
			// MouseWheelChanged(delta);

			UIWidget? mouseWheelFocusedWidget = null;
			if (focusedWidget is not null && focusedWidget.MouseWheelFocusType == UIMouseWheelFocusType.Focus)
			{
				mouseWheelFocusedWidget = focusedWidget;
			}
			else
			{
				// Go through the parents chain in order to find first widget that accepts mouse wheel events
				var widget = mouseInsideWidget;
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
	}

	public void UpdateKeyboardInput()
	{
		var now = DateTime.Now;
		for (var i = 0; i < _downKeys.Length; ++i)
		{
			var key = (Button)i;
			if (_downKeys[i] && !_lastDownKeys[i])
			{
				if (key == Button.KB_Tab)
				{
					FocusNextWidget();
				}

				KeyDownHandler(key);

				_lastKeyDown = now;
				_keyDownCount = 0;
			}
			else if (!_downKeys[i] && _lastDownKeys[i])
			{
				// Key had been released
				KeyUp(key);
				if (_focusedWidget is not null && _focusedWidget.Active)
				{
					_focusedWidget.OnKeyUp(key);
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
					KeyDownHandler(key);

					_lastKeyDown = now;
					++_keyDownCount;
				}
			}
		}

		Array.Copy(_downKeys, _lastDownKeys, _downKeys.Length);
	}

	void FocusNextWidget()
	{
		if (widgets.Count == 0) return;

		var isNull = focusedWidget is null;
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
				if (w == focusedWidget)
				{
					isNull = true;
					// Next widget will be focused
				}
			}

			return true;
		});

		// Either new focus had been set or there are no focusable widgets
		if (focusChanged || focusedWidget is null) return;

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

	static bool CanFocusWidget(UIWidget widget) =>
		widget is not null && widget.Visible && widget.Active &&
		widget.Enabled && widget.AcceptsKeyboardFocus;

	public void UpdateInput()
	{
		UpdateMouseInput();
		UpdateKeyboardInput();
	}

	public void OnButtonDown(Button key)
	{
		KeyDown(key);

		if (isMenuBarActive)
		{
			MenuBar.OnKeyDown(key);
		}
		else
		{
			if (_focusedWidget is not null && _focusedWidget.Active)
			{
				_focusedWidget.OnKeyDown(key);

				if (!hasExternalTextInput && !isControlDown && !isAltDown)
				{
					var c = key.ToChar(isShiftDown);
					if (c is not null)
					{
						OnChar(c.Value);
					}
				}
			}
		}

		if (key == Button.KB_Escape && contextMenu is not null)
		{
			HideContextMenu();
		}
	}

	public void OnChar(char c)
	{
		// Don't accept chars if menubar is open
		if (isMenuBarActive) return;

		if (_focusedWidget is not null && _focusedWidget.Active)
		{
			_focusedWidget.OnChar(c);
		}

		Char(c);
	}

	void UpdateWidgetsCopy()
	{
		if (!_widgetsDirty) return;

		_widgetsCopy.Clear();
		_widgetsCopy.AddRange(widgets);

		_widgetsCopy.SortWidgetsByZIndex();

		_widgetsDirty = false;
	}

	bool InternalIsPointOverGUI(Vector2Int p, UIWidget w)
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
		foreach (var widget in childrenCopy)
		{
			if (InternalIsPointOverGUI(p, widget)) return true;
		}
		return false;
	}
}
