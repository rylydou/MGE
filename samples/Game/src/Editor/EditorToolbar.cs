namespace Game.Editor;

public class EditorHotbar
{
	public float itemSize = 18;
	public float spacing = 1;
	public float gap = 2;

	public float sizeX => (editor.items.Count * itemSize) + ((editor.items.Count - 1) * spacing);

	public EditorScreen editor;

	bool _isOnBottom;
	bool _visibleOnBottom;

	bool _transIn;
	float _transTime;

	float _transOutDuration = 0.1f;
	float _transInDuration = 0.3f;

	public EditorHotbar(EditorScreen editor)
	{
		this.editor = editor;
	}

	public void Update(float delta)
	{
		if (!_transIn && _transTime >= _transOutDuration)
		{
			_visibleOnBottom = _isOnBottom;
			_transIn = true;
			_transTime = 0;
		}

		_transTime = Mathf.MoveTowards(_transTime, _transIn ? _transInDuration : _transOutDuration, delta);
	}

	public void Render(Batch2D batch)
	{
		if (editor.controllerMode || App.window.mouseOver)
		{
			if (_isOnBottom && editor.cursor.y > Main.screenSize.y - itemSize * 2)
			{
				_isOnBottom = false;
				_transTime = 0;
				_transIn = false;
			}

			if (!_isOnBottom && editor.cursor.y < itemSize * 2)
			{
				_isOnBottom = true;
				_transTime = 0;
				_transIn = false;
			}
		}

		var ease = _transIn ? Ease.quadOut : Ease.quadIn;
		var t = ease(_transTime / (_transIn ? _transInDuration : _transOutDuration));
		var offsetY = _transIn ?
			Mathf.LerpClamped(itemSize + gap + 1, 0, t) :
			Mathf.LerpClamped(0, itemSize + gap + 1, t);

		var position = new Vector2((Main.screenSize.x - sizeX) / 2, gap - offsetY);
		if (_visibleOnBottom)
		{
			position.y = Main.screenSize.y - itemSize - gap + offsetY;
		}

		batch.PushMatrix(position);

		for (int i = 0; i < editor.items.Count; i++)
		{
			var item = editor.items[i];

			var itemRect = new Rect(i * (itemSize + spacing), 0, itemSize, itemSize);

			// Background
			batch.SetBox(itemRect, Main.fg.WithAlpha(0.5f));
			batch.SetRect(itemRect, -1, Main.fg);

			// Icon
			batch.Draw(item.GetEditorIcon(), itemRect.center, Color.white);

			// Selected item border
			if (editor.selection == i)
			{
				batch.SetRect(itemRect, 1, new(0xdff6f5FF));
			}
		}

		batch.PopMatrix();
	}
}
