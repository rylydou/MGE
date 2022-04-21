using System;
using System.Linq;
using MGE;
using Math = MGE.Math;

namespace Demo;

public class Player : Body2D
{
	[Prop] public float moveSpeed;
	[Prop] public float moveSpeedAir;
	[Prop] public float jumpVel;
	[Prop] public float gravity;
	[Prop] public float friction;
	[Prop] public float frictionAir;

	Texture _sprite = App.content.Get<Texture>("Scene/Player/Red.ase");

	bool _alt;
	public bool isPlayer2
	{
		get => _alt;
		set
		{
			_alt = value;
			if (_alt)
				_sprite = App.content.Get<Texture>("Scene/Player/Blue.ase");
		}
	}
	public Vector2 velocity;
	float _moveInput;
	bool _jumpDown;
	// bool _jumpUp;

	private Vector2 movementCounter;
	public bool IgnoreJumpThrus;

	public Vector2 ExactPosition => globalPosition + movementCounter;

	public Vector2 PositionRemainder => movementCounter;

	public void ZeroRemainderX() => movementCounter.x = 0.0f;
	public void ZeroRemainderY() => movementCounter.y = 0.0f;

	// public virtual bool IsRiding(JumpThru jumpThru) => !this.IgnoreJumpThrus && this.CollideCheckOutside(jumpThru, globalPosition + Vector2.down);
	// public virtual bool IsRiding(Solid solid) => this.CollideCheck(solid, globalPosition + Vector2.down);

	public bool OnGround(int downCheck = 1)
	{
		if (this.CollideCheck<Ground>(globalPosition + Vector2.down * (float)downCheck))
			return true;
		return false;
		// return !this.IgnoreJumpThrus && this.CollideCheckOutside<JumpThru>(globalPosition + Vector2.down * (float)downCheck);
	}
	public bool OnGround(Vector2 at, int downCheck = 1)
	{
		var position = globalPosition;
		globalPosition = at;
		var num = this.OnGround(downCheck) ? 1 : 0;
		globalPosition = position;
		return num != 0;
	}

	public bool MoveH(float moveH, Action<CollisionInfo>? onCollide = null, Ground? pusher = null)
	{
		this.movementCounter.x += moveH;
		int moveH1 = (int)System.Math.Round((double)this.movementCounter.x, MidpointRounding.ToEven);
		if (moveH1 == 0)
			return false;
		this.movementCounter.x -= (float)moveH1;
		return this.MoveHExact(moveH1, onCollide, pusher);
	}

	public bool MoveV(float moveV, Action<CollisionInfo>? onCollide = null, Ground? pusher = null)
	{
		this.movementCounter.y += moveV;
		int moveV1 = (int)System.Math.Round((double)this.movementCounter.y, MidpointRounding.ToEven);
		if (moveV1 == 0)
			return false;
		this.movementCounter.y -= (float)moveV1;
		return this.MoveVExact(moveV1, onCollide, pusher);
	}

	public bool MoveHExact(int moveH, Action<CollisionInfo>? onCollide = null, Ground? pusher = null)
	{
		Vector2 vector2 = this.globalPosition + Vector2.down * (float)moveH;
		int num1 = Math.Sign(moveH);
		int num2 = 0;
		while (moveH != 0)
		{
			var solid = this.CollideFirst<Ground>(globalPosition + Vector2.right * (float)num1);
			if (solid != null)
			{
				this.movementCounter.x = 0.0f;
				if (onCollide != null)
					onCollide(new CollisionInfo()
					{
						Direction = Vector2.right * (float)num1,
						Moved = Vector2.right * (float)num2,
						TargetPosition = vector2,
						// Hit = (Platform)solid,
						// Pusher = pusher
					});
				return true;
			}
			num2 += num1;
			moveH -= num1;
			globalPosition = new(globalPosition.x + (float)num1, globalPosition.y);
		}
		return false;
	}

	public bool MoveVExact(int moveV, Action<CollisionInfo>? onCollide = null, Ground? pusher = null)
	{
		Vector2 vector2 = globalPosition + Vector2.down * (float)moveV;
		int num1 = Math.Sign(moveV);
		int num2 = 0;
		while (moveV != 0)
		{
			var platform1 = this.CollideFirst<Ground>(globalPosition + Vector2.down * (float)num1);
			if (platform1 != null)
			{
				this.movementCounter.y = 0.0f;
				if (onCollide != null)
					onCollide(new CollisionInfo()
					{
						Direction = Vector2.down * (float)num1,
						Moved = Vector2.down * (float)num2,
						TargetPosition = vector2,
						// Hit = platform1,
						// Pusher = pusher
					});
				return true;
			}
			// if (moveV > 0 && !this.IgnoreJumpThrus)
			// {
			// 	Platform platform2 = (Platform)this.CollideFirstOutside<JumpThru>(globalPosition + Vector2.down * (float)num1);
			// 	if (platform2 != null)
			// 	{
			// 		this.movementCounter.y = 0.0f;
			// 		if (onCollide != null)
			// 			onCollide(new CollisionInfo()
			// 			{
			// 				Direction = Vector2.down * (float)num1,
			// 				Moved = Vector2.down * (float)num2,
			// 				TargetPosition = vector2,
			// 				Hit = platform2,
			// 				Pusher = pusher
			// 			});
			// 		return true;
			// 	}
			// }
			num2 += num1;
			moveV -= num1;
			globalPosition = new(globalPosition.x, globalPosition.y + (float)num1);
		}
		return false;
	}

	public void MoveTowardsX(float targetX, float maxAmount, Action<CollisionInfo>? onCollide = null) => this.MoveToX(Calc.Approach(this.ExactPosition.x, targetX, maxAmount), onCollide);
	public void MoveTowardsY(float targetY, float maxAmount, Action<CollisionInfo>? onCollide = null) => this.MoveToY(Calc.Approach(this.ExactPosition.y, targetY, maxAmount), onCollide);

	public void MoveToX(float toX, Action<CollisionInfo>? onCollide = null) => this.MoveH(toX - this.ExactPosition.x, onCollide);
	public void MoveToY(float toY, Action<CollisionInfo>? onCollide = null) => this.MoveV(toY - this.ExactPosition.y, onCollide);

	public void NaiveMove(Vector2 amount)
	{
		this.movementCounter += amount;
		int num1 = Math.RoundToInt(this.movementCounter.x);
		int num2 = Math.RoundToInt(this.movementCounter.y);
		globalPosition = globalPosition + new Vector2((float)num1, (float)num2);
		this.movementCounter -= new Vector2((float)num1, (float)num2);
	}

	protected override void Ready()
	{
		collider = new HitboxCollider2D(new(24, 24));
	}

	protected override void Update(float delta)
	{
		var kb = App.input.keyboard;

		// Get input
		_moveInput = 0;
		if (kb.Down(isPlayer2 ? Keys.Left : Keys.A)) _moveInput -= 1;
		if (kb.Down(isPlayer2 ? Keys.Right : Keys.D)) _moveInput += 1;

		if (kb.Pressed(isPlayer2 ? Keys.Up : Keys.W)) _jumpDown = true;
		// if (kb.Released(isPlayer2 ? Keys.Up : Keys.W)) _jumpUp = true;

		if (kb.Pressed(Keys.Tab))
		{
			globalPosition = Vector2.zero;
			velocity = Vector2.zero;
		}

		Log.Info(scene!.bodies.Where(body => body is Ground).Count().ToString());
	}

	protected override void Tick(float delta)
	{
		var grounded = OnGround();

		if (grounded)
			velocity.y = 0;
		else
			velocity.y += gravity;

		if (grounded && _jumpDown)
			velocity.y = -jumpVel;
		_jumpDown = false;

		velocity.x *= grounded ? friction : frictionAir;
		if (grounded)
		{
			if (Math.Abs(_moveInput) > 0.15f)
				velocity.x = _moveInput * moveSpeed;
		}
		else velocity.x += _moveInput * moveSpeedAir;

		MoveV(velocity.y * delta, info => velocity.y = 0);
		MoveH(velocity.x * delta, info => velocity.x = 0);
		// position += velocity * delta;
		// position = new(position.x, Math.Clamp(position.y, floorY));
	}

	protected override void Render(Batch2D batch)
	{
		var time = (float)Time.duration.TotalSeconds;
		batch.Image(_sprite, Vector2.zero, Vector2.one, Vector2.zero, 0, Color.white);
	}

	bool OnGround() => CollideCheck<Ground>(globalPosition + Vector2.down);

	// bool MoveX(float moveH)
	// {
	// 	var direction = (int)Math.Sign(moveH);
	// 	var movement = 0;
	// 	while (moveH != 0)
	// 	{
	// 		var collision = CollideFirst<Ground>(globalPosition, Vector2.right * moveH);
	// 		if (collision is not null)
	// 		{
	// 			return true;
	// 		}
	// 		movement += direction;
	// 		moveH -= direction;
	// 		globalPosition = new( globalPosition.y);
	// 	}
	// 	return false;
	// }
}
