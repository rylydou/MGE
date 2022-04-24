using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MGE;

public abstract class CanvasItem : Node
{
	public bool visible = true;

	public abstract Transform2D GetTransform();

	bool _globalInvalid;
	public void GlobalInvalid()
	{
		_globalInvalid = true;
		GetChildren<CanvasItem>().ForEach(ci => ci.GlobalInvalid());
	}

	Transform2D _globalTransform = Transform2D.identity;
	public virtual Transform2D GetGlobalTransform()
	{
		if (_globalInvalid)
		{
			if (TryGetParentItem(out var pi))
			{
				_globalTransform = pi._globalTransform * GetTransform();
			}
			else
			{
				_globalTransform = GetTransform();
			}

			_globalInvalid = false;
		}

		return _globalTransform;
	}

	public Vector2 right => Vector2.TransformNormal(Vector2.right, GetGlobalTransform());

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

		onExitScene += () => _globalInvalid = true;

		onTransformChanged += OnTransformChanged;
		onUpdate = delta =>
		{
			Update(delta);

			if (visible)
			{
				var batch = Batch2D.current;
				batch.PushMatrix(GetGlobalTransform(), false);
				onDraw(batch);
				batch.PopMatrix();
			}

			GetChildren<Node>().ToArray().ForEach(c => c.onUpdate(delta));
		};

		onDraw += Render;
	}

	public Action onTransformChanged = () => { };
	protected virtual void OnTransformChanged() { }

	public delegate void DrawDelegate(Batch2D batch);
	public DrawDelegate onDraw = (batch) => { };
	protected virtual void Render(Batch2D batch) { }
}
