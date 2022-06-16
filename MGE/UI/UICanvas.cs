namespace MGE.UI;

public class UICanvas : UIBox
{
	public bool enableDebug = false;

	public UIWidget? hoveredWidget;

	public UIWidget? focusedWidget;
	public float focusTransitionDuration = 1f / 15;
	float _focusTransitionTime;
	Rect _previousFocusRect;

	public override bool isIntractable => true;

	public UICanvas()
	{
		canvas = this;
	}

	internal override void OnPropertiesChanged(int dir)
	{
		base.OnPropertiesChanged(dir);

		SetMySize(dir, fixedSize[dir]);
	}

	public void UpdateInputs(Vector2 mousePosition, Mouse mouse, Keyboard keyboard)
	{
		hoveredWidget = FindHoveredWidget(this, mousePosition);
	}

	static UIWidget? FindHoveredWidget(UIWidget widget, in Vector2 mousePosition)
	{
		if (widget is UIContainer container)
		{
			foreach (var child in container.children)
			{
				if (!child.absoluteRect.Contains(mousePosition)) continue;

				var hoveredWidget = FindHoveredWidget(child, mousePosition);
				if (hoveredWidget is not null)
					return hoveredWidget;
			}
		}

		return widget.isIntractable ? widget : null;
	}

	public void Update(float delta)
	{
		_focusTransitionTime += delta;
	}

	public void RenderCanvas(Batch2D batch)
	{
		DoRender(batch);

		if (focusedWidget is not null)
		{
			var previousRect = _previousFocusRect.Expanded(2);
			var currentRect = focusedWidget.absoluteRect.Expanded(2);
			var time = _focusTransitionTime / focusTransitionDuration;
			var rect = Rect.LerpClamped(previousRect, currentRect, time);

			batch.SetRect(rect, 1, new(0x0A84FFFF));
		}
	}
}
