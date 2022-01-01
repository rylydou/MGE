using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace MGE.UI;

public abstract class UIContainer : UIWidget
{
	private bool _childrenDirty = true;
	private readonly List<UIWidget> _childrenCopy = new List<UIWidget>();

	[XmlIgnore]
	[Browsable(false)]
	public abstract int ChildrenCount { get; }

	internal List<UIWidget> ChildrenCopy
	{
		get
		{
			// We return copy of our collection
			// To prevent exception when someone modifies the collection during the iteration
			UpdateWidgets();

			return _childrenCopy;
		}
	}

	public override bool Enabled
	{
		get => base.Enabled;
		set
		{
			if (base.Enabled == value) return;

			base.Enabled = value;

			foreach (var item in ChildrenCopy)
			{
				item.Enabled = value;
			}
		}
	}

	public override UIDesktop? Desktop
	{
		get => base.Desktop;
		internal set
		{
			base.Desktop = value;

			foreach (var child in ChildrenCopy)
			{
				child.Desktop = value;
			}
		}
	}

	public abstract UIWidget GetChild(int index);

	private void UpdateWidgets()
	{
		if (!_childrenDirty) return;

		_childrenCopy.Clear();

		for (var i = 0; i < ChildrenCount; ++i)
		{
			_childrenCopy.Add(GetChild(i));
		}

		_childrenCopy.SortWidgetsByZIndex();

		_childrenDirty = false;
	}

	protected void InvalidateChildren()
	{
		InvalidateMeasure();
		_childrenDirty = true;
	}

	public override void OnMouseEntered()
	{
		base.OnMouseEntered();

		ChildrenCopy.ProcessMouseMovement();
	}

	public override void OnMouseLeft()
	{
		base.OnMouseLeft();

		ChildrenCopy.ProcessMouseMovement();
	}

	public override void OnMouseMoved()
	{
		base.OnMouseMoved();

		ChildrenCopy.ProcessMouseMovement();
	}

	public override void OnTouchEntered()
	{
		base.OnTouchEntered();

		ChildrenCopy.ProcessTouchMovement();
	}

	public override void OnTouchLeft()
	{
		base.OnTouchLeft();

		ChildrenCopy.ProcessTouchMovement();
	}

	public override void OnTouchMoved()
	{
		base.OnTouchMoved();

		ChildrenCopy.ProcessTouchMovement();
	}

	public override void OnTouchDown()
	{
		base.OnTouchDown();

		ChildrenCopy.ProcessTouchDown();
	}

	public override void OnTouchUp()
	{
		base.OnTouchUp();

		ChildrenCopy.ProcessTouchUp();
	}

	public override void OnTouchDoubleClick()
	{
		base.OnTouchDoubleClick();

		ChildrenCopy.ProcessTouchDoubleClick();
	}

	internal override void MoveChildren(Vector2Int delta)
	{
		base.MoveChildren(delta);

		foreach (var child in ChildrenCopy)
		{
			if (!child.Visible) continue;

			child.MoveChildren(delta);
		}
	}

	public override void InternalRender(UIRenderContext context)
	{
		foreach (var child in ChildrenCopy)
		{
			if (!child.Visible) continue;

			child.Render(context);
		}
	}

	public int CalculateTotalChildCount(bool visibleOnly)
	{
		var result = ChildrenCopy.Count;

		foreach (var child in ChildrenCopy)
		{
			if (visibleOnly && !child.Visible) continue;

			var asCont = child as UIContainer;
			if (asCont is not null)
			{
				result += asCont.CalculateTotalChildCount(visibleOnly);
			}
		}

		return result;
	}

	public abstract void RemoveChild(UIWidget widget);


	/// <summary>
	/// Finds first child widget of type <typeparamref name="T"/> with specified <paramref name="Id"/>
	/// </summary>
	/// <typeparam name="T">UIWidget type</typeparam>
	/// <param name="Id">Id of widget</param>
	/// <returns>UIWidget instance if found otherwise null</returns>
	public T? FindChildById<T>(string Id) where T : UIWidget
	{
		return FindChild<T>(this, w => w.Id == Id);
	}

	/// <summary>
	/// Finds first child widget of type <typeparamref name="T"/>. If <paramref name="predicate"/> is null -
	/// the first widget of <typeparamref name="T"/> is returned,
	/// otherwise the first widget of <typeparamref name="T"/> matching <paramref name="predicate"/> is returned.
	/// </summary>
	/// <typeparam name="T">UIWidget type</typeparam>
	/// <param name="predicate">Predicate to match on widget</param>
	/// <returns>UIWidget instance if found otherwise null</returns>
	public T? FindChild<T>(Func<T, bool>? predicate = null) where T : UIWidget
	{
		return FindChild(this, predicate);
	}

	/// <summary>
	/// Finds the first widget with matching <paramref name="Id"/>
	/// </summary>
	/// <param name="Id">Id to match on</param>
	/// <returns>UIWidget instance if found otherwise null</returns>
	public UIWidget? FindChildById(string Id)
	{
		return FindChild(this, w => w.Id == Id);
	}

	/// <summary>
	/// Finds the first child found by predicate.
	/// </summary>
	/// <param name="predicate">Predicate to match on widget</param>
	/// <returns>UIWidget instance if found otherwise null</returns>
	public UIWidget? FindChild(Func<UIWidget, bool> predicate)
	{
		return FindChild(this, predicate);
	}

	/// <summary>
	/// Gets all children in container matching on optional predicate.
	/// </summary>
	/// <param name="recursive">If true, indicates that child containers will also be iterated.</param>
	/// <param name="predicate">Predicate to filter children</param>
	/// <returns>Children found</returns>
	public IEnumerable<UIWidget> GetChildren(bool recursive = false, Func<UIWidget, bool>? predicate = null)
	{
		return GetChildren(this, recursive, predicate);
	}

	internal static IEnumerable<UIWidget> GetChildren(UIContainer container, bool recursive = false, Func<UIWidget, bool>? predicate = null)
	{
		foreach (var widget in container.ChildrenCopy)
		{
			if (predicate?.Invoke(widget) ?? true)
			{
				yield return widget;
			}

			if (recursive && widget is UIContainer childContainer)
			{
				foreach (var innerWidget in GetChildren(childContainer, recursive, predicate))
				{
					yield return innerWidget;
				}
			}
		}
	}

	internal static T? FindChildById<T>(UIContainer container, string Id) where T : UIWidget
	{
		return FindChild<T>(container, w => w.Id == Id);
	}

	internal static UIWidget? FindChildById(UIContainer container, string Id)
	{
		return FindChild(container, w => w.Id == Id);
	}

	internal static T? FindChild<T>(UIContainer container, Func<T, bool>? predicate = null) where T : UIWidget
	{
		foreach (var widget in container.ChildrenCopy)
		{
			if (widget is T casted && (predicate?.Invoke(casted) ?? true))
			{
				return casted;
			}
			else if (widget is UIContainer childContainer)
			{
				var child = FindChild(childContainer, predicate);

				if (child is not null) return child;
			}
		}

		return null;
	}

	internal static UIWidget? FindChild(UIContainer container, Func<UIWidget, bool>? predicate = null)
	{
		foreach (var widget in container.ChildrenCopy)
		{
			if (predicate?.Invoke(widget) ?? true)
			{
				return widget;
			}
			else if (widget is UIContainer childContainer)
			{
				var child = FindChild(childContainer, predicate);

				if (child is not null)
				{
					return child;
				}
			}
		}

		return null;
	}
}
