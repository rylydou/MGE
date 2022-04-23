using MGE;

namespace Demo;

public class Ground : Solid
{
	public Vector2Int mapSize;
	public float tileSize;

	GridCollider2D? tilemap;

	Vector2 lastTileMousePosition;

	Texture _tileSprite = App.content.Get<Texture>("Scene/Ground/Dirt.ase");
	Font _font = App.content.Get<Font>("Fonts/Inter/Regular.ttf");

	Vector2 worldMousePosition;
	Vector2 tileMousePosition;

	protected override void Ready()
	{
		tilemap = new GridCollider2D(mapSize, tileSize);
		SetCollider(tilemap);
	}

	protected override void Update(float delta)
	{
		var screenScale = App.window.size / Game.screenSize;
		worldMousePosition = App.window.renderMouse / screenScale;
		tileMousePosition = worldMousePosition / tileSize;
		tileMousePosition = new(Math.Clamp(tileMousePosition.x, 0, mapSize.x - 1), Math.Clamp(tileMousePosition.y, 0, mapSize.y - 1));

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
		for (int x = 0; x < mapSize.x; x++)
		{
			for (int y = 0; y < mapSize.y; y++)
			{
				if (tilemap!.data[x, y])
				{
					var r1 = System.HashCode.Combine(x, y) % 4 + 4;
					var r2 = System.HashCode.Combine(y, x) % 4 + 4;
					var src = new RectInt(r1, r2, 8, 8);
					batch.Image(_tileSprite, src, new Vector2(x, y) * tileSize, Color.white);
				}
			}
		}

		var pos = (Vector2Int)tileMousePosition * tileSize;
		var guideColor = new Color(255, 255, 255, 25);

		batch.Rect(new(0, pos.y + tileSize / 2, mapSize.x * tileSize, 1), guideColor);
		batch.Rect(new(pos.x + tileSize / 2, 0, 1, mapSize.y * tileSize), guideColor);

		batch.HollowRect(new(pos, tileSize), 1, Color.green.translucent);
	}
}
