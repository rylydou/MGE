using MGE;

namespace Demo;

public class Controls
{
	public int index;
	public string name = "Unknown";

	public bool isPresent;
	public bool hasError;
	public bool isGamepad => index >= 0;
	public bool isKeyboard => index == -1 || index == -2;
	public bool isWASD => index == -1;
	public bool isArrowKeys => index == -2;

	// General
	public bool pause;

	// Gameplay
	public float move;
	public bool jump;
	public bool jumpCancel;
	public bool crouch;
	public bool action;
	public bool altAction;

	// Menu
	public bool navigateUp;
	public bool navigateDown;
	public bool navigateLeft;
	public bool navigateRight;

	public bool confirm;
	public bool back;

	public Controls(int id)
	{
		this.index = id;
	}

	public void Update()
	{
		switch (index)
		{
			// WASD
			case -1:
				GetWASD();
				break;

			// Arrow keys
			case -2:
				GetArrowKeys();
				break;

			default:
				GetController(App.input.controllers[index]);
				break;
		}
	}

	void GetWASD()
	{
		name = "WASD";

		var kb = App.input.keyboard;

		isPresent = true;
		hasError = false;

		// Gameplay
		move = (kb.Down(Keys.D) ? 1 : 0) - (kb.Down(Keys.A) ? 1 : 0);
		if (kb.Pressed(Keys.W)) jump = true;
		if (kb.Released(Keys.W)) jumpCancel = true;
		crouch = kb.Down(Keys.S);
		if (kb.Pressed(Keys.E)) action = true;

		// Menu
		navigateUp = kb.Repeated(Keys.W);
		navigateDown = kb.Repeated(Keys.S);
		navigateLeft = kb.Repeated(Keys.A);
		navigateRight = kb.Repeated(Keys.D);

		confirm = kb.Pressed(Keys.E);
		back = kb.Pressed(Keys.Q);
	}

	void GetArrowKeys()
	{
		name = "Arrow Keys";

		var kb = App.input.keyboard;

		isPresent = true;
		hasError = false;

		// Gameplay
		move = (kb.Down(Keys.Right) ? 1 : 0) - (kb.Down(Keys.Left) ? 1 : 0);
		if (kb.Pressed(Keys.Up)) jump = true;
		if (kb.Released(Keys.Up)) jumpCancel = true;
		crouch = kb.Down(Keys.Down);
		if (kb.Pressed(Keys.RightControl)) action = true;
		if (kb.Pressed(Keys.RightShift)) action = true;
		if (kb.Pressed(Keys.RightAlt)) action = true;

		// Menu
		navigateUp = kb.Repeated(Keys.Up);
		navigateDown = kb.Repeated(Keys.Down);
		navigateLeft = kb.Repeated(Keys.Left);
		navigateRight = kb.Repeated(Keys.Right);

		confirm = kb.Pressed(Keys.Enter);
		back = kb.Pressed(Keys.Backspace);
	}

	void GetController(Controller controller)
	{
		name = controller.name;

		const float moveThreshold = 0.15f;
		const float crouchThreshold = 0.15f;
		const float triggerTreshold = 0.15f;

		isPresent = controller.connected;
		hasError = !controller.isGamepad;

		if (!isPresent || hasError) return;

		// Gameplay
		move = Math.Abs(controller.leftStick.x) > moveThreshold
			? Math.Sign(controller.leftStick.x)
			: 0.0f;
		if (controller.Pressed(Buttons.B)) jump = true;
		if (controller.Released(Buttons.B)) jumpCancel = true;
		crouch = controller.leftStick.y > crouchThreshold || controller.Axis(Axes.LeftTrigger) > triggerTreshold;
		if (controller.Pressed(Buttons.Y)) action = true;
		// Don't make this spam the button
		// if (controller.Axis(Axes.RightTrigger) > triggerTreshold) action = true;

		// Menu
		navigateUp = controller.Repeated(Buttons.Up);
		navigateDown = controller.Repeated(Buttons.Down);
		navigateLeft = controller.Repeated(Buttons.Left);
		navigateRight = controller.Repeated(Buttons.Right);

		confirm = controller.Pressed(Buttons.A);
		back = controller.Pressed(Buttons.B);
	}
}
