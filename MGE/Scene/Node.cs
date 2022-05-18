using System;
using System.Collections.Generic;
using System.Linq;

namespace MGE;

public abstract class Node
{
	[HiddenProp] public string name;

	public Node()
	{
		name = GetType().Name;
		RegisterCallbacks();
	}

	[HiddenProp] public List<Node> _children = new();

#nullable enable
	public Node parent { get; private set; } = null!;
#nullable restore

#nullable enable
	public Scene scene => this is Scene s ? s : parent?.scene!;
#nullable restore

	bool _initialized;

	#region Node Querying

	public T? GetChild<T>() where T : Node
	{
		return _children.FirstOrDefault(c => c is T) as T;
	}

	public T? GetChild<T>(string name) where T : Node
	{
		return GetChildren<T>().FirstOrDefault(c => name.Equals(c.name, StringComparison.OrdinalIgnoreCase));
	}

	public IEnumerable<T> GetChildren<T>() where T : Node
	{
		foreach (var child in _children)
		{
			if (child is T childT)
			{
				yield return childT;
			}
		}
	}

	public IEnumerable<T> GetChildrenRecursive<T>() where T : Node
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

	public IEnumerable<T> GetChildrenRecursiveDeep<T>() where T : Node
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
		if (scene is not null)
		{
			node.onEnterScene();
		}
	}

	public void RemoveChild(Node node)
	{
		if (node.parent is null) throw new Exception("Remove child", "Node is not a child of anything");
		if (node.parent != this) throw new Exception("Remove child", "Parent doesn't own the child");

		_children.Remove(node);
		if (scene is not null)
		{
			node.onExitScene();
		}
		node.parent = null!;
	}

	public void RemoveAllChildren()
	{
		foreach (var child in GetChildren<Node>().ToArray())
		{
			RemoveChild(child);
		}
	}

	public void RemoveSelf()
	{
		if (parent is null) throw new Exception("Remove self", "Node is not a child of anything");
		parent.RemoveChild(this);
	}

	#endregion Node Management

	#region Signal Management

	List<(Action signal, Action callback)> _externalSignals = new();

	public void ConnectSignal(Action signal, Action callback)
	{
		signal += callback;

		if (signal.Target != this)
		{
			_externalSignals.Add((signal, callback));
		}
	}

	public void DisconnectSignal(Action signal, Action callback)
	{
		signal = signal - callback
			?? throw new Exception("Cannot remove callback from signal", "Removing this callback leads to the signal being null", "This might be because an empty callback like '() => { }' is being removed");

		if (signal.Target != this)
		{
			_externalSignals.Remove((signal, callback));
		}
	}

	#endregion Signal Management

	#region Events

	void RegisterSelfAsParentForChildren()
	{
		foreach (var child in _children)
		{
			if (child.parent is not null) continue;

			child.parent = this;
			child.RegisterSelfAsParentForChildren();
		}
	}

	protected virtual void RegisterCallbacks()
	{
		RegisterSelfAsParentForChildren();

		onReady += () =>
		{
			Ready();
			GetChildren<Node>().ToArray().ForEach(c =>
			{
				if (c.parent != this) return;

				c.onReady();
			});
		};

		onEnterScene += () =>
		{
			RegisterSelfAsParentForChildren();

			if (!_initialized)
			{
				_initialized = true;

				onReady();
			}

			parent!.onChildAdded(this);
			OnEnterScene();
			GetChildren<Node>().ToArray().ForEach(c =>
			{
				if (c.parent != this) return;

				c.onEnterScene();
			});
		};

		onExitScene += () =>
		{
			parent!.onChildRemoved(this);
			OnExitScene();
			GetChildren<Node>().ToArray().ForEach(c =>
			{
				if (c.parent != this) return;

				c.onExitScene();
				c.parent = null!;
			});
		};

		onChildAdded += node =>
		{
			OnChildAdded(node);
			onChildAddedDeep(node);
		};

		onChildRemoved += node =>
		{
			OnChildRemoved(node);
			onChildRemovedDeep(node);
		};

		onChildAddedDeep += node =>
		{
			OnChildAddedDeep(node);
			parent?.onChildAddedDeep(node);
		};

		onChildRemovedDeep += node =>
		{
			OnChildRemovedDeep(node);
			parent?.onChildRemovedDeep(node);
		};

		onTick += delta =>
		{
			Tick(delta);
			GetChildren<Node>().ToArray().ForEach(c =>
			{
				if (c.parent != this) return;

				c.onTick(delta);
			});
		};

		onUpdate += delta =>
		{
			Update(delta);
			GetChildren<Node>().ToArray().ForEach(c =>
			{
				if (c.parent != this) return;

				c.onUpdate(delta);
			});
		};
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

	// // When a child or grandchild is changed and the signal has not been handled by any other parent
	// public Func<Node, bool> onChildAddedDeepUnhandled = node => false;
	// protected virtual bool OnChildAddedDeepUnhandled(Node node) => false;

	// public Func<Node, bool> onChildRemovedDeepUnhandled = node => false;
	// protected virtual bool OnChildRemovedDeepUnhandled(Node node) => false;

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

	#region Utilities

	public Prefab CreatePrefab() => new Prefab(Prefab.converter.CreateStructureFromObject(this, GetType()));

	public Node CreateInstance() => CreatePrefab().CreateInstance();
	public T CreateInstance<T>() where T : Node => CreatePrefab().CreateInstance<T>();

	#endregion Utilities
}
