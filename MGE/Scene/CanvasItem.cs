using System;
using System.Diagnostics.CodeAnalysis;

namespace MGE;

public abstract class CanvasItem : Node
{
	public bool visible = true;

	public abstract Transform2D GetTransform();

	public bool globalInvalid { get; protected set; }
	Transform2D _globalTransform = Transform2D.identity;
	public virtual Transform2D GetGlobalTransform()
	{
		if (globalInvalid)
		{
			if (TryGetParentItem(out var pi))
			{
				_globalTransform = pi._globalTransform * GetTransform();
			}
			else
			{
				_globalTransform = GetTransform();
			}

			globalInvalid = false;
		}

		return _globalTransform;
	}

	public bool TryGetParentItem([MaybeNullWhen(false)] out CanvasItem pi)
	{
		if (parent is CanvasItem canvasItem)
		{
			pi = canvasItem;
			return true;
		}
		pi = null;
		return false;
	}

	protected override void RegisterCallbacks()
	{
		base.RegisterCallbacks();

		onExitScene += () => globalInvalid = true;

		onTransformChanged += OnTransformChanged;
		onTransformChanged += () =>
		{
			globalInvalid = true;
		};

		onUpdate += (delta) =>
		{
			if (!visible) return;

			var batch = Batch2D.current;
			batch.PushMatrix(GetTransform());
			onDraw(batch);
			batch.PopMatrix();
		};
		onDraw += Draw;
	}

	public Action onTransformChanged = () => { };
	protected virtual void OnTransformChanged() { }

	public delegate void DrawDelegate(Batch2D batch);
	public DrawDelegate onDraw = (batch) => { };
	protected virtual void Draw(Batch2D batch) { }
}
