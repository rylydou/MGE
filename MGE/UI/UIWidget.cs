namespace MGE.UI;

public abstract class UIWidget
{
	public string? id;

	public UIContainer? parent { get; private set; }
	public bool isActive { get; private set; }

	#region Layout

	UIResizing _horizontalResizing;
	public UIResizing horizontalResizing
	{
		get => _horizontalResizing;
		set
		{
			if (_horizontalResizing == value) return;
			_horizontalResizing = value;

			UpdateMeasure();
		}
	}

	UIResizing _verticalResizing;
	public UIResizing verticalResizing
	{
		get => _verticalResizing;
		set
		{
			if (_verticalResizing == value) return;
			_verticalResizing = value;

			UpdateMeasure();
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

			UpdateMeasure();
		}
	}

	int _width;
	/// <summary>
	/// The width of the widget. Only has an effect when <see cref="horizontalResizing"/> is set to <see cref="UIResizing.Fixed"/>.
	/// </summary>
	public int fixedWidth
	{
		get => _width;
		set
		{
			if (_width == value) return;
			_width = value;

			if (_horizontalResizing == UIResizing.Fixed)
			{
				_rect.width = fixedWidth;
				UpdateMeasure();
			}
		}
	}

	int _height;
	/// <summary>
	/// The height of the widget. Only has an effect when <see cref="verticalResizing"/> is set to <see cref="UIResizing.Fixed"/>.
	/// </summary>
	public int fixedHeight
	{
		get => _height;
		set
		{
			if (_height == value) return;
			_height = value;

			if (_verticalResizing == UIResizing.Fixed)
			{
				_rect.height = fixedHeight;
				UpdateMeasure();
			}
		}
	}

	Vector2Int _position;
	public Vector2Int position
	{
		get => _position;
		set
		{
			if (_position == value) return;
			_position = value;

			if (parent is UIFrame)
			{
				_rect.position = value;
			}
		}
	}

	internal RectInt _rect;
	public RectInt rect { get => _rect; }
	public RectInt contentRect { get => _rect - _padding; }

	#endregion Layout

	public bool clipContent;

	internal void AttachTo(UIContainer parent)
	{
		this.parent = parent;
		OnAttached();
	}
	protected virtual void OnAttached() { }

	protected virtual void UpdateMeasure()
	{
		parent?.ChildMeasureChanged(this);
	}

	internal virtual void DoRender()
	{
		Render();
	}

	protected virtual void Render()
	{
		GFX.DrawBox(rect, new Color(1f, 0.1f));
		GFX.DrawRect(rect, Color.green, -1);

		Font.normal.DrawString(GetType().Name, rect.topLeft, Color.white);
	}

	/// <summary>
	/// The width is guaranteed to be set but the height might also change in responce, eg. a label running out of space horizontally so needing another line.
	/// When overriding call the base function last.
	/// </summary>
	/// <param name="width">The width of the widget</param>
	public virtual void RequestWidth(int width)
	{
		_rect.width = width;
	}

	public void SetHeight(int height)
	{
		_rect.height = height;
	}
}
