namespace MGE.Editor
{
	public class TestNode
	{
		public string name { get; set; } = "Name";

		public bool enabled { get; set; }
		public bool visible { get; set; }

		public Vector2 position { get; set; } = new Vector2();
		public float rotation { get; set; }
		public Vector2 scale { get; set; } = new Vector2();

		public Direction direction { get; set; }
	}

	public enum Direction
	{
		Up,
		Down,
		Left,
		Right,
	}
}
