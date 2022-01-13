using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace MGE.UI;

public abstract class UISingleItemContainer<T> : UIContainer where T : UIWidget
{
	T? _internalChild;

	[Browsable(false)]
	[XmlIgnore]
	protected internal virtual T? InternalChild
	{
		get => _internalChild;
		set
		{
			if (_internalChild is not null)
			{
				_internalChild.parent = null;
				_internalChild.canvas = null;

				_internalChild = null;
			}

			_internalChild = value;

			if (_internalChild is not null)
			{
				_internalChild.parent = this;
				_internalChild.canvas = canvas;
			}

			InvalidateChildren();
		}
	}

	public override int ChildrenCount { get => InternalChild is not null ? 1 : 0; }

	public override UIWidget GetChild(int index)
	{
		if (index < 0 || InternalChild is null || index >= 1) throw new ArgumentOutOfRangeException("index");

		return InternalChild;
	}

	public override void Arrange()
	{
		base.Arrange();

		if (InternalChild is null) return;

		InternalChild.Layout(actualBounds);
	}

	protected override Vector2Int InternalMeasure(Vector2Int availableSize)
	{
		var result = Vector2Int.zero;

		if (InternalChild is not null)
		{
			result = InternalChild.Measure(availableSize);
		}

		return result;
	}

	public override void RemoveChild(UIWidget widget)
	{
		if (widget != InternalChild) return;

		InternalChild = null;
	}
}
