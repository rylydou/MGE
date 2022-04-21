// using System;
// using System.Collections.Generic;
// using MGE;

// namespace Demo;

// class Platform : Body2D
// {
// 	private Vector2 movementCounter;
// 	private Vector2 shakeAmount;
// 	private bool shaking;
// 	private float shakeTimer;
// 	protected List<StaticMover> staticMovers = new();
// 	public Vector2 LiftSpeed;
// 	public bool Safe;
// 	public bool BlockWaterfalls = true;
// 	public int SurfaceSoundIndex = 8;
// 	public int SurfaceSoundPriority;
// 	public DashCollision OnDashCollide;
// 	public Action<Vector2> OnCollide;

// 	public Vector2 Shake => this.shakeAmount;

// 	public HitboxCollider2D? Hitbox => this.collider as HitboxCollider2D;

// 	public Vector2 ExactPosition => this.globalPosition + this.movementCounter;

// 	public Platform(Vector2 position, bool safe) : base(position)
// 	{
// 		this.Safe = safe;
// 	}

// 	public void ClearRemainder() => this.movementCounter = Vector2.Zero;

// 	public override void Update()
// 	{
// 		base.Update();
// 		this.LiftSpeed = Vector2.Zero;
// 		if (!this.shaking)
// 			return;
// 		if (this.Scene.OnInterval(0.04f))
// 		{
// 			Vector2 shakeAmount = this.shakeAmount;
// 			this.shakeAmount = Calc.Random.ShakeVector();
// 			this.OnShake(this.shakeAmount - shakeAmount);
// 		}
// 		if ((double)this.shakeTimer <= 0.0)
// 			return;
// 		this.shakeTimer -= Engine.DeltaTime;
// 		if ((double)this.shakeTimer > 0.0)
// 			return;
// 		this.shaking = false;
// 		this.StopShaking();
// 	}

// 	public void StartShaking(float time = 0.0f)
// 	{
// 		this.shaking = true;
// 		this.shakeTimer = time;
// 	}

// 	public void StopShaking()
// 	{
// 		this.shaking = false;
// 		if (!(this.shakeAmount != Vector2.Zero))
// 			return;
// 		this.OnShake(-this.shakeAmount);
// 		this.shakeAmount = Vector2.Zero;
// 	}

// 	public virtual void OnShake(Vector2 amount) => this.ShakeStaticMovers(amount);

// 	public void ShakeStaticMovers(Vector2 amount)
// 	{
// 		foreach (StaticMover staticMover in this.staticMovers)
// 			staticMover.Shake(amount);
// 	}

// 	public void MoveStaticMovers(Vector2 amount)
// 	{
// 		foreach (StaticMover staticMover in this.staticMovers)
// 			staticMover.Move(amount);
// 	}

// 	public void DestroyStaticMovers()
// 	{
// 		foreach (StaticMover staticMover in this.staticMovers)
// 			staticMover.Destroy();
// 		this.staticMovers.Clear();
// 	}

// 	public void DisableStaticMovers()
// 	{
// 		foreach (StaticMover staticMover in this.staticMovers)
// 			staticMover.Disable();
// 	}

// 	public void EnableStaticMovers()
// 	{
// 		foreach (StaticMover staticMover in this.staticMovers)
// 			staticMover.Enable();
// 	}

// 	public virtual void OnStaticMoverTrigger(StaticMover sm)
// 	{
// 	}

// 	public virtual int GetLandSoundIndex(Entity entity) => this.SurfaceSoundIndex;

// 	public virtual int GetWallSoundIndex(Player player, int side) => this.SurfaceSoundIndex;

// 	public virtual int GetStepSoundIndex(Entity entity) => this.SurfaceSoundIndex;

// 	public void MoveH(float moveH)
// 	{
// 		this.LiftSpeed.x = (double)Engine.DeltaTime != 0.0 ? moveH / Engine.DeltaTime : 0.0f;
// 		this.movementCounter.x += moveH;
// 		int move = (int)Math.Round((double)this.movementCounter.x);
// 		if (move == 0)
// 			return;
// 		this.movementCounter.x -= (float)move;
// 		this.MoveHExact(move);
// 	}

// 	public void MoveH(float moveH, float liftSpeedH)
// 	{
// 		this.LiftSpeed.x = liftSpeedH;
// 		this.movementCounter.x += moveH;
// 		int move = (int)Math.Round((double)this.movementCounter.x);
// 		if (move == 0)
// 			return;
// 		this.movementCounter.x -= (float)move;
// 		this.MoveHExact(move);
// 	}

// 	public void MoveV(float moveV)
// 	{
// 		this.LiftSpeed.y = (double)Engine.DeltaTime != 0.0 ? moveV / Engine.DeltaTime : 0.0f;
// 		this.movementCounter.y += moveV;
// 		int move = (int)Math.Round((double)this.movementCounter.y);
// 		if (move == 0)
// 			return;
// 		this.movementCounter.y -= (float)move;
// 		this.MoveVExact(move);
// 	}

// 	public void MoveV(float moveV, float liftSpeedV)
// 	{
// 		this.LiftSpeed.y = liftSpeedV;
// 		this.movementCounter.y += moveV;
// 		int move = (int)Math.Round((double)this.movementCounter.y);
// 		if (move == 0)
// 			return;
// 		this.movementCounter.y -= (float)move;
// 		this.MoveVExact(move);
// 	}

// 	public void MoveToX(float x) => this.MoveH(x - this.ExactPosition.x);

// 	public void MoveToX(float x, float liftSpeedX) => this.MoveH(x - this.ExactPosition.x, liftSpeedX);

// 	public void MoveToY(float y) => this.MoveV(y - this.ExactPosition.y);

// 	public void MoveToY(float y, float liftSpeedY) => this.MoveV(y - this.ExactPosition.y, liftSpeedY);

// 	public void MoveTo(Vector2 position)
// 	{
// 		this.MoveToX(position.x);
// 		this.MoveToY(position.y);
// 	}

// 	public void MoveTo(Vector2 position, Vector2 liftSpeed)
// 	{
// 		this.MoveToX(position.x, liftSpeed.x);
// 		this.MoveToY(position.y, liftSpeed.y);
// 	}

// 	public void MoveTowardsX(float x, float amount) => this.MoveToX(Calc.Approach(this.ExactPosition.x, x, amount));

// 	public void MoveTowardsY(float y, float amount) => this.MoveToY(Calc.Approach(this.ExactPosition.y, y, amount));

// 	public abstract void MoveHExact(int move);

// 	public abstract void MoveVExact(int move);

// 	public void MoveToNaive(Vector2 position)
// 	{
// 		this.MoveToXNaive(position.x);
// 		this.MoveToYNaive(position.y);
// 	}

// 	public void MoveToXNaive(float x) => this.MoveHNaive(x - this.ExactPosition.x);

// 	public void MoveToYNaive(float y) => this.MoveVNaive(y - this.ExactPosition.y);

// 	public void MoveHNaive(float moveH)
// 	{
// 		this.LiftSpeed.x = (double)Engine.DeltaTime != 0.0 ? moveH / Engine.DeltaTime : 0.0f;
// 		this.movementCounter.x += moveH;
// 		int num = (int)Math.Round((double)this.movementCounter.x);
// 		if (num == 0)
// 			return;
// 		this.movementCounter.x -= (float)num;
// 		this.x += (float)num;
// 		this.MoveStaticMovers(Vector2.UnitX * (float)num);
// 	}

// 	public void MoveVNaive(float moveV)
// 	{
// 		this.LiftSpeed.y = (double)Engine.DeltaTime != 0.0 ? moveV / Engine.DeltaTime : 0.0f;
// 		this.movementCounter.y += moveV;
// 		int num = (int)Math.Round((double)this.movementCounter.y);
// 		if (num == 0)
// 			return;
// 		this.movementCounter.y -= (float)num;
// 		this.y += (float)num;
// 		this.MoveStaticMovers(Vector2.UnitY * (float)num);
// 	}

// 	public bool MoveHCollideSolids(
// 		float moveH,
// 		bool thruDashBlocks,
// 		Action<Vector2, Vector2, Platform> onCollide = null)
// 	{
// 		this.LiftSpeed.x = (double)Engine.DeltaTime != 0.0 ? moveH / Engine.DeltaTime : 0.0f;
// 		this.movementCounter.x += moveH;
// 		int moveH1 = (int)Math.Round((double)this.movementCounter.x);
// 		if (moveH1 == 0)
// 			return false;
// 		this.movementCounter.x -= (float)moveH1;
// 		return this.MoveHExactCollideSolids(moveH1, thruDashBlocks, onCollide);
// 	}

// 	public bool MoveVCollideSolids(
// 		float moveV,
// 		bool thruDashBlocks,
// 		Action<Vector2, Vector2, Platform> onCollide = null)
// 	{
// 		this.LiftSpeed.y = (double)Engine.DeltaTime != 0.0 ? moveV / Engine.DeltaTime : 0.0f;
// 		this.movementCounter.y += moveV;
// 		int moveV1 = (int)Math.Round((double)this.movementCounter.y);
// 		if (moveV1 == 0)
// 			return false;
// 		this.movementCounter.y -= (float)moveV1;
// 		return this.MoveVExactCollideSolids(moveV1, thruDashBlocks, onCollide);
// 	}

// 	public bool MoveHCollideSolidsAndBounds(
// 		Level level,
// 		float moveH,
// 		bool thruDashBlocks,
// 		Action<Vector2, Vector2, Platform> onCollide = null)
// 	{
// 		this.LiftSpeed.x = (double)Engine.DeltaTime != 0.0 ? moveH / Engine.DeltaTime : 0.0f;
// 		this.movementCounter.x += moveH;
// 		int moveH1 = (int)Math.Round((double)this.movementCounter.x);
// 		if (moveH1 == 0)
// 			return false;
// 		this.movementCounter.x -= (float)moveH1;
// 		double num1 = (double)this.Left + (double)moveH1;
// 		Rectangle bounds = level.Bounds;
// 		double left = (double)bounds.Left;
// 		bool flag;
// 		if (num1 < left)
// 		{
// 			flag = true;
// 			bounds = level.Bounds;
// 			moveH1 = bounds.Left - (int)this.Left;
// 		}
// 		else
// 		{
// 			double num2 = (double)this.Right + (double)moveH1;
// 			bounds = level.Bounds;
// 			double right = (double)bounds.Right;
// 			if (num2 > right)
// 			{
// 				flag = true;
// 				bounds = level.Bounds;
// 				moveH1 = bounds.Right - (int)this.Right;
// 			}
// 			else
// 				flag = false;
// 		}
// 		return this.MoveHExactCollideSolids(moveH1, thruDashBlocks, onCollide) | flag;
// 	}

// 	public bool MoveVCollideSolidsAndBounds(
// 		Level level,
// 		float moveV,
// 		bool thruDashBlocks,
// 		Action<Vector2, Vector2, Platform> onCollide = null,
// 		bool checkBottom = true)
// 	{
// 		this.LiftSpeed.y = (double)Engine.DeltaTime != 0.0 ? moveV / Engine.DeltaTime : 0.0f;
// 		this.movementCounter.y += moveV;
// 		int moveV1 = (int)Math.Round((double)this.movementCounter.y);
// 		if (moveV1 == 0)
// 			return false;
// 		this.movementCounter.y -= (float)moveV1;
// 		int num = level.Bounds.Bottom + 32;
// 		bool flag;
// 		if ((double)this.Top + (double)moveV1 < (double)level.Bounds.Top)
// 		{
// 			flag = true;
// 			moveV1 = level.Bounds.Top - (int)this.Top;
// 		}
// 		else if (checkBottom && (double)this.Bottom + (double)moveV1 > (double)num)
// 		{
// 			flag = true;
// 			moveV1 = num - (int)this.Bottom;
// 		}
// 		else
// 			flag = false;
// 		return this.MoveVExactCollideSolids(moveV1, thruDashBlocks, onCollide) | flag;
// 	}

// 	public bool MoveHExactCollideSolids(
// 		int moveH,
// 		bool thruDashBlocks,
// 		Action<Vector2, Vector2, Platform> onCollide = null)
// 	{
// 		float x = this.x;
// 		int num = Math.Sign(moveH);
// 		int move = 0;
// 		Solid solid = (Solid)null;
// 		while (moveH != 0)
// 		{
// 			if (thruDashBlocks)
// 			{
// 				foreach (DashBlock entity in this.Scene.Tracker.GetEntities<DashBlock>())
// 				{
// 					if (this.CollideCheck((Entity)entity, this.globalPosition + Vector2.UnitX * (float)num))
// 					{
// 						entity.Break(this.Center, Vector2.UnitX * (float)num, true, true);
// 						this.SceneAs<Level>().Shake(0.2f);
// 						Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
// 					}
// 				}
// 			}
// 			solid = this.CollideFirst<Solid>(this.globalPosition + Vector2.UnitX * (float)num);
// 			if (solid == null)
// 			{
// 				move += num;
// 				moveH -= num;
// 				this.x += (float)num;
// 			}
// 			else
// 				break;
// 		}
// 		this.x = x;
// 		this.MoveHExact(move);
// 		if (solid != null && onCollide != null)
// 			onCollide(Vector2.UnitX * (float)num, Vector2.UnitX * (float)move, (Platform)solid);
// 		return solid != null;
// 	}

// 	public bool MoveVExactCollideSolids(
// 		int moveV,
// 		bool thruDashBlocks,
// 		Action<Vector2, Vector2, Platform> onCollide = null)
// 	{
// 		float y = this.y;
// 		int num = Math.Sign(moveV);
// 		int move = 0;
// 		Platform platform = (Platform)null;
// 		while (moveV != 0)
// 		{
// 			if (thruDashBlocks)
// 			{
// 				foreach (DashBlock entity in this.Scene.Tracker.GetEntities<DashBlock>())
// 				{
// 					if (this.CollideCheck((Entity)entity, this.globalPosition + Vector2.UnitY * (float)num))
// 					{
// 						entity.Break(this.Center, Vector2.UnitY * (float)num, true, true);
// 						this.SceneAs<Level>().Shake(0.2f);
// 						Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
// 					}
// 				}
// 			}
// 			platform = (Platform)this.CollideFirst<Solid>(this.globalPosition + Vector2.UnitY * (float)num);
// 			if (platform == null)
// 			{
// 				if (moveV > 0)
// 				{
// 					platform = (Platform)this.CollideFirstOutside<JumpThru>(this.globalPosition + Vector2.UnitY * (float)num);
// 					if (platform != null)
// 						break;
// 				}
// 				move += num;
// 				moveV -= num;
// 				this.y += (float)num;
// 			}
// 			else
// 				break;
// 		}
// 		this.y = y;
// 		this.MoveVExact(move);
// 		if (platform != null && onCollide != null)
// 			onCollide(Vector2.UnitY * (float)num, Vector2.UnitY * (float)move, platform);
// 		return platform != null;
// 	}

// 	public bool MoveVCollideSolidsAndBounds(
// 		Level level,
// 		float moveV,
// 		bool thruDashBlocks,
// 		Action<Vector2, Vector2, Platform> onCollide = null)
// 	{
// 		return this.MoveVCollideSolidsAndBounds(level, moveV, thruDashBlocks, onCollide, true);
// 	}
// }
