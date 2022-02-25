using System.Collections.Generic;

namespace MGE.UI;

public abstract class UIContainer : UIWidget
{
	Thickness _padding;
	public Thickness padding
	{
		get => _padding;
		set
		{
			if (_padding == value) return;
			_padding = value;

			PropertiesChanged();
		}
	}

	public RectInt contentRect { get => _rect - _padding; }

	public List<UIWidget> widgets { get; } = new();

	protected override void OnAttached()
	{
		foreach (var widget in widgets)
		{
			widget.AttachTo(this);
		}

		base.OnAttached();
	}

	public void AddChild(UIWidget widget)
	{
		widgets.Add(widget);
		widget.AttachTo(this);
		OnChildAdded(widget);
	}
	protected virtual void OnChildAdded(UIWidget widget) { }

	public void RemoveChild(UIWidget widget)
	{
		widgets.Remove(widget);
		OnChildRemoved(widget);
	}
	protected virtual void OnChildRemoved(UIWidget widget) { }

	internal void ChildMeasureChanged(UIWidget widget)
	{
		if (!(_flashTime > 0 && _flashColor == Color.magenta))
		{
			_flashTime = 1f;
			_flashColor = Color.yellow;
		}
		OnChildMeasureChanged(widget);
	}
	protected virtual void OnChildMeasureChanged(UIWidget widget) { }

	internal override void DoRender(Batch2D batch)
	{
		base.DoRender(batch);

		foreach (var widget in widgets)
		{
			widget.DoRender(batch);
		}
	}
}
