namespace Demo;

public class JumpThough : Semisolid
{
	public Vector2Int mapSize;
	public float tileSize;

	public Dictionary<(bool left, bool right), Vector2Int> lut = new(){
		{(false, true), new(0, 0)},
		{(true, false), new(1, 0)},
		{(true, true), new(0, 1)},
		{(false, false), new(1, 1)},
	};
	GridCollider2D? tilemap;

	Vector2 lastTileMousePosition;

	Texture _tileset = App.content.Get<Texture>("Scene/Ground/Platform.ase");

	Vector2 worldMousePosition;
	Vector2 tileMousePosition;

	protected override void Ready()
	{
		tilemap = new GridCollider2D(mapSize, tileSize);
		collider = tilemap;
	}

	protected override void Update(float delta)
	{
		var screenScale = App.window.size / Game.screenSize;
		worldMousePosition = App.window.renderMouse / screenScale;
		tileMousePosition = worldMousePosition / tileSize;
		tileMousePosition = new(Mathf.Clamp(tileMousePosition.x, 0, mapSize.x - 1), Mathf.Clamp(tileMousePosition.y, 0, mapSize.y - 1));

		if (App.input.keyboard.alt)
		{
			if (App.input.mouse.Down(MouseButtons.Left))
				SetTiles(tileMousePosition, lastTileMousePosition, !App.input.keyboard.shift);
			else if (App.input.mouse.Down(MouseButtons.Right))
				SetTiles(tileMousePosition, lastTileMousePosition, false);
		}

		lastTileMousePosition = tileMousePosition;
	}

	void SetTiles(Vector2 from, Vector2 to, bool value)
	{
		foreach (var item in Calc.GetTilesOnLine(from, to))
		{
			tilemap![item.x, item.y] = value;
		}
	}

	protected override void Render(Batch2D batch)
	{
		for (int x = 0; x < mapSize.x; x++)
		{
			for (int y = 0; y < mapSize.y; y++)
			{
				if (tilemap!.data[x, y])
				{
					(bool left, bool right) key = new();
					key.left = (x == 0) ? true : tilemap.data[x - 1, y];
					key.right = (x == mapSize.x - 1) ? true : tilemap.data[x + 1, y];

					var src = new RectInt(lut[key] * 8, 8, 8);
					batch.Image(_tileset, src, new Vector2(x, y) * tileSize, Color.white);
				}
			}
		}
	}
}
