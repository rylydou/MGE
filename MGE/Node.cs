using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MGE.Graphics;

namespace MGE;

public class Node : Object, IList<Node>
{
	class ChildrenEnumerator : IEnumerator<Node>
	{
		public Node Current => _childrenEnum.Current;
		object IEnumerator.Current => _childrenEnum.Current;

		Node _node;
		IEnumerator<Node> _childrenEnum;

		public ChildrenEnumerator(Node node)
		{
			_node = node;
			_childrenEnum = node.GetEnumerator();
			node.activeEnums++;
		}

		public bool MoveNext()
		{
			while (_childrenEnum.MoveNext())
			{
				var current = _childrenEnum.Current;

				// Ignore the node if it is destroyed or not attached to the node anymore
				if (current.destroyed) continue;
				if (current.parent != _node) continue;

				return true;
			}
			return false;
		}

		public void Reset()
		{
			_childrenEnum.Reset();
		}

		public void Dispose()
		{
			_childrenEnum.Dispose();
			_node.activeEnums--;
			_node.TryFlushActions();
		}
	}

	public string name;

	public bool enabled;
	public bool visible;

	bool destroyed;

	public Node? parent { get; internal set; }

	public int Count => _children.Count;

	public bool IsReadOnly => false;

	public Node this[int index] { get => _children.ElementAt(index); set => throw new NotImplementedException(); }

	List<Node> _children = new();
	int activeEnums;

	// For actions that involes directly changing the raw list of children, this cannot be done during iteration so they will be queued for when iteration is done
	#region Actions

	Queue<Action> _queuedActions = new();

	void TryDoAction(Action action)
	{
		if (activeEnums < 1)
		{
			action.Invoke();
			return;
		}
		_queuedActions.Enqueue(action);
	}

	void TryFlushActions()
	{
		if (activeEnums > 0) return;

		Debug.Log("Flushing actions");

		while (_queuedActions.TryDequeue(out var action))
		{
			action.Invoke();
		}
	}

	#endregion Actions

	public Node()
	{
		name = GetType().ToString();
	}

	#region Node Management

	public void Add(Node node)
	{
		if (node.parent is not null) throw new Exception("Cannot add node - Node already has an owner");

		node.parent = this;

		TryDoAction(() => _children.Add(node));
	}

	public void Insert(int index, Node node)
	{
		if (node.parent is not null) throw new Exception("Cannot insert node - Node already has an owner");

		node.parent = this;

		TryDoAction(() => _children.Insert(index, node));
	}

	public bool Remove(Node node)
	{
		if (!Contains(node)) return false;

		node.Detach();

		TryDoAction(() => _children.Remove(node));

		return true;
	}

	public void RemoveAt(int index)
	{
		if (index < 0) throw new IndexOutOfRangeException();

		var node = this.ElementAt(index);

		Remove(node);
	}

	public void Clear()
	{
		this.ForEach(child => child.Detach());
		TryDoAction(() => _children.Clear());
	}

	public int IndexOf(Node node) => this.IndexOf(node);

	public bool Contains(Node node) => this.Contains<Node>(node);

	public void CopyTo(Node[] array, int arrayIndex)
	{
		var nodesEnum = this.GetEnumerator();
		var length = array.Length;
		for (int i = arrayIndex; i < length; i++)
		{
			if (!nodesEnum.MoveNext()) return;
			array[i] = nodesEnum.Current;
		}
	}

	public IEnumerator<Node> GetEnumerator() => new ChildrenEnumerator(this);
	IEnumerator IEnumerable.GetEnumerator() => new ChildrenEnumerator(this);

	#endregion Node Management

	#region Game Loop

	internal void DoInit()
	{
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
		Update(deltaTime);
	}
	protected virtual void Update(float deltaTime)
	{
		if (!enabled) return;

		this.ForEach(child => child.DoUpdate(deltaTime));
	}

	internal void DoDraw(SpriteBatch sb)
	{
		if (!visible) return;

		Draw(sb);
	}
	/// <summary>
	/// Called when the object is going to be rendered.
	/// </summary>
	/// <remarks>
	/// This is not guaranteed to be called after every update due to it being called in a separate thread.
	/// </remarks>
	protected virtual void Draw(SpriteBatch sb)
	{
		this.ForEach(child => child.DoDraw(sb));
	}

	#endregion

	#region Events

	internal void DoAttach()
	{
		Attached();
	}
	/// <summary>
	/// Called after the node is attached to a parent.
	/// </summary>
	/// <remarks>
	/// Handle referencing parents here.
	/// <see cref="parent"/> is not null.
	/// </remarks>
	protected virtual void Attached()
	{
		this.ForEach(child => child.DoAttach());
	}

	internal void DoDetach()
	{
		Detached();
	}
	/// <summary>
	/// Called before the node is detached from its parent and before the node is destroyed.
	/// </summary>
	/// <remarks>
	/// <see cref="parent"/> is not null.
	/// </remarks>
	protected virtual void Detached()
	{
		this.ForEach(child => child.DoDetach());
	}

	public void Destroy()
	{
		if (destroyed) throw new Exception("Cannot destroy - Node is already destroyed");
		Destroyed();
		destroyed = true;
	}
	/// <summary>
	/// Called when the object is being destroyed after <see cref="Detached"/> is called.
	/// </summary>
	/// <remarks>
	/// Handle things like deleting <see cref="IDisposable"/> resources here like <see cref="Graphics.Texture"/>.
	/// </remarks>
	protected virtual void Destroyed()
	{
		foreach (var child in _children) child.Destroy();
	}

	#endregion Events

	#region Internal Util

	internal void Attach(Node parent)
	{
		this.parent = parent;

		DoAttach();
	}

	internal void Detach()
	{
		DoDetach();

		parent = null;
	}

	#endregion Internal Util
}
