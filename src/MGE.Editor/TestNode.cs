using System.Collections.Generic;

namespace MGE.Editor
{
	public class TestNode
	{
		static int _nextID;
		public static Dictionary<int, TestNode> nodes = new();

		public readonly int id;

		public string name { get; set; }

		public bool enabled { get; set; } = true;
		public bool visible { get; set; } = true;

		public Vector2 position { get; set; } = new();
		public float rotation { get; set; }
		public Vector2 scale { get; set; } = new();

		public Direction direction { get; set; }

		public List<TestNode> _nodes { get; set; } = new();

		public TestNode()
		{
			name = $"Node #{_nextID}";
			id = _nextID;
			nodes.Add(id, this);
			_nextID++;
		}
	}

	public enum Direction
	{
		Up,
		Down,
		Left,
		Right,
	}
}
