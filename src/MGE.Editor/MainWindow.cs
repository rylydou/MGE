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

			var left = new Notebook();

			left.AppendPage(EditorUtil.GenerateInspector(null), new Label("Hierarchy"));
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
			SetIconFromFile("images/icons/icon.png");

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
