using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MGE;

public class Node : Object, IEnumerable<Node>
{
	class ChildEnumerator : IEnumerator<Node>
	{
		public Node Current => _intEnum.Current;
		object IEnumerator.Current => _intEnum.Current;

		Node _node;
		IEnumerator<Node> _intEnum;

		public ChildEnumerator(Node node)
		{
			_node = node;
			_intEnum = node._children.GetEnumerator();
			node._activeEnums++;
		}

		public bool MoveNext()
		{
			while (_intEnum.MoveNext())
			{
				var current = _intEnum.Current;

				// Ignore the node if it is destroyed or not attached to the node anymore
				if (current._isDestroyed) continue;
				if (current.parent != _node) continue;

				return true;
			}
			return false;
		}

		public void Reset()
		{
			_intEnum.Reset();
		}

		public void Dispose()
		{
			_intEnum.Dispose();
			_node._activeEnums--;
			_node.TryFlushActions();
		}
	}

	class ParentEnumerator : IEnumerator<Node>
	{
		readonly Node _node;

		Node _currentParentNode;

		public Node Current => _currentParentNode;
		object IEnumerator.Current => _currentParentNode;

		public ParentEnumerator(Node node)
		{
			_node = node;
			_currentParentNode = node;
		}

		public bool MoveNext()
		{
			if (_currentParentNode.parent is not null)
			{
				_currentParentNode = _currentParentNode.parent;
				return true;
			}
			return false;
		}

		public void Reset() => _currentParentNode = _node;

		public void Dispose() { }
	}

	class NodeCollection : IEnumerable<Node>
	{
		readonly IEnumerator<Node> enumerator;

		public NodeCollection(IEnumerator<Node> enumerator)
		{
			this.enumerator = enumerator;
		}

		public IEnumerator<Node> GetEnumerator() => enumerator;
		IEnumerator IEnumerable.GetEnumerator() => enumerator;
	}

	public string name;

	public bool enabled = true;
	public bool visible = true;

	bool _isActive;
	bool _isInitialized;
	bool _isDestroyed;

	public Node? parent { get; private set; }

	public int childCount => _children.Count;

	public IEnumerable<Node> children => new NodeCollection(new ChildEnumerator(this));
	public IEnumerable<Node> parents => new NodeCollection(new ParentEnumerator(this));

	public Node this[int index] { get => _children.ElementAt(index); set => AttachNode(value, index); }

	List<Node> _children = new();
	int _activeEnums;

	#region Actions

	Queue<Action> _queuedActions = new();

	void TryDoAction(Action action)
	{
		if (_activeEnums < 1)
		{
			action.Invoke();
			return;
		}
		_queuedActions.Enqueue(action);
	}

	void TryFlushActions()
	{
		if (_activeEnums > 0) return;

		// Debug.Log("Flushing actions");

		while (_queuedActions.TryDequeue(out var action))
		{
			action.Invoke();
		}
	}

	#endregion Actions

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

		TryDoAction(() => _children.Add(node));
	}

	public void AttachNode(Node node, int index)
	{
		if (node.parent is not null) throw new MGEException("Attach node", "Node already has an owner");

		node.DoAttach(this);

		TryDoAction(() => _children.Insert(index, node));
	}

	public void DetachNode(Node node)
	{
		if (node.parent != this) throw new MGEException("Detach node", "This node does not own the node");

		node.DoDetach();
		TryDoAction(() => _children.Remove(node));
	}

	public Node DetachNode(int index)
	{
		if (index < 0 || index >= childCount) throw new IndexOutOfRangeException();

		var node = this.ElementAt(index);
		DetachNode(node);
		TryDoAction(() => _children.RemoveAt(index));

		return node;
	}

	public Node[] DetachAllNodes()
	{
		this.ForEach(child => child.DoDetach());
		TryDoAction(() => _children.Clear());
		return _children.ToArray();
	}

	public int FindIndexOfNode(Node node) => this.IndexOf(node);

	public bool ContainsNode(Node node) => this.Contains<Node>(node);

	public IEnumerator<Node> GetEnumerator() => new ChildEnumerator(this);
	IEnumerator IEnumerable.GetEnumerator() => new ChildEnumerator(this);

	#endregion Node Management

	#region Game Loop

	internal void DoInit()
	{
		if (_isInitialized) throw new MGEException("Initialize node", "Node is already initialized");
		if (!_isActive) throw new MGEException("Initialize node", "Node is not active in scene");

		Debug.Log("Inited " + this);

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
		this.ForEach(child => child.DoInit());
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

		this.ForEach(child => child.DoTick(deltaTime));
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
		this.ForEach(child => child.DoUpdate(deltaTime));
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
		this.ForEach(child => child.DoDraw());
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
		this.ForEach(child => child.DoDetach());
	}

	void DoDetach()
	{
		WhenDetached();

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
		this.ForEach(child => child.DoDetach());
	}

	/// <summary>
	/// A Node can be destroyed even if its not active in the scene
	/// </summary>
	public void Destroy()
	{
		if (_isInitialized) throw new MGEException("Destroy node", "Node is not initialized");
		if (_isDestroyed) throw new MGEException("Destroy node", "Node is already destroyed");

		if (parent is not null)
		{
			parent.DetachNode(this);
		}

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
		foreach (var child in _children) child.Destroy();
	}

	#endregion Events
}
