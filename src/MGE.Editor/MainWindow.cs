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

			page1.Attach(new Label("Enabled") { Xalign = 0 }, 0, 0, 1, 1);
			page1.Attach(new CheckButton(), 1, 0, 1, 1);

			page1.Attach(new Label("Visible") { Xalign = 0 }, 0, 1, 1, 1);
			page1.Attach(new CheckButton(), 1, 1, 1, 1);

			page1.Attach(new Label("Name") { Xalign = 0 }, 0, 2, 1, 1);
			page1.Attach(new Entry(), 1, 2, 1, 1);

			page1.Attach(new Label("Element") { Xalign = 0 }, 0, 3, 1, 1);
			page1.Attach(new ComboBox(new[] { "Fire", "Water", "Air", "Nature" }), 1, 3, 1, 1);

			var left = new Notebook();

			left.AppendPage(page1, new Label("Hierarchy"));
			left.AppendPage(new Label("Common Settings"), new Label("Common Settings"));

			var right = new Notebook();

			right.Hexpand = true;

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
			titleBar.ShowCloseButton = true;

			Titlebar = titleBar;

			resetProvider = new CssProvider();
			styleProvider = new CssProvider();

			ReloadStyles();

			StyleContext.AddProviderForScreen(Screen, resetProvider, int.MaxValue - 1);
			StyleContext.AddProviderForScreen(Screen, styleProvider, int.MaxValue);

			ShowAll();
		}

		[GLib.ConnectBefore]
		void KeyPress(object sender, KeyPressEventArgs args)
		{
			if (args.Event.Key == Gdk.Key.space)
			{
				ReloadStyles();
			}
		}

		void ReloadStyles()
		{
			SetIconFromFile("icon.png");

			try
			{
				resetProvider.LoadFromPath("styles/reset.css");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			try
			{
				styleProvider.LoadFromPath("styles/styles.css");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
