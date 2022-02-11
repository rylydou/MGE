namespace MGE.UI;

public abstract class UIWidget
{
	public string? id;

	public UIContainer? parent { get; private set; }
	public UICanvas? canvas { get; internal set; }

	#region Layout

	Vec2<UIResizing> _resizing;
	public Vec2<UIResizing> resizing
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

	float flashTime;
	Color flashColor;

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
		flashTime = 1f;
		flashColor = Color.green;

		OnMeasureChanged();
	}

	internal virtual void PropertiesChanged()
	{
		flashTime = 1f;
		flashColor = Color.yellow;
		parent?.ChildMeasureChanged(this);

		OnMeasureChanged();
	}

	protected virtual void OnMeasureChanged() { }

	internal virtual void DoRender()
	{
		if (!rect.isProper) return;

		RectInt? oldScissor = null;
		if (clipContent)
		{
			oldScissor = GFX.GetScissor();
			GFX.SetScissor(_rect);
		}

		Render();

		flashTime -= Time.drawTime;
		if (canvas!.enableDebug)
		{
			GFX.DrawBox(rect, new Color(1f, 0.1f));
			GFX.DrawRect(rect, Color.LerpClamped(Color.green.translucent, flashColor, flashTime), -1);

			Font.normal.DrawString(GetType().Name, rect.topLeft, Color.white);
		}

		if (clipContent)
		{
			GFX.DrawBatches();
			GFX.SetScissor(oldScissor);
		}
	}

	protected virtual void Render() { }

	// /// <summary>
	// /// The width is guaranteed to be set but the height might also change in responce, eg. a label running out of space horizontally so needing another line.
	// /// When overriding call the base function last.
	// /// </summary>
	// /// <param name="width">The width of the widget</param>
	// public virtual void RequestWidth(int width)
	// {
	// 	_rect.width = width;
	// }

	// public void SetHeight(int height)
	// {
	// 	_rect.height = height;
	// }
}
