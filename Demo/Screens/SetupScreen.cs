using System.Linq;
using MGE;
using MGE.UI;

namespace Demo.Screens;

public class SetupScreen : GameScreen
{
	public Vector2 canvasSize = new(320, 180);

	public UICanvas canvas = new();

	Font _font = App.content.Get<Font>("Fonts/Montserrat/Bold.ttf");

	public SetupScreen()
	{
		canvas.fixedSize = canvasSize;
	}

	public override void Start()
	{
	}

	public override void Tick(float delta)
	{
	}

	public override void Update(float delta)
	{
		foreach (var controls in Game.controls)
		{
			controls.Update();
		}

		for (int i = 0; i < Game.players.Length; i++)
		{
			var player = Game.players[i];

			if (player is null)
			{
				// Loop over unassigned controllers
				foreach (var controls in Game.controls.Where(c => c.player is null))
				{

					// If a button is pressed
					if (controls.confirm)
					{
						// Join a new player
						player = new PlayerData();
						player.controls = controls;
						controls.player = player;
						Game.players[i] = player;

						break;
					}
				}
			}
			else
			{
				if (player.controls is null)
				{
					// TODO  Setup controls if controls are missing
					Log.Info($"Player {i + 1} does not have controls");
					continue;
				}

				// Update player inputs
				if (player.controls.navigateDown)
				{
				}

				if (player.controls.navigateUp)
				{
				}

				if (player.controls.confirm)
				{
					player.isReady = true;
				}

				if (player.controls.back)
				{
					if (player.isReady)
					{
						// Unready the player
						player.isReady = false;
					}
					else
					{
						// Remove the owner from the controls
						player.controls.player = null;
						Game.players[i] = null;
					}
				}
			}
		}
	}

	public override void Render(Batch2D batch)
	{
		var bg = new Color(0xFAB23A);
		var fg = new Color(0x343032);

		var scale = (Vector2)App.window.size / canvasSize;
		// Log.Info(((Vector2)App.window.size / canvasSize).ToString());
		batch.PushMatrix(Vector2.zero, scale, Vector2.zero, 0);
		batch.Rect(new(canvasSize), bg);

		var cardGap = 4f;
		var cardHeight = 120f - cardGap * 2;
		var cardWidth = (canvasSize.x - cardGap * 5) / 4;

		var x = cardGap;
		for (int i = 0; i < 4; i++)
		{
			var player = Game.players[i];
			var color = Game.playerColors[i];

			if (player is not null)
			{
				var cardRect = new Rect(x, canvasSize.y - cardHeight - cardGap, cardWidth, cardHeight);
				if (player.isReady)
				{
					batch.Rect(cardRect, color);
				}
				else
				{
					batch.HollowRect(cardRect, 2, color);
				}

				batch.Draw(player.skin.texture, new(x + cardWidth / 2, (canvasSize.y - cardHeight - cardGap * 2) / 2), Color.white);

				// _font.DrawString(batch, player.controls!.name, new(x + cardGap, canvasSize.y - cardHeight), fg, 24);
			}
			x += cardWidth + cardGap;
		}

		// canvas.RenderCanvas(batch);

		batch.PopMatrix();
	}
}
