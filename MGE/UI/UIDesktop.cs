using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MGE.UI;

public static class UICanvas
{
	const int _doubleClickIntervalInMs = 500;
	const int _doubleClickRadius = 2;

	public static UIRenderContext renderContext = new();

	static bool _layoutDirty = true;
	static bool _widgetsDirty = true;
	static UIWidget? _focusedWidget, _mouseInsideWidget;
	static DateTime _lastClickDown;
	static UIWidget? _previousFocus;
	static bool _isClickDown;
	static Vector2Int _previousMousePosition, _mousePosition, _previousClickPosition, _clickPosition;
	static bool _contextMenuShown = false;
	static bool _focusSet = false;
	public static bool hasExternalTextInput = false;

	public static UIWidget? root
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

	public static Vector2Int previousMousePosition { get => _previousMousePosition; }

	public static Vector2Int mousePosition
	{
		get => _mousePosition;
		set
		{
			if (value == _mousePosition) return;

			_previousMousePosition = _mousePosition;
			_mousePosition = value;
			// MouseMoved.Invoke();

			childrenCopy.ProcessMouseMovement();

			if (isClickDown)
			{
				clickPosition = mousePosition;
			}
		}
	}

	public static Vector2Int clickPosition
	{
		get => _clickPosition;
		set
		{
			_previousClickPosition = _clickPosition;

			if (value == _clickPosition) return;

			_clickPosition = value;
			// TouchMoved.Invoke();

			childrenCopy.ProcessDragMovement();
		}
	}

	static readonly List<UIWidget> _widgetsCopy = new();
	internal static List<UIWidget> childrenCopy
	{
		get
		{
			UpdateWidgetsCopy();
			return _widgetsCopy;
		}
	}

	public static ObservableCollection<UIWidget> widgets { get; } = new ObservableCollection<UIWidget>();

	internal static RectInt internalBounds { get; set; }

	public static UIWidget? contextMenu { get; set; }

	/// <summary>
	/// UIWidget having keyboard focus
	/// </summary>
	public static UIWidget? focusedWidget
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

	public static UIWidget? mouseInsideWidget
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

	public static bool isMouseOverGUI { get => IsPointOverGUI(mousePosition); }
	public static bool isClickOverGUI { get => IsPointOverGUI(clickPosition); }

	internal static bool isShiftDown { get => Input.IsButtonPressed(Button.KB_LeftShift) || Input.IsButtonPressed(Button.KB_RightShift); }
	internal static bool isControlDown { get => Input.IsButtonPressed(Button.KB_LeftControl) || Input.IsButtonPressed(Button.KB_RightControl); }
	internal static bool isAltDown { get => Input.IsButtonPressed(Button.KB_LeftAlt) || Input.IsButtonPressed(Button.KB_RightAlt); }

	public static bool isClickDown
	{
		get => _isClickDown;
		set
		{
			if (_isClickDown == value) return;
			_isClickDown = value;
			if (_isClickDown)
			{
				OnClickDown();
				// TouchDown.Invoke();
			}
			else
			{
				OnClickReleased();
				// TouchUp.Invoke();
			}
		}
	}

	public static bool hasModalWidget
	{
		get
		{
			for (var i = childrenCopy.Count - 1; i >= 0; --i)
			{
				var w = childrenCopy[i];
				if (w.visible && w.enabled && w.isModal) return true;
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

	// static UICanvas()
	// {
	// 	widgets.CollectionChanged += WidgetsOnCollectionChanged;
	// }

	public static UIWidget? GetWidgetAtPoint(Vector2Int point)
	{
		return root!.GetWidgetAtPoint(point);
	}

	public static UIWidget GetChild(int index) => childrenCopy[index];

	static void HandleDoubleClick()
	{
		if ((DateTime.Now - _lastClickDown).TotalMilliseconds < _doubleClickIntervalInMs && Vector2.DistanceLessThan(_clickPosition, _previousClickPosition, _doubleClickRadius))
		{
			// TouchDoubleClick();

			childrenCopy.ProcessTouchDoubleClick();

			_lastClickDown = DateTime.MinValue;
		}
		else
		{
			_lastClickDown = DateTime.Now;
		}
	}

	static void ContextMenuOnTouchDown()
	{
		if (contextMenu is null || contextMenu.bounds.Contains(clickPosition)) return;

		var ev = ContextMenuClosing;
		var cancel = ev(contextMenu);

		if (cancel) return;

		HideContextMenu();
	}

	static void OnClickDown()
	{
		_contextMenuShown = false;
		_focusSet = false;

		var widget = GetWidgetAtPoint(Input.mousePosition);

		if (widget is not null)
		{
			widget.OnClick();
		}

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

	static void OnClickReleased()
	{

	}

	public static void ShowContextMenu(UIWidget menu, Vector2Int position)
	{
		HideContextMenu();

		contextMenu = menu;
		if (contextMenu is null) return;

		contextMenu.horizontalAlignment = UIAlignment.Start;
		contextMenu.verticalAlignment = UIAlignment.Start;

		var measure = contextMenu.Measure(internalBounds.size);

		if (position.x + measure.x > internalBounds.right)
		{
			position.x = internalBounds.right - measure.x;
		}

		if (position.y + measure.y > internalBounds.bottom)
		{
			position.y = internalBounds.bottom - measure.y;
		}

		contextMenu.left = position.x;
		contextMenu.top = position.y;

		contextMenu.visible = true;

		widgets.Add(contextMenu);

		if (contextMenu.acceptsKeyboardFocus)
		{
			_previousFocus = focusedWidget;
			focusedWidget = contextMenu;
		}

		_contextMenuShown = true;
	}

	public static void HideContextMenu()
	{
		if (contextMenu is null) return;

		widgets.Remove(contextMenu);
		contextMenu.visible = false;

		// ContextMenuClosed(contextMenu);
		contextMenu = null;

		if (_previousFocus is not null)
		{
			focusedWidget = _previousFocus;
			_previousFocus = null;
		}
	}

	static void WidgetsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
	{
		if (args.Action == NotifyCollectionChangedAction.Add)
		{
			foreach (UIWidget w in args.NewItems!)
			{
				w.isPlaced = true;
			}
		}
		else if (args.Action == NotifyCollectionChangedAction.Remove)
		{
			foreach (UIWidget w in args.OldItems!)
			{
				w.isPlaced = false;
			}
		}
		else if (args.Action == NotifyCollectionChangedAction.Reset)
		{
			foreach (UIWidget w in childrenCopy)
			{
				w.isPlaced = false;
			}
		}

		InvalidateLayout();
		_widgetsDirty = true;
	}

	public static void RenderVisual()
	{
		var oldScissorRectangle = renderContext.scissor;

		// _renderContext.Begin();

		// Disable transform during setting the scissor rectangle for the Desktop
		// var transform = _renderContext.Transform;
		// _renderContext.Transform = null;
		renderContext.scissor = internalBounds;
		// _renderContext.Transform = transform;

		// _renderContext.View = InternalBounds;

		// if (Stylesheet.Current.DesktopStyle is not null && Stylesheet.Current.DesktopStyle.Background is not null)
		// {
		// 	Stylesheet.Current.DesktopStyle.Background.Draw(_renderContext, InternalBounds);
		// }

		foreach (var widget in childrenCopy)
		{
			if (widget.visible)
			{
				widget.Render(renderContext);
			}
		}

		// _renderContext.End();

		renderContext.scissor = oldScissorRectangle;
	}

	public static void Render()
	{
		UpdateInput();
		UpdateLayout();
		RenderVisual();
	}

	public static void InvalidateLayout()
	{
		_layoutDirty = true;
	}

	public static void UpdateLayout()
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
			if (i.visible)
			{
				i.Layout(internalBounds);
			}
		}

		// Rest processing
		var active = true;
		for (var i = childrenCopy.Count - 1; i >= 0; --i)
		{
			var w = childrenCopy[i];
			if (!w.visible) continue;

			w.ProcessWidgets(widget =>
			{
				widget.active = active;

				// Continue
				return true;
			});

			// Everything after first modal widget is not active
			if (w.isModal)
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

	internal static void ProcessWidgets(Func<UIWidget, bool> operation)
	{
		for (var i = childrenCopy.Count - 1; i >= 0; --i)
		{
			var w = childrenCopy[i];
			var result = w.ProcessWidgets(operation);
			if (!result) return;
		}
	}

	static void UpdateRecursiveLayout(IEnumerable<UIWidget> widgets)
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

	static UIWidget? GetWidgetBy(UIWidget root, Func<UIWidget, bool> filter)
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

	public static UIWidget? GetWidgetBy(Func<UIWidget, bool> filter)
	{
		foreach (var w in childrenCopy)
		{
			var result = GetWidgetBy(w, filter);
			if (result is not null) return result;
		}

		return null;
	}

	public static UIWidget? GetWidgetByID(string ID) => GetWidgetBy(w => w.Id == ID);

	public static int CalculateTotalWidgets(bool visibleOnly)
	{
		var result = 0;
		foreach (var w in widgets)
		{
			if (visibleOnly && !w.visible) continue;

			++result;

			var asContainer = w as UIContainer;
			if (asContainer is not null)
			{
				result += asContainer.CalculateTotalChildCount(visibleOnly);
			}
		}

		return result;
	}

	static UIWidget? GetTopWidget()
	{
		for (var i = childrenCopy.Count - 1; i >= 0; --i)
		{
			var w = childrenCopy[i];
			if (w.visible && w.enabled && w.active) return w;
		}

		return null;
	}

	public static void HandleButton(bool isDown, bool wasDown, Button buttons)
	{
		if (isDown && !wasDown)
		{
			clickPosition = mousePosition;
			isClickDown = true;
			HandleDoubleClick();
		}
		else if (!isDown && wasDown)
		{
			isClickDown = false;
		}
	}

	public static void UpdateMouseInput()
	{
		mousePosition = Input.mousePosition;

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
			if (focusedWidget is not null && focusedWidget.mouseWheelFocusType == UIMouseWheelFocusType.Focus)
			{
				mouseWheelFocusedWidget = focusedWidget;
			}
			else
			{
				// Go through the parents chain in order to find first widget that accepts mouse wheel events
				var widget = mouseInsideWidget;
				while (widget is not null)
				{
					if (widget.mouseWheelFocusType == UIMouseWheelFocusType.Hover)
					{
						mouseWheelFocusedWidget = widget;
						break;
					}

					widget = widget.parent;
				}
			}

			if (mouseWheelFocusedWidget is not null)
			{
				mouseWheelFocusedWidget.OnMouseWheel(delta);
			}
		}
	}

	public static void UpdateKeyboardInput()
	{
		// TODO
		if (Input.IsKeyEntered(Button.KB_Tab))
		{
			FocusNextWidget();
			return;
		}

		if (_focusedWidget is null || !_focusedWidget.active) return;

		_focusedWidget.TextInput(Input.textInput);

		foreach (var button in Input.GetKeysEntered())
			_focusedWidget.ButtonEntered(button);

		foreach (var button in Input.GetKeysPressed())
			_focusedWidget.ButtonPressed(button);

		foreach (var button in Input.GetKeysDown())
			_focusedWidget.ButtonDown(button);

		foreach (var button in Input.GetKeysReleased())
			_focusedWidget.ButtonReleased(button);
	}

	static void FocusNextWidget()
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

	static bool CanFocusWidget(UIWidget? widget) => widget is not null && widget.visible && widget.active && widget.enabled && widget.acceptsKeyboardFocus;

	public static void UpdateInput()
	{
		UpdateMouseInput();
		UpdateKeyboardInput();
	}

	public static void OnButtonDown(Button key)
	{
		KeyDown(key);

		if (isMenuBarActive)
		{
			MenuBar.OnKeyDown(key);
		}
		else
		{
			if (_focusedWidget is not null && _focusedWidget.active)
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

	public static void OnChar(char c)
	{
		// Don't accept chars if menubar is open
		if (isMenuBarActive) return;

		if (_focusedWidget is not null && _focusedWidget.active)
		{
			_focusedWidget.OnChar(c);
		}

		Char(c);
	}

	static void UpdateWidgetsCopy()
	{
		if (!_widgetsDirty) return;

		_widgetsCopy.Clear();
		_widgetsCopy.AddRange(widgets);

		_widgetsCopy.SortWidgetsByZIndex();

		_widgetsDirty = false;
	}

	static bool InternalIsPointOverGUI(Vector2Int p, UIWidget w)
	{
		if (!w.visible || !w.borderBounds.Contains(p)) return false;
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

	public static bool IsPointOverGUI(Vector2Int p)
	{
		foreach (var widget in childrenCopy)
		{
			if (InternalIsPointOverGUI(p, widget)) return true;
		}
		return false;
	}
}
