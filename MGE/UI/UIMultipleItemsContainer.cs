using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace MGE.UI;

public interface UIIMultipleItemsContainer
{
	ObservableCollection<UIWidget> Widgets { get; }
	T AddChild<T>(T widget) where T : UIWidget;
}

public class UIMultipleItemsContainerBase : UIContainer, UIIMultipleItemsContainer
{
	protected readonly ObservableCollection<UIWidget> _widgets = new ObservableCollection<UIWidget>();

	public override int ChildrenCount { get => _widgets.Count; }

	[Browsable(false)]
	// [Content]
	public virtual ObservableCollection<UIWidget> Widgets { get => _widgets; }

	[DefaultValue(UIAlignment.Fill)]
	public override UIAlignment horizontalAlignment { get => base.horizontalAlignment; set => base.horizontalAlignment = value; }

	[DefaultValue(UIAlignment.Fill)]
	public override UIAlignment verticalAlignment { get => base.verticalAlignment; set => base.verticalAlignment = value; }

	public UIMultipleItemsContainerBase()
	{
		_widgets.CollectionChanged += WidgetsOnCollectionChanged;

		horizontalAlignment = UIAlignment.Fill;
		verticalAlignment = UIAlignment.Fill;
	}

	public override UIWidget GetChild(int index) => _widgets[index];

	private void WidgetsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
	{
		if (args.Action == NotifyCollectionChangedAction.Add)
		{
			foreach (UIWidget w in args.NewItems!)
			{
				w.canvas = canvas;
				w.parent = this;
			}
		}
		else if (args.Action == NotifyCollectionChangedAction.Remove)
		{
			foreach (UIWidget w in args.OldItems!)
			{
				w.canvas = null;
				w.parent = null;
			}
		}
		else if (args.Action == NotifyCollectionChangedAction.Reset)
		{
			foreach (UIWidget w in ChildrenCopy)
			{
				w.canvas = null;
				w.parent = null;
			}
		}

		InvalidateChildren();
	}

	public T AddChild<T>(T widget) where T : UIWidget
	{
		Widgets.Add(widget);
		return widget;
	}

	public override void RemoveChild(UIWidget widget)
	{
		_widgets.Remove(widget);
	}
}
