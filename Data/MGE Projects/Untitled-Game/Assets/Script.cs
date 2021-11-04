// Using C# 6
// using MGE; No need because of implicit using tags

namespace Game;

public class Player : TransformNode
{
	[Prop] public float moveSpeed = 4.0f;

	Vector2 moveInput;

	protected override void Update()
	{
		moveInput.x = (Input.GetButtonDown(Buttons.D) ? 1 : 0) - (Input.GetButtonDown(Buttons.A) ? 1 : 0);
		moveInput.y = (Input.GetButtonDown(Buttons.W) ? 1 : 0) - (Input.GetButtonDown(Buttons.S) ? 1 : 0);
	}

	protected override void Tick()
	{
		position += moveInput.normalised * Time.tickTime * moveSpeed;
	}
}
