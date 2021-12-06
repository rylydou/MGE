namespace MGE;

public class TransformNode : Node
{
	bool _needsWorldUpdate = true;
	bool _needsLocalUpdate = true;

	[Prop] Vector2 _localPosition = Vector2.zero;
	public Vector2 localPosition
	{
		get => _localPosition;
		set
		{
			if (_localPosition != value)
			{
				_localPosition = value;
				SetNeedsLocalUpdate();
			}
		}
	}
	Vector2 _worldPosition;
	public Vector2 worldPosition => UpdateWorldAndGet(ref _worldPosition);

	[Prop] float _localRotation = 0;
	public float localRotation
	{
		get => _localRotation;
		set
		{
			if (_localRotation != value)
			{
				_localRotation = value;
				SetNeedsLocalUpdate();
			}
		}
	}
	float _worldRotation;
	public float worldRotation => UpdateWorldAndGet(ref _worldRotation);

	[Prop] Vector2 _localScale = Vector2.one;
	public Vector2 localScale
	{
		get => _localScale;
		set
		{
			if (_localScale != value)
			{
				_localScale = value;
				SetNeedsLocalUpdate();
			}
		}
	}
	Vector2 _worldScale;
	public Vector2 worldScale => UpdateWorldAndGet(ref _worldScale);

	Matrix _localTransform;
	public Matrix localTransform => UpdateLocalAndGet(ref _worldTransform);

	Matrix _worldTransform;
	public Matrix worldTransform => UpdateWorldAndGet(ref _worldTransform);

	Matrix _invertWorldTransform;
	public Matrix invertWorld => UpdateWorldAndGet(ref _invertWorldTransform);

	#region Utils

	public void ToLocalPosition(ref Vector2 world, out Vector2 local) => Vector2.Transform(ref world, ref _invertWorldTransform, out local);

	public void ToWorldPosition(ref Vector2 local, out Vector2 world) => Vector2.Transform(ref local, ref _worldTransform, out world);

	public Vector2 ToLocalPosition(Vector2 world)
	{
		ToLocalPosition(ref world, out var result);
		return result;
	}

	public Vector2 ToWorldPosition(Vector2 local)
	{
		ToWorldPosition(ref local, out var result);
		return result;
	}

	#endregion

	#region Transformations

	void SetNeedsLocalUpdate()
	{
		_needsLocalUpdate = true;
		SetNeedsWorldUpdate();
	}

	void SetNeedsWorldUpdate()
	{
		_needsWorldUpdate = true;

		foreach (var child in GetChildrenRecursive<TransformNode>())
		{
			child.SetNeedsWorldUpdate();
		}
	}

	void UpdateLocal()
	{
		var result = Matrix.CreateScale(localScale.x, localScale.y, 1);
		result *= Matrix.CreateRotationZ(localRotation);
		result *= Matrix.CreateTranslation(localPosition.x, localPosition.y, 0);
		_localTransform = result;

		_needsLocalUpdate = false;
	}

	void UpdateWorld()
	{
		if (TryGetParent<TransformNode>(out var t))
		{
			var parentWorld = t.worldTransform;
			Matrix.Multiply(ref _localTransform, ref parentWorld, out _worldTransform);
			_worldScale = t.worldScale * localScale;
			_worldRotation = t.worldRotation + localRotation;
			_worldPosition = Vector2.zero;
			ToWorldPosition(ref _worldPosition, out _worldPosition);
		}
		else
		{
			_worldTransform = _localTransform;
			_worldScale = _localScale;
			_worldRotation = _localRotation;
			_worldPosition = _localPosition;
		}

		Matrix.Invert(ref _worldTransform, out _invertWorldTransform);

		_needsWorldUpdate = false;
	}

	T UpdateLocalAndGet<T>(ref T field)
	{
		if (_needsLocalUpdate) UpdateLocal();
		return field;
	}

	T UpdateWorldAndGet<T>(ref T field)
	{
		if (_needsLocalUpdate) UpdateLocal();
		if (_needsWorldUpdate) UpdateWorld();
		return field;
	}

	#endregion
}
