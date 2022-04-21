using MGE;

namespace Demo;

public class Ground : Body2D
{
	public Vector2Int mapSize;
	public float tileSize;

	GridCollider2D? tilemap;

	protected override void Ready()
	{
		tilemap = new GridCollider2D(mapSize, tileSize);
		collider = tilemap;
	}

	protected override void Update(float delta)
	{
		var screenScale = App.window.size / Game.screenSize;
		var mousePosition = App.window.renderMouse / screenScale;
		var tilePosition = mousePosition / tileSize;
		tilePosition = new(Math.Clamp(tilePosition.x, 0, mapSize.x - 1), Math.Clamp(tilePosition.y, 0, mapSize.y - 1));

		if (App.input.mouse.Down(MouseButtons.Left))
			tilemap![(int)tilePosition.x, (int)tilePosition.y] = true;
		else if (App.input.mouse.Down(MouseButtons.Right))
			tilemap![(int)tilePosition.x, (int)tilePosition.y] = false;
	}

	protected override void Render(Batch2D batch)
	{
		for (int y = 0; y < mapSize.y; y++)
		{
			for (int x = 0; x < mapSize.x; x++)
			{
				var tile = tilemap!.data[x, y];
				if (tile == false) continue;

				batch.HollowRect(new(x * tileSize, y * tileSize, tileSize, tileSize), 1, Color.green);
			}
		}
	}
}
