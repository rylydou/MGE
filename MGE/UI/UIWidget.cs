namespace MGE.UI;

public abstract class UIWidget
{
	public string? id;

	public UIContainer? parent { get; private set; }
	public UICanvas? canvas { get; internal set; }

	#region Layout

	Vector2<UIResizing> _resizing;
	public Vector2<UIResizing> resizing
	{
		get => _resizing;
		set
		{
			if (_resizing == value) return;
			_resizing = value;

			PropertiesChanged();
		}
	}

	Vector2Int _fixedSize;
	public Vector2Int fixedSize
	{
		get => _fixedSize;
		set
		{
			if (_fixedSize == value) return;
			_fixedSize = value;

			var updated = false;
			if (_resizing.horizontal == UIResizing.Fixed)
			{
				updated = true;
				_rect.width = value.x;
			}

			if (_resizing.vertical == UIResizing.Fixed)
			{
				updated = true;
				_rect.height = value.y;
			}

			if (updated)
			{
				PropertiesChanged();
			}

			// Debug.Log(fixedSize, GetType().Name);
		}
	}

	Vector2Int _position;
	public Vector2Int position
	{
		get => _position;
		set
		{
			if (_position == value) return;
			_position = value;

			if (parent is UIFrame || this is UICanvas)
			{
				_rect.position = value;
			}
		}
	}

	internal RectInt _rect;
	public RectInt rect { get => _rect; }

	internal float _flashTime;
	internal Color _flashColor;

	#endregion Layout

	public bool clipContent = false;

	internal virtual void AttachTo(UIContainer parent)
	{
		this.parent = parent;

		canvas = parent.canvas;

		if (canvas is not null)
		{
			OnAttached();
			parent?.ChildMeasureChanged(this);
		}
	}
	protected virtual void OnAttached() { }

	internal virtual void ParentChangedMeasure()
	{
		if (!(_flashTime > 0 && (_flashColor == Color.magenta || _flashColor == Color.yellow)))
		{
			_flashTime = 1f;
			_flashColor = Color.green;
		}

		OnMeasureChanged();
	}

	internal virtual void PropertiesChanged()
	{
		_flashTime = 1f;
		_flashColor = Color.magenta;
		parent?.ChildMeasureChanged(this);

		OnMeasureChanged();
	}

	protected virtual void OnMeasureChanged() { }

	internal virtual void DoRender(Batch2D batch)
	{
		_flashTime -= Time.rawDelta;

		if (!rect.isProper) return;

		if (canvas!.enableDebug)
		{
			if (this is not UICanvas)
			{
				batch.Rect(rect, new Color(0xFFFFFF11));
			}
		}

		Render(batch);

		if (canvas!.enableDebug)
		{
			batch.HollowRect(rect, -1, Color.LerpClamped(_flashColor.WithAlpha(0), _flashColor, _flashTime));

			// Font.normal.DrawString(GetType().Name, rect.topLeft, Color.white);
		}
	}

	protected virtual void Render(Batch2D batch) { }
}
