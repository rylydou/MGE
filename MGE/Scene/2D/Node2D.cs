using System;

namespace MGE;

public abstract class Node2D : CanvasItem
{
	#region Local

	Vector2 _position = new(0.0f, 0.0f);
	public Vector2 position
	{
		get => _position;
		set
		{
			if (_position == value) return;

			_position = value;
			_transform.origin = _position;
			if (isInScene) onTransformChanged();
		}
	}

	float _rotation;
	public float rotation
	{
		get => _rotation;
		set
		{
			if (_rotation == value) return;
			_rotation = value;

			_rotation = value;
			_transform.rotation = _rotation;
			if (isInScene) onTransformChanged();
		}
	}

	Vector2 _scale;
	public Vector2 scale
	{
		get => _scale;
		set
		{
			if (_scale == value) return;
			_scale = value;

			_scale = value;
			_transform.scale = _scale;
			if (isInScene) onTransformChanged();
		}
	}

	Transform2D _transform;
	public Transform2D transform
	{
		get => _transform;
		set
		{
			if (_transform == value) return;
			_transform = value;

			_position = _transform.origin;
			_rotation = _transform.rotation;
			_scale = _transform.scale;
			if (isInScene) onTransformChanged();
		}
	}

	#endregion Local

	#region Global

	Transform2D _globalTransform;
	public Transform2D globalTransform
	{
		get => _globalTransform;
		set
		{
			var pi = GetParentItem();
			if (pi)
			{
				var inv = pi.globalTransform.AffineInverse();
				transform = inv * value;
			}
			else
			{
				transform = value;
			}
		}
	}

	public Vector2 globalPosition
	{
		get => globalTransform.origin;
		set
		{
			var pi = GetParentItem();
			if (pi)
			{
				var inv = pi.globalTransform.AffineInverse();
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
		get => globalTransform.rotation;
		set
		{
			var pi = GetParentItem();
			if (pi)
			{
				var parentGlobalRot = pi.globalTransform.Rotation;
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
		get => globalTransform.scale;
		set
		{
			var pi = GetParentItem();
			if (pi)
			{
				var parentGlobalScale = pi.globalTransform.Scale;
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
		return globalTransform.AffineInverse() * globalPoint;
	}

	public Vector2 ToGlobal(Vector2 localPoint)
	{
		return globalTransform * localPoint;
	}

	public void LookAt(Vector2 globalPoint)
	{
		rotation += GetAngleTo(globalPoint);
	}

	public float GetAngleTo(Vector2 globalPoint)
	{
		return (ToLocal(globalPoint) * scale).angle;
	}

	#endregion Utils

	public Action onTransformChanged = () => { };
	protected virtual void OnTransformChanged() { }
}
