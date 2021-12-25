namespace MGE.UI;

public abstract class UIContainer : UIWidget
{
	public virtual void AddWidget(UIWidget widget)
	{
		widget.parent = this;
	}

	public virtual void RemoveWidget(UIWidget widget)
	{
		widget.parent = null;
	}
}
