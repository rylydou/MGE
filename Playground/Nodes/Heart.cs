using System.Runtime.Serialization;
using MGE;

namespace Playground.Nodes
{
	[DataContract]
	public class Heart : KinematicNode
	{
		[DataMember] public float moveSpeed = 2f;
		[DataMember] public float moveTime = 1f;
		[DataMember] public float moveDelay = 1f;

		Vector2 _direction;
		float _timeMoving;

		public Heart()
		{
			SetCollider(new HitboxCollider(Vector2.one));
			layer = new LayerMask("Heart");
		}

		protected override void Init()
		{
			NewDir();
			position = new Vector2(3);

			base.Init();
		}

		protected override void Tick()
		{
			_timeMoving += Time.tickTime;
			if (_timeMoving < moveTime)
			{
				MoveAndSlide(_direction * moveSpeed * Time.tickTime);
			}
			else if (_timeMoving > moveTime + moveDelay)
			{
				_timeMoving = 0;
				NewDir();
			}

			base.Tick();
		}

		void NewDir() => _direction = Random.UnitVector();
	}
}