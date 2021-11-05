using System;
using System.Collections;
using System.Collections.Generic;

namespace MGE.Editor
{
	public class GameNode : IEnumerable<GameNode>
	{
		static int _nextID;
		public static Dictionary<int, GameNode> nodeDatabase = new();

		public readonly int id;

		public GameNode? parent;
		public int siblingIndex;

		public string name { get; set; }

		public bool enabled { get; set; } = true;
		public bool visible { get; set; } = true;

		public Vector2 position { get; set; } = new();
		public float rotation { get; set; }
		public Vector2 scale { get; set; } = new();

		public Direction direction { get; set; }

		List<GameNode> _nodes = new();

		public GameNode(params GameNode[] nodes)
		{
			id = _nextID;
			name = $"Node #{id}";
			nodeDatabase.Add(id, this);
			_nextID++;

			foreach (var node in nodes) AttachNode(node);
		}

		public void AttachNode(GameNode node)
		{
			if (node.parent is not null) throw new InvalidOperationException("Cannot attach node - It is already attached to something");
			if (node.id == id) throw new InvalidOperationException("Cannot attach node - Cannot attach node to itself");

			_nodes.Add(node);
			node.siblingIndex = _nodes.Count;
			node.parent = this;
		}

		public void AttachNode(GameNode node, int position)
		{
			if (position < 0) throw new ArgumentOutOfRangeException(nameof(position), position, "Position cannot be less than 0");
			if (position > _nodes.Count) throw new ArgumentOutOfRangeException(nameof(position), position, $"Position greater than {_nodes.Count} (number of nodes)");

			if (node.parent is not null) throw new InvalidOperationException("Cannot attach node - It is already attached to something");
			if (node.id == id) throw new InvalidOperationException("Cannot attach node - Cannot attach node to itself");

			node.siblingIndex = position;
			for (int i = position; i < _nodes.Count; i++)
			{
				_nodes[i].siblingIndex++;
			}

			_nodes.Insert(position, node);
			node.parent = this;
		}

		public void Detach()
		{
			if (parent is null) throw new InvalidOperationException("Cannot attach node - It is not attached to anything");

			parent._nodes.Remove(this);
			parent = null;
		}

		public IEnumerator<GameNode> GetEnumerator() => _nodes.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => _nodes.GetEnumerator();
	}

	public class GameNodePlus : GameNode
	{
		public Vector2 groundCheckPosition { get; set; } = new();
		public Vector2 groundCheckSize { get; set; } = new();

		public int health { get; set; }
		public float speed { get; set; }

		public GameNodePlus(params GameNode[] nodes) : base(nodes) { }
	}

	public enum Direction
	{
		Up,
		Down,
		Left,
		Right,
	}
}
