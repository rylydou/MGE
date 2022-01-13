using System;

namespace MGE.UI;

public struct Thickness : IEquatable<Thickness>
{
	public int left;
	public int top;
	public int right;
	public int bottom;

	public Thickness(int left, int top, int right, int bottom)
	{
		this.left = left;
		this.top = top;
		this.right = right;
		this.bottom = bottom;
	}

	public int width { get => left + right; }
	public int height { get => top + bottom; }

	public static bool operator ==(Thickness a, Thickness b) => a.Equals(b);
	public static bool operator !=(Thickness a, Thickness b) => !a.Equals(b);

	public override bool Equals(object? obj) => obj is Thickness thickness && Equals(thickness);
	public bool Equals(Thickness other)
	{
		return
			left == other.left &&
			top == other.top &&
			right == other.right &&
			bottom == other.bottom;
	}

	public override int GetHashCode() => HashCode.Combine(left, top, right, bottom);

	public static RectInt operator -(RectInt a, Thickness b)
	{
		var result = a;
		result.x += b.left;
		result.y += b.top;

		result.width -= b.width;
		if (result.width < 0)
		{
			result.width = 0;
		}

		result.height -= b.height;
		if (result.height < 0)
		{
			result.height = 0;
		}

		return result;
	}
}

public class UIWidget
{
	enum LayoutState
	{
		Normal,
		LocationInvalid,
		Invalid
	}

	UICanvas? _canvas;
	public virtual UICanvas? canvas
	{
		get => _canvas;

		internal set
		{
			if (_canvas is not null && value is null)
			{
				if (_canvas.focusedWidget == this)
				{
					_canvas.focusedWidget = null;
				}

				if (_canvas.hoveredWidget == this)
				{
					_canvas.hoveredWidget = null;
				}
			}

			_canvas = value;
			isHoveredWithin = false;
			isFocused = false;
			isClicked = false;

			if (_canvas is not null)
			{
				InvalidateLayout();
			}

			// SubscribeOnTouchMoved(IsPlaced && IsDraggable);
			OnPlacedChanged();
		}
	}

	public string? id;

	Vector2Int? _dragStartPos;

	LayoutState _layoutState = LayoutState.Invalid;
	bool _measureDirty = true;
	bool _isModal;
	bool _active;

	Vector2Int _lastMeasureSize;
	Vector2Int _lastMeasureAvailableSize;
	Vector2Int _lastLocationHint;

	int _left;
	public int left
	{
		get => _left;
		set
		{
			if (_left == value) return;

			_left = value;
			if (_layoutState == LayoutState.Normal)
			{
				_layoutState = LayoutState.LocationInvalid;
			}

			FireLocationChanged();
		}
	}

	int _top;
	public int top
	{
		get => _top;
		set
		{
			if (_top == value) return;

			_top = value;
			if (_layoutState == LayoutState.Normal)
			{
				_layoutState = LayoutState.LocationInvalid;
			}

			FireLocationChanged();
		}
	}

	int? _minWidth;
	public int? minWidth
	{
		get => _minWidth;
		set
		{
			if (_minWidth == value) return;
			_minWidth = value;
			InvalidateMeasure();
			FireSizeChanged();
		}
	}

	int? _maxWidth;
	public int? maxWidth
	{
		get => _maxWidth;
		set
		{
			if (_maxWidth == value) return;
			_maxWidth = value;
			InvalidateMeasure();
			FireSizeChanged();
		}
	}

	int? _width;
	public int? width
	{
		get => _width;
		set
		{
			if (_width == value) return;
			_width = value;
			InvalidateMeasure();
			FireSizeChanged();
		}
	}

	int? _minHeight;
	public int? minHeight
	{
		get => _minHeight;
		set
		{
			if (_minHeight == value) return;
			_minHeight = value;
			InvalidateMeasure();
			FireSizeChanged();
		}
	}

	int? _maxHeight;
	public int? maxHeight
	{
		get => _maxHeight;
		set
		{
			if (_maxHeight == value) return;
			_maxHeight = value;
			InvalidateMeasure();
			FireSizeChanged();
		}
	}

	int? _height;
	public int? height
	{
		get => _height;
		set
		{
			if (_height == value) return;
			_height = value;
			InvalidateMeasure();
			FireSizeChanged();
		}
	}

	Thickness _margin;
	public Thickness margin
	{
		get => _margin;
		set
		{
			if (_margin == value) return;
			_margin = value;
			InvalidateMeasure();
		}
	}

	Thickness _borderThickness;
	public Thickness borderThickness
	{
		get => _borderThickness;

		set
		{
			if (_borderThickness == value) return;
			_borderThickness = value;
			InvalidateMeasure();
		}
	}

	Thickness _padding;
	public Thickness padding
	{
		get => _padding;
		set
		{
			if (_padding == value) return;
			_padding = value;
			InvalidateMeasure();
		}
	}

	UIAlignment _horizontalAlignment = UIAlignment.Start;
	public virtual UIAlignment horizontalAlignment
	{
		get => _horizontalAlignment;
		set
		{
			if (_horizontalAlignment == value) return;
			_horizontalAlignment = value;
			InvalidateMeasure();
		}
	}

	UIAlignment _verticalAlignment = UIAlignment.Start;
	public virtual UIAlignment verticalAlignment
	{
		get => _verticalAlignment;
		set
		{
			if (_verticalAlignment == value) return;
			_verticalAlignment = value;
			InvalidateMeasure();
		}
	}

	int _gridColumn;
	public int gridColumn
	{
		get => _gridColumn;
		set
		{
			if (_gridColumn == value) return;
			if (value < 0) throw new ArgumentOutOfRangeException("value");
			_gridColumn = value;
			InvalidateMeasure();
		}
	}

	int _gridRow;
	public int gridRow
	{
		get => _gridRow;
		set
		{
			if (_gridRow == value) return;
			if (value < 0) throw new ArgumentOutOfRangeException("value");
			_gridRow = value;
			InvalidateMeasure();
		}
	}

	int _gridColumnSpan = 1;
	public int gridColumnSpan
	{
		get => _gridColumnSpan;
		set
		{
			if (_gridColumnSpan == value) return;
			if (value < 0) throw new ArgumentOutOfRangeException("value");
			_gridColumnSpan = value;
			InvalidateMeasure();
		}
	}

	int _gridRowSpan = 1;
	public int gridRowSpan
	{
		get => _gridRowSpan;
		set
		{
			if (_gridRowSpan == value) return;
			if (value < 0) throw new ArgumentOutOfRangeException("value");
			_gridRowSpan = value;
			InvalidateMeasure();
		}
	}

	bool _enabled;
	public virtual bool enabled
	{
		get => _enabled;
		set
		{
			if (_enabled == value) return;
			_enabled = value;
			enabledChanged();
		}
	}

	bool _visible;
	public virtual bool visible
	{
		get => _visible;
		set
		{
			if (_visible == value) return;
			_visible = value;
			isHoveredWithin = false;
			isClicked = false;
			OnVisibleChanged();
		}
	}

	int _zIndex;
	public int zIndex
	{
		get => _zIndex;
		set
		{
			if (_zIndex == value) return;
			_zIndex = value;
			InvalidateMeasure();
		}
	}

	public UIWidget? dragHandle { get; set; }

	int _relativeLeft;
	int _relativeTop;
	int _relativeRight;
	int _relativeBottom;

	public bool isPlaced { get => canvas is not null; }

	public bool isModal
	{
		get => _isModal;
		set
		{
			if (_isModal == value) return;
			_isModal = value;
			InvalidateMeasure();
		}
	}

	protected internal bool active
	{
		get => _active;
		set
		{
			if (_active == value) return;
			_active = value;
			OnActiveChanged();
		}
	}

	// public Layout2D layout2d { get; set; } = Layout2D.NullLayout;

	// 	// public IBrush background { get; set; }

	// 	// public IBrush overBackground { get; set; }

	// 	// public IBrush disabledBackground { get; set; }

	// 	// public IBrush focusedBackground { get; set; }

	// 	// public IBrush border { get; set; }

	// 	// public IBrush overBorder { get; set; }

	// 	// public IBrush disabledBorder { get; set; }

	// 	// public IBrush focusedBorder { get; set; }

	public virtual bool clipToBounds { get; set; }

	public bool hovered { get; internal set; }
	public bool isHoveredWithin { get; internal set; }

	public bool acceptsKeyboardFocus = true;
	public bool isFocused { get; private set; }

	public bool isClicked { get; private set; }

	public UIContainer? parent { get; internal set; }

	internal bool containsMouse { get => borderBounds.Contains(canvas!.mousePosition); }
	internal bool containsClick { get => borderBounds.Contains(canvas!.clickPosition); }

	RectInt _bounds;
	public RectInt bounds { get => _bounds; }

	RectInt _actualBounds;
	public RectInt actualBounds { get => _actualBounds; }

	RectInt _containerBounds;
	public RectInt containerBounds { get => _containerBounds; }

	internal RectInt borderBounds { get => _bounds - _margin; }
	protected RectInt backgroundBounds { get => borderBounds - _borderThickness; }

	public int mbpWidth { get => margin.left + margin.right + borderThickness.left + borderThickness.right + padding.left + padding.right; }
	public int mbpHeight { get => margin.top + margin.bottom + borderThickness.top + borderThickness.bottom + padding.top + padding.bottom; }

	public Action placedChanged = () => { };
	public Action visibleChanged = () => { };
	public Action enabledChanged = () => { };

	public Action locationChanged = () => { };
	public Action sizeChanged = () => { };
	public Action layoutUpdated = () => { };

	// public Action MouseLeft = () => { };
	// public Action MouseEntered = () => { };
	// public Action MouseMoved = () => { };

	// public Action TouchLeft = () => { };
	// public Action TouchEntered = () => { };
	// public Action TouchMoved = () => { };
	// public Action TouchDown = () => { };
	// public Action TouchUp = () => { };
	// public Action TouchDoubleClick = () => { };

	// public Action KeyboardFocusChanged = () => { };

	// public Action<float> MouseWheelChanged = (delta) => { };

	// public Action<Button> KeyUp = (keys) => { };
	// public Action<Button> KeyDown = (keys) => { };
	// public Action<Button> Char = (keys) => { };

	public Action<UIRenderContext> beforeRender = (context) => { };
	public Action<UIRenderContext> afterRender = (context) => { };

	public UIWidget()
	{
		visible = true;
		enabled = true;
	}

	// public virtual IBrush GetCurrentBackground()
	// {
	// 	var result = background;

	// 	if (!enabled && disabledBackground is not null)
	// 	{
	// 		result = disabledBackground;
	// 	}
	// 	else if (enabled && isKeyboardFocused && focusedBackground is not null)
	// 	{
	// 		result = focusedBackground;
	// 	}
	// 	else if (useHoverRenderable && overBackground is not null)
	// 	{
	// 		result = overBackground;
	// 	}

	// 	return result;
	// }

	// public virtual IBrush GetCurrentBorder()
	// {
	// 	var result = border;

	// 	if (!enabled && disabledBorder is not null)
	// 	{
	// 		result = disabledBorder;
	// 	}
	// 	else if (enabled && isKeyboardFocused && focusedBorder is not null)
	// 	{
	// 		result = focusedBorder;
	// 	}
	// 	else if (useHoverRenderable && overBorder is not null)
	// 	{
	// 		result = overBorder;
	// 	}

	// 	return result;
	// }

	public void Render(UIRenderContext context)
	{
		if (!visible) return;

		UpdateLayout();

		var oldScissorRectangle = context.scissor;
		if (clipToBounds)
		{
			var newScissorRectangle = RectInt.Intersect(oldScissorRectangle, bounds);
			if (newScissorRectangle.isEmpty) return;

			context.scissor = newScissorRectangle;
		}

		beforeRender(context);

		GFX.DrawBox(backgroundBounds, Color.white.WithAlpha(0.05f));

		// Background
		// var background = GetCurrentBackground();
		// if (background is not null)
		// {
		// 	background.Draw(context, backgroundBounds);
		// }

		GFX.DrawRect(borderBounds, Color.green.translucent, borderThickness.left);
		// Borders
		// var border = GetCurrentBorder();
		// if (border is not null)
		// {
		// 	var borderBounds = this.borderBounds;
		// 	if (borderThickness.left > 0)
		// 	{
		// 		border.Draw(context, new(borderBounds.x, borderBounds.y, borderThickness.left, borderBounds.height));
		// 	}

		// 	if (borderThickness.top > 0)
		// 	{
		// 		border.Draw(context, new(borderBounds.x, borderBounds.y, borderBounds.width, borderThickness.top));
		// 	}

		// 	if (borderThickness.right > 0)
		// 	{
		// 		border.Draw(context, new(borderBounds.right - borderThickness.right, borderBounds.y, borderThickness.right, borderBounds.height));
		// 	}

		// 	if (borderThickness.bottom > 0)
		// 	{
		// 		border.Draw(context, new(borderBounds.x, borderBounds.bottom - borderThickness.bottom, borderBounds.width, borderThickness.bottom));
		// 	}
		// }

		InternalRender(context);

		afterRender(context);

		// Restore context settings

		// Optional debug rendering
		// if (MyraEnvironment.DrawWidgetsFrames)
		// {
		// 	context.DrawRectangle(Bounds, Color.green);
		// }

		// if (MyraEnvironment.DrawKeyboardFocusedWidgetFrame && IsKeyboardFocused)
		// {
		// 	context.DrawRectangle(Bounds, Color.red);
		// }

		// if (MyraEnvironment.DrawMouseHoveredWidgetFrame && IsMouseInside)
		// {
		// 	context.DrawRectangle(Bounds, Color.yellow);
		// }

		if (clipToBounds)
		{
			// Restore scissor
			context.scissor = oldScissorRectangle;
		}
	}

	public virtual void InternalRender(UIRenderContext context) { }

	#region Layout

	public Vector2Int Measure(Vector2Int availableSize)
	{
		if (!_measureDirty && _lastMeasureAvailableSize == availableSize)
		{
			return _lastMeasureSize;
		}

		Vector2Int result;

		// Lerp available size by Width/Height or MaxWidth/MaxHeight
		if (width is not null && availableSize.x > width.Value)
		{
			availableSize.x = width.Value;
		}
		else if (maxWidth is not null && availableSize.x > maxWidth.Value)
		{
			availableSize.x = maxWidth.Value;
		}

		if (height is not null && availableSize.y > height.Value)
		{
			availableSize.y = height.Value;
		}
		else if (maxHeight is not null && availableSize.y > maxHeight.Value)
		{
			availableSize.y = maxHeight.Value;
		}

		availableSize.x -= mbpWidth;
		availableSize.y -= mbpHeight;

		// Do the actual measure
		// Previously I skipped this step if both Width & Height were set
		// However that raised an issue - custom InternalMeasure stuff(such as in Menu.InternalMeasure) was skipped as well
		// So now InternalMeasure is called every time
		result = InternalMeasure(availableSize);

		// Result lerp
		if (width.HasValue)
		{
			result.x = width.Value;
		}
		else
		{
			if (minWidth.HasValue && result.x < minWidth.Value)
			{
				result.x = minWidth.Value;
			}

			if (maxWidth.HasValue && result.x > maxWidth.Value)
			{
				result.x = maxWidth.Value;
			}
		}

		if (height.HasValue)
		{
			result.y = height.Value;
		}
		else
		{
			if (minHeight.HasValue && result.y < minHeight.Value)
			{
				result.y = minHeight.Value;
			}

			if (maxHeight.HasValue && result.y > maxHeight.Value)
			{
				result.y = maxHeight.Value;
			}
		}

		result.x += mbpWidth;
		result.y += mbpHeight;

		_lastMeasureSize = result;
		_lastMeasureAvailableSize = availableSize;
		_measureDirty = false;

		return result;
	}

	protected virtual Vector2Int InternalMeasure(Vector2Int availableSize) => Vector2Int.zero;

	public virtual void Arrange() { }

	public void Layout(RectInt containerBounds)
	{
		if (_containerBounds != containerBounds)
		{
			InvalidateLayout();
			_containerBounds = containerBounds;
		}

		UpdateLayout();
	}

	public void InvalidateLayout() { _layoutState = LayoutState.Invalid; }

	internal virtual void MoveChildren(Vector2Int delta)
	{
		_bounds.x += delta.x;
		_bounds.y += delta.y;

		_actualBounds.x += delta.x;
		_actualBounds.y += delta.y;

		_containerBounds.x += delta.x;
		_containerBounds.y += delta.y;
	}

	public virtual void UpdateLayout()
	{
		if (_layoutState == LayoutState.Normal) return;

		if (_layoutState == LayoutState.Invalid)
		{
			// Full rearrange
			Vector2Int size;
			if (horizontalAlignment != UIAlignment.Fill || verticalAlignment != UIAlignment.Fill)
			{
				size = Measure(_containerBounds.size);
			}
			else
			{
				size = _containerBounds.size;
			}

			if (size.x > _containerBounds.width)
			{
				size.x = _containerBounds.width;
			}

			if (size.y > _containerBounds.height)
			{
				size.y = _containerBounds.height;
			}

			// Resolve possible conflict beetween Alignment set to Streth and Size explicitly set
			var containerSize = _containerBounds.size;

			if (horizontalAlignment == UIAlignment.Fill && width is not null && width.Value < containerSize.x)
			{
				containerSize.x = width.Value;
			}

			if (verticalAlignment == UIAlignment.Fill && height is not null && height.Value < containerSize.y)
			{
				containerSize.y = height.Value;
			}

			// Align
			var controlBounds = AlignLayout(containerSize, size, verticalAlignment, verticalAlignment, parent is null);
			controlBounds.Offset(_containerBounds.position);

			controlBounds.Offset(left, top);

			_bounds = controlBounds;
			_actualBounds = CalculateClientBounds(controlBounds);

			Arrange();

			CalculateRelativePositions();
		}
		else
		{
			// Only location
			MoveChildren(new(left - _lastLocationHint.x, top - _lastLocationHint.y));
		}

		_lastLocationHint = new(left, top);
		_layoutState = LayoutState.Normal;

		layoutUpdated?.Invoke();
	}

	public static RectInt AlignLayout(Vector2Int containerSize, Vector2Int controlSize, UIAlignment horizontalAlignment, UIAlignment verticalAlignment, bool isTopLevel = false)
	{
		var result = new RectInt(controlSize);

		switch (horizontalAlignment)
		{
			case UIAlignment.Center:
				result.x = (containerSize.x - controlSize.x) / 2;
				break;
			case UIAlignment.End:
				result.x = containerSize.x - controlSize.x;
				break;
			case UIAlignment.Fill:
				result.width = containerSize.x;
				break;
		}

		switch (verticalAlignment)
		{
			case UIAlignment.Center:
				result.y = (containerSize.y - controlSize.y) / 2;
				break;
			case UIAlignment.End:
				result.y = containerSize.y - controlSize.y;
				break;
			case UIAlignment.Fill:
				result.height = containerSize.y;
				break;
		}

		return result;
	}

	void CalculateRelativePositions()
	{
		_relativeLeft = left - bounds.x;
		_relativeTop = top - bounds.y;

		if (parent is not null)
		{
			_relativeRight = left + parent.bounds.width - bounds.x;
			_relativeBottom = top + parent.bounds.height - bounds.y;
		}
		else
		{
			_relativeRight = left + canvas!.internalBounds.width - bounds.x;
			_relativeBottom = top + canvas!.internalBounds.height - bounds.y;
		}
	}

	public virtual void InvalidateMeasure()
	{
		_measureDirty = true;

		InvalidateLayout();

		if (parent is not null)
		{
			parent.InvalidateMeasure();
		}
		else if (canvas is not null)
		{
			canvas.InvalidateLayout();
		}
	}

	internal RectInt CalculateClientBounds(RectInt clientBounds) => clientBounds - margin - borderThickness - padding;

	void ConstrainToBounds(ref int newLeft, ref int newTop)
	{
		if (newLeft < _relativeLeft) newLeft = _relativeLeft;
		if (newTop < _relativeTop) newTop = _relativeTop;

		if (newTop + bounds.height > _relativeBottom) newTop = _relativeBottom - bounds.height;
		if (newLeft + bounds.width > _relativeRight) newLeft = _relativeRight - bounds.width;
	}

	#endregion Layout

	#region Hierarchy Management

	public void BringToFront()
	{
		if (parent is not null && !(parent is UIIMultipleItemsContainer)) return;

		var widgets = (parent as UIIMultipleItemsContainer)?.Widgets ?? canvas!.widgets;

		if (widgets[widgets.Count - 1] == this) return;

		widgets.Remove(this);
		widgets.Add(this);
	}

	public void BringToBack()
	{
		if (parent is not null && !(parent is UIIMultipleItemsContainer)) return;

		var widgets = (parent as UIIMultipleItemsContainer)?.Widgets ?? canvas!.widgets;

		if (widgets[widgets.Count - 1] == this) return;

		widgets.Remove(this);
		widgets.Insert(0, this);
	}

	UIWidget? FindWidgetBy(Func<UIWidget, bool> finder)
	{
		if (finder(this)) return this;

		var asContainer = this as UIContainer;
		if (asContainer is not null)
		{
			foreach (var widget in asContainer.ChildrenCopy)
			{
				var result = widget.FindWidgetBy(finder);
				if (result is not null)
				{
					return result;
				}
			}
		}

		return null;
	}

	public UIWidget? FindWidgetById(string id)
	{
		return FindWidgetBy(w => w.id == id);
	}

	public UIWidget EnsureWidgetById(string id)
	{
		var result = FindWidgetById(id);
		if (result is null)
		{
			throw new Exception(string.Format($"Could not find widget with id {id}"));
		}

		return result;
	}

	public void RemoveFromParent()
	{
		if (parent is null) return;
		parent.RemoveChild(this);
	}

	public void RemoveFromDesktop() => canvas!.widgets.Remove(this);

	#endregion Hierarchy Management

	#region Mouse Input

	public virtual void OnCursorEnter()
	{
		isHoveredWithin = true;
		canvas!.hoveredWidget = this;
	}

	public virtual void OnCursorMove()
	{
		isHoveredWithin = true;
		canvas!.hoveredWidget = this;
	}

	public virtual void OnCursorLeave()
	{
		isHoveredWithin = false;
	}

	public virtual void OnClick()
	{
		isClicked = true;

		if (enabled) canvas!.focusedWidget = this;

		// var x = this.bounds.x;
		// var y = this.bounds.y;

		// var bounds = dragHandle is null ? RectInt.zero : new(x, y, dragHandle.bounds.right - x, dragHandle.bounds.bottom - y);

		// var clickPos = canvas.clickPosition;

		// if (bounds == RectInt.zero || bounds.Contains(clickPos))
		// {
		// 	_dragStartPos = new(clickPos.x - left, clickPos.y - top);
		// }
	}

	public void Scroll(float delta)
	{
		if (!OnScroll(delta))
		{
			parent?.Scroll(delta);
		}
	}

	protected virtual bool OnScroll(float delta) => false;

	public virtual UIWidget? GetWidgetAtPoint(Vector2Int point)
	{
		if (!active || !visible || !enabled) return null;

		if (bounds.Contains(point))
		{
			return this;
		}

		return null;
	}

	#endregion Mouse Input

	#region Keyboard Input

	internal void ButtonPressed(Button button)
	{
		if (!OnButtonPressed(button))
		{
			if (parent is null) return;

			parent.ButtonDown(button);
		}
	}

	protected virtual bool OnButtonPressed(Button button) => false;

	internal void ButtonDown(Button button)
	{
		if (!OnButtonDown(button))
		{
			if (parent is null) return;

			parent.ButtonDown(button);
		}
	}

	protected virtual bool OnButtonDown(Button button) => false;

	internal void ButtonReleased(Button button)
	{
		if (!OnButtonReleased(button))
		{
			if (parent is null) return;

			parent.ButtonDown(button);
		}
	}

	protected virtual bool OnButtonReleased(Button button) => false;

	internal void ButtonEntered(Button button)
	{
		if (!OnButtonEntered(button))
		{
			if (parent is null) return;

			parent.ButtonDown(button);
		}
	}

	protected virtual bool OnButtonEntered(Button button) => false;

	internal void TextInput(string textInput)
	{
		if (!OnTextInput(textInput))
		{
			if (parent is null) return;

			parent.TextInput(textInput);
		}
	}

	protected virtual bool OnTextInput(string textInput) => false;

	public virtual void OnLostFocus() => isFocused = false;

	public virtual void OnGotFocus() => isFocused = true;

	public void SetKeyboardFocus() => canvas!.focusedWidget = this;

	#endregion Keyboard Input

	protected virtual void OnPlacedChanged() => placedChanged();

	public virtual void OnVisibleChanged()
	{
		InvalidateMeasure();
		visibleChanged();
	}

	protected internal virtual void OnActiveChanged() { }

	void FireLocationChanged() => locationChanged();

	void FireSizeChanged() => sizeChanged();
}
