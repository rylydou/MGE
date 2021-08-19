namespace MGE
{
	public class Tilemap : Node
	{
		public byte[,] data;

		public Tilemap(Vector2Int size)
		{
			data = new byte[size.x, size.y];
		}

		public void Clear()
		{
			for (int y = 0; y < data.GetLength(1); y++)
				for (int x = 0; x < data.GetLength(0); x++)
					data[x, y] = 0;
		}
	}
}