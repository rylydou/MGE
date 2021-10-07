using System.Collections.Generic;

namespace MGE.Editor
{
	public class TestNode
	{
		public static TestNode root = new TestNode() { nodes = new List<TestNode>() { new TestNode(), new TestNode(), new TestNode(), } };

		public string name { get; set; } = "Name";

		public bool enabled { get; set; } = true;
		public bool visible { get; set; } = true;

		public Vector2 position { get; set; } = new Vector2();
		public float rotation { get; set; }
		public Vector2 scale { get; set; } = new Vector2();

		public Direction direction { get; set; }

		public List<TestNode> nodes { get; set; } = new List<TestNode>();
	}

	public enum Direction
	{
		Up,
		Down,
		Left,
		Right,
	}
}
