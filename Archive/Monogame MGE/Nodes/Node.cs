using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MGE
{
	public sealed class NodeLoader : AssetLoader<Node>
	{
		protected override Node LoadAsset(string path)
		{
			return Folder.root.FileReadReadable<Node>(path);
		}
	}

	public class Node : Object
	{
		[Prop] public bool enabled = true;
		[Prop] public bool visible = true;

		public bool initialized { get; private set; } = false;
		public bool destroyed { get; private set; } = false;

		public Node parent { get; private set; }

		[Prop("nodes", order = int.MaxValue)] List<Node> _nodes = new List<Node>();
		public int childCount { get => _nodes.Count; }
		public Action<Node> onNodeAddedDirectly = (n) => { };
		public Action<Node> onNodeAdded = (n) => { };
		public Action<Node> onNodeRemovedDirectly = (n) => { };
		public Action<Node> onNodeRemoved = (n) => { };

		public int siblingIndex
		{
			get
			{
				if (parent is null) return -1;
				return parent._nodes.IndexOf(this);
			}
		}
		public int hierarchyDepth { get => GetParentNodes<Node>().Count() - 1; }

		bool _inGameLoop = false;
		Queue<Action> _queuedActions = new Queue<Action>();

		protected Node() : base() { }

		bool QueueAction(Action action)
		{
			if (_inGameLoop)
			{
				_queuedActions.Enqueue(action);
				return false;
			}
			action();
			return true;
		}

		void FinishQueue()
		{
			var length = _queuedActions.Count;
			for (int i = 0; i < length; i++)
				_queuedActions.Dequeue()();
		}

		#region Node Management

		public Node AttachNode(params Node[] nodes)
		{
			foreach (var node in nodes)
				AttachNode(node);
			return this;
		}
		public Node AttachNode(Node node)
		{
			if (!node) throw new ArgumentNullException(nameof(node));
			if (node.parent) throw new Exception($"Can't Attach Node - {node} already has an owner");
			if (node.initialized) throw new Exception($"Can't Attach Node - {node} already has already been initialized");
			if (node.destroyed) throw new Exception($"Can't Attach Node - {node} has been destroyed");

			// var directAttr = (DirectChildOfAttribute)Attribute.GetCustomAttribute(node.GetType(), typeof(DirectChildOfAttribute));
			// var indirectAttr = (IndirectChildOfAttribute)Attribute.GetCustomAttribute(node.GetType(), typeof(IndirectChildOfAttribute));

			// if (directAttr is not null && !directAttr.type.Equals(this.GetType())) throw new Exception($"Can't Attach Node - Node must be directly attached to {directAttr.type}");
			// if (indirectAttr is not null && !GetParentNode(indirectAttr.type)) throw new Exception($"Can't Attach Node - Node must be indirectly attached to {indirectAttr.type}");

			QueueAction(() =>
			{
				node.parent = this;
				_nodes.Add(node);
				if (initialized)
					node.DoInit();

				NodeAddedDirectly(node);
				foreach (var node in node.GetNodes())
					OnNodeAdded(node);
				OnNodeAdded(node);
				foreach (var parent in GetParentNodes())
				{
					foreach (var node in node.GetNodes())
						OnNodeAdded(node);
					parent.OnNodeAdded(node);
				}
			});
			return this;
		}

		public Node DeatachNodes(params Node[] nodes)
		{
			foreach (var node in nodes)
				DeatachNode(node);
			return this;
		}
		public Node DeatachNode(Node node)
		{
			if (!node) throw new ArgumentNullException(nameof(node));
			if (node.parent != this) throw new Exception($"Can't Deatach Node - {this} doesn't own {node}");

			QueueAction(() =>
			{
				OnNodeRemovedDirectly(node);
				foreach (var node in node.GetNodes())
					OnNodeRemoved(node);
				OnNodeRemoved(node);
				foreach (var parent in GetParentNodes())
				{
					foreach (var node in node.GetNodes())
						OnNodeRemoved(node);
					parent.OnNodeRemoved(node);
				}

				node.parent = null;
				_nodes.Remove(node);
			});
			return this;
		}

		#endregion

		#region Node Finding

		public Node GetNode(int index) => _nodes[index];
		public Node GetNode(string name) => _nodes.Find(n => n.name == name);
		public T GetNode<T>() where T : Node => (T)_nodes.Find(n => !n.destroyed && n is T);
		public Node GetNode(Type type)
		{
			if (type is null) throw new ArgumentNullException(nameof(type));
			return _nodes.Find(n => !n.destroyed && n.GetType().Equals(type));
		}

		public Node[] GetNodes() => _nodes.ToArray();
		public Node[] GetNodes(string name) => _nodes.FindAll(n => n.name == name).ToArray();
		public T[] GetNodes<T>() where T : Node => _nodes.FindAll(n => !n.destroyed && n is T).Select(n => (T)n).ToArray();
		public Node[] GetNodes(Type type)
		{
			if (type is null) throw new ArgumentNullException(nameof(type));
			return _nodes.FindAll(n => !n.destroyed && n.GetType().Equals(type)).ToArray();
		}

		public T GetNodeRecursive<T>() where T : Node
		{
			var node = GetNode<T>();
			foreach (var child in _nodes)
			{
				if (node) break;
				node = child.GetNodeRecursive<T>();
			}
			return node;
		}
		public Node GetNodeRecursive(Type type)
		{
			if (type is null) throw new ArgumentNullException(nameof(type));
			var node = GetNode(type);
			foreach (var child in _nodes)
			{
				if (node) break;
				node = child.GetNodeRecursive(type);
			}
			return node;
		}

		public Node[] GetNodesRecursive()
		{
			var nodes = new List<Node>();
			nodes.Concat(GetNodes());
			foreach (var child in _nodes)
				nodes.Concat(child.GetNodesRecursive());
			return nodes.ToArray();
		}
		public T[] GetNodesRecursive<T>() where T : Node
		{
			var nodes = new List<T>();
			nodes.Concat(GetNodes<T>());
			foreach (var child in _nodes)
				nodes.Concat(child.GetNodesRecursive<T>());
			return nodes.ToArray();
		}
		public Node[] GetNodesRecursive(Type type)
		{
			var nodes = new List<Node>();
			nodes.Concat(GetNodes(type));
			foreach (var child in _nodes)
				nodes.Concat(child.GetNodesRecursive(type));
			return nodes.ToArray();
		}

		public T GetParentNode<T>() where T : Node
		{
			var node = parent;
			while (true)
			{
				if (!node) return null;
				if (node is T n) return n;
				node = node.parent;
			}
		}
		public Node GetParentNode(Type type)
		{
			if (type is null) throw new ArgumentNullException(nameof(type));
			var node = parent;
			while (true)
			{
				if (!node) return null;
				if (node.GetType().Equals(type)) return node;
				node = node.parent;
			}
		}
		public bool GetParentNode<T>(out T result) where T : Node
		{
			result = GetParentNode<T>();
			return result is not null;
		}
		public bool GetParentNode(Type type, out Node result)
		{
			if (type is null) throw new ArgumentNullException(nameof(type));
			result = GetParentNode(type);
			return result is not null;
		}

		public Node[] GetParentNodes()
		{
			var node = parent;
			var nodes = new List<Node>();
			while (node)
			{
				nodes.Add(node);
				node = node.parent;
			}
			return nodes.ToArray();
		}
		public T[] GetParentNodes<T>() where T : Node
		{
			var node = parent;
			var nodes = new List<T>();
			while (node)
			{
				if (node is T n) nodes.Add(n);
				node = node.parent;
			}
			return nodes.ToArray();
		}
		public Node[] GetParentNodes(Type type)
		{
			if (type is null) throw new ArgumentNullException(nameof(type));
			var node = parent;
			var nodes = new List<Node>();
			while (node)
			{
				if (node.GetType().Equals(type)) nodes.Add(node);
				node = node.parent;
			}
			return nodes.ToArray();
		}

		#endregion

		#region Game Loop

		internal void DoInit()
		{
			if (initialized) throw new Exception("Can't Init Node - Node has already been initialized");
			if (destroyed) throw new Exception("Can't Init Node - Node is destroyed try cloning it instead");

			initialized = true;

			_inGameLoop = true;

			// name = $"{Util.GetFamilyTree(this.GetType())} {depth} / {siblingIndex}";
			// name = Util.GetFamilyTree(this.GetType()).Remove(0, 5);
			// name = GetType().Name;

			Log("Initializing...");

			try
			{
				Init();
			}
			catch (Exception e)
			{
				LogError(e.ToString());
			}

			_inGameLoop = false;
		}

		protected virtual void Init()
		{
			foreach (var child in _nodes)
				child.DoInit();
		}

		internal void DoTick()
		{
			_inGameLoop = true;
			if (enabled && !destroyed && initialized)
			{
				try
				{
					Tick();
				}
				catch (Exception e)
				{
					LogException(e);
				}
			}
			_inGameLoop = false;

			FinishQueue();
		}

		protected virtual void Tick()
		{
			foreach (var child in _nodes)
				child.DoTick();
		}

		internal void DoUpdate()
		{
			_inGameLoop = true;
			if (enabled && !destroyed && initialized)
			{
				try
				{
					Update();
				}
				catch (Exception e)
				{
					LogException(e);
				}
			}
			_inGameLoop = false;

			FinishQueue();
		}

		protected virtual void Update()
		{
			foreach (var child in _nodes)
				child.DoUpdate();
		}

		internal void DoDraw()
		{
			_inGameLoop = true;
			if (visible && !destroyed && initialized)
			{
				try
				{
					Draw();
				}
				catch (Exception e)
				{
					LogException(e);
				}
			}
			_inGameLoop = false;
		}

		protected virtual void Draw()
		{
			if (Hierarchy.DEBUG) DebugDraw();

			foreach (var child in _nodes)
				child.DoDraw();
		}

		protected virtual void DebugDraw() { }

		public void Destroy()
		{
			Log("Destroying...");

			if (!initialized) throw new Exception("Can't Destroy Node - Node hasn't been initialized");
			if (destroyed) return;
			destroyed = true;

			_inGameLoop = true;
			try
			{
				OnDestroy();
			}
			catch (Exception e)
			{
				LogException(e);
			}
			_inGameLoop = false;

			FinishQueue();

			parent?.DeatachNode(this);
		}

		protected virtual void OnDestroy()
		{
			foreach (var child in _nodes)
				child.Destroy();
		}

		#endregion

		#region Events

		internal void NodeAddedDirectly(Node node)
		{
			_inGameLoop = true;

			try
			{
				onNodeAddedDirectly(node);
				OnNodeAddedDirectly(node);
			}
			catch (Exception e)
			{
				LogError(e.ToString());
			}

			_inGameLoop = false;
		}
		protected virtual void OnNodeAddedDirectly(Node node)
		{
			foreach (var child in _nodes)
				child.NodeAddedDirectly(node);
		}

		internal void NodeAdded(Node node)
		{
			_inGameLoop = true;

			try
			{
				onNodeAdded(node);
				OnNodeAdded(node);
			}
			catch (Exception e)
			{
				LogError(e.ToString());
			}

			_inGameLoop = false;
		}
		protected virtual void OnNodeAdded(Node node)
		{
			foreach (var child in _nodes)
				child.NodeAdded(node);
		}

		internal void NodeRemovedDirectly(Node node)
		{
			_inGameLoop = true;

			try
			{
				onNodeAddedDirectly(node);
				OnNodeRemovedDirectly(node);
			}
			catch (Exception e)
			{
				LogError(e.ToString());
			}

			_inGameLoop = false;
		}
		protected virtual void OnNodeRemovedDirectly(Node node)
		{
			foreach (var child in _nodes)
				child.NodeAddedDirectly(node);
		}

		internal void NodeRemoved(Node node)
		{
			_inGameLoop = true;

			try
			{
				onNodeAdded(node);
				OnNodeRemoved(node);
			}
			catch (Exception e)
			{
				LogError(e.ToString());
			}

			_inGameLoop = false;
		}
		protected virtual void OnNodeRemoved(Node node)
		{
			foreach (var child in _nodes)
				child.NodeRemoved(node);
		}

		#endregion

		#region Logging

		protected void Log(object obj)
		{
			Logger.Log(obj, this);
		}

		protected void LogVar(string name, object value)
		{
			Logger.LogVar(name, value, this);
		}

		protected void LogWarn(string msg)
		{
			Logger.LogWarn(msg, this);
		}

		protected void LogError(string msg)
		{
			Logger.LogError(msg, this);
		}

		void LogException(Exception e)
		{
			Logger.LogError(e.ToString(), this);
		}

		#endregion

		protected void SafelyCallFunction(Action action)
		{
			var alreadyInGameLoop = _inGameLoop;
			_inGameLoop = true;

			try
			{
				action();
			}
			catch (Exception e)
			{
				LogError(e.ToString());
			}

			if (!alreadyInGameLoop)
				_inGameLoop = false;
		}

		[OnDeserialized]
		void OnDeserialized(StreamingContext context)
		{
			foreach (var node in _nodes)
				node.parent = this;
		}

		public IEnumerator<Node> GetEnumerator()
		{
			return _nodes.GetEnumerator();
		}

		// IEnumerator IEnumerable.GetEnumerator()
		// {
		// 	return _nodes.GetEnumerator();
		// }

		// public override string ToString() => name;

		// public static implicit operator bool(Node node) => node is not null;
	}
}
