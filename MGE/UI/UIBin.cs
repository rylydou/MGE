namespace MGE.UI;

public abstract class UIBin : UIContainer
{
	public UIWidget? child;

	public override void AddWidget(UIWidget widget)
	{
		base.AddWidget(widget);

		child = widget;
	}

	public override void RemoveWidget(UIWidget widget)
	{
		base.AddWidget(widget);

		// Who cares about the args amirite?
		child = null;
	}

	public override void Draw()
	{
		base.Draw();

		child?.Draw();
	}
}
