using System;
using System.Collections.Generic;

namespace MGE;

public abstract class Node
{
	[Prop] public string name;

	public Node()
	{
		name = GetType().Name;
	}

	[Prop] List<Node> _children = new();

	#region Node Querying

	public IEnumerable<T> GetChildren<T>()
	{
		foreach (var child in _children)
		{
			if (child is T childT)
			{
				yield return childT;
			}
		}
	}

	public IEnumerable<T> GetChildrenRecursive<T>()
	{
		foreach (var child in _children)
		{
			if (child is T childT)
			{
				yield return childT;
			}
			else
			{
				foreach (var item in child.GetChildrenRecursive<T>())
				{
					yield return item;
				}
			}
		}
	}

	public IEnumerable<T> GetChildrenRecursiveDeep<T>()
	{
		foreach (var child in _children)
		{
			if (child is T childT)
			{
				yield return childT;
			}

			foreach (var item in child.GetChildrenRecursive<T>())
			{
				yield return item;
			}
		}
	}

	#endregion Node Querying

	#region Node Management

	public Action<Node> onNodeAttached = node => { };
	public void AttachNode(Node node)
	{
		_children.Add(node);
		OnAttachNode(node);
	}
	public virtual void OnAttachNode(Node node)
	{
		onNodeAttached.Invoke(node);
	}

	public Action<Node> onNodeDetached = node => { };
	public void DetachNode(Node node)
	{
		_children.Remove(node);
		OnDetachNode(node);
	}
	public virtual void OnDetachNode(Node node)
	{
		onNodeDetached.Invoke(node);
	}

	#endregion Node Management

	#region Events

	public virtual void _OnEnterScene() { }
	public virtual void _OnExitScene() { }

	#endregion Events

	#region Loops

	internal void Update(float delta)
	{
		_Update(delta);

		foreach (var child in GetChildren<Node>())
		{
			child._Update(delta);
		}
	}

	public virtual void _Update(float delta) { }

	public virtual void _Tick(float delta)
	{
		foreach (var child in GetChildren<Node>())
		{
			child._Tick(delta);
		}
	}

	#endregion Loops
}
