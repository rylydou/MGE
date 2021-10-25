using System.Collections.Generic;

namespace MGE.Editor
{
	public class TestNode
	{
		static int _nextID;
		public static Dictionary<int, TestNode> nodeDatabase = new();

		public readonly int id;

		public string name { get; set; }

		public bool enabled { get; set; } = true;
		public bool visible { get; set; } = true;

		public Vector2 position { get; set; } = new();
		public float rotation { get; set; }
		public Vector2 scale { get; set; } = new();

		public Direction direction { get; set; }

		public List<TestNode> nodes = new();

		public TestNode()
		{
			id = _nextID;
			name = $"Node #{_nextID}";
			nodeDatabase.Add(id, this);
			_nextID++;
		}
	}

	public class TestChildNode : TestNode
	{
		public Vector2 groundCheckPosition { get; set; } = new();
		public Vector2 groundCheckSize { get; set; } = new();

		public int health { get; set; }
		public float speed { get; set; }
	}

	public enum Direction
	{
		Up,
		Down,
		Left,
		Right,
	}
}
