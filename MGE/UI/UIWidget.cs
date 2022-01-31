namespace MGE.UI;

public abstract class UIWidget
{
	public string? id;

	UIContainer? _parent;
	public UIContainer? parent
	{
		get => _parent;
		set
		{
			if (_parent == value) return;
			_parent = value;

			UpdateMeasure();
		}
	}

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

	int? _width;
	/// <summary>
	/// The width of the widget. Only has an effect when <see cref="horizontalResizing"/> is set to <see cref="UIResizing.Fixed"/>.
	/// </summary>
	public int? fixedWidth
	{
		get => _width;
		set
		{
			if (_width == value) return;
			_width = value;

			if (horizontalResizing == UIResizing.FillContainer) return;
			UpdateMeasure();
		}
	}

	int? _height;
	/// <summary>
	/// The height of the widget. Only has an effect when <see cref="verticalResizing"/> is set to <see cref="UIResizing.Fixed"/>.
	/// </summary>
	public int? fixedHeight
	{
		get => _height;
		set
		{
			if (_height == value) return;
			_height = value;

			if (horizontalResizing == UIResizing.FillContainer) return;
			UpdateMeasure();
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
				SetPosition(_position.x, _position.y);
			}
		}
	}

	RectInt _realBounds;
	public RectInt realBounds { get => _realBounds; }
	public RectInt realContentBounds { get => _realBounds - _padding; }

	protected virtual void UpdateMeasure()
	{
		if (parent is null) return;

		if (parent is UIBox box)
		{
			box.UpdateLayout();
		}
	}

	public virtual void SetWidth(int width)
	{
		_realBounds.width = width;
	}

	public virtual void SetHeight(int height)
	{
		_realBounds.height = height;
	}

	internal virtual void SetPosition(int x, int y)
	{
		_realBounds.x = x;
		_realBounds.y = y;
	}
}
