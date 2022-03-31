using System;
using System.Collections.Generic;

namespace MGE;

public abstract class Node
{
	[Prop] public string name;

	public Node()
	{
		Log.Info("Node constructed");
		name = GetType().Name;
		RegisterCallbacks();
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

	protected virtual void RegisterCallbacks()
	{
		onEnterScene += OnEnterScene;
		onExitScene += OnExitScene;

		onTick += Tick;
		onTick += (delta) => GetChildren<Node>().ForEach(c => c.onTick(delta));
		onUpdate += Update;
		onUpdate += (delta) => GetChildren<Node>().ForEach(c => c.onUpdate(delta));
	}

	public Action onEnterScene = () => { };
	protected virtual void OnEnterScene() { }

	public Action onExitScene = () => { };
	protected virtual void OnExitScene() { }

	#endregion Events

	#region Loops

	public delegate void TickDelegate(float delta);
	public TickDelegate onTick = (delta) => { };
	protected virtual void Tick(float delta) { }

	public delegate void UpdateDelegate(float delta);
	public UpdateDelegate onUpdate = (delta) => { };
	protected virtual void Update(float delta) { }

	#endregion Loops
}
