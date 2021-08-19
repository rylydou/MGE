using System.Runtime.Serialization;
using MGE;

namespace Playground.Nodes
{
	[DataContract]
	public class Player : KinematicNode
	{
		[DataMember] public float jumpRem = 0.275f;
		[DataMember] public float groundedRem = 0.1f;
		[DataMember] public float smallJumpVel = -6f;
		[DataMember] public float bigJumpVel = -14f;

		[DataMember] public float maxGroundSpeed = 6f;
		[DataMember] public float groundAcceleration = 47.5f;
		[DataMember] public float groundFriction = 0.1f;

		[DataMember] public float maxAirSpeed = 7f;
		[DataMember] public float airAcceleration = 30f;
		[DataMember] public float airFriction = 0.95f;

		// [DataMember] public float maxVelSpeedAir = 6.5f;
		[DataMember] public float velGroundFriction = 0.5f;
		[DataMember] public float velAirFriction = 0.95f;

		[DataMember] public Vector2 gravity = new Vector2(0, 35);
		[DataMember] public float maxFallSpeed = 45f;
		// [DataMember] public float maxFallSpeed = float.PositiveInfinity;

		float move;
		float extraVel;
		Vector2 velocity;
		bool jumped;
		float groundedMem;
		float jumpMem;

		float moveInput;
		bool jumpDownInput;
		bool jumpUpInput;

		CollidableNode groundCheck;

		public Player()
		{
			SetCollider(new HitboxCollider(new Vector2(0.5f, 0.5f), new Vector2(0.25f, 0.5f)));
			layer = new LayerMask("Player");

			position = new Vector2(4);
		}

		protected override void Init()
		{
			groundCheck = (CollidableNode)GetNode("Ground Check");

			base.Init();
		}

		protected override void Tick()
		{
			var grounded = world.Overlaps(groundCheck.collider, collisionMask);

			velocity += gravity * Time.tickTime;

			groundedMem -= Time.tickTime;
			if (grounded)
				groundedMem = groundedRem;

			jumpMem -= Time.tickTime;
			if (jumpDownInput)
				jumpMem = jumpRem;

			if (groundedMem > 0 && jumpMem > 0)
			{
				velocity.y = bigJumpVel;
				jumped = true;
			}
			jumpDownInput = false;

			if (jumpUpInput && jumped && velocity.y < smallJumpVel)
			{
				velocity.y = smallJumpVel;
				jumped = false;
			}
			jumpUpInput = false;

			velocity.y = Math.Min(velocity.y, maxFallSpeed);

			if (Math.Abs(moveInput) < 0.1f)
				move *= (grounded ? groundFriction : airFriction) * (1 - Time.tickTime);
			else
				move += moveInput * (grounded ? groundAcceleration : airAcceleration) * Time.tickTime;

			var cap = grounded ? maxGroundSpeed : maxAirSpeed;
			move = Math.Clamp(move, -cap, cap);

			extraVel *= (grounded ? velGroundFriction : velAirFriction) * (1 - Time.tickTime);

			velocity.x = move + extraVel;

			if (hitBottom && velocity.y > 0)
			{
				jumped = false;
				velocity.y = 0;
			}
			if (hitTop && velocity.y < 0)
			{
				velocity.y = 0;
			}
			if (hitLeft && velocity.x < 0)
			{
				move = 0;
				extraVel = 0;
				velocity.x = 0;
			}
			if (hitRight && velocity.x > 0)
			{
				move = 0;
				extraVel = 0;
				velocity.x = 0;
			}

			MoveAndSlide(velocity * Time.tickTime);

			base.Tick();
		}

		protected override void Update()
		{
			moveInput = (Input.IsButtonHeld(Buttons.KB_D) ? 1 : 0) - (Input.IsButtonHeld(Buttons.KB_A) ? 1 : 0);
			if (Input.IsButtonPressed(Buttons.KB_Space)) jumpDownInput = true;
			if (Input.IsButtonReleased(Buttons.KB_Space)) jumpUpInput = true;
			// if (Input.IsButtonPressed(Buttons.KB_LeftControl)) ApplyForce(new Vector2(26, -6));

			if (Input.IsButtonPressed(Buttons.KB_R))
			{
				position = new Vector2(4);
				velocity = Vector2.zero;
			}

			if (Input.IsButtonPressed(Buttons.KB_G))
			{
				Folder.saveData.FileWriteReadable("Player.node.json", this);
			}

			// _camera.position = absolutePosition;

			base.Update();
		}

		public virtual void ApplyForce(Vector2 force)
		{
			extraVel = force.x;
			velocity.y = force.y;
		}
	}
}