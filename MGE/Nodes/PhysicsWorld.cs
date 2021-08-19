using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MGE
{
	[DataContract]
	public class PhysicsWorld : Node
	{
		[DataContract]
		public class Layer
		{
			public readonly string name;
			public GridCollider grid;
			public List<CollidableNode> nodes = new List<CollidableNode>();

			public Layer() { }

			public Layer(string name)
			{
				this.name = name;
			}
		}

		[DataMember] public Vector2 gravity = new Vector2(0, 20);
		[DataMember] public float stepsPerUnit = 1f / 64;

		AutoDictionary<string, Layer> _layers = new AutoDictionary<string, Layer>(l => l.name);

		protected override void OnNodeAdded(Node node)
		{
			if (node is CollidableNode actor)
			{
				// _actors.Add(actor);
				actor.world = this;
				ChangeActorLayer(actor, null, actor.layer);
			}

			base.OnNodeAdded(node);
		}

		protected override void OnNodeRemoved(Node node)
		{
			if (node is CollidableNode actor)
			{
				// _actors.Remove(actor);
				actor.world = null;
				ChangeActorLayer(actor, actor.layer, null);
			}

			base.OnNodeRemoved(node);
		}

		void ChangeActorLayer(CollidableNode actor, LayerMask from, LayerMask to)
		{
			if (from is object)
			{
				foreach (var layer in from)
				{
					_layers[layer].nodes.Remove(actor);
				}
			}

			if (to is object)
			{
				foreach (var layer in to)
				{
					if (_layers.TryGetValue(layer, out var list))
					{
						list.nodes.Add(actor);
						if (actor.collider is GridCollider gc) list.grid = gc;
					}
					else
					{
						var newLayer = new Layer(layer);
						newLayer.nodes.Add(actor);
						if (actor.collider is GridCollider gc) newLayer.grid = gc;
						_layers.Add(newLayer);
					}
				}
			}
		}

		public CollidableNode[] GetActors(Vector2 point, LayerMask layermask)
		{
			var foundActors = new List<CollidableNode>();
			foreach (var layer in layermask)
				foreach (var actor in _layers[layer].nodes)
					if (actor.collider.PointCheck(point))
						foundActors.Add(actor);
			return foundActors.ToArray();
		}
		public CollidableNode[] GetActors(Rect rect, LayerMask layermask)
		{
			var foundActors = new List<CollidableNode>();
			foreach (var layer in layermask)
				foreach (var actor in _layers[layer].nodes)
					if (actor.collider.RectCheck(rect))
						foundActors.Add(actor);
			return foundActors.ToArray();
		}
		public CollidableNode[] GetActors(Vector2 from, Vector2 to, LayerMask layermask)
		{
			var foundActors = new List<CollidableNode>();
			foreach (var layer in layermask)
				foreach (var actor in _layers[layer].nodes)
					if (actor.collider.LineCheck(from, to))
						foundActors.Add(actor);
			return foundActors.ToArray();
		}
		public CollidableNode[] GetActors(Collider collider, LayerMask layermask)
		{
			var foundActors = new List<CollidableNode>();
			foreach (var layer in layermask)
				foreach (var actor in _layers[layer].nodes)
					if (actor.collider.Collide(collider))
						foundActors.Add(actor);
			return foundActors.ToArray();
		}

		public bool Overlaps(Vector2 point, LayerMask layermask)
		{
			foreach (var layer in layermask)
				foreach (var actor in _layers[layer].nodes)
					if (actor.collider.PointCheck(point)) return true;
			return false;
		}
		public bool Overlaps(Rect rect, LayerMask layermask)
		{
			foreach (var layer in layermask)
				foreach (var actor in _layers[layer].nodes)
					if (actor.collider.RectCheck(rect)) return true;
			return false;
		}
		public bool Overlaps(Vector2 from, Vector2 to, LayerMask layermask)
		{
			foreach (var layer in layermask)
				foreach (var actor in _layers[layer].nodes)
					if (actor.collider.LineCheck(from, to)) return true;
			return false;
		}
		public bool Overlaps(Collider collider, LayerMask layermask)
		{
			foreach (var layer in layermask)
				foreach (var actor in _layers[layer].nodes)
					if (actor.collider.Collide(collider)) return true;
			return false;
		}

		public RaycastHit RaycastOnGrid(Vector2 position, Vector2 direction, float distance, LayerMask layermask)
		{
			foreach (var layer in layermask)
			{
				var raycast = _layers[layer].grid.Raycast(position, direction);
				if (raycast is object) return raycast;
			}
			return null;
		}
	}
}