
using System.Text;

namespace MGE;

public class GridCollider2D : Collider2D
{
	[Prop] public VirtualMap<bool> data;

	[Prop] public float cellSize;

	public GridCollider2D(Vector2Int mapSize, float cellSize, Vector2? position = null) : base(position)
	{
		data = new VirtualMap<bool>(mapSize);

		this.cellSize = cellSize;
	}

	public GridCollider2D(float cellSize, string mapStr, Vector2? position = null) : base(position)
	{
		this.cellSize = cellSize;

		var longest = 0;
		var currentX = 0;
		var currentY = 1;
		for (int i = 0; i < mapStr.Length; i++)
		{
			if (mapStr[i] == '\n')
			{
				currentY++;
				longest = Mathf.Max(currentX, longest);
				currentX = 0;
			}
			else currentX++;
		}

		data = new VirtualMap<bool>(longest, currentY);
		LoadMarStr(mapStr);
	}

	public GridCollider2D(float cellSize, bool[,] data, Vector2? position = null) : base(position)
	{
		this.cellSize = cellSize;

		this.data = new VirtualMap<bool>(data);
	}

	public GridCollider2D(float cellSize, VirtualMap<bool> data, Vector2? position = null) : base(position)
	{
		this.cellSize = cellSize;

		this.data = data;
	}

	public void Extend(int left, int right, int up, int down)
	{
		position -= new Vector2(left * cellSize, up * cellSize);

		var newWidth = data.columns + left + right;
		var newHeight = data.rows + up + down;

		if (newWidth <= 0 || newHeight <= 0)
		{
			data = new VirtualMap<bool>(0, 0);
			return;
		}

		var newData = new VirtualMap<bool>(newWidth, newHeight);

		// Center
		for (int x = 0; x < data.columns; x++)
		{
			for (int y = 0; y < data.rows; y++)
			{
				var atX = x + left;
				var atY = y + up;

				if (atX >= 0 && atX < newWidth && atY >= 0 && atY < newHeight)
					newData[atX, atY] = data[x, y];
			}
		}

		// Left
		for (int x = 0; x < left; x++)
			for (int y = 0; y < newHeight; y++)
				newData[x, y] = data[0, Mathf.Clamp(y - up, 0, data.rows - 1)];

		// Right
		for (int x = newWidth - right; x < newWidth; x++)
			for (int y = 0; y < newHeight; y++)
				newData[x, y] = data[data.columns - 1, Mathf.Clamp(y - up, 0, data.rows - 1)];

		// Top
		for (int y = 0; y < up; y++)
			for (int x = 0; x < newWidth; x++)
				newData[x, y] = data[Mathf.Clamp(x - left, 0, data.columns - 1), 0];

		// Bottom
		for (int y = newHeight - down; y < newHeight; y++)
			for (int x = 0; x < newWidth; x++)
				newData[x, y] = data[Mathf.Clamp(x - left, 0, data.columns - 1), data.rows - 1];

		data = newData;
	}

	public void LoadMarStr(string bitstring)
	{
		var x = 0;
		var y = 0;

		for (int i = 0; i < bitstring.Length; i++)
		{
			if (bitstring[i] == '\n')
			{
				while (x < cellsX)
				{
					data[x, y] = false;
					x++;
				}

				x = 0;
				y++;

				if (y >= cellsY) return;
			}
			else if (x < cellsX)
			{
				data[x, y] = bitstring[i] != ' ';
				x++;
			}
		}
	}

	public string GetBitstring()
	{
		var bits = new StringBuilder();
		for (int y = 0; y < cellsY; y++)
		{
			if (y != 0)
				bits.Append('\n');

			for (int x = 0; x < cellsX; x++)
			{
				if (data[x, y]) bits.Append('1');
				else bits.Append('0');
			}
		}

		return bits.ToString();
	}

	public void Clear(bool to = false)
	{
		for (int x = 0; x < cellsX; x++)
			for (int y = 0; y < cellsY; y++)
				data[x, y] = to;
	}

	public void SetRect(int x, int y, int width, int height, bool to = true)
	{
		if (x < 0)
		{
			width += x;
			x = 0;
		}

		if (y < 0)
		{
			height += y;
			y = 0;
		}

		if (x + width > cellsX)
			width = cellsX - x;

		if (y + height > cellsY)
			height = cellsY - y;

		for (int i = 0; i < width; i++)
			for (int j = 0; j < height; j++)
				data[x + i, y + j] = to;
	}

	public bool CheckRect(int x, int y, int width, int height)
	{
		if (x < 0)
		{
			width += x;
			x = 0;
		}

		if (y < 0)
		{
			height += y;
			y = 0;
		}

		if (x + width > cellsX)
			width = cellsX - x;

		if (y + height > cellsY)
			height = cellsY - y;

		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				if (data[x + i, y + j])
					return true;
			}
		}

		return false;
	}

	public bool CheckColumn(int x)
	{
		for (int i = 0; i < cellsY; i++)
			if (!data[x, i]) return false;
		return true;
	}

	public bool CheckRow(int y)
	{
		for (int i = 0; i < cellsX; i++)
			if (!data[i, y]) return false;
		return true;
	}

	public bool this[int x, int y] { get { if (x >= 0 && y >= 0 && x < cellsX && y < cellsY) return data[x, y]; else return false; } set => data[x, y] = value; }

	public int cellsX { get => data.columns; }
	public int cellsY { get => data.rows; }

	public override float width { get => cellSize * cellsX; set => throw new System.NotImplementedException(); }
	public override float height { get => cellSize * cellsY; set => throw new System.NotImplementedException(); }

	public bool isEmpty
	{
		get
		{
			for (int i = 0; i < cellsX; i++)
				for (int j = 0; j < cellsY; j++)
					if (data[i, j]) return false;
			return true;
		}
	}

	public override float left { get => position.x; set => position.x = value; }
	public override float top { get => position.y; set => position.y = value; }
	public override float right { get => position.x + width; set => position.x = value - width; }
	public override float bottom { get => position.y + height; set => position.y = value - height; }

	protected override void Render(Batch2D batch, Color color)
	{
		for (int x = 0; x < cellsX; x++)
			for (int y = 0; y < cellsY; y++)
				if (data[x, y])
					batch.SetRect(new(left + x * cellSize, top + y * cellSize, cellSize, cellSize), -1, color);
	}

	// public override void Draw(Camera camera, Color color)
	// {
	// 	var left = (int)Math.Max(0, ((camera.left - absLeft) / cellSize));
	// 	var right = (int)Math.Min(cellsX - 1, Math.Ceil((camera.right - absLeft) / cellSize));
	// 	var top = (int)Math.Max(0, ((camera.top - absTop) / cellSize));
	// 	var bottom = (int)Math.Min(cellsY - 1, Math.Ceil((camera.bottom - absTop) / cellSize));

	// 	for (int tx = left; tx <= right; tx++)
	// 		for (int ty = top; ty <= bottom; ty++)
	// 			if (data[tx, ty])
	// 				GFX.DrawRect(new Rect(absLeft + tx * cellSize, absTop + ty * cellSize, cellSize, cellSize), color);
	// }

	override public bool PointCheck(Vector2 point)
	{
		if (point.x >= absLeft && point.y >= absTop && point.x < absRight && point.y < absBottom)
			return data[(int)((point.x - absLeft) / cellSize), (int)((point.y - absTop) / cellSize)];
		else return false;
	}

	public override bool RectCheck(Rect rect)
	{
		var left = (int)((rect.left - absLeft) / cellSize);
		var right = (int)((rect.right - absLeft) / cellSize);
		var top = (int)((rect.top - absTop) / cellSize);
		var bottom = (int)((rect.bottom - absTop) / cellSize);

		if (left < 0) left = 0;
		if (right > cellsX) right = cellsX;
		if (top < 0) top = 0;
		if (bottom > cellsY) bottom = cellsY;

		for (int i = left; i <= right; i++)
		{
			for (int j = top; j <= bottom; j++)
			{
				if (data[i, j])
					return true;
			}
		}

		return false;
	}

	public override bool LineCheck(Vector2 from, Vector2 to)
	{
		from -= absPosition;
		to -= absPosition;
		from /= new Vector2(cellSize, cellSize);
		to /= new Vector2(cellSize, cellSize);

		var steep = Mathf.Abs(to.y - from.y) > Mathf.Abs(to.x - from.x);
		if (steep)
		{
			var temp = from.x;
			from.x = from.y;
			from.y = temp;

			temp = to.x;
			to.x = to.y;
			to.y = temp;
		}

		if (from.x > to.x)
		{
			var temp = from;
			from = to;
			to = temp;
		}

		var error = 0f;
		var deltaError = Mathf.Abs(to.y - from.y) / (to.x - from.x);
		var yStep = (from.y < to.y) ? 1 : -1;
		var y = (int)from.y;
		var toX = (int)to.x;

		for (int x = (int)from.x; x <= toX; x++)
		{
			if (steep)
			{
				if (this[y, x]) return true;
			}
			else if (this[x, y]) return true;

			error += deltaError;
			if (error >= 0.5f)
			{
				y += yStep;
				error--;
			}
		}

		return false;
	}

	protected override bool? CheckCollider(Collider2D collider)
	{
		if (collider is HitboxCollider2D hitbox)
			return RectCheck(hitbox.bounds);
		return null;
	}

	public static bool IsBitstringEmpty(string bitstring)
	{
		for (int i = 0; i < bitstring.Length; i++)
			if (bitstring[i] == '1') return false;
		return true;
	}
}
