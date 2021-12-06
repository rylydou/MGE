using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MGE;

public class Node : Object
{
	public string name;

	public bool enabled = true;
	public bool visible = true;

	bool _isActive;
	bool _isInitialized;
	bool _isDestroyed;

	public Node? parent { get; private set; }

	public int childCount => _children.Count;

	public Node this[int index] { get => _children.ElementAt(index); set => AttachNode(value, index); }

	List<Node> _children = new();

	public Node()
	{
		name = GetType().ToString();

		if (this is RootNode)
		{
			_isActive = true;
		}
	}

	#region Node Management

	public void AttachNode(Node node)
	{
		if (node.parent is not null) throw new MGEException("Attach node", "Node already has an owner");

		node.DoAttach(this);
		_children.Add(node);
	}

	public void AttachNode(Node node, int index)
	{
		if (node.parent is not null) throw new MGEException("Attach node", "Node already has an owner");

		node.DoAttach(this);
		_children.Insert(index, node);
	}

	public void DetachNode(Node node)
	{
		if (node.parent != this) throw new MGEException("Detach node", "This node does not own the node");

		node.DoDetach();
		_children.Remove(node);
	}

	public Node DetachNode(int index)
	{
		if (index < 0 || index >= childCount) throw new IndexOutOfRangeException();

		var node = _children[index];
		DetachNode(node);
		_children.Remove(node);

		return node;
	}

	public Node[] DetachAllNodes()
	{
		var children = _children.ToArray();

		foreach (var child in _children)
			child.DoDetach();
		_children.Clear();

		return children;
	}

	#endregion Node Management

	#region Node Lookup

	public T GetChild<T>() where T : Node
	{
		foreach (var child in _children)
			if (child is T node) return node;
		throw new MGEException($"Cannot find child of type {typeof(T)}");
	}

	public IEnumerable<T> GetChildren<T>() where T : Node
	{
		var foundChildren = new List<T>(_children.Count);
		foreach (var child in _children)
			if (child is T node) foundChildren.Add(node);
		return foundChildren;
	}

	public IEnumerable<T> GetChildrenRecursive<T>() where T : Node
	{
		var foundChildren = new List<T>(_children.Count);
		foreach (var child in _children)
			foundChildren.AddRange(child.GetChildrenRecursive<T>());
		return foundChildren;
	}

	public T GetParent<T>() where T : Node
	{
		var parent = this.parent;
		while (parent is not null)
		{
			if (parent is T node) return node;
			parent = parent.parent;
		}
		throw new MGEException($"Cannot find parent of type {typeof(T)}");
	}

	public bool TryGetParent<T>([MaybeNullWhen(false)] out T result) where T : Node
	{
		var parent = this.parent;
		while (parent is not null)
		{
			if (parent is T node)
			{
				result = node;
				return true;
			}
			parent = parent.parent;
		}
		result = default(T);
		return false;
	}

	#endregion Node Lookup

	#region Game Loop

	internal void DoInit()
	{
		if (_isInitialized) throw new MGEException("Initialize node", "Node is already initialized");
		if (!_isActive) throw new MGEException("Initialize node", "Node is not active in scene");

		_isInitialized = true;

		Init();
	}
	/// <summary>
	/// Called only once when the node is created.
	/// </summary>
	/// <remarks>
	/// Handle loading assets here.
	/// </remarks>
	protected virtual void Init()
	{
		foreach (var child in _children.ToArray())
			child.DoInit();
	}

	internal void DoTick(float deltaTime)
	{
		Tick(deltaTime);
	}
	/// <summary>
	/// Called every tick a constant amount of times a second.
	/// </summary>
	/// <param name="deltaTime">The time in seconds between ticks.</param>
	/// <remarks>
	/// Handle things like gameplay, physics, and networking logic here.
	/// </remarks>
	protected virtual void Tick(float deltaTime)
	{
		if (!enabled) return;

		foreach (var child in _children.ToArray())
			child.DoTick(deltaTime);
	}

	/// <summary>
	/// Called every update, can be a variable amount of times per second.
	/// </summary>
	/// <param name="deltaTime">The amount of time in seconds since the last update.</param>
	/// <remarks>
	/// Handle non gamplay logic here, like getting input or animations.
	/// </remarks>
	internal void DoUpdate(float deltaTime)
	{
		if (!enabled) return;

		Update(deltaTime);
	}
	protected virtual void Update(float deltaTime)
	{
		foreach (var child in _children.ToArray())
			child.DoUpdate(deltaTime);
	}

	internal void DoDraw()
	{
		if (!visible) return;

		Draw();
	}
	/// <summary>
	/// Called when the object is going to be rendered.
	/// </summary>
	/// <remarks>
	/// This is not guaranteed to be called after every update due to it being called in a separate thread.
	/// </remarks>
	protected virtual void Draw()
	{
		foreach (var child in _children.ToArray())
			child.DoDraw();
	}

	#endregion

	#region Events

	void DoAttach(Node parent)
	{
		this.parent = parent;

		_isActive = parent._isActive;

		if (_isActive && !_isInitialized)
		{
			DoInit();
		}

		WhenAttached();

		parent.ChildAttached(this);
	}
	/// <summary>
	/// Called after the node is attached to a parent.
	/// </summary>
	/// <remarks>
	/// Handle referencing parents here.
	/// <see cref="parent"/> is not null.
	/// </remarks>
	protected virtual void WhenAttached()
	{
		foreach (var child in _children.ToArray())
			child.DoDetach();
	}

	void DoDetach()
	{
		WhenDetached();

		parent!.ChildDetached(this);

		parent = null;
		_isActive = false;
	}
	/// <summary>
	/// Called before the node is detached from its parent and before the node is destroyed.
	/// </summary>
	/// <remarks>
	/// <see cref="parent"/> is not null.
	/// </remarks>
	protected virtual void WhenDetached()
	{
		foreach (var child in _children.ToArray())
			child.DoDetach();
	}

	void ChildAttached(Node child)
	{
		if (WhenChildAttached(child))
			parent?.ChildAttached(child);
	}
	/// <summary>
	/// Called when a child is attached to the node or its children.
	/// </summary>
	/// <param name="child">The child that was attached.</param>
	/// <returns><see langword="true"/> if the singal should be passed onto the parent.</returns>
	protected virtual bool WhenChildAttached(Node child) => true;

	/// <summary>
	/// Called when a child is detached to the node or its children.
	/// </summary>
	/// <param name="child">The child that was detached.</param>
	/// <returns><see langword="true"/> if the singal should be passed onto the parent.</returns>
	void ChildDetached(Node child)
	{
		if (WhenChildDetached(child))
			parent?.ChildDetached(child);
	}
	protected virtual bool WhenChildDetached(Node child) => true;

	/// <summary>
	/// A Node can be destroyed even if its not active in the scene
	/// </summary>
	public void Destroy()
	{
		if (_isInitialized) throw new MGEException("Destroy node", "Node is not initialized");
		if (_isDestroyed) throw new MGEException("Destroy node", "Node is already destroyed");

		if (parent is not null)
			parent.DetachNode(this);

		Destroyed();
		_isDestroyed = true;
	}
	/// <summary>
	/// Called when the object is being destroyed after <see cref="WhenDetached"/> is called.
	/// </summary>
	/// <remarks>
	/// Handle things like deleting <see cref="IDisposable"/> resources here like <see cref="Graphics.Texture"/>.
	/// </remarks>
	protected virtual void Destroyed()
	{
		foreach (var child in _children.ToArray())
			child.Destroy();
	}

	#endregion Events

	public override string? ToString() => $"{name} ({GetType().Name})";
}
