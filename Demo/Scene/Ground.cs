using System.Collections.Generic;
using MGE;

namespace Demo;

public class Ground : Solid
{
	public Vector2Int mapSize;
	public float tileSize;

	public Dictionary<(bool top, bool right, bool bottom, bool left), Vector2Int> lut = new(){
		{(false, true, true, false), new(0, 0)},
		{(false, true, true, true), new(1, 0)},
		{(false, false, true, true), new(2, 0)},
		{(false, false, true, false), new(3, 0)},

		{(true, true, true, false), new(0, 1)},
		{(true, true, true, true), new(1, 1)},
		{(true, false, true, true), new(2, 1)},
		{(true, false, true, false), new(3, 1)},

		{(true, true, false, false), new(0, 2)},
		{(true, true, false, true), new(1, 2)},
		{(true, false, false, true), new(2, 2)},
		{(true, false, false, false), new(3, 2)},

		{(false, true, false, false), new(0, 3)},
		{(false, true, false, true), new(1, 3)},
		{(false, false, false, true), new(2, 3)},
		{(false, false, false, false), new(3, 3)},
	};
	GridCollider2D? tilemap;

	Vector2 lastTileMousePosition;

	Texture _tileset = App.content.Get<Texture>("Scene/Ground/Grass.ase");

	Vector2 worldMousePosition;
	Vector2 tileMousePosition;

	protected override void Ready()
	{
		tilemap = new GridCollider2D(mapSize, tileSize);
		collider = tilemap;

		var noise = new Noise();

		var zoom = 4f;
		var topBias = +1.00f;
		var bottomBias = -0.00f;

		for (int y = 0; y < mapSize.y; y++)
		{
			for (int x = 0; x < mapSize.x; x++)
			{
				var v = noise.GetNoise(x * zoom, y * zoom);
				var bias = Mathf.Lerp(topBias, bottomBias, (float)y / mapSize.y);
				tilemap.data[x, y] = v > bias;
			}
		}
	}

	protected override void Update(float delta)
	{
		var screenScale = App.window.size / Game.gameScreenSize;
		worldMousePosition = App.window.renderMouse / screenScale;
		tileMousePosition = worldMousePosition / tileSize;
		tileMousePosition = new(Mathf.Clamp(tileMousePosition.x, 0, mapSize.x - 1), Mathf.Clamp(tileMousePosition.y, 0, mapSize.y - 1));

		if (App.input.mouse.Down(MouseButtons.Left))
			SetTiles(tileMousePosition, lastTileMousePosition, !App.input.keyboard.shift);
		else if (App.input.mouse.Down(MouseButtons.Right))
			SetTiles(tileMousePosition, lastTileMousePosition, false);

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
		var pos = (Vector2Int)tileMousePosition * tileSize;
		var guideColor = new Color(255, 255, 255, 25);

		batch.Rect(new(0, pos.y + tileSize / 2, mapSize.x * tileSize, 1), guideColor);
		batch.Rect(new(pos.x + tileSize / 2, 0, 1, mapSize.y * tileSize), guideColor);

		for (int x = 0; x < mapSize.x; x++)
		{
			for (int y = 0; y < mapSize.y; y++)
			{
				if (tilemap!.data[x, y])
				{
					(bool top, bool right, bool bottom, bool left) key = new();
					key.top = (y == 0) ? true : tilemap.data[x, y - 1];
					key.right = (x == mapSize.x - 1) ? true : tilemap.data[x + 1, y];
					key.bottom = (y == mapSize.y - 1) ? true : tilemap.data[x, y + 1];
					key.left = (x == 0) ? true : tilemap.data[x - 1, y];

					var src = new RectInt(lut[key] * 8, 8, 8);
					batch.Image(_tileset, src, new Vector2(x, y) * tileSize, Color.white);
				}
			}
		}

		batch.HollowRect(new(pos, tileSize), 1, Color.green.translucent);
	}
}
