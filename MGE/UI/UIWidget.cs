namespace MGE.UI;

public abstract class UIWidget
{
	public Rect box;
	public UIStyle currentStyle = new();

	public bool isMouseOver { get; private set; }

	public UIContainer? parent { get; internal set; }

	public virtual void Update(float deltaTime)
	{
		if (box.Contains(Input.mousePosition))
		{
			if (!isMouseOver)
			{
				OnMouseEnter();
			}

			OnMouseOver();

			isMouseOver = true;
		}
		else if (isMouseOver)
		{
			isMouseOver = false;
			OnMouseExit();
		}
	}

	protected virtual void OnMouseEnter() { }
	protected virtual void OnMouseOver() { }
	protected virtual void OnMouseExit() { }

	protected virtual void OnPressed() { }

	public virtual void Draw()
	{
		GFX.DrawBox(box, currentStyle.backgroundColor);
		GFX.DrawRect(box, currentStyle.borderColor, currentStyle.borderWidth);
	}
}
