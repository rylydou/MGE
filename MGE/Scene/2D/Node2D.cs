using System.Collections.Generic;

namespace MGE;

public class Node2D : CanvasItem
{
	#region Local

	bool _localInvalid = true;

	Transform2D _transform = Transform2D.identity;
	public Transform2D transform
	{
		get => GetTransform();
		set
		{
			if (_transform == value) return;
			_transform = value;

			_position = _transform.origin;
			_rotation = _transform.rotation;
			_scale = _transform.scale;

			_localInvalid = false;
			GlobalInvalid();
			if (isInScene) onTransformChanged();
		}
	}
	public override Transform2D GetTransform()
	{
		if (_localInvalid) _transform = Transform2D.CreateMatrix(_position, Vector2.zero, _scale, _rotation);
		return _transform;
	}

	Vector2 _position = new(0.0f, 0.0f);
	public Vector2 position
	{
		get => _position;
		set
		{
			if (_position == value) return;
			_position = value;

			// _transform.origin = _position;

			_localInvalid = true;
			GlobalInvalid();
			if (isInScene) onTransformChanged();
		}
	}

	float _rotation = 0;
	public float rotation
	{
		get => _rotation;
		set
		{
			if (_rotation == value) return;
			_rotation = value;

			// _transform.rotation = _rotation;

			_localInvalid = true;
			GlobalInvalid();
			if (isInScene) onTransformChanged();
		}
	}

	Vector2 _scale = Vector2.one;
	public Vector2 scale
	{
		get => _scale;
		set
		{
			if (_scale == value) return;
			_scale = value;

			// _transform.scale = _scale;

			_localInvalid = true;
			GlobalInvalid();
			if (isInScene) onTransformChanged();
		}
	}

	#endregion Local

	#region Global

	public void SetGlobalTransform(Transform2D transform)
	{
		if (TryGetParentItem(out var pi))
		{
			var inv = pi.GetGlobalTransform().AffineInverse();
			this.transform = inv * transform;
		}
		else
		{
			this.transform = transform;
		}
	}

	public Vector2 globalPosition
	{
		get => GetGlobalTransform().origin;
		set
		{
			if (TryGetParentItem(out var pi))
			{
				var inv = pi.GetGlobalTransform().AffineInverse();
				position = inv * value;
			}
			else
			{
				position = value;
			}
		}
	}

	public float globalRotation
	{
		get => GetGlobalTransform().rotation;
		set
		{
			if (TryGetParentItem(out var pi))
			{
				var parentGlobalRot = pi.GetGlobalTransform().rotation;
				rotation = value - parentGlobalRot;
			}
			else
			{
				rotation = value;
			}
		}
	}

	public Vector2 globalScale
	{
		get => GetGlobalTransform().scale;
		set
		{
			if (TryGetParentItem(out var pi))
			{
				var parentGlobalScale = pi.GetGlobalTransform().scale;
				scale = value / parentGlobalScale;
			}
			else
			{
				scale = value;
			}
		}
	}

	#endregion Global

	#region Utils

	public Vector2 ToLocal(Vector2 globalPoint)
	{
		return GetGlobalTransform().AffineInverse() * globalPoint;
	}

	public Vector2 ToGlobal(Vector2 localPoint)
	{
		return GetGlobalTransform() * localPoint;
	}

	public void LookAt(Vector2 globalPoint)
	{
		rotation += GetAngleTo(globalPoint);
	}

	public float GetAngleTo(Vector2 globalPoint)
	{
		return (ToLocal(globalPoint) * scale).angle;
	}

	public Node2D? Closest(IEnumerable<Node2D> list)
	{
		Node2D? closest = null;
		var closestDistanceSqr = float.PositiveInfinity;

		foreach (var body in list)
		{
			var distanceSqr = Vector2.DistanceSqr(body.globalPosition, body.globalPosition);
			if (distanceSqr >= closestDistanceSqr) continue;
			closest = body;
			closestDistanceSqr = distanceSqr;
		}

		return closest;
	}

	#endregion Utils
}
