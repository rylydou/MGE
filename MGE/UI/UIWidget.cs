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

	UIAlignment _alignment;
	public UIAlignment alignment
	{
		get => _alignment;
		set
		{
			if (_alignment == value) return;
			_alignment = value;

			UpdateMeasure();
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

			if (alignment == UIAlignment.FillContainer) return;
			UpdateMeasure();
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

			if (alignment == UIAlignment.FillContainer) return;
			UpdateMeasure();
		}
	}

	RectInt _bounds;
	public RectInt bounds { get => _bounds; }

	RectInt _contentBounds;
	public RectInt contentBounds { get => _contentBounds; }

	void UpdateMeasure()
	{
		if (parent is null) return;

		parent.UpdateLayout();
	}
}
