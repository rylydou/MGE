using System;
using System.Numerics;

namespace MGE;

/// <summary>
/// An interface that implements a 2D Transform
/// </summary>
public interface ITransform2D
{
	/// <summary>
	/// Gets or Sets the World Position of the Transform
	/// </summary>
	Vector2 position { get; set; }

	/// <summary>
	/// Gets or Sets the World Scale of the Transform
	/// </summary>
	Vector2 scale { get; set; }

	/// <summary>
	/// Gets or Sets the Origin of the Transform
	/// </summary>
	Vector2 origin { get; set; }

	/// <summary>
	/// Gets or Sets the World Rotation of the Transform
	/// </summary>
	float rotation { get; set; }
}

/// <summary>
/// A 2D Transform
/// </summary>
public class Transform2D : ITransform2D
{
	Transform2D? _parent = null;
	bool _dirty = true;

	Vector2 _position = Vector2.zero;
	Vector2 _localPosition = Vector2.zero;
	Vector2 _scale = Vector2.one;
	Vector2 _localScale = Vector2.one;
	Vector2 _origin = Vector2.zero;
	float _rotation = 0f;
	float _localRotation = 0f;

	Matrix3x2 _localMatrix = Matrix3x2.Identity;
	Matrix3x2 _worldMatrix = Matrix3x2.Identity;
	Matrix3x2 _worldToLocalMatrix = Matrix3x2.Identity;

	/// <summary>
	/// An action called whenever the Transform is modified
	/// </summary>
	public event Action? onChanged;

	/// <summary>
	/// Gets or Sets the Transform's Parent
	/// Modifying this does not change the World position of the Transform
	/// </summary>
	public Transform2D? parent
	{
		get => _parent;
		set => SetParent(value, true);
	}

	/// <summary>
	/// Gets or Sets the Origin of the Transform
	/// This is useful for setting Rotation Origins for things like Sprites
	/// </summary>
	public Vector2 origin
	{
		get => _origin;
		set
		{
			if (_origin != value)
			{
				_origin = value;
				MakeDirty();
			}
		}
	}

	/// <summary>
	/// Gets or Sets the World Position of the Transform
	/// </summary>
	public Vector2 position
	{
		get
		{
			if (_dirty)
				Update();

			return _position;
		}
		set
		{
			if (_parent == null)
				localPosition = value;
			else
				localPosition = Vector2.Transform(value, worldToLocalMatrix);
		}
	}

	/// <summary>
	/// Gets or Sets the X Component of the World Position of the Transform
	/// </summary>
	public float x
	{
		get => position.x;
		set => position = new Vector2(value, position.y);
	}

	/// <summary>
	/// Gets or Sets the Y Component of the World Position of the Transform
	/// </summary>
	public float y
	{
		get => position.y;
		set => position = new Vector2(position.x, value);
	}

	/// <summary>
	/// Gets or Sets the Local Position of the Transform
	/// </summary>
	public Vector2 localPosition
	{
		get => _localPosition;
		set
		{
			if (_localPosition != value)
			{
				_localPosition = value;
				MakeDirty();
			}
		}
	}

	/// <summary>
	/// Gets or Sets the World Scale of the Transform
	/// </summary>
	public Vector2 scale
	{
		get
		{
			if (_dirty)
				Update();

			return _scale;
		}
		set
		{
			if (_parent == null)
			{
				localScale = value;
			}
			else
			{
				if (_parent.scale.x == 0)
					value.x = 0;
				else
					value.x /= _parent.scale.x;

				if (_parent.scale.y == 0)
					value.y = 0;
				else
					value.y /= _parent.scale.y;

				localScale = value;
			}
		}
	}

	/// <summary>
	/// Gets or Sets the Local Scale of the Transform
	/// </summary>
	public Vector2 localScale
	{
		get => _localScale;
		set
		{
			if (_localScale != value)
			{
				_localScale = value;
				MakeDirty();
			}
		}
	}

	/// <summary>
	/// Gets or Sets the World Rotation of the Transform
	/// </summary>
	public float rotation
	{
		get
		{
			if (_dirty)
				Update();

			return _rotation;
		}
		set
		{
			if (_parent == null)
				localRotation = value;
			else
				localRotation = value - _parent.rotation;
		}
	}

	/// <summary>
	/// Gets or Sets the Local Rotation of the Transform
	/// </summary>
	public float localRotation
	{
		get => _localRotation;
		set
		{
			if (_localRotation != value)
			{
				_localRotation = value;
				MakeDirty();
			}
		}
	}

	/// <summary>
	/// Gets the Local Matrix of the Transform
	/// </summary>
	public Matrix3x2 localMatrix
	{
		get
		{
			if (_dirty)
				Update();
			return _localMatrix;
		}
	}

	/// <summary>
	/// Gets the World Matrix of the Transform
	/// </summary>
	public Matrix3x2 worldMatrix
	{
		get
		{
			if (_dirty)
				Update();
			return _worldMatrix;
		}
	}

	/// <summary>
	/// Gets the World-to-Local Matrix of the Transform
	/// </summary>
	public Matrix3x2 worldToLocalMatrix
	{
		get
		{
			if (_dirty)
				Update();
			return _worldToLocalMatrix;
		}
	}

	/// <summary>
	/// Sets the Parent of this Transform
	/// </summary>
	/// <param name="value">The new Parent</param>
	/// <param name="retainWorldPosition">Whether this Transform should retain its world position when it is transfered to the new parent</param>
	public void SetParent(Transform2D? value, bool retainWorldPosition)
	{
		if (_parent != value)
		{
			// Circular Hierarchy isn't allowed
			// TODO: this only checks 1 parent, instead of the whole tree
			if (value != null && value.parent == this)
				throw new Exception("Circular Transform Heritage is not allowed");

			// Remove our OnChanged listener from the existing parent
			if (_parent != null)
				_parent.onChanged -= MakeDirty;

			// store state
			var position = this.position;
			var scale = this.scale;
			var rotation = this.rotation;

			// update parent
			_parent = value;
			_dirty = true;

			// retain state
			if (retainWorldPosition)
			{
				this.position = position;
				this.scale = scale;
				this.rotation = rotation;
			}

			// Add our OnChanged listener to the new parent
			if (_parent != null)
				_parent.onChanged += MakeDirty;

			// we have changed
			onChanged?.Invoke();
		}
	}

	void Update()
	{
		_dirty = false;

		_localMatrix = CreateMatrix(_localPosition, _origin, _localScale, _localRotation);

		if (_parent == null)
		{
			_worldMatrix = _localMatrix;
			_worldToLocalMatrix = Matrix3x2.Identity;
			_position = _localPosition;
			_scale = _localScale;
			_rotation = _localRotation;
		}
		else
		{
			_worldMatrix = _localMatrix * _parent.worldMatrix;
			Matrix3x2.Invert(_parent._worldMatrix, out _worldToLocalMatrix);
			_position = Vector2.Transform(_localPosition, _parent.worldMatrix);
			_scale = _localScale * _parent.scale;
			_rotation = _localRotation + _parent.rotation;
		}
	}

	void MakeDirty()
	{
		if (!_dirty)
		{
			_dirty = true;
			onChanged?.Invoke();
		}
	}

	/// <summary>
	/// Creates a Matrix3x2 given the Transform Values
	/// </summary>
	public static Matrix3x2 CreateMatrix(in Vector2 position, in Vector2 origin, in Vector2 scale, in float rotation)
	{
		Matrix3x2 matrix;

		if (origin != Vector2.zero)
			matrix = Matrix3x2.CreateTranslation(-origin.x, -origin.y);
		else
			matrix = Matrix3x2.Identity;

		if (scale != Vector2.one)
			matrix *= Matrix3x2.CreateScale(scale.x, scale.y);

		if (rotation != 0)
			matrix *= Matrix3x2.CreateRotation(rotation);

		if (position != Vector2.zero)
			matrix *= Matrix3x2.CreateTranslation(position.x, position.y);

		return matrix;
	}
}
