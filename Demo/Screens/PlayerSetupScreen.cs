using System.Linq;
using MGE;
using MGE.UI;

namespace Demo.Screens;

public class PlayerSetupScreen : GameScreen
{
	public UICanvas canvas = new();
	public Vector2 canvasSize = new(320, 180);

	public PlayerSetupScreen()
	{
		canvas.fixedSize = canvasSize;
	}

	public override void Tick(float delta)
	{
	}

	public override void Update(float delta)
	{
		for (int pid = 0; pid < Game.instance.players.Length; pid++)
		{
			var player = Game.instance.players[pid];
			if (player is null)
			{
				// Loop over unassined controllers
				foreach (var controls in Game.instance.controls.Where(c => c.player is null))
				{
					// If a button is pressed
					if (controls.confirm)
					{
						// Join a new player
						player = new PlayerData();
						player.controls = controls;
						controls.player = player;
						Game.instance.players[pid] = player;
						break;
					}
				}
			}
			else
			{
				// Update player inputs
			}
		}
	}

	public override void Render(Batch2D batch)
	{
		batch.PushMatrix(Vector2.zero, (Vector2)App.window.size / canvasSize, Vector2.zero, 0);

		canvas.RenderCanvas(batch);

		batch.PopMatrix();
	}
}
