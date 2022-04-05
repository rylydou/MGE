using System;
using System.Collections.Generic;

namespace MGE;

public abstract class Node
{
	[HiddenProp] public string name;

	public Node()
	{
		name = GetType().Name;
		RegisterCallbacks();
	}

	[HiddenProp] List<Node> _children = new();

	public Node? parent { get; private set; }

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

	public void AddChild(Node node)
	{
		if (node.parent is not null) throw new Exception("Add child", "Node is already a child of something");

		node.parent = this;
		_children.Add(node);
		onChildAdded(node);
	}

	public void RemoveChild(Node node)
	{
		if (node.parent is null) throw new Exception("Remove Child", "Node is not a child of anything");
		if (node.parent != this) throw new Exception("Remove Child", "Parent doesn't own the child");

		node.parent = null;
		_children.Remove(node);
		onChildRemoved(node);
	}

	#endregion Node Management

	#region Events

	protected virtual void RegisterCallbacks()
	{
		onEnterScene += OnEnterScene;
		onExitScene += OnExitScene;

		onChildAdded += OnChildAdded;
		onChildRemoved += OnChildRemoved;

		onChildAddedDeep += OnChildAddedDeep;
		onChildRemovedDeep += OnChildRemovedDeep;

		onReady += Ready;

		onTick += Tick;
		onTick += (delta) => GetChildren<Node>().ForEach(c => c.onTick(delta));
		onUpdate += Update;
		onUpdate += (delta) => GetChildren<Node>().ForEach(c => c.onUpdate(delta));
	}

	public Action onEnterScene = () => { };
	protected virtual void OnEnterScene() { }

	public Action onExitScene = () => { };
	protected virtual void OnExitScene() { }

	public Action<Node> onChildAdded = node => { };
	protected virtual void OnChildAdded(Node node) { }

	public Action<Node> onChildRemoved = node => { };
	protected virtual void OnChildRemoved(Node node) { }

	// When a child or grandchild is changed
	public Action<Node> onChildAddedDeep = node => { };
	protected virtual void OnChildAddedDeep(Node node) { }

	public Action<Node> onChildRemovedDeep = node => { };
	protected virtual void OnChildRemovedDeep(Node node) { }

	// When a child or grandchild is changed and the signal has not been handled by any other parent
	public Func<Node, bool> onChildAddedDeepUnhandled = node => false;
	protected virtual bool OnChildAddedDeepUnhandled(Node node) => false;

	public Func<Node, bool> onChildRemovedDeepUnhandled = node => false;
	protected virtual bool OnChildRemovedDeepUnhandled(Node node) => false;

	#endregion Events

	#region Loops

	public Action onReady = () => { };
	protected virtual void Ready() { }

	public delegate void TickDelegate(float delta);
	public TickDelegate onTick = (delta) => { };
	protected virtual void Tick(float delta) { }

	public delegate void UpdateDelegate(float delta);
	public UpdateDelegate onUpdate = (delta) => { };
	protected virtual void Update(float delta) { }

	#endregion Loops
}
