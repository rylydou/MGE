using System.Collections.Generic;

namespace MGE.UI;

public abstract class UIContainer : UIWidget
{
	public List<UIWidget> widgets { get; } = new();

	protected virtual void OnChildAdded(UIWidget widget) { }
	protected virtual void OnChildRemoved(UIWidget widget) { }
}
