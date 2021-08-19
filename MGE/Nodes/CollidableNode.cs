using System;
using System.Runtime.Serialization;

namespace MGE
{
	[DataContract]
	public class CollidableNode : Transform
	{
		[DataMember] public Collider collider { get; private set; }
		[DataMember] public LayerMask layer = new LayerMask();

		public PhysicsWorld world { get; internal set; }

		public Action<CollidableNode> onOverlap = (a) => { };

		public CollidableNode() { }

		public CollidableNode(Collider collider)
		{
			this.collider = collider;
		}

		protected override void DebugDraw()
		{
			collider?.Draw();

			base.DebugDraw();
		}

		public virtual void OnOverlap(CollidableNode other) { }

		public void SetCollider(Collider collider)
		{
			this.collider = collider;
			this.collider.node = this;
		}

		[OnDeserialized]
		void OnDeserialized(StreamingContext context)
		{
			if (collider is object)
				collider.node = this;
		}
	}
}