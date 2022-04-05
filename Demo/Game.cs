using MGE;

namespace Demo;

public class Game : Module
{
	Player playerNode = App.content.Get<Prefab>("Scene/Player/Player.node").CreateInstance<Player>();

	protected override void Startup()
	{
		App.window.onRender += Render;

		App.window.SetAspectRatio(new(320, 180));
		App.window.SetMinSize(new(320, 180));

		App.window.Center();
	}

	protected override void Shutdown()
	{
		App.window.onRender -= Render;
	}

	protected override void Update(float delta)
	{
		var kb = App.input.keyboard;
		if (kb.Pressed(Keys.F11) || (kb.Pressed(Keys.RightAlt) && kb.Pressed(Keys.Enter)))
		{
			App.window.fullscreen = !App.window.fullscreen;
			return;
		}

		Batch2D.current.CheckeredPattern(new(0, 0, App.window.size.x, App.window.size.y), 24, 24, Color.darkGray, Color.gray);

		playerNode.onUpdate(delta);
	}

	void Render(Window window)
	{
		Batch2D.current.Render(window);
		Batch2D.current.Clear();
	}
}
