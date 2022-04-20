namespace MGE;

public class VirtualMap<T> : System.ICloneable where T : struct
{
	public const int segmentSize = 50;
	public readonly int columns;
	public readonly int rows;
	public readonly int segmentColumns;
	public readonly int segmentRows;
	public readonly T emptyValue;

	private T[,][,] segments;

	public VirtualMap(Vector2Int size, T emptyValue = default)
	{
		this.columns = size.x;
		this.rows = size.y;
		segmentColumns = (columns / segmentSize) + 1;
		segmentRows = (rows / segmentSize) + 1;
		segments = new T[segmentColumns, segmentRows][,];
		this.emptyValue = emptyValue;
	}

	public VirtualMap(int columns, int rows, T emptyValue = default)
	{
		this.columns = columns;
		this.rows = rows;
		segmentColumns = (columns / segmentSize) + 1;
		segmentRows = (rows / segmentSize) + 1;
		segments = new T[segmentColumns, segmentRows][,];
		this.emptyValue = emptyValue;
	}

	public VirtualMap(T[,] map, T emptyValue = default) : this(map.GetLength(0), map.GetLength(1), emptyValue)
	{
		for (int x = 0; x < columns; x++)
			for (int y = 0; y < rows; y++)
				this[x, y] = map[x, y];
	}

	public bool AnyInSegmentAtTile(int x, int y)
	{
		var cx = x / segmentSize;
		var cy = y / segmentSize;
		return segments[cx, cy] is not null;
	}

	public bool AnyInSegment(int segmentX, int segmentY) => segments[segmentX, segmentY] is not null;
	public T InSegment(int segmentX, int segmentY, int x, int y) => segments[segmentX, segmentY][x, y];
	public T[,] GetSegment(int segmentX, int segmentY) => segments[segmentX, segmentY];

	public T SafeCheck(int x, int y)
	{
		if (x >= 0 && y >= 0 && x < columns && y < rows) return this[x, y];
		return emptyValue;
	}

	public T this[int x, int y]
	{
		get
		{
			var cx = x / segmentSize;
			var cy = y / segmentSize;

			var seg = segments[cx, cy];
			if (seg is null)
				return emptyValue;

			return seg[x - cx * segmentSize, y - cy * segmentSize];
		}
		set
		{
			var cx = x / segmentSize;
			var cy = y / segmentSize;

			if (segments[cx, cy] is null)
			{
				segments[cx, cy] = new T[segmentSize, segmentSize];

				if (/* emptyValue is not null && */ !emptyValue.Equals(default(T)))
					for (int tx = 0; tx < segmentSize; tx++)
						for (int ty = 0; ty < segmentSize; ty++)
							segments[cx, cy][tx, ty] = emptyValue;
			}

			segments[cx, cy][x - cx * segmentSize, y - cy * segmentSize] = value;
		}
	}

	public T[,] ToArray()
	{
		var array = new T[columns, rows];
		for (int x = 0; x < columns; x++)
			for (int y = 0; y < rows; y++)
				array[x, y] = this[x, y];
		return array;
	}

	object System.ICloneable.Clone() => Clone();
	public VirtualMap<T> Clone()
	{
		var clone = new VirtualMap<T>(columns, rows, emptyValue);
		for (int x = 0; x < columns; x++)
			for (int y = 0; y < rows; y++)
				clone[x, y] = this[x, y];
		return clone;
	}
}
