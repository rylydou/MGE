using System;
using Gtk;
using MGE.Editor.GUI.Windows;

namespace MGE.Editor
{
	class MainWindow : Gtk.Window
	{
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
			var menu = new TestWindow();
			left.AppendPage(menu.root, new Label("Other Menu"));

			var right = new Notebook();

			right.Hexpand = true;

			right.AppendPage(EditorUtil.GenerateInspector("TODO"), new Label("Inspector"));
			right.AppendPage(new Label("Project Settings"), new Label("Project Settings"));

			var mainLayout = new Paned(Orientation.Horizontal) { WideHandle = true };

			mainLayout.Add(left);
			mainLayout.Add(right);

			Add(mainLayout);

			var titleBar = new HeaderBar();

			titleBar.Add(new Image("images/icons/icon-symbolic.svg"));

			titleBar.Add(new VSeparator());

			var menubar = new MenuBar();
			var filemenu = new Menu();
			var file = new MenuItem("File");
			file.Submenu = filemenu;
			filemenu.Append(new MenuItem("Open Project..."));
			filemenu.Append(new SeparatorMenuItem());
			filemenu.Append(new MenuItem("Save"));
			filemenu.Append(new MenuItem("Save As..."));
			filemenu.Append(new SeparatorMenuItem());
			var exit = new MenuItem("Exit");
			exit.Activated += (sender, args) => Application.Quit();
			filemenu.Append(exit);
			menubar.Append(file);

			var editmenu = new Menu();
			var edit = new MenuItem("Edit");
			edit.Submenu = editmenu;
			editmenu.Append(new MenuItem("Undo"));
			editmenu.Append(new MenuItem("Redo"));
			editmenu.Append(new SeparatorMenuItem());
			editmenu.Append(new MenuItem("User Settings"));
			editmenu.Append(new MenuItem("Project Settings"));
			menubar.Append(edit);

			var viewmenu = new Menu();
			var view = new MenuItem("View");
			view.Submenu = viewmenu;
			viewmenu.Append(new CheckMenuItem("Zoomed GUI"));
			menubar.Append(view);

			var helpmenu = new Menu();
			var help = new MenuItem("Help");
			help.Submenu = helpmenu;
			helpmenu.Append(new MenuItem("About"));
			helpmenu.Append(new SeparatorMenuItem());
			helpmenu.Append(new MenuItem("About"));
			menubar.Append(help);

			titleBar.Add(menubar);

			titleBar.ShowCloseButton = true;

			Titlebar = titleBar;

			styleProvider = new CssProvider();

			ReloadStyles();

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

		private void MainWindow_Destroyed(object? sender, EventArgs e)
		{
			Application.Quit();
		}

		void ReloadStyles()
		{
			SetIconFromFile("images/icons/icon.svg");

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
