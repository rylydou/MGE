using System;
using Gtk;
namespace MGE.Editor
{
	class MainWindow : Gtk.Window
	{
		CssProvider resetProvider;
		CssProvider styleProvider;

		public MainWindow() : base("MGE EDITOR")
		{
			SetPosition(WindowPosition.Center);
			SetDefaultSize(1280, 720);
			// Maximize();

			KeyPressEvent += KeyPress;

			FocusInEvent += delegate { ReloadStyles(); };
			Destroyed += MainWindow_Destroyed;

			var left = new Notebook();

			left.AppendPage(new Label("Hierarchy"), new Label("Hierarchy"));
			left.AppendPage(new Label("Common Settings"), new Label("Common Settings"));
			left.AppendPage(new Label("Other Menu"), new Label("Other Menu"));

			var right = new Notebook();

			right.Hexpand = true;

			right.AppendPage(EditorUtil.GenerateInspector(null), new Label("Inspector"));
			right.AppendPage(new Label("Project Settings"), new Label("Project Settings"));

			var mainLayout = new Paned(Orientation.Horizontal) { WideHandle = true };

			mainLayout.Add(left);
			mainLayout.Add(right);

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
		}

		[GLib.ConnectBefore]
		void KeyPress(object sender, KeyPressEventArgs args)
		{
			// if (args.Event.Key == Gdk.Key.space)
			// {
			// 	ReloadStyles();
			// }
		}

		private void MainWindow_Destroyed(object sender, EventArgs e)
		{
			Application.Quit();
		}

		void ReloadStyles()
		{
			SetIconFromFile("images/icons/icon.png");

			try
			{
				styleProvider.LoadFromPath("styles/styles.css");

				try
				{
					resetProvider.LoadFromPath("styles/reset.css");
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				resetProvider.LoadFromData(string.Empty);
			}
		}
	}
}
