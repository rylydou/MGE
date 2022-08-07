using MGE.UI;

namespace Game;

public class SetupScreen : GameScreen
{
	public UICanvas canvas = new();

	public SetupScreen()
	{
		canvas.fixedSize = Main.screenSize;
	}

	public override void Start()
	{
	}

	public override void Tick(float delta)
	{
	}

	public override void Update(float delta)
	{
		for (int i = 0; i < Main.players.Length; i++)
		{
			ref var data = ref Main.players[i];

			if (data is null)
			{
				// Loop over unassigned controllers
				foreach (var controls in Main.controls.Where(c => c.player is null))
				{
					// If a button is pressed
					if (controls.anyButton.pressed)
					{
						controls.anyButton.ConsumeBuffer();

						// Join a new player
						data = new PlayerData();
						data.controls = controls;
						controls.player = data;

						break;
					}
				}
			}
			else
			{
				if (data.controls is null)
				{
					// TODO  Setup controls if controls are missing
					Log.Info($"Player {i + 1} does not have controls");
					continue;
				}

				// Update player inputs
				if (data.controls.navigateDown.repeated)
				{
				}

				if (data.controls.navigateUp.repeated)
				{
				}

				if (data.controls.navigateLeft.repeated)
				{
					data.skinIndex = Mathf.Wrap(data.skinIndex - 1, Main.skins.Count);
				}

				if (data.controls.navigateRight.repeated)
				{
					data.skinIndex = Mathf.Wrap(data.skinIndex + 1, Main.skins.Count);
				}

				if (data.controls.confirm.pressed)
				{
					data.controls.confirm.ConsumeBuffer();

					if (data.isReady)
					{
						// TEMP Start game
						Main.ChangeScreen(new PlayingScreen());
						return;
					}
					else
					{
						// Ready up
						data.isReady = true;
					}
				}

				if (data.controls.back.pressed)
				{
					data.controls.back.ConsumeBuffer();

					if (data.isReady)
					{
						// Unready the player
						data.isReady = false;
					}
					else
					{
						// Remove the owner from the controls
						data.controls.player = null;
						Main.players[i] = null;
					}
				}
			}
		}
	}

	public override void Render(Batch2D batch)
	{
		var bg = new Color(0xFAB23AFF);
		var fg = new Color(0x343032FF);

		batch.SetBox(new(Main.screenSize), bg);

		var cardGap = 8f;
		var cardHeight = 240f - cardGap * 2;
		var cardWidth = (Main.screenSize.x - cardGap * 5) / 4;

		var x = cardGap;
		for (int i = 0; i < 4; i++)
		{
			var data = Main.players[i];
			var color = Main.playerColors[i];

			if (data is not null)
			{
				var cardRect = new Rect(x, Main.screenSize.y - cardHeight - cardGap, cardWidth, cardHeight);
				if (data.isReady)
				{
					batch.SetBox(cardRect, color);
				}
				else
				{
					batch.SetRect(cardRect, -2, color);
				}

				var iconPos = new Vector2(x + cardWidth / 2, (Main.screenSize.y - cardHeight - cardGap * 2) / 2);

				var sprite = data.skin.spriteSheet.GetSprite(0);
				batch.Draw(sprite, iconPos, Vector2.zero, 0, new(2), Color.white);

				// batch.SetCircle(iconPos, 2, 8, Color.black);
				// batch.SetCircle(iconPos, 1, 8, data.color);

				batch.DrawString(App.content.font, data.controls!.name, new(x, Main.screenSize.y - cardHeight - cardGap, cardWidth, cardHeight), TextAlignment.Center, fg, 8);
			}
			x += cardWidth + cardGap;
		}

		// canvas.RenderCanvas(batch);
	}
}
