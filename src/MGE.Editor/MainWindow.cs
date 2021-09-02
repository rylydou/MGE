using System;
using Gtk;

namespace MGE.Editor
{
	class MainWindow : Window
	{
		CssProvider resetProvider;
		CssProvider styleProvider;

		public MainWindow() : base("MGE EDITOR")
		{
			SetPosition(WindowPosition.Center);
			SetDefaultSize(1280, 720);
			// Maximize();

			KeyPressEvent += KeyPress;

			DeleteEvent += delegate { Application.Quit(); };

			var page1 = new Grid();

			page1.ColumnSpacing = 16;
			page1.RowSpacing = 4;

			page1.Attach(new Label("Enabled"), 0, 0, 1, 1);
			page1.Attach(new CheckButton(), 1, 0, 1, 1);

			page1.Attach(new Label("Visible"), 0, 1, 1, 1);
			page1.Attach(new CheckButton(), 1, 1, 1, 1);

			page1.Attach(new Label("Name"), 0, 2, 1, 1);
			page1.Attach(new Entry(), 1, 2, 1, 1);

			page1.Attach(new Label("Element"), 0, 3, 1, 1);
			page1.Attach(new ComboBox(new[] { "Fire", "Water", "Air", "Nature" }), 1, 3, 1, 1);

			var left = new Notebook();

			left.AppendPage(page1, new Label("Hierarchy"));
			left.AppendPage(new Label("Common Settings"), new Label("Common Settings"));

			var right = new Notebook();

			right.AppendPage(new Label("Inspector"), new Label("Inspector"));
			right.AppendPage(new Label("Project Settings"), new Label("Project Settings"));

			var mainLayout = new Paned(Orientation.Horizontal) { WideHandle = true };

			mainLayout.Add(left);
			// mainLayout.Add(new VSeparator());
			var r = new Paned(Orientation.Horizontal) { WideHandle = true };
			r.Add(new VSeparator());
			r.Add(right);
			mainLayout.Add(r);

			Add(mainLayout);

			var titleBar = new HeaderBar();

			titleBar.Add(new Label("MGE EDITOR"));
			titleBar.Add(new VSeparator());
			titleBar.Add(new Label("Untitled Scene.mges"));
			// titleBar.Add(new VSeparator());
			// var closeButton = new Button(new Image("images/icons/cross.png"));
			// closeButton.Clicked += (sender, args) => Close();
			// titleBar.Add(closeButton);

			Titlebar = titleBar;

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
			SetIconFromFile("icon.png");

			try
			{
				resetProvider.LoadFromPath("styles/reset.css");
				styleProvider.LoadFromPath("styles/styles.css");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
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

				if (widget is Bin bin)
				{
					if (bin.Child is not null)
					{
						System.Console.WriteLine("Appied to child");
						ApplyCss(bin.Child, provider, priority);
					}
				}
			}
		}
	}
}
