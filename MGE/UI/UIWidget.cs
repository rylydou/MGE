using System;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Serialization;

namespace MGE.UI;

public enum UIMouseWheelFocusType
{
	None,
	Hover,
	Focus
}

[System.Flags]
public enum UIDragDirection
{
	None = 0,
	Vertical = 1,
	Horizontal = 2,
	Both = Vertical | Horizontal
}

public class UIWidget
{
	enum LayoutState
	{
		Normal,
		LocationInvalid,
		Invalid
	}

	public string? Id;

	Vector2Int? _startPos;
	Thickness _margin, _borderThickness, _padding;
	int _left, _top;
	int? _minWidth, _minHeight, _maxWidth, _maxHeight, _width, _height;
	int _gridColumn, _gridRow, _gridColumnSpan = 1, _gridRowSpan = 1;
	int _zIndex;
	UIAlignment _horizontalAlignment = UIAlignment.Start;
	UIAlignment _verticalAlignment = UIAlignment.Start;
	LayoutState _layoutState = LayoutState.Invalid;
	bool _isModal = false;
	bool _measureDirty = true;
	bool _active = false;

	Vector2Int _lastMeasureSize;
	Vector2Int _lastMeasureAvailableSize;
	Vector2Int _lastLocationHint;

	bool _visible;

	bool _isMouseInside, _enabled;
	bool _isKeyboardFocused = false;

	/// <summary>
	/// Internal use only. (MyraPad)
	/// </summary>
	[DefaultValue(Stylesheet.DefaultStyleName)]
	public string? styleName { get; set; }

	[Category("Layout")]
	[DefaultValue(0)]
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

	[Category("Layout")]
	[DefaultValue(0)]
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

	[Category("Layout")]
	[DefaultValue(null)]
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

	[Category("Layout")]
	[DefaultValue(null)]
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

	[Category("Layout")]
	[DefaultValue(null)]
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

	[Category("Layout")]
	[DefaultValue(null)]
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

	[Category("Layout")]
	[DefaultValue(null)]
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

	[Category("Layout")]
	[DefaultValue(null)]
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

	[Obsolete("Use Padding")]
	public int paddingLeft
	{
		get => padding.Left;
		set
		{
			var p = padding;
			p.Left = value;
			padding = p;
		}
	}

	[Category("Layout")]
	[DesignerFolded]
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

	[Category("Appearance")]
	[DesignerFolded]
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

	[Category("Layout")]
	[DesignerFolded]
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

	[Category("Layout")]
	[DefaultValue(UIAlignment.Start)]
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

	[Category("Layout")]
	[DefaultValue(UIAlignment.Start)]
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

	[Category("Layout")]
	[DefaultValue(0)]
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

	[Category("Layout")]
	[DefaultValue(0)]
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

	[Category("Layout")]
	[DefaultValue(1)]
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

	[Category("Layout")]
	[DefaultValue(1)]
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

	[Category("Behavior")]
	[DefaultValue(true)]
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

	[Category("Behavior")]
	[DefaultValue(true)]
	public virtual bool visible
	{
		get => _visible;
		set
		{
			if (_visible == value) return;
			_visible = value;
			isMouseInside = false;
			isTouchInside = false;
			OnVisibleChanged();
		}
	}

	[Category("Behavior")]
	[DefaultValue(UIDragDirection.None)]
	public virtual UIDragDirection dragDirection { get; set; } = UIDragDirection.None;

	internal bool isDraggable { get => dragDirection != UIDragDirection.None; }

	[Category("Behavior")]
	[DefaultValue(0)]
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

	// TODO
	public bool isPlaced { get; internal set; }

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

	public Layout2D layout2d { get; set; } = Layout2D.NullLayout;

	// [Category("Appearance")]
	// public IBrush background { get; set; }

	// [Category("Appearance")]
	// public IBrush overBackground { get; set; }

	// [Category("Appearance")]
	// public IBrush disabledBackground { get; set; }

	// [Category("Appearance")]
	// public IBrush focusedBackground { get; set; }

	// [Category("Appearance")]
	// public IBrush border { get; set; }

	// [Category("Appearance")]
	// public IBrush overBorder { get; set; }

	// [Category("Appearance")]
	// public IBrush disabledBorder { get; set; }

	// [Category("Appearance")]
	// public IBrush focusedBorder { get; set; }

	[Category("Appearance")]
	[DefaultValue(false)]
	public virtual bool clipToBounds { get; set; }

	public bool isMouseInside
	{
		get => _isMouseInside;
		set
		{
			_isMouseInside = value;
			if (UICanvas.mouseInsideWidget == this)
			{
				UICanvas.mouseInsideWidget = null;
			}
		}
	}

	public bool isTouchInside { get; set; }

	public UIContainer? parent { get; internal set; }

	public object? tag { get; set; }

	RectInt _bounds;
	public RectInt bounds { get => _bounds; }

	internal RectInt borderBounds { get => _bounds - _margin; }
	internal bool containsMouse { get => borderBounds.Contains(UICanvas.mousePosition); }
	internal bool containsTouch { get => borderBounds.Contains(UICanvas.clickPosition); }

	protected RectInt backgroundBounds { get => borderBounds - _borderThickness; }

	RectInt _actualBounds;
	public RectInt actualBounds { get => _actualBounds; }

	RectInt _containerBounds;
	public RectInt containerBounds { get => _containerBounds; }

	public int mbpWidth { get => margin.Left + margin.Right + borderThickness.Left + borderThickness.Right + padding.Left + padding.Right; }

	public int mbpHeight { get => margin.Top + margin.Bottom + borderThickness.Top + borderThickness.Bottom + padding.Top + padding.Bottom; }

	/// <summary>
	/// Determines whether a widget accepts keyboard focus
	/// </summary>
	public bool acceptsKeyboardFocus { get; set; } = true;

	internal protected virtual UIMouseWheelFocusType mouseWheelFocusType { get => UIMouseWheelFocusType.None; }

	public bool isKeyboardFocused
	{
		get => _isKeyboardFocused;
		internal set
		{
			if (_isKeyboardFocused == value) return;
			_isKeyboardFocused = value;
			// KeyboardFocusChanged();
		}
	}

	protected virtual bool useHoverRenderable { get => isMouseInside && active; }

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

		// Background
		var background = GetCurrentBackground();
		if (background is not null)
		{
			background.Draw(context, backgroundBounds);
		}

		// Borders
		var border = GetCurrentBorder();
		if (border is not null)
		{
			var borderBounds = this.borderBounds;
			if (borderThickness.Left > 0)
			{
				border.Draw(context, new(borderBounds.x, borderBounds.y, borderThickness.Left, borderBounds.height));
			}

			if (borderThickness.Top > 0)
			{
				border.Draw(context, new(borderBounds.x, borderBounds.y, borderBounds.width, borderThickness.Top));
			}

			if (borderThickness.Right > 0)
			{
				border.Draw(context, new(borderBounds.right - borderThickness.Right, borderBounds.y, borderThickness.Right, borderBounds.height));
			}

			if (borderThickness.Bottom > 0)
			{
				border.Draw(context, new(borderBounds.x, borderBounds.bottom - borderThickness.Bottom, borderBounds.width, borderThickness.Bottom));
			}
		}

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

	public void BringToFront()
	{
		if (parent is not null && !(parent is UIIMultipleItemsContainer)) return;

		var widgets = (parent as UIIMultipleItemsContainer)?.Widgets ?? UICanvas.widgets;

		if (widgets[widgets.Count - 1] == this) return;

		widgets.Remove(this);
		widgets.Add(this);
	}

	public void BringToBack()
	{
		if (parent is not null && !(parent is UIIMultipleItemsContainer)) return;

		var widgets = (parent as UIIMultipleItemsContainer)?.Widgets ?? UICanvas.widgets;

		if (widgets[widgets.Count - 1] == this) return;

		widgets.Remove(this);
		widgets.Insert(0, this);
	}

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
			_relativeRight = left + UICanvas.internalBounds.width - bounds.x;
			_relativeBottom = top + UICanvas.internalBounds.height - bounds.y;
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
		else if (UICanvas is not null)
		{
			UICanvas.InvalidateLayout();
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

	UIWidget? FindWidgetBy(Func<UIWidget, bool> finder)
	{
		if (finder(this))
		{
			return this;
		}

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

	/// <summary>
	/// Finds a widget by id
	/// Returns null if not found
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public UIWidget? FindWidgetById(string id)
	{
		return FindWidgetBy(w => w.Id == id);
	}

	/// <summary>
	/// Find a widget by id
	/// Throws exception if not found
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
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

	public void RemoveFromDesktop() => UICanvas.widgets.Remove(this);

	#endregion Hierarchy Management

	#region Styling

	public void ApplyWidgetStyle(WidgetStyle style)
	{
		width = style.width;
		height = style.height;
		minWidth = style.MinWidth;
		minHeight = style.MinHeight;
		maxWidth = style.MaxWidth;
		maxHeight = style.MaxHeight;

		background = style.Background;
		overBackground = style.OverBackground;
		disabledBackground = style.DisabledBackground;
		focusedBackground = style.FocusedBackground;

		border = style.Border;
		overBorder = style.OverBorder;
		disabledBorder = style.DisabledBorder;
		focusedBorder = style.FocusedBorder;

		margin = style.Margin;
		borderThickness = style.BorderThickness;
		padding = style.Padding;
	}

	public void SetStyle(Stylesheet stylesheet, string name)
	{
		styleName = name;

		if (styleName is not null)
		{
			InternalSetStyle(stylesheet, styleName);
		}
	}

	public void SetStyle(string name) => SetStyle(Stylesheet.Current, name);

	protected virtual void InternalSetStyle(Stylesheet stylesheet, string name) { }

	#endregion Styling

	#region Mouse Input

	public virtual void OnMouseLeft()
	{
		isMouseInside = false;
		// MouseLeft();
	}

	public virtual void OnMouseEntered()
	{
		isMouseInside = true;
		UICanvas.mouseInsideWidget = this;
		// MouseEntered();
	}

	public virtual void OnMouseMoved()
	{
		isMouseInside = true;
		UICanvas.mouseInsideWidget = this;
		// MouseMoved();
	}

	public virtual void OnTouchDoubleClick() => TouchDoubleClick();

	public virtual void OnMouseWheel(float delta) => MouseWheelChanged(delta);

	public virtual void OnClickLeft()
	{
		isTouchInside = false;
		// TouchLeft();
	}

	public virtual void OnClickEntered()
	{
		isTouchInside = true;
		// TouchEntered();
	}

	public virtual void OnDragMoved()
	{
		isTouchInside = true;
		// TouchMoved();
	}

	public virtual void OnClick()
	{
		isTouchInside = true;

		if (enabled)
		{
			UICanvas.focusedWidget = this;
		}

		var x = this.bounds.x;
		var y = this.bounds.y;

		var bounds = dragHandle is not null ? new RectInt(x, y, dragHandle.bounds.right - x, dragHandle.bounds.bottom - y) : RectInt.zero;

		var touchPos = UICanvas.clickPosition;

		if (bounds == RectInt.zero || bounds.Contains(touchPos))
		{
			_startPos = new(touchPos.x - left, touchPos.y - top);
		}

		// TouchDown();
	}

	public virtual void OnTouchUp()
	{
		_startPos = null;
		isTouchInside = false;
		// TouchUp();
	}

	void SubscribeOnTouchMoved(bool subscribe)
	{
		if (parent is not null)
		{
			parent.TouchMoved -= DesktopOnTouchMoved;
			parent.TouchUp -= DesktopTouchUp;
		}
		else if (UICanvas is not null)
		{
			UICanvas.TouchMoved -= DesktopOnTouchMoved;
			UICanvas.TouchUp -= DesktopTouchUp;
		}

		if (subscribe)
		{
			if (parent is not null)
			{
				parent.TouchMoved += DesktopOnTouchMoved;
				parent.TouchUp += DesktopTouchUp;
			}
			else if (UICanvas is not null)
			{
				UICanvas.TouchMoved += DesktopOnTouchMoved;
				UICanvas.TouchUp += DesktopTouchUp;
			}
		}
	}

	void DesktopOnTouchMoved()
	{
		if (_startPos is null || !isDraggable) return;

		var position = new Vector2Int(UICanvas.clickPosition.x - _startPos.Value.x, UICanvas.clickPosition.y - _startPos.Value.y);

		var newLeft = left;
		var newTop = top;

		if (dragDirection.HasFlag(UIDragDirection.Horizontal)) newLeft = position.x;

		if (dragDirection.HasFlag(UIDragDirection.Vertical)) newTop = position.y;

		ConstrainToBounds(ref newLeft, ref newTop);

		left = newLeft;
		top = newTop;
	}

	void DesktopTouchUp() => _startPos = null;

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

	public virtual void OnLostKeyboardFocus() => isKeyboardFocused = false;

	public virtual void OnGotKeyboardFocus() => isKeyboardFocused = true;

	public void SetKeyboardFocus() => UICanvas.focusedWidget = this;

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
