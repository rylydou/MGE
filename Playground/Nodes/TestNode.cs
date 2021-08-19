using System.Runtime.Serialization;
using MGE;

namespace Playground.Nodes
{
	[DataContract]
	public class TestNode : CollidableNode
	{
		// Params
		[DataMember] public float walkSpeed = 5;
		[DataMember] public float runSpeed = 8;
		// [DataMember] string actionMsg = "Spawning Apple";

		// Cache
		// PixelCamera _camera;

		protected override void Init()
		{
			// _camera = GetParentNode<PixelCamera>();

			SetCollider(new HitboxCollider(Vector2.one));

			base.Init();
		}

		protected override void Tick()
		{
			var moveInput = new Vector2();
			moveInput.x = (Input.IsButtonHeld(Buttons.KB_D) ? 1 : 0) - (Input.IsButtonHeld(Buttons.KB_A) ? 1 : 0);
			moveInput.y = (Input.IsButtonHeld(Buttons.KB_S) ? 1 : 0) - (Input.IsButtonHeld(Buttons.KB_W) ? 1 : 0);
			moveInput.Normalize();

			position += moveInput * (Input.IsButtonHeld(Buttons.KB_LeftShift) ? runSpeed : walkSpeed) * Time.tickTime;

			// moveInput.x = (Input.GetButton(Buttons.Right) ? 1 : 0) - (Input.GetButton(Buttons.Left) ? 1 : 0);
			// moveInput.y = (Input.GetButton(Buttons.Down) ? 1 : 0) - (Input.GetButton(Buttons.Up) ? 1 : 0);
			// moveInput.Normalize();

			// _camera.position += moveInput * (Input.GetButton(Buttons.RightShift) ? runSpeed / 2 : walkSpeed / 2) * Time.tickTime;

			base.Tick();
		}

		// protected override void Update()
		// {
		// if (Input.GetButton(Buttons.LeftShift))
		// 	_camera.rotation += Math.Deg2Rad(Input.scroll * 5);
		// else
		// {
		// 	if (Input.scroll > 0)
		// 		_camera.zoom -= Input.scroll * _camera.zoom * 0.5f;
		// 	else
		// 		_camera.zoom -= Input.scroll * _camera.zoom;
		// }

		// base.Update();
		// }
	}
}