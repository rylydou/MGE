using System;
using System.Collections.Generic;

namespace MGE.UI;

public abstract class UIWidget
{
	[Prop] public List<string> classes = new();

	[Prop] public Color foregroundColor;
	[Prop] public Color backgroundColor;

	[Prop] public string? id;

	public bool clipContent = false;

	public UIContainer? parent { get; private set; }
	public UICanvas? canvas { get; internal set; }

	public virtual bool isIntractable => false;

	public bool isHovered { get => isIntractable && canvas?.hoveredWidget == this; }

	protected virtual void RegisterCallbacks()
	{
		onMeasureChanged += OnMeasureChanged;
	}

	#region Widget Management

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

	#endregion Widget Management

	#region Layout

	internal Vector2<float> _layoutFlashTime;

	public Vector2<UISizing> actualSizing
	{
		get
		{
			var result = new Vector2<UISizing>();
			for (int i = 0; i < 2; i++)
			{
				switch (_sizing[i])
				{
					case UISizing.Fix:
						result[i] = UISizing.Fix;
						break;
					case UISizing.Hug:
						break;
					case UISizing.Fill:
						if (parent is not null && parent.sizing[i] == UISizing.Hug)
						{
							result[i] = UISizing.Fix;
						}
						else
						{
							result[i] = UISizing.Fill;
						}
						break;
				}
			}
			return result;
		}
	}

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
					// switch (value[i])
					// {
					// 	case UISizing.Fix:
					// 		_actualSize[i] = _fixedSize[i];
					// 		break;
					// 	case UISizing.Hug:
					// 		break;
					// 	case UISizing.Fill:
					// 		break;
					// }

					_sizing[i] = value[i];
					OnPropertiesChanged(i);
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
						OnPropertiesChanged(i);
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
		_layoutFlashTime[dir] = 1f;

		OnMeasureChanged(dir);

		return true;
	}

	protected bool SetMySize(int dir, int targetSize)
	{
		if (_actualSize[dir] == targetSize) return false;

		_actualSize[dir] = targetSize;
		_layoutFlashTime[dir] = 1f;

		parent?.ChildMeasureChanged(dir, this);

		return true;
	}

	internal virtual void OnPropertiesChanged(int dir)
	{
		_layoutFlashTime[dir] = 1f;

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

	public Action<int> onMeasureChanged = (dir) => { };
	protected virtual void OnMeasureChanged(int dir) { }

	#endregion Layout

	internal virtual void DoRender(Batch2D batch)
	{
		_layoutFlashTime[0] -= Time.rawDelta;
		_layoutFlashTime[1] -= Time.rawDelta;

		if (_actualSize.x <= 0 || _actualSize.y <= 0) return;

		Render(batch);

		if (canvas!.enableDebug)
		{
			if (this is not UICanvas)
			{
				if (parent is UICanvas)
				{
					batch.SetBox(absoluteRect, new(0x303030FF));
				}
				else
				{
					var pressed = isHovered && App.input.mouse.Down(MouseButtons.Left);

					batch.SetBox(absoluteRect, pressed ? new(0xFFFFFFFF) : new(0x404040FF));
					var shadowRect = new Rect(absolutePosition.x, absolutePosition.y + actualSize.y, actualSize.x, 1);
					batch.SetBox(shadowRect, new(0x00000040));

					if (isHovered)
					{
						batch.SetRect(absoluteRect, -1, new(0x505050FF));
					}
				}
			}

			// Horizontal
			if (_layoutFlashTime[0] > 0)
				batch.SetLine(new(absoluteRect.left, absoluteRect.center.y), new(absoluteRect.right, absoluteRect.center.y), 1, Color.LerpClamped(Color.clear, Color.green, _layoutFlashTime[0]));

			// Vertical
			if (_layoutFlashTime[1] > 0)
				batch.SetLine(new(absoluteRect.center.x, absoluteRect.top), new(absoluteRect.center.x, absoluteRect.bottom), 1, Color.LerpClamped(Color.clear, Color.green, _layoutFlashTime[1]));
		}
	}

	protected virtual void Render(Batch2D batch) { }
}
