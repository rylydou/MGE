using System.Collections.Generic;

namespace MGE.Editor
{
	public class TestNode
	{
		public static TestNode root = new() { _nodes = new() { new(), new(), new(), } };

		public string name { get; set; } = "Name";

		public bool enabled { get; set; } = true;
		public bool visible { get; set; } = true;

		public Vector2 position { get; set; } = new();
		public float rotation { get; set; }
		public Vector2 scale { get; set; } = new();

		public Direction direction { get; set; }

		public List<TestNode> _nodes { get; set; } = new();
	}

	public enum Direction
	{
		Up,
		Down,
		Left,
		Right,
	}
}
