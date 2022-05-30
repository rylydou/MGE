using MGE.UI;

namespace UIDemo;

public class MainModule : Module
{
	UICanvas canvas = new()
	{
		enableDebug = true,
		direction = UIFlow.Vertical,
		spacing = 6,
		padding = new(12),
	};

	UIContainer _container;
	UIContainer _item => (_container.widgets[_index] as UIContainer)!;
	int _index;

	Rect _lastItemRect;
	float _rectTransitionTime;
	float _rectTransitionDuration = 1f / 30;

	public MainModule()
	{
		_container = canvas;
	}

	protected override void Startup()
	{
		App.window.onRender += Render;
	}

	protected override void Update(float delta)
	{
		_rectTransitionTime -= delta;

		var kb = App.input.keyboard;

		canvas.fixedSize = App.window.size;

		if (kb.Repeated(Keys.D))
		{
			if (_index < _container.widgets.Count - 1)
			{
				_lastItemRect = _item.absoluteRect;
				_rectTransitionTime = _rectTransitionDuration;

				_index++;
			}
		}

		if (kb.Repeated(Keys.A))
		{
			if (_index > 0)
			{
				_lastItemRect = _item.absoluteRect;
				_rectTransitionTime = _rectTransitionDuration;

				_index--;
			}
		}

		if (kb.Pressed(Keys.Escape))
		{
			if (_container == canvas) return;
			if (_container.widgets.Count > 0)
			{
				_lastItemRect = _item.absoluteRect;
				_rectTransitionTime = _rectTransitionDuration;
			}

			_index = _container.parent!.widgets.IndexOf(_container);
			_container = _container.parent;
		}

		if (kb.Pressed(Keys.Tab))
		{
			var item = new UIBox()
			{
				padding = new(6),
				spacing = 6,
				fixedSize = new(28, 28),
				sizing = new(UISizing.Fix, UISizing.Fix),
				direction = UIFlow.Horizontal,
			};

			_container.InsertChild(_index, item);
		}

		if (kb.Pressed(Keys.Space))
		{
			if (_container.widgets.Count > 0)
			{
				_lastItemRect = _item.absoluteRect;
				_rectTransitionTime = _rectTransitionDuration;

				_container = _item;
				_index = 0;
			}
		}

		if (kb.Pressed(Keys.S))
		{
			if (_container is UIBox box)
			{
				box.direction = box.direction == UIFlow.Horizontal ? UIFlow.Vertical : UIFlow.Horizontal;
			}
		}

		if (kb.Pressed(Keys.Q))
		{
			_container.sizing = _container.sizing.With(0, _container.sizing[0] switch
			{
				UISizing.Fix => UISizing.Hug,
				UISizing.Hug => UISizing.Fill,
				_ => UISizing.Fix,
			});
		}

		if (kb.Pressed(Keys.W))
		{
			_container.sizing = _container.sizing.With(1, _container.sizing[1] switch
			{
				UISizing.Fix => UISizing.Hug,
				UISizing.Hug => UISizing.Fill,
				_ => UISizing.Fix,
			});
		}

		var mod = kb.shift ? -1 : 1;

		if (kb.Repeated(Keys.E))
		{
			_container.fixedSize = _container.fixedSize.With(0, _container.fixedSize[0] + mod);
		}

		if (kb.Repeated(Keys.R))
		{
			_container.fixedSize = _container.fixedSize.With(1, _container.fixedSize[1] + mod);
		}
	}

	void Render(Window window)
	{
		var batch = new Batch2D();

		canvas.RenderCanvas(batch);

		var text =
			_container.sizing + "\n" +
			_container.fixedSize + "\n" +
		"";

		if (_container is UIBox box)
		{
			text +=
				box.direction + "\n" +
			"";
		}

		batch.DrawString(App.content.font, text, new(8, 488), Colors.white, 16);

		batch.HollowRect(((Rect)_container.absoluteRect).Expanded(2), 1, new(0x22AAEE));
		if (_container.widgets.Count > 0)
		{
			var rect = Rect.LerpClamped(_item.absoluteRect, _lastItemRect, _rectTransitionTime / _rectTransitionDuration);
			batch.HollowRect(rect.Expanded(2), 1, new(0x22AAEE));
		}

		App.graphics.Clear(window, new Color(0x222222));
		batch.Render(window);
	}
}
