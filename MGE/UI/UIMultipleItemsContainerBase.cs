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
	public override UIAlignment HorizontalAlignment { get => base.HorizontalAlignment; set => base.HorizontalAlignment = value; }

	[DefaultValue(UIAlignment.Fill)]
	public override UIAlignment VerticalAlignment { get => base.VerticalAlignment; set => base.VerticalAlignment = value; }

	public UIMultipleItemsContainerBase()
	{
		_widgets.CollectionChanged += WidgetsOnCollectionChanged;

		HorizontalAlignment = UIAlignment.Fill;
		VerticalAlignment = UIAlignment.Fill;
	}

	public override UIWidget GetChild(int index) => _widgets[index];

	private void WidgetsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
	{
		if (args.Action == NotifyCollectionChangedAction.Add)
		{
			foreach (UIWidget w in args.NewItems!)
			{
				w.Desktop = Desktop;
				w.Parent = this;
			}
		}
		else if (args.Action == NotifyCollectionChangedAction.Remove)
		{
			foreach (UIWidget w in args.OldItems!)
			{
				w.Desktop = null;
				w.Parent = null;
			}
		}
		else if (args.Action == NotifyCollectionChangedAction.Reset)
		{
			foreach (UIWidget w in ChildrenCopy)
			{
				w.Desktop = null;
				w.Parent = null;
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
