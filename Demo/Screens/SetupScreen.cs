using MGE.UI;

namespace Demo.Screens;

public class SetupScreen : GameScreen
{
	public Vector2 canvasSize = new(320, 180);

	public UICanvas canvas = new();

	SDFFont _font = App.content.Get<SDFFont>("Fonts/Regular.json");

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
		for (int i = 0; i < Game.players.Length; i++)
		{
			ref var data = ref Game.players[i];

			if (data is null)
			{
				// Loop over unassigned controllers
				foreach (var controls in Game.controls.Where(c => c.player is null))
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
					data.skinIndex = Mathf.Wrap(data.skinIndex - 1, Game.skins.Count);
				}

				if (data.controls.navigateRight.repeated)
				{
					data.skinIndex = Mathf.Wrap(data.skinIndex + 1, Game.skins.Count);
				}

				if (data.controls.confirm.pressed)
				{
					data.controls.confirm.ConsumeBuffer();

					if (data.isReady)
					{
						// TEMP Start game
						Game.ChangeScreen(new PlayingScreen());
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
						Game.players[i] = null;
					}
				}
			}
		}
	}

	public override void Render(Batch2D batch)
	{
		var bg = new Color(0xFAB23AFF);
		var fg = new Color(0x343032FF);

		var scale = (Vector2)App.window.size / canvasSize;
		batch.PushMatrix(Vector2.zero, scale, Vector2.zero, 0);
		batch.SetBox(new(canvasSize), bg);

		var cardGap = 4f;
		var cardHeight = 120f - cardGap * 2;
		var cardWidth = (canvasSize.x - cardGap * 5) / 4;

		var x = cardGap;
		for (int i = 0; i < 4; i++)
		{
			var data = Game.players[i];
			var color = Game.playerColors[i];

			if (data is not null)
			{
				var cardRect = new Rect(x, canvasSize.y - cardHeight - cardGap, cardWidth, cardHeight);
				if (data.isReady)
				{
					batch.SetBox(cardRect, color);
				}
				else
				{
					batch.SetRect(cardRect, 2, color);
				}

				batch.Draw(data.skin.texture, new(x + cardWidth / 2, (canvasSize.y - cardHeight - cardGap * 2) / 2), Color.white);

				batch.DrawString(_font, data.controls!.name, new(x, canvasSize.y - cardHeight - cardGap, cardWidth, cardHeight), TextAlignment.Center, fg, 4);
			}
			x += cardWidth + cardGap;
		}

		// canvas.RenderCanvas(batch);

		batch.PopMatrix();
	}
}
