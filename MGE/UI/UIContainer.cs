using System.Collections.Generic;

namespace MGE.UI;

public abstract class UIContainer : UIWidget
{
	public List<UIWidget> widgets { get; } = new();

	internal void ChildAdded(UIWidget widget) => OnChildAdded(widget);
	protected virtual void OnChildAdded(UIWidget widget) { }

	internal void ChildRemoved(UIWidget widget) => OnChildRemoved(widget);
	protected virtual void OnChildRemoved(UIWidget widget) { }

	internal void ChildMeasureChanged(UIWidget widget) => OnChildMeasureChanged(widget);
	protected virtual void OnChildMeasureChanged(UIWidget widget) { }
}
