namespace MGE.UI;

public class UIWidget
{
	public string? id;

	UIWidget? _parent;
	public UIWidget? parent
	{
		get => _parent;
		set
		{
			_parent = value;
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
			UpdateMeasure();
		}
	}

	public RectInt bounds;

	void UpdateMeasure()
	{

	}
}
