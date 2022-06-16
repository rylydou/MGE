using System.Collections.Generic;

namespace MGE.UI;

public abstract class UIContainer : UIWidget
{
	[Prop] Thickness _padding;
	public Thickness padding
	{
		get => _padding;
		set
		{
			for (int i = 0; i < 2; i++)
			{
				if (_padding[i] != value[i] || _padding[i + 2] != value[i + 2])
				{
					_padding[i] = value[i];
					_padding[i + 2] = value[i];
					OnPropertiesChanged(i);
				}
			}
		}
	}

	public RectInt relativeContentRect => relativeRect - _padding;
	public RectInt absoluteContentRect => absoluteRect - _padding;

	[Prop] public List<UIWidget> children = new();

	internal override void OnPositionChanged(int dir)
	{
		base.OnPositionChanged(dir);

		foreach (var widget in children)
		{
			widget.OnPositionChanged(dir);
		}
	}

	protected override void OnAttached()
	{
		foreach (var widget in children)
		{
			widget.AttachTo(this);
		}

		base.OnAttached();
	}

	public void AddChild(UIWidget widget)
	{
		children.Add(widget);
		widget.AttachTo(this);
		OnChildAdded(widget);
	}

	public void InsertChild(int index, UIWidget widget)
	{
		children.Insert(index, widget);
		widget.AttachTo(this);
		OnChildAdded(widget);
	}

	protected virtual void OnChildAdded(UIWidget widget) { }

	public void RemoveChild(UIWidget widget)
	{
		children.Remove(widget);
		OnChildRemoved(widget);
	}
	protected virtual void OnChildRemoved(UIWidget widget) { }

	internal void ChildMeasureChanged(int dir, UIWidget widget)
	{
		_layoutFlashTime[dir] = 1f;
		OnChildMeasureChanged(dir, widget);
	}
	protected virtual void OnChildMeasureChanged(int dir, UIWidget widget) { }

	internal override void DoRender(Batch2D batch)
	{
		base.DoRender(batch);

		foreach (var widget in children)
		{
			widget.DoRender(batch);
		}
	}

	public UIWidget this[int index]
	{
		get => children[index];
		set => children[index] = value;
	}
}
