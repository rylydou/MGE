using System.Collections.Generic;

namespace MGE.UI;

public abstract class UIWidget
{
	[Prop] public List<string> classes = new();

	[Prop] public Color foregroundColor;
	[Prop] public Color backgroundColor;

	[Prop] public string? id;

	public UIContainer? parent { get; private set; }
	public UICanvas? canvas { get; internal set; }

	#region Layout

	[Prop] Vector2<UISizing> _sizing;
	public Vector2<UISizing> sizing
	{
		get => _sizing;
		set
		{
			for (int i = 0; i < 2; i++)
			{
				if (_sizing[i] != value[i])
				{
					_sizing[i] = value[i];
					PropertiesChanged(i);
				}
			}
		}
	}

	[Prop] Vector2Int _fixedSize;
	public Vector2Int fixedSize
	{
		get => _fixedSize;
		set
		{
			if (_fixedSize == value) return;

			for (int i = 0; i < 2; i++)
			{
				// If the parent is null or sizing is set to fix or the parent is hug which treats this to act like fix
				if (parent is null || _sizing[i] == UISizing.Fix || (_sizing[i] == UISizing.Fill && parent.sizing[i] == UISizing.Hug))
					if (_fixedSize[i] != value[i])
					{
						_actualSize[i] = value[i];
						PropertiesChanged(i);
					}

				_fixedSize[i] = value[i];
			}
		}
	}

	[Prop] Vector2Int _positionInFrame;
	public Vector2Int positionInFrame
	{
		get => _positionInFrame;
		set
		{
			if (_positionInFrame == value) return;
			_positionInFrame = value;

			if (parent is UIFrame || this is UICanvas)
			{
				_relativePosition = value;
			}
		}
	}

	Vector2Int _relativePosition;
	public Vector2Int relativePosition => _relativePosition;

	Vector2Int _absolutePosition;
	public Vector2Int absolutePosition => _absolutePosition;
	// public Vector2Int absolutePosition => parent is null ? _relativePosition : parent.absolutePosition + _relativePosition;

	Vector2Int _actualSize;
	public Vector2Int actualSize => _actualSize;

	public RectInt relativeRect => new(_relativePosition, _actualSize);
	public RectInt absoluteRect => new(_absolutePosition, _actualSize);

	#endregion Layout

	public bool clipContent = false;

	internal Vector2<float> _flashTime;

	internal virtual void AttachTo(UIContainer parent)
	{
		this.parent = parent;

		canvas = parent.canvas;

		if (canvas is not null)
		{
			OnAttached();

			parent?.ChildMeasureChanged(0, this);
			parent?.ChildMeasureChanged(1, this);
		}
	}
	protected virtual void OnAttached() { }

	public void SetPosition(int dir, int value)
	{
		if (_relativePosition[dir] == value) return;
		_relativePosition[dir] = value;

		OnPositionChanged(dir);
	}

	internal virtual void OnPositionChanged(int dir)
	{
		if (parent is not null)
		{
			_absolutePosition[dir] = parent._absolutePosition[dir] + _relativePosition[dir];
		}
		else
		{
			_absolutePosition[dir] = _relativePosition[dir];
		}
	}

	public bool SetSize(int dir, int targetSize)
	{
		if (_actualSize[dir] == targetSize) return false;

		_actualSize[dir] = targetSize;
		_flashTime[dir] = 1f;

		OnMeasureChanged(dir);

		return true;
	}

	protected bool SetMySize(int dir, int targetSize)
	{
		if (_actualSize[dir] == targetSize) return false;

		_actualSize[dir] = targetSize;
		_flashTime[dir] = 1f;

		parent?.ChildMeasureChanged(dir, this);

		return true;
	}

	internal virtual void PropertiesChanged(int dir)
	{
		_flashTime[dir] = 1f;

		// parent?.ChildMeasureChanged(dir, this);
		// OnMeasureChanged(dir);

		if (parent is not null)
		{
			parent.ChildMeasureChanged(dir, this);
		}
		else
		{
			OnMeasureChanged(dir);
		}
	}

	protected virtual void OnMeasureChanged(int dir) { }

	internal virtual void DoRender(Batch2D batch)
	{
		_flashTime[0] -= Time.rawDelta;
		_flashTime[1] -= Time.rawDelta;

		if (_actualSize.x <= 0 || _actualSize.y <= 0) return;

		Render(batch);

		if (canvas!.enableDebug)
		{
			if (this is not UICanvas)
			{
				// batch.HollowRect(absoluteRect, 1, Colors.white);
				if (parent is UICanvas)
				{
					batch.Rect(absoluteRect, new(0x333333));
				}
				else
				{
					batch.Rect(absoluteRect, new(0x444444));
					var shadowRect = new Rect(absolutePosition.x, absolutePosition.y + actualSize.y, actualSize.x, 1);
					batch.Rect(shadowRect, Color.black.WithAlpha(64));

					if (absoluteRect.Contains(App.window.mouse))
					{
						batch.HollowRect(absoluteRect, -1, Color.white.WithAlpha(64));

						if (App.input.mouse.Down(MouseButtons.Left))
						{
							batch.Rect(absoluteRect, new(0xffffff));
						}
					}
				}
			}

			// Horizontal
			if (_flashTime[0] > 0)
				batch.Line(new(absoluteRect.left, absoluteRect.center.y), new(absoluteRect.right, absoluteRect.center.y), 1, Color.LerpClamped(Color.clear, Color.magenta, _flashTime[0]));

			// Vertical
			if (_flashTime[1] > 0)
				batch.Line(new(absoluteRect.center.x, absoluteRect.top), new(absoluteRect.center.x, absoluteRect.bottom), 1, Color.LerpClamped(Color.clear, Color.magenta, _flashTime[1]));
		}
	}

	protected virtual void Render(Batch2D batch) { }
}
