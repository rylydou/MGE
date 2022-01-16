namespace MGE.UI;

public class UIButton : UISingleItemContainer<UIWidget>
{
	public Action onClicked = () => { };

	public override void OnClick()
	{
		base.OnClick();

		onClicked.Invoke();
	}
}
