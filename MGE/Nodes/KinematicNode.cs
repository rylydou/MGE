using System.Runtime.Serialization;

namespace MGE
{
	[DataContract]
	public class KinematicNode : Actor
	{
		const int STEPS = 16;

		[DataMember] public Vector2Int rayCount;
		[DataMember] public LayerMask collisionMask = new LayerMask("Wall");

		public bool hitTop;
		public bool hitBottom;
		public bool hitLeft;
		public bool hitRight;

		// public bool MoveAndStop(Vector2 amount)
		// {
		// 	var amountPerStep = amount / STEPS;
		// 	for (int i = 0; i < STEPS; i++)
		// 	{
		// 		if (StepAndStop(amountPerStep)) return true;
		// 	}
		// 	return false;

		// 	// return StepAndStop(amount);

		// 	// var starting = amount.magnitude;
		// 	// var amountLeft = starting;
		// 	// while (amountLeft > 0)
		// 	// {
		// 	// 	var before = starting;
		// 	// 	amountLeft = Math.Clamp(amountLeft - world.maxStepDist, 0, float.MaxValue);
		// 	// 	var change = before - amountLeft;
		// 	// 	if (!StepAndStop(amount * (change / starting))) return false;
		// 	// }
		// 	// return true;
		// }

		bool StepAndStop(Vector2 amount)
		{
			position += amount;
			if (world.Overlaps(collider, collisionMask))
			{
				position -= amount;
				return true;
			}
			return false;
		}

		public void MoveAndSlide(Vector2 amount)
		{
			var amountPerStep = amount / STEPS;
			hitTop = false;
			hitBottom = false;
			hitLeft = false;
			hitRight = false;

			for (int i = 0; i < STEPS; i++)
			{
				var hitX = StepAndStop(amountPerStep.isolateX);
				var hitY = StepAndStop(amountPerStep.isolateY);

				if (hitX)
				{
					if (amount.x < 0)
						hitLeft = true;
					else
						hitRight = true;
				}

				if (hitY)
				{
					if (amount.y < 0)
						hitTop = true;
					else
						hitBottom = true;
				}
			}
		}
	}
}

/*
		public struct RaycastOrigins
		{
			public Vector2 topLeft;
			public Vector2 topRight;
			public Vector2 bottomLeft;
			public Vector2 bottomRight;
		}

		public struct CollisionInfo
		{
			public bool above;
			public bool below;
			public bool left;
			public bool right;

			public bool climbingSlope;
			public bool descendingSlope;
			public bool slidingDownMaxSlope;

			public float slopeAngle, slopeAngleOld;
			public Vector2 slopeNormal;
			public Vector2 moveAmountOld;
			public int faceDir;
			public bool fallingThroughPlatform;

			public void Reset()
			{
				above = below = false;
				left = right = false;
				climbingSlope = false;
				descendingSlope = false;
				slidingDownMaxSlope = false;
				slopeNormal = Vector2.zero;

				slopeAngleOld = slopeAngle;
				slopeAngle = 0;
			}
		}

		[DataMember] public float maxSlopeAngle = 80;
		[DataMember] public float skinWidth = 0.015f;
		[DataMember] public int horizontalRayCount = 4;
		[DataMember] public int verticalRayCount = 4;
		[DataMember] public LayerMask collisionMask = new LayerMask("Wall");

		public CollisionInfo collisions;

		float horizontalRaySpacing;
		float verticalRaySpacing;
		float distBetweenRays = 0.25f;

		public RaycastOrigins raycastOrigins;

		public void UpdateRaycastOrigins()
		{
			var bounds = collider.bounds;
			bounds.Expand(skinWidth * -2);

			raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
			raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
			raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
			raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
		}

		public void CalculateRaySpacing()
		{
			var bounds = collider.bounds;
			bounds.Expand(skinWidth * -2);

			var boundsWidth = bounds.width;
			var boundsHeight = bounds.height;

			horizontalRayCount = Math.RoundToInt(boundsHeight / distBetweenRays);
			verticalRayCount = Math.RoundToInt(boundsWidth / distBetweenRays);

			horizontalRaySpacing = bounds.height / (horizontalRayCount - 1);
			verticalRaySpacing = bounds.width / (verticalRayCount - 1);

			LogVar(nameof(horizontalRayCount), horizontalRayCount);
			LogVar(nameof(verticalRayCount), verticalRayCount);
		}

		protected override void Init()
		{
			CalculateRaySpacing();
			collisions.faceDir = 1;

			base.Init();
		}

		public void Move(Vector2 moveAmount, bool standingOnPlatform = false)
		{
			UpdateRaycastOrigins();

			collisions.Reset();
			collisions.moveAmountOld = moveAmount;

			if (moveAmount.y < 0)
			{
				DescendSlope(ref moveAmount);
			}

			if (moveAmount.x != 0)
			{
				collisions.faceDir = (int)Math.Sign(moveAmount.x);
			}

			HorizontalCollisions(ref moveAmount);
			if (moveAmount.y != 0)
			{
				VerticalCollisions(ref moveAmount);
			}

			position += moveAmount;

			if (standingOnPlatform)
			{
				collisions.below = true;
			}
		}

		void HorizontalCollisions(ref Vector2 moveAmount)
		{
			var directionX = collisions.faceDir;
			var rayLength = Math.Abs(moveAmount.x) + skinWidth;

			if (Math.Abs(moveAmount.x) < skinWidth)
			{
				rayLength = 2 * skinWidth;
			}

			for (int i = 0; i < horizontalRayCount; i++)
			{
				var rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
				rayOrigin += Vector2.up * (horizontalRaySpacing * i);
				var hit = world.RaycastOnGrid(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

				// if (DEBUG) Debug.DrawRay(rayOrigin, Vector2.right * directionX, new Color(0, 1, 0, 0.1f));

				if (hit is object)
				{
					if (hit.distance == 0) continue;

					var slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

					if (i == 0 && slopeAngle <= maxSlopeAngle)
					{
						if (collisions.descendingSlope)
						{
							collisions.descendingSlope = false;
							moveAmount = collisions.moveAmountOld;
						}
						var distanceToSlopeStart = 0f;
						if (slopeAngle != collisions.slopeAngleOld)
						{
							distanceToSlopeStart = hit.distance - skinWidth;
							moveAmount.x -= distanceToSlopeStart * directionX;
						}
						ClimbSlope(ref moveAmount, slopeAngle, hit.normal);
						moveAmount.x += distanceToSlopeStart * directionX;
					}

					if (!collisions.climbingSlope || slopeAngle > maxSlopeAngle)
					{
						moveAmount.x = (hit.distance - skinWidth) * directionX;
						rayLength = hit.distance;

						if (collisions.climbingSlope)
						{
							moveAmount.y = Math.Tan(Math.Deg2Rad(collisions.slopeAngle)) * Math.Abs(moveAmount.x);
						}

						collisions.left = directionX == -1;
						collisions.right = directionX == 1;
					}
				}
			}
		}

		void VerticalCollisions(ref Vector2 moveAmount)
		{
			var directionY = Math.Sign(moveAmount.y);
			var rayLength = Math.Abs(moveAmount.y) + skinWidth;

			for (int i = 0; i < verticalRayCount; i++)
			{

				var rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
				rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
				var hit = world.RaycastOnGrid(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

				// if (DEBUG) Debug.DrawRay(rayOrigin, Vector2.up * directionY, new Color(0, 1, 0, 0.1f));

				if (hit is object)
				{
					// if (hit.collider.tag == "Through")
					// {
					// 	if (directionY == 1 || hit.distance == 0)
					// 	{
					// 		continue;
					// 	}
					// 	if (collisions.fallingThroughPlatform)
					// 	{
					// 		continue;
					// 	}
					// 	if (input.y == -1)
					// 	{
					// 		collisions.fallingThroughPlatform = true;
					// 		Invoke("ResetFallingThroughPlatform", .5f);
					// 		continue;
					// 	}
					// }

					moveAmount.y = (hit.distance - skinWidth) * directionY;
					rayLength = hit.distance;

					if (collisions.climbingSlope)
					{
						moveAmount.x = moveAmount.y / Math.Tan(Math.Deg2Rad(collisions.slopeAngle)) * Math.Sign(moveAmount.x);
					}

					collisions.below = directionY == -1;
					collisions.above = directionY == 1;
				}
			}

			if (collisions.climbingSlope)
			{
				var directionX = Math.Sign(moveAmount.x);
				rayLength = Math.Abs(moveAmount.x) + skinWidth;
				var rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * moveAmount.y;
				var hit = world.RaycastOnGrid(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

				if (hit is object)
				{
					var slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
					if (slopeAngle != collisions.slopeAngle)
					{
						moveAmount.x = (hit.distance - skinWidth) * directionX;
						collisions.slopeAngle = slopeAngle;
						collisions.slopeNormal = hit.normal;
					}
				}
			}
		}

		void ClimbSlope(ref Vector2 moveAmount, float slopeAngle, Vector2 slopeNormal)
		{
			var moveDistance = Math.Abs(moveAmount.x);
			var climbmoveAmountY = Math.Sin(Math.Deg2Rad(slopeAngle)) * moveDistance;

			if (moveAmount.y <= climbmoveAmountY)
			{
				moveAmount.y = climbmoveAmountY;
				moveAmount.x = Math.Cos(Math.Deg2Rad(slopeAngle)) * moveDistance * Math.Sign(moveAmount.x);
				collisions.below = true;
				collisions.climbingSlope = true;
				collisions.slopeAngle = slopeAngle;
				collisions.slopeNormal = slopeNormal;
			}
		}

		// TODO: Fix this
		void DescendSlope(ref Vector2 moveAmount)
		{
			var maxSlopeHitLeft = world.RaycastOnGrid(raycastOrigins.bottomLeft, Vector2.down, Math.Abs(moveAmount.y) + skinWidth, collisionMask);
			var maxSlopeHitRight = world.RaycastOnGrid(raycastOrigins.bottomRight, Vector2.down, Math.Abs(moveAmount.y) + skinWidth, collisionMask);
			// SlideDownMaxSlope(maxSlopeHitLeft, ref moveAmount);
			// SlideDownMaxSlope(maxSlopeHitRight, ref moveAmount);

			// if (maxSlopeHitLeft ^ maxSlopeHitRight)
			// {
			// 	SlideDownMaxSlope(maxSlopeHitLeft, ref moveAmount);
			// 	SlideDownMaxSlope(maxSlopeHitRight, ref moveAmount);
			// }

			if (!collisions.slidingDownMaxSlope)
			{
				var directionX = Math.Sign(moveAmount.x);
				var rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
				var hit = world.RaycastOnGrid(rayOrigin, -Vector2.up, float.PositiveInfinity, collisionMask);

				if (hit is object)
				{
					var slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
					if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
					{
						if (Math.Sign(hit.normal.x) == directionX)
						{
							if (hit.distance - skinWidth <= Math.Tan(Math.Deg2Rad(slopeAngle)) * Math.Abs(moveAmount.x))
							{
								var moveDistance = Math.Abs(moveAmount.x);
								var descendmoveAmountY = Math.Sin(Math.Deg2Rad(slopeAngle)) * moveDistance;
								moveAmount.x = Math.Cos(Math.Deg2Rad(slopeAngle)) * moveDistance * Math.Sign(moveAmount.x);
								moveAmount.y -= descendmoveAmountY;

								collisions.slopeAngle = slopeAngle;
								collisions.descendingSlope = true;
								collisions.below = true;
								collisions.slopeNormal = hit.normal;
							}
						}
					}
				}
			}
		}

		void SlideDownMaxSlope(RaycastHit hit, ref Vector2 moveAmount)
		{
			if (hit is object)
			{
				var slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if (slopeAngle > maxSlopeAngle)
				{
					moveAmount.x = Math.Sign(hit.normal.x) * (Math.Abs(moveAmount.y) - hit.distance) / Math.Tan(Math.Deg2Rad(slopeAngle));

					collisions.slopeAngle = slopeAngle;
					collisions.slidingDownMaxSlope = true;
					collisions.slopeNormal = hit.normal;
				}
			}
		}

		void ResetFallingThroughPlatform()
		{
			collisions.fallingThroughPlatform = false;
		}
 */