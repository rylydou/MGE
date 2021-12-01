namespace MGE
{
	public class TransformNode : Node
	{
		public bool _needsAbsoluteUpdate = true;
		public bool _needsLocalUpdate = true;

		Vector2 _localPosition = Vector2.zero;
		[Prop]
		public Vector2 position
		{
			get => _localPosition;
			set
			{
				if (_localPosition != value)
				{
					// onLocalPositionChanged.Invoke();
					// SafelyCallFunction(OnLocalPositionChanged);
					_localPosition = value;
					SetNeedsLocalUpdate();
				}
			}
		}

		Vector2 _absolutePosition;
		public Vector2 absolutePosition => UpdateAbsoluteAndGet(ref _absolutePosition);

		float _localRotation = 0;
		[Prop]
		public float rotation
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

		float _absoluteRotation;
		public float absoluteRotation => UpdateAbsoluteAndGet(ref _absoluteRotation);

		Vector2 _localScale = Vector2.one;
		[Prop]
		public Vector2 scale
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

		Vector2 _absoluteScale;
		public Vector2 absoluteScale => UpdateAbsoluteAndGet(ref _absoluteScale);

		Matrix _local;
		public Matrix local => UpdateLocalAndGet(ref _absolute);

		Matrix _absolute;
		public Matrix absolute => UpdateAbsoluteAndGet(ref _absolute);

		Matrix _invertAbsolute;
		public Matrix invertAbsolute => UpdateAbsoluteAndGet(ref _invertAbsolute);

		// public Event onLocalPositionChanged = new Event();
		// public Event onAbsPositionChanged = new Event();

		#region Utils

		public void ToLocalPosition(ref Vector2 absolute, out Vector2 local) => Vector2.Transform(ref absolute, ref _invertAbsolute, out local);

		public void ToAbsolutePosition(ref Vector2 local, out Vector2 absolute) => Vector2.Transform(ref local, ref _absolute, out absolute);

		public Vector2 ToLocalPosition(Vector2 absolute)
		{
			ToLocalPosition(ref absolute, out var result);
			return result;
		}

		public Vector2 ToAbsolutePosition(Vector2 local)
		{
			ToAbsolutePosition(ref local, out var result);
			return result;
		}

		#endregion

		#region Transformations

		void SetNeedsLocalUpdate()
		{
			_needsLocalUpdate = true;
			SetNeedsAbsoluteUpdate();
		}

		void SetNeedsAbsoluteUpdate()
		{
			_needsAbsoluteUpdate = true;

			foreach (var child in this.Where<Node, TransformNode>())
			{
				child.SetNeedsAbsoluteUpdate();
			}
		}

		void UpdateLocal()
		{
			// Log("Updating Local...");

			var result = Matrix.CreateScale(scale.x, scale.y, 1);
			result *= Matrix.CreateRotationZ(rotation);
			result *= Matrix.CreateTranslation(position.x, position.y, 0);
			_local = result;

			_needsLocalUpdate = false;
		}

		void UpdateAbsolute()
		{
			if (parents.First<Node, TransformNode>(out var t))
			{
				// Log("Updating Absolute based on Parent...");

				var parentAbsolute = t.absolute;
				Matrix.Multiply(ref _local, ref parentAbsolute, out _absolute);
				_absoluteScale = t.absoluteScale * scale;
				_absoluteRotation = t.absoluteRotation + rotation;
				_absolutePosition = Vector2.zero;
				ToAbsolutePosition(ref _absolutePosition, out _absolutePosition);
			}
			else
			{
				// Log("Updating Absolute based on Local...");

				_absolute = _local;
				_absoluteScale = _localScale;
				_absoluteRotation = _localRotation;
				_absolutePosition = _localPosition;
			}

			Matrix.Invert(ref _absolute, out _invertAbsolute);

			_needsAbsoluteUpdate = false;
		}

		T UpdateLocalAndGet<T>(ref T field)
		{
			if (_needsLocalUpdate) UpdateLocal();
			return field;
		}

		T UpdateAbsoluteAndGet<T>(ref T field)
		{
			if (_needsLocalUpdate) UpdateLocal();
			if (_needsAbsoluteUpdate) UpdateAbsolute();
			return field;
		}

		#endregion

		// protected virtual void OnLocalPositionChanged() { }
	}
}
