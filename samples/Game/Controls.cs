namespace Game;

public class Controls
{
	public PlayerData? player;

	public int id;
	public string name => id switch
	{
		-2 => "Arrowkeys",
		-1 => "WASD",
		>= 0 and <= 32 => App.input.controllers[id].name,
		_ => "Unknown",
	};

	public bool isPresent => id switch
	{
		-2 => true,
		-1 => true,
		>= 0 and <= 32 => App.input.controllers[id].isConnected,
		_ => false,
	};
	public bool hasError => id switch
	{
		-2 => false,
		-1 => false,
		>= 0 and <= 32 => !App.input.controllers[id].isGamepad,
		_ => true,
	};

	public bool isGamepad => id >= 0 && id <= 32;
	public bool isWASD => id == -1;
	public bool isArrowKeys => id == -2;
	public bool isKeyboard => isWASD || isArrowKeys;

	// General
	public VirtualButton anyButton = new(App.input);

	public VirtualButton pause = new(App.input);

	// Gameplay
	public VirtualAxis move = new(App.input);
	public VirtualButton jump = new(App.input, 0.1f);
	public VirtualButton crouch = new(App.input);
	public VirtualButton action = new(App.input, 0.1f);
	public VirtualButton altAction = new(App.input, 0.1f);

	// Menu
	public VirtualStick navigation = new(App.input, 0.15f);
	public VirtualButton navigateLeft = new(App.input);
	public VirtualButton navigateRight = new(App.input);
	public VirtualButton navigateUp = new(App.input);
	public VirtualButton navigateDown = new(App.input);

	public VirtualButton confirm = new(App.input);
	public VirtualButton back = new(App.input);

	public Controls(int id)
	{
		this.id = id;

		switch (this.id)
		{
			case -2: InitArrowKeys(); break;
			case -1: InitWASD(); break;
			case >= 0 and <= 32: InitController(this.id); break;
			default: throw new Exception("Unknown controls id #" + this.id);
		}
	}

	void InitArrowKeys()
	{
		anyButton.Add(Keys.Enter);

		pause.Add(Keys.Delete);

		move.Add(Keys.Left, Keys.Right);
		jump.Add(Keys.Up);
		crouch.Add(Keys.Down);
		action.Add(Keys.RightShift, Keys.RightControl);
		action.Add(Keys.RightAlt, Keys.End);

		navigation.Add(Keys.Left, Keys.Right, Keys.Up, Keys.Down);
		navigateLeft.Add(Keys.Left);
		navigateRight.Add(Keys.Right);
		navigateUp.Add(Keys.Up);
		navigateDown.Add(Keys.Down);

		confirm.Add(Keys.Enter);
		back.Add(Keys.Backspace);
	}

	void InitWASD()
	{
		anyButton.Add(Keys.E, Keys.Space);

		pause.Add(Keys.Escape);

		move.Add(Keys.A, Keys.D);
		jump.Add(Keys.W);
		crouch.Add(Keys.S);
		action.Add(Keys.E);
		altAction.Add(Keys.R);

		navigation.Add(Keys.A, Keys.D, Keys.W, Keys.S);
		navigateLeft.Add(Keys.A);
		navigateRight.Add(Keys.D);
		navigateUp.Add(Keys.W);
		navigateDown.Add(Keys.S);

		confirm.Add(Keys.E, Keys.Space);
		back.Add(Keys.Q, Keys.Escape);
	}

	void InitController(int index)
	{
		anyButton.Add(id, Buttons.A, Buttons.Start);

		pause.Add(id, Buttons.Start);

		move.Add(id, Axes.LeftX).Add(id, Buttons.Left, Buttons.Right);
		jump.Add(id, Buttons.A);
		crouch.Add(id, Axes.RightY, 0.15f).Add(id, Axes.LeftTrigger, 0.15f);
		action.Add(id, Buttons.Y);
		altAction.Add(id, Buttons.X);

		navigation.Add(id, Axes.LeftX, Axes.LeftY, 0.15f, 0.15f).Add(id, Buttons.Left, Buttons.Right, Buttons.Up, Buttons.Down);
		navigateLeft.Add(id, Axes.LeftX, -0.15f).Add(id, Buttons.Left);
		navigateRight.Add(id, Axes.LeftX, 0.15f).Add(id, Buttons.Right);
		navigateUp.Add(id, Axes.LeftY, -0.15f).Add(id, Buttons.Up);
		navigateDown.Add(id, Axes.LeftY, 0.15f).Add(id, Buttons.Down);

		confirm.Add(id, Buttons.A);
		back.Add(id, Buttons.B);
	}
}
