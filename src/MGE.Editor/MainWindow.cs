using System;
using Gtk;

namespace MGE.Editor
{
	class MainWindow : Window
	{
		CssProvider resetProvider;
		CssProvider styleProvider;

		public MainWindow() : base("Mangrove Game Engine Editor DEBUG - Untitled Scene.mges")
		{
			SetPosition(WindowPosition.Center);
			SetDefaultSize(1280, 720);
			SetSizeRequest(1280, 720);
			Maximize();

			KeyPressEvent += KeyPress;

			DeleteEvent += delegate { Application.Quit(); };

			// ModifyBg(StateType.Normal, Colors.@base);

			var page1 = new Box(Orientation.Vertical, 4);

			page1.Add(new Label("Enim sunt non ipsum irure commodo ipsum id proident consectetur proident deserunt. Sunt enim cupidatat commodo sint commodo ex enim voluptate tempor. Dolore velit esse officia amet tempor quis eu reprehenderit consequat enim in."));
			page1.Add(new Label("Ullamco incididunt proident non consequat quis laboris officia ex pariatur aute occaecat. Qui consectetur occaecat laboris ad reprehenderit commodo. Magna ipsum voluptate qui culpa occaecat excepteur qui in exercitation incididunt veniam. Ullamco aute cupidatat est culpa eiusmod dolor fugiat nisi labore elit ut aliqua Lorem. Cupidatat proident nostrud pariatur ea cillum velit dolor in aliqua labore. Occaecat nulla aute duis aliquip laboris aute sit nulla pariatur. Et minim sit tempor veniam aute et dolor."));

			var page2 = new Box(Orientation.Vertical, 4);

			page2.Add(new Button("Button"));
			page2.Add(new ToggleButton("Toggle Button"));
			page2.Add(new CheckButton("Check Button"));
			page2.Add(new CheckButton("Check Button"));
			page2.Add(new RadioButton("Radio Button"));
			page2.Add(new RadioButton("Radio Button"));
			page2.Add(new RadioButton("Radio Button"));
			page2.Add(new ProgressBar());
			page2.Add(new HScale(0, 100, 10));
			page2.Add(new Entry());

			var left = new Notebook();

			left.AppendPage(page1, new Label("Hierarchy"));
			left.AppendPage(page2, new Label("Common Settings"));

			var right = new Notebook();

			right.AppendPage(new Label("Inspector"), new Label("Inspector"));
			right.AppendPage(new Label("Project Settings"), new Label("Project Settings"));

			var mainLayout = new HBox(true, 0);

			mainLayout.Add(left);
			mainLayout.Add(new VSeparator());
			mainLayout.Add(right);

			Add(mainLayout);

			resetProvider = new CssProvider();
			styleProvider = new CssProvider();
			ReloadStyles();
			ApplyCss(this, resetProvider, 800);
			ApplyCss(this, styleProvider, int.MaxValue);

			ShowAll();
		}

		[GLib.ConnectBefore]
		void KeyPress(object sender, KeyPressEventArgs args)
		{
			if (args.Event.Key == Gdk.Key.space)
			{
				ReloadStyles();
			}

			Console.WriteLine(args.Event.Key);
		}

		void ReloadStyles()
		{
			// ResetCss(this);

			try
			{
				resetProvider.LoadFromPath("styles/reset.css");
				styleProvider.LoadFromPath("styles/styles.css");
			}
			catch { }
		}

		void ResetCss(Widget widget)
		{
			widget.ResetStyle();

			if (widget is Container container)
			{
				foreach (var child in container.Children)
				{
					ResetCss(child);
				}
			}
		}

		void ApplyCss(Widget widget, CssProvider provider, uint priority)
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
