using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MGE.UI;

public class UICanvas
{
	const int _doubleClickIntervalInMs = 500;
	const int _doubleClickRadius = 2;
	const int _dragStartDist = 2;

	public UIRenderContext renderContext = new();

	bool _layoutDirty = true;
	bool _widgetsDirty = true;
	DateTime _lastClickDown;
	UIWidget? _previousFocus;
	bool _isClickDown;

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

	Vector2Int _mousePosition;
	Vector2Int _previousMousePosition;
	public Vector2Int mousePosition
	{
		get => _mousePosition;
		set
		{
			if (value == _mousePosition) return;

			_previousMousePosition = _mousePosition;
			_mousePosition = value;
			// MouseMoved.Invoke();

			// childrenCopy.ProcessMouseMovement();

			if (isClickDown)
			{
				clickPosition = mousePosition;
			}
		}
	}

	Vector2Int _clickPosition;
	Vector2Int _previousClickPosition;
	public Vector2Int clickPosition
	{
		get => _clickPosition;
		set
		{
			_previousClickPosition = _clickPosition;

			if (value == _clickPosition) return;

			_clickPosition = value;
			// TouchMoved.Invoke();

			// childrenCopy.ProcessDragMovement();
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

	bool _contextMenuShown = false;
	public UIWidget? contextMenu { get; set; }

	bool _focusSet = false;
	UIWidget? _focusedWidget;
	public UIWidget? focusedWidget
	{
		get => _focusedWidget;
		set
		{
			if (value == _focusedWidget) return;
			if (value is not null) _focusSet = true;

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
				oldValue.OnLostFocus();
			}

			if (_focusedWidget is not null)
			{
				_focusedWidget.OnGotFocus();
				// WidgetGotKeyboardFocus.Invoke(_focusedWidget);
			}
		}
	}

	UIWidget? _mouseInsideWidget;
	public UIWidget? hoveredWidget
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

	public bool isMouseOverGUI { get => IsPointOverGUI(mousePosition); }
	public bool isClickOverGUI { get => IsPointOverGUI(clickPosition); }

	internal bool isShiftDown { get => Input.IsButtonPressed(Button.KB_LeftShift) || Input.IsButtonPressed(Button.KB_RightShift); }
	internal bool isControlDown { get => Input.IsButtonPressed(Button.KB_LeftControl) || Input.IsButtonPressed(Button.KB_RightControl); }
	internal bool isAltDown { get => Input.IsButtonPressed(Button.KB_LeftAlt) || Input.IsButtonPressed(Button.KB_RightAlt); }

	public bool isClickDown
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

	public bool hasModalWidget
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

	public Func<UIWidget, bool> contextMenuClosing = (widget) => false /* Do not cancel */;
	// public Action<UIWidget> ContextMenuClosed = (widget) => { };

	// public Func<UIWidget, bool> widgetLosingKeyboardFocus = (widget) => false /* Do not cancel */;
	// public Action<UIWidget> WidgetGotKeyboardFocus = (widget) => { };

	// public Action<UIWidget?> MouseInsideWidgetChanged = (widget) => { };

	//  UICanvas()
	// {
	// 	widgets.CollectionChanged += WidgetsOnCollectionChanged;
	// }

	public UIWidget? GetWidgetAtPoint(Vector2Int point)
	{
		return root!.GetWidgetAtPoint(point);
	}

	public UIWidget GetChild(int index) => childrenCopy[index];

	void HandleDoubleClick()
	{
		if ((DateTime.Now - _lastClickDown).TotalMilliseconds < _doubleClickIntervalInMs && Vector2.DistanceLessThan(_clickPosition, _previousClickPosition, _doubleClickRadius))
		{
			// TouchDoubleClick();

			// childrenCopy.ProcessTouchDoubleClick();

			_lastClickDown = DateTime.MinValue;
		}
		else
		{
			_lastClickDown = DateTime.Now;
		}
	}

	void ContextMenuOnTouchDown()
	{
		if (contextMenu is null || contextMenu.bounds.Contains(clickPosition)) return;

		var cancel = contextMenuClosing(contextMenu);

		if (cancel) return;

		HideContextMenu();
	}

	void OnClickDown()
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

	void OnClickReleased()
	{

	}

	public void ShowContextMenu(UIWidget menu, Vector2Int position)
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

	public void HideContextMenu()
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

	void WidgetsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
	{
		if (args.Action == NotifyCollectionChangedAction.Add)
		{
			foreach (UIWidget w in args.NewItems!)
			{
				w.canvas = this;
			}
		}
		else if (args.Action == NotifyCollectionChangedAction.Remove)
		{
			foreach (UIWidget w in args.OldItems!)
			{
				w.canvas = null;
			}
		}
		else if (args.Action == NotifyCollectionChangedAction.Reset)
		{
			foreach (UIWidget w in childrenCopy)
			{
				w.canvas = null;
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
		var newBounds = new RectInt(GameWindow.current.Size);
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

		// childrenCopy.ProcessMouseMovement();

		_layoutDirty = false;
	}

	void UpdateMousePosition()
	{
		SetHoveredWidget(root!.GetWidgetAtPoint(_mousePosition));
	}

	UIWidget? _hoverredWidget;

	void SetHoveredWidget(UIWidget? widget)
	{
		if (_hoverredWidget is not null)
		{
			_hoverredWidget.hovered = false;
		}

		if (widget is not null)
		{
			widget.hovered = true;
		}

		_hoverredWidget = widget;
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

	public UIWidget? GetWidgetByID(string ID) => GetWidgetBy(w => w.id == ID);

	public int CalculateTotalWidgets(bool visibleOnly)
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

	UIWidget? GetTopWidget()
	{
		for (var i = childrenCopy.Count - 1; i >= 0; --i)
		{
			var w = childrenCopy[i];
			if (w.visible && w.enabled && w.active) return w;
		}

		return null;
	}

	public void HandleButton(bool isDown, bool wasDown, Button buttons)
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

	public void UpdateMouseInput()
	{
		UpdateMousePosition();

		// 		HandleButton(mouseInfo.IsLeftButtonDown, _lastMouseInfo.IsLeftButtonDown, Button.Mouse_Left);
		// 		HandleButton(mouseInfo.IsMiddleButtonDown, _lastMouseInfo.IsMiddleButtonDown, Button.Mouse_Middle);
		// 		HandleButton(mouseInfo.IsRightButtonDown, _lastMouseInfo.IsRightButtonDown, Button.Mouse_Right);

		// #if STRIDE
		// 				var handleWheel = mouseInfo.Wheel != 0;
		// #else
		// 		var handleWheel = mouseInfo.Wheel != _lastMouseInfo.Wheel;
		// #endif

		// 		if (handleWheel)
		// 		{
		// 			var delta = mouseInfo.Wheel;
		// #if !STRIDE
		// 			delta -= _lastMouseInfo.Wheel;
		// #endif
		// 			// MouseWheelChanged(delta);

		// 			UIWidget? mouseWheelFocusedWidget = null;
		// 			if (focusedWidget is not null && focusedWidget.mouseWheelFocusType == UIMouseWheelFocusType.Focus)
		// 			{
		// 				mouseWheelFocusedWidget = focusedWidget;
		// 			}
		// 			else
		// 			{
		// 				// Go through the parents chain in order to find first widget that accepts mouse wheel events
		// 				var widget = mouseInsideWidget;
		// 				while (widget is not null)
		// 				{
		// 					if (widget.mouseWheelFocusType == UIMouseWheelFocusType.Hover)
		// 					{
		// 						mouseWheelFocusedWidget = widget;
		// 						break;
		// 					}

		// 					widget = widget.parent;
		// 				}
		// 			}

		// 			if (mouseWheelFocusedWidget is not null)
		// 			{
		// 				mouseWheelFocusedWidget.OnMouseWheel(delta);
		// 			}
		// 		}
	}

	public void UpdateKeyboardInput()
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
		{
			_focusedWidget.ButtonEntered(button);
		}

		foreach (var button in Input.GetKeysPressed())
		{
			_focusedWidget.ButtonPressed(button);
		}

		foreach (var button in Input.GetKeysDown())
		{
			_focusedWidget.ButtonDown(button);
		}

		foreach (var button in Input.GetKeysReleased())
		{
			_focusedWidget.ButtonReleased(button);
		}
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

	bool CanFocusWidget(UIWidget? widget) => widget is not null && widget.visible && widget.active && widget.enabled && widget.acceptsKeyboardFocus;

	public void UpdateInput()
	{
		UpdateMouseInput();
		UpdateKeyboardInput();
	}

	void UpdateWidgetsCopy()
	{
		if (!_widgetsDirty) return;

		_widgetsCopy.Clear();
		_widgetsCopy.AddRange(widgets);

		_widgetsCopy.SortWidgetsByZIndex();

		_widgetsDirty = false;
	}

	// bool InternalIsPointOverGUI(Vector2Int p, UIWidget w)
	// {
	// 	if (!w.visible || !w.borderBounds.Contains(p)) return false;
	// 	if (!w.FallsThrough(p)) return true;

	// 	// If widget fell through, then it is UIContainer for sure
	// 	var asContainer = (UIContainer)w;

	// 	// Or if any child is solid
	// 	foreach (var ch in asContainer.ChildrenCopy)
	// 	{
	// 		if (InternalIsPointOverGUI(p, ch)) return true;
	// 	}
	// 	return false;
	// }

	public bool IsPointOverGUI(Vector2Int point)
	{
		return root!.GetWidgetAtPoint(point) is not null;
	}
}
