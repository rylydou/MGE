using Gtk;

namespace MGE.Editor
{
	class MainWindow : Window
	{
		CssProvider resetProvider;
		CssProvider styleProvider;

		public MainWindow() : base("Mangrove Game Engine Editor DEBUG - Untitled Scene.mges")
		{
			SetDefaultSize(1280, 720);
			SetPosition(WindowPosition.Center);
			Maximize();

			DeleteEvent += delegate { Application.Quit(); };

			// ModifyBg(StateType.Normal, Colors.@base);

			var fix = new Box(Orientation.Vertical, 4);

			fix.Add(new Button("Button"));
			fix.Add(new Button(Stock.Yes));
			fix.Add(new Button(Stock.Ok));
			fix.Add(new Button(Stock.Close));

			Add(fix);
			ShowAll();

			resetProvider = new CssProvider();
			resetProvider.LoadFromPath("styles/reset.css");
			ApplyCss(this, resetProvider, 800);

			styleProvider = new CssProvider();
			styleProvider.LoadFromPath("styles/styles.css");
			ApplyCss(this, styleProvider, int.MaxValue);
		}

		private void ApplyCss(Widget widget, CssProvider provider, uint priority)
		{
			widget.StyleContext.AddProvider(provider, priority);

			if (widget is Container container)
			{
				foreach (var child in container.Children)
				{
					ApplyCss(child, provider, priority);
				}
			}
		}
	}
}
