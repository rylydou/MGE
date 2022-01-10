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
	UIDesktop? _desktop;

	Vector2Int _lastMeasureSize;
	Vector2Int _lastMeasureAvailableSize;
	Vector2Int _lastLocationHint;

	RectInt _containerBounds;
	RectInt _bounds;
	RectInt _actualBounds;
	bool _visible;

	float _opacity = 1.0f;

	bool _isMouseInside, _enabled;
	bool _isKeyboardFocused = false;

	/// <summary>
	/// Internal use only. (MyraPad)
	/// </summary>
	[DefaultValue(Stylesheet.DefaultStyleName)]
	public string? StyleName { get; set; }

	[Category("Layout")]
	[DefaultValue(0)]
	public int Left
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
	public int Top
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
	public int? MinWidth
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
	public int? MaxWidth
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
	public int? Width
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
	public int? MinHeight
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
	public int? MaxHeight
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
	public int? Height
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
	[Browsable(false)]
	public int PaddingLeft
	{
		get => Padding.Left;
		set
		{
			var p = Padding;
			p.Left = value;
			Padding = p;
		}
	}

	[Category("Layout")]
	[DesignerFolded]
	public Thickness Margin
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
	public Thickness BorderThickness
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
	public Thickness Padding
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
	public virtual UIAlignment HorizontalAlignment
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
	public virtual UIAlignment VerticalAlignment
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
	public int GridColumn
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
	public int GridRow
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
	public int GridColumnSpan
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
	public int GridRowSpan
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
	public virtual bool Enabled
	{
		get => _enabled;
		set
		{
			if (_enabled == value) return;
			_enabled = value;
			EnabledChanged();
		}
	}

	[Category("Behavior")]
	[DefaultValue(true)]
	public virtual bool Visible
	{
		get => _visible;
		set
		{
			if (_visible == value) return;
			_visible = value;
			IsMouseInside = false;
			IsTouchInside = false;
			OnVisibleChanged();
		}
	}

	[Category("Behavior")]
	[DefaultValue(UIDragDirection.None)]
	public virtual UIDragDirection dragDirection { get; set; } = UIDragDirection.None;

	[XmlIgnore]
	[Browsable(false)]
	internal bool IsDraggable { get => dragDirection != UIDragDirection.None; }

	[Category("Behavior")]
	[DefaultValue(0)]
	public int ZIndex
	{
		get => _zIndex;
		set
		{
			if (_zIndex == value) return;
			_zIndex = value;
			InvalidateMeasure();
		}
	}

	[XmlIgnore]
	[Browsable(false)]
	public UIWidget? DragHandle { get; set; }

	[XmlIgnore]
	[Browsable(false)]
	int RelativeLeft { get; set; }

	[XmlIgnore]
	[Browsable(false)]
	int RelativeTop { get; set; }

	[XmlIgnore]
	[Browsable(false)]
	int RelativeRight { get; set; }

	[XmlIgnore]
	[Browsable(false)]
	int RelativeBottom { get; set; }

	/// <summary>
	/// Determines whether the widget had been placed on UIDesktop
	/// </summary>
	[XmlIgnore]
	[Browsable(false)]
	public bool IsPlaced
	{
		get => Desktop is not null;
	}

	[XmlIgnore]
	[Browsable(false)]
	public virtual UIDesktop? Desktop
	{
		get => _desktop;
		internal set
		{
			if (_desktop is not null && value is null)
			{
				if (_desktop.focusedWidget == this)
				{
					_desktop.focusedWidget = null;
				}

				if (_desktop.mouseInsideWidget == this)
				{
					_desktop.mouseInsideWidget = null;
				}
			}

			_desktop = value;
			IsMouseInside = false;
			IsTouchInside = false;

			if (_desktop is not null)
			{
				InvalidateLayout();
			}

			SubscribeOnTouchMoved(IsPlaced && IsDraggable);
			OnPlacedChanged();
		}
	}

	[XmlIgnore]
	[Browsable(false)]
	public bool IsModal
	{
		get => _isModal;
		set
		{
			if (_isModal == value) return;
			_isModal = value;
			InvalidateMeasure();
		}
	}

	protected internal bool Active
	{
		get => _active;
		set
		{
			if (_active == value) return;
			_active = value;
			OnActiveChanged();
		}
	}

	[Category("Appearance")]
	[DefaultValue(1f)]
	public float Opacity
	{
		get => _opacity;
		set
		{
			if (value < 0f || value > 1f) throw new ArgumentOutOfRangeException("value");
			_opacity = value;
		}
	}

	/// <summary>
	/// Dynamic layout expression
	/// </summary>
	[XmlIgnore]
	[Browsable(false)]
	public Layout2D Layout2d { get; set; } = Layout2D.NullLayout;

	[Category("Appearance")]
	public IBrush Background { get; set; }

	[Category("Appearance")]
	public IBrush OverBackground { get; set; }

	[Category("Appearance")]
	public IBrush DisabledBackground { get; set; }

	[Category("Appearance")]
	public IBrush FocusedBackground { get; set; }

	[Category("Appearance")]
	public IBrush Border { get; set; }

	[Category("Appearance")]
	public IBrush OverBorder { get; set; }

	[Category("Appearance")]
	public IBrush DisabledBorder { get; set; }

	[Category("Appearance")]
	public IBrush FocusedBorder { get; set; }

	[Category("Appearance")]
	[DefaultValue(false)]
	public virtual bool ClipToBounds { get; set; }

	[Browsable(false)]
	[XmlIgnore]
	public bool IsMouseInside
	{
		get => _isMouseInside;
		set
		{
			_isMouseInside = value;
			if (Desktop is not null && Desktop.mouseInsideWidget == this)
			{
				Desktop.mouseInsideWidget = null;
			}
		}
	}

	[Browsable(false)]
	[XmlIgnore]
	public bool IsTouchInside { get; set; }

	[Browsable(false)]
	[XmlIgnore]
	public UIContainer? Parent { get; internal set; }

	[Browsable(false)]
	[XmlIgnore]
	public object? Tag { get; set; }

	[Browsable(false)]
	[XmlIgnore]
	public RectInt Bounds { get => _bounds; }

	internal RectInt BorderBounds { get => _bounds - _margin; }

	internal bool ContainsMouse { get => Desktop is not null && BorderBounds.Contains(Desktop.mousePosition); }

	internal bool ContainsTouch { get => Desktop is not null && BorderBounds.Contains(Desktop.touchPosition); }

	protected RectInt BackgroundBounds { get => BorderBounds - _borderThickness; }

	[Browsable(false)]
	[XmlIgnore]
	public RectInt ActualBounds { get => _actualBounds; }

	[Browsable(false)]
	[XmlIgnore]
	public RectInt ContainerBounds { get => _containerBounds; }

	[Browsable(false)]
	[XmlIgnore]
	public int MBPWidth { get => Margin.Left + Margin.Right + BorderThickness.Left + BorderThickness.Right + Padding.Left + Padding.Right; }

	[Browsable(false)]
	[XmlIgnore]
	public int MBPHeight { get => Margin.Top + Margin.Bottom + BorderThickness.Top + BorderThickness.Bottom + Padding.Top + Padding.Bottom; }

	/// <summary>
	/// Determines whether a widget accepts keyboard focus
	/// </summary>
	[Browsable(false)]
	[XmlIgnore]
	public bool AcceptsKeyboardFocus { get; set; } = true;

	[Browsable(false)]
	[XmlIgnore]
	internal protected virtual UIMouseWheelFocusType MouseWheelFocusType { get => UIMouseWheelFocusType.None; }

	[Browsable(false)]
	[XmlIgnore]
	public bool IsKeyboardFocused
	{
		get => _isKeyboardFocused;
		internal set
		{
			if (_isKeyboardFocused == value) return;
			_isKeyboardFocused = value;
			KeyboardFocusChanged();
		}
	}

	protected virtual bool UseHoverRenderable { get => IsMouseInside && Active; }

	// public Action PlacedChanged = () => { };
	// public Action VisibleChanged = () => { };
	// public Action EnabledChanged = () => { };

	// public Action LocationChanged = () => { };
	// public Action SizeChanged = () => { };
	// public Action LayoutUpdated = () => { };

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

	[Browsable(false)]
	[XmlIgnore]
	public Action<UIRenderContext> BeforeRender = (context) => { };

	[Browsable(false)]
	[XmlIgnore]
	public Action<UIRenderContext> AfterRender = (context) => { };

	public UIWidget()
	{
		Visible = true;
		Enabled = true;
	}

	public virtual IBrush GetCurrentBackground()
	{
		var result = Background;

		if (!Enabled && DisabledBackground is not null)
		{
			result = DisabledBackground;
		}
		else if (Enabled && IsKeyboardFocused && FocusedBackground is not null)
		{
			result = FocusedBackground;
		}
		else if (UseHoverRenderable && OverBackground is not null)
		{
			result = OverBackground;
		}

		return result;
	}

	public virtual IBrush GetCurrentBorder()
	{
		var result = Border;

		if (!Enabled && DisabledBorder is not null)
		{
			result = DisabledBorder;
		}
		else if (Enabled && IsKeyboardFocused && FocusedBorder is not null)
		{
			result = FocusedBorder;
		}
		else if (UseHoverRenderable && OverBorder is not null)
		{
			result = OverBorder;
		}

		return result;
	}

	public void BringToFront()
	{
		if (Parent is not null && !(Parent is UIIMultipleItemsContainer)) return;

		var widgets = (Parent as UIIMultipleItemsContainer)?.Widgets ?? Desktop!.widgets;

		if (widgets[widgets.Count - 1] == this) return;

		widgets.Remove(this);
		widgets.Add(this);
	}

	public void BringToBack()
	{
		if (Parent is not null && !(Parent is UIIMultipleItemsContainer)) return;

		var widgets = (Parent as UIIMultipleItemsContainer)?.Widgets ?? Desktop!.widgets;

		if (widgets[widgets.Count - 1] == this) return;

		widgets.Remove(this);
		widgets.Insert(0, this);
	}

	public void Render(UIRenderContext context)
	{
		if (!Visible) return;

		UpdateLayout();

		var oldScissorRectangle = context.scissor;
		if (ClipToBounds)
		{
			var newScissorRectangle = RectInt.Intersect(oldScissorRectangle, Bounds);
			if (newScissorRectangle.isEmpty) return;

			context.scissor = newScissorRectangle;
		}

		var oldOpacity = context.opacity;
		context.opacity *= Opacity;

		BeforeRender(context);

		// Background
		var background = GetCurrentBackground();
		if (background is not null)
		{
			background.Draw(context, BackgroundBounds);
		}

		// Borders
		var border = GetCurrentBorder();
		if (border is not null)
		{
			var borderBounds = BorderBounds;
			if (BorderThickness.Left > 0)
			{
				border.Draw(context, new(borderBounds.x, borderBounds.y, BorderThickness.Left, borderBounds.height));
			}

			if (BorderThickness.Top > 0)
			{
				border.Draw(context, new(borderBounds.x, borderBounds.y, borderBounds.width, BorderThickness.Top));
			}

			if (BorderThickness.Right > 0)
			{
				border.Draw(context, new(borderBounds.right - BorderThickness.Right, borderBounds.y, BorderThickness.Right, borderBounds.height));
			}

			if (BorderThickness.Bottom > 0)
			{
				border.Draw(context, new(borderBounds.x, borderBounds.bottom - BorderThickness.Bottom, borderBounds.width, BorderThickness.Bottom));
			}
		}

		InternalRender(context);

		AfterRender(context);

		// Restore context settings
		context.opacity = oldOpacity;

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

		if (ClipToBounds)
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
		if (Width is not null && availableSize.x > Width.Value)
		{
			availableSize.x = Width.Value;
		}
		else if (MaxWidth is not null && availableSize.x > MaxWidth.Value)
		{
			availableSize.x = MaxWidth.Value;
		}

		if (Height is not null && availableSize.y > Height.Value)
		{
			availableSize.y = Height.Value;
		}
		else if (MaxHeight is not null && availableSize.y > MaxHeight.Value)
		{
			availableSize.y = MaxHeight.Value;
		}

		availableSize.x -= MBPWidth;
		availableSize.y -= MBPHeight;

		// Do the actual measure
		// Previously I skipped this step if both Width & Height were set
		// However that raised an issue - custom InternalMeasure stuff(such as in Menu.InternalMeasure) was skipped as well
		// So now InternalMeasure is called every time
		result = InternalMeasure(availableSize);

		// Result lerp
		if (Width.HasValue)
		{
			result.x = Width.Value;
		}
		else
		{
			if (MinWidth.HasValue && result.x < MinWidth.Value)
			{
				result.x = MinWidth.Value;
			}

			if (MaxWidth.HasValue && result.x > MaxWidth.Value)
			{
				result.x = MaxWidth.Value;
			}
		}

		if (Height.HasValue)
		{
			result.y = Height.Value;
		}
		else
		{
			if (MinHeight.HasValue && result.y < MinHeight.Value)
			{
				result.y = MinHeight.Value;
			}

			if (MaxHeight.HasValue && result.y > MaxHeight.Value)
			{
				result.y = MaxHeight.Value;
			}
		}

		result.x += MBPWidth;
		result.y += MBPHeight;

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
			if (HorizontalAlignment != UIAlignment.Fill || VerticalAlignment != UIAlignment.Fill)
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

			if (HorizontalAlignment == UIAlignment.Fill && Width is not null && Width.Value < containerSize.x)
			{
				containerSize.x = Width.Value;
			}

			if (VerticalAlignment == UIAlignment.Fill && Height is not null && Height.Value < containerSize.y)
			{
				containerSize.y = Height.Value;
			}

			// Align
			var controlBounds = AlignLayout(containerSize, size, VerticalAlignment, VerticalAlignment, Parent is null);
			controlBounds.Offset(_containerBounds.position);

			controlBounds.Offset(Left, Top);

			_bounds = controlBounds;
			_actualBounds = CalculateClientBounds(controlBounds);

			Arrange();

			CalculateRelativePositions();
		}
		else
		{
			// Only location
			MoveChildren(new(Left - _lastLocationHint.x, Top - _lastLocationHint.y));
		}

		_lastLocationHint = new(Left, Top);
		_layoutState = LayoutState.Normal;

		LayoutUpdated?.Invoke();
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
		RelativeLeft = Left - Bounds.x;
		RelativeTop = Top - Bounds.y;

		if (Parent is not null)
		{
			RelativeRight = Left + Parent.Bounds.width - Bounds.x;
			RelativeBottom = Top + Parent.Bounds.height - Bounds.y;
		}
		else
		{
			RelativeRight = Left + Desktop!.internalBounds.width - Bounds.x;
			RelativeBottom = Top + Desktop!.internalBounds.height - Bounds.y;
		}
	}

	public virtual void InvalidateMeasure()
	{
		_measureDirty = true;

		InvalidateLayout();

		if (Parent is not null)
		{
			Parent.InvalidateMeasure();
		}
		else if (Desktop is not null)
		{
			Desktop.InvalidateLayout();
		}
	}

	internal RectInt CalculateClientBounds(RectInt clientBounds) => clientBounds - Margin - BorderThickness - Padding;

	void ConstrainToBounds(ref int newLeft, ref int newTop)
	{
		if (newLeft < RelativeLeft) newLeft = RelativeLeft;
		if (newTop < RelativeTop) newTop = RelativeTop;

		if (newTop + Bounds.height > RelativeBottom) newTop = RelativeBottom - Bounds.height;
		if (newLeft + Bounds.width > RelativeRight) newLeft = RelativeRight - Bounds.width;
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
		if (Parent is null) return;
		Parent.RemoveChild(this);
	}

	public void RemoveFromDesktop() => Desktop!.widgets.Remove(this);

	#endregion Hierarchy Management

	#region Styling

	public void ApplyWidgetStyle(WidgetStyle style)
	{
		Width = style.width;
		Height = style.height;
		MinWidth = style.MinWidth;
		MinHeight = style.MinHeight;
		MaxWidth = style.MaxWidth;
		MaxHeight = style.MaxHeight;

		Background = style.Background;
		OverBackground = style.OverBackground;
		DisabledBackground = style.DisabledBackground;
		FocusedBackground = style.FocusedBackground;

		Border = style.Border;
		OverBorder = style.OverBorder;
		DisabledBorder = style.DisabledBorder;
		FocusedBorder = style.FocusedBorder;

		Margin = style.Margin;
		BorderThickness = style.BorderThickness;
		Padding = style.Padding;
	}

	public void SetStyle(Stylesheet stylesheet, string name)
	{
		StyleName = name;

		if (StyleName is not null)
		{
			InternalSetStyle(stylesheet, StyleName);
		}
	}

	public void SetStyle(string name) => SetStyle(Stylesheet.Current, name);

	protected virtual void InternalSetStyle(Stylesheet stylesheet, string name) { }

	#endregion Styling

	#region Mouse Input

	public virtual void OnMouseLeft()
	{
		IsMouseInside = false;
		// MouseLeft();
	}

	public virtual void OnMouseEntered()
	{
		IsMouseInside = true;
		Desktop!.mouseInsideWidget = this;
		// MouseEntered();
	}

	public virtual void OnMouseMoved()
	{
		IsMouseInside = true;
		Desktop!.mouseInsideWidget = this;
		// MouseMoved();
	}

	public virtual void OnTouchDoubleClick() => TouchDoubleClick();

	public virtual void OnMouseWheel(float delta) => MouseWheelChanged(delta);

	public virtual void OnTouchLeft()
	{
		IsTouchInside = false;
		// TouchLeft();
	}

	public virtual void OnTouchEntered()
	{
		IsTouchInside = true;
		// TouchEntered();
	}

	public virtual void OnTouchMoved()
	{
		IsTouchInside = true;
		// TouchMoved();
	}

	public virtual void OnTouchDown()
	{
		IsTouchInside = true;

		if (Enabled)
		{
			Desktop!.focusedWidget = this;
		}

		var x = Bounds.x;
		var y = Bounds.y;

		var bounds = DragHandle is not null ? new RectInt(x, y, DragHandle.Bounds.right - x, DragHandle.Bounds.bottom - y) : RectInt.zero;

		var touchPos = Desktop!.touchPosition;

		if (bounds == RectInt.zero || bounds.Contains(touchPos))
		{
			_startPos = new(touchPos.x - Left, touchPos.y - Top);
		}

		// TouchDown();
	}

	public virtual void OnTouchUp()
	{
		_startPos = null;
		IsTouchInside = false;
		// TouchUp();
	}

	void SubscribeOnTouchMoved(bool subscribe)
	{
		if (Parent is not null)
		{
			Parent.TouchMoved -= DesktopOnTouchMoved;
			Parent.TouchUp -= DesktopTouchUp;
		}
		else if (Desktop is not null)
		{
			Desktop.TouchMoved -= DesktopOnTouchMoved;
			Desktop.TouchUp -= DesktopTouchUp;
		}

		if (subscribe)
		{
			if (Parent is not null)
			{
				Parent.TouchMoved += DesktopOnTouchMoved;
				Parent.TouchUp += DesktopTouchUp;
			}
			else if (Desktop is not null)
			{
				Desktop.TouchMoved += DesktopOnTouchMoved;
				Desktop.TouchUp += DesktopTouchUp;
			}
		}
	}

	void DesktopOnTouchMoved()
	{
		if (_startPos is null || !IsDraggable) return;

		var position = new Vector2Int(Desktop!.touchPosition.x - _startPos.Value.x, Desktop!.touchPosition.y - _startPos.Value.y);

		var newLeft = Left;
		var newTop = Top;

		if (dragDirection.HasFlag(UIDragDirection.Horizontal)) newLeft = position.x;

		if (dragDirection.HasFlag(UIDragDirection.Vertical)) newTop = position.y;

		ConstrainToBounds(ref newLeft, ref newTop);

		Left = newLeft;
		Top = newTop;
	}

	void DesktopTouchUp() => _startPos = null;

	public virtual bool TestPoint(Vector2Int point) => Bounds.Contains(point);

	#endregion Mouse Input

	#region Keyboard Input

	internal void ButtonPressed(Button button)
	{
		if (!OnButtonPressed(button))
		{
			if (Parent is null) return;

			Parent.ButtonDown(button);
		}
	}

	protected virtual bool OnButtonPressed(Button button) => false;

	internal void ButtonDown(Button button)
	{
		if (!OnButtonDown(button))
		{
			if (Parent is null) return;

			Parent.ButtonDown(button);
		}
	}

	protected virtual bool OnButtonDown(Button button) => false;

	internal void ButtonReleased(Button button)
	{
		if (!OnButtonReleased(button))
		{
			if (Parent is null) return;

			Parent.ButtonDown(button);
		}
	}

	protected virtual bool OnButtonReleased(Button button) => false;

	internal void ButtonEntered(Button button)
	{
		if (!OnButtonEntered(button))
		{
			if (Parent is null) return;

			Parent.ButtonDown(button);
		}
	}

	protected virtual bool OnButtonEntered(Button button) => false;

	internal void TextInput(string textInput)
	{
		if (!OnTextInput(textInput))
		{
			if (Parent is null) return;

			Parent.TextInput(textInput);
		}
	}

	protected virtual bool OnTextInput(string textInput) => false;

	public virtual void OnLostKeyboardFocus() => IsKeyboardFocused = false;

	public virtual void OnGotKeyboardFocus() => IsKeyboardFocused = true;

	public void SetKeyboardFocus() => Desktop!.focusedWidget = this;

	#endregion Keyboard Input

	protected virtual void OnPlacedChanged() => PlacedChanged();

	public virtual void OnVisibleChanged()
	{
		InvalidateMeasure();
		VisibleChanged();
	}

	protected internal virtual void OnActiveChanged() { }

	void FireLocationChanged() => LocationChanged();

	void FireSizeChanged() => SizeChanged();
}
