using System.Collections.Generic;

namespace MGE.UI;

public abstract class UIContainer : UIWidget
{
	public List<UIWidget> widgets { get; } = new();

	internal override void AttachChildren()
	{
		foreach (var widget in widgets)
		{
			widget.AttachTo(this);
		}
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

	internal void ChildMeasureChanged(UIWidget widget) => OnChildMeasureChanged(widget);
	protected virtual void OnChildMeasureChanged(UIWidget widget) { }

	internal override void DoRender()
	{
		base.DoRender();

		foreach (var widget in widgets)
		{
			widget.DoRender();
		}
	}
}
