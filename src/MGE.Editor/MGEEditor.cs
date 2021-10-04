using System;
using Gtk;
using MGE.Editor.GUI;
using MGE.Editor.GUI.Windows;

namespace MGE.Editor
{
	class MGEEditor : Gtk.Window
	{
		public static MGEEditor current { get { if (_current is null) throw new NullReferenceException("_current is null"); return _current; } }
		static MGEEditor? _current;

		CssProvider styleProvider;

		public MGEEditor() : base("MGE EDITOR")
		{
			if (_current is not null) throw new Exception($"A {nameof(MGEEditor)} instance already exists");

			_current = this;

			SetPosition(WindowPosition.Center);
			SetDefaultSize(1280, 720);

			FocusInEvent += (sender, args) => ReloadStyles();
			Destroyed += (sender, args) => MainWindow_Destroyed();

			var left = new Notebook();

			var hierarchy = new HierarchyWindow();
			left.AppendPage(hierarchy.root, new Label(hierarchy.title));

			var console = new ConsoleWindow();
			left.AppendPage(console.root, new Label(console.title));

			var right = new Notebook();

			var inspector = new InspectorWindow();
			right.AppendPage(inspector.root, new Label(inspector.title));
			right.AppendPage(new Label("Project Settings"), new Label("Project Settings"));

			var leftLayout = new Paned(Orientation.Horizontal) { WideHandle = true };

			var rightLayout = new Paned(Orientation.Horizontal) { WideHandle = true };

			var center = new Notebook();

			var scene = new SceneWindow();

			center.AppendPage(scene.root, new Label(scene.title));

			rightLayout.Add(center);

			rightLayout.Add(right);

			leftLayout.Add(left);
			leftLayout.Add(rightLayout);

			Add(leftLayout);

			var menubar = new MenuBar();

			var filemenu = new Menu();
			var file = new MenuItem("File");
			file.Submenu = filemenu;
			filemenu.Append(new MenuItem("New Project..."));
			filemenu.Append(new SeparatorMenuItem());
			filemenu.Append(new MenuItem("Open Project..."));
			filemenu.Append(new SeparatorMenuItem());
			filemenu.Append(new MenuItem("Save"));
			filemenu.Append(new MenuItem("Save As..."));
			filemenu.Append(new SeparatorMenuItem());
			filemenu.Append(new MenuItem("Backups..."));
			filemenu.Append(new SeparatorMenuItem());
			var exit = new MenuItem("Exit...");
			exit.Activated += (sender, args) => Application.Quit();

			filemenu.Append(exit);
			menubar.Append(file);

			var editmenu = new Menu();
			var edit = new MenuItem("Edit");
			edit.Submenu = editmenu;
			editmenu.Append(new MenuItem("Undo"));
			editmenu.Append(new MenuItem("Redo"));
			editmenu.Append(new SeparatorMenuItem());
			editmenu.Append(new MenuItem("Cut"));
			editmenu.Append(new MenuItem("Copy"));
			editmenu.Append(new MenuItem("Paste"));
			editmenu.Append(new SeparatorMenuItem());
			editmenu.Append(new MenuItem("Previous Selection"));
			editmenu.Append(new SeparatorMenuItem());
			editmenu.Append(new MenuItem("Recompile Project"));
			editmenu.Append(new SeparatorMenuItem());
			editmenu.Append(new MenuItem("Editor Settings..."));
			editmenu.Append(new MenuItem("Project Settings..."));
			menubar.Append(edit);

			var viewmenu = new Menu();
			var view = new MenuItem("View");
			view.Submenu = viewmenu;
			viewmenu.Append(new CheckMenuItem("Show Node Gizmos") { Active = true });
			viewmenu.Append(new CheckMenuItem("Show Node Icons") { Active = true });
			viewmenu.Append(new SeparatorMenuItem());
			viewmenu.Append(new CheckMenuItem("Show Hidden Nodes"));
			viewmenu.Append(new CheckMenuItem("Show Disabled Nodes"));
			viewmenu.Append(new CheckMenuItem("Show Graphicless Nodes") { Active = true });
			viewmenu.Append(new SeparatorMenuItem());
			viewmenu.Append(new CheckMenuItem("Large GUI"));
			viewmenu.Append(new CheckMenuItem("Static GUI"));
			viewmenu.Append(new CheckMenuItem("Focus Outlines"));
			viewmenu.Append(new SeparatorMenuItem());
			var reloadStylesMenuItem = new MenuItem("Reload Styles");
			reloadStylesMenuItem.Activated += (sender, args) => ReloadStyles();
			viewmenu.Append(reloadStylesMenuItem);
			menubar.Append(view);

			var windowmenu = new Menu();
			var window = new MenuItem("Window");
			window.Submenu = windowmenu;
			windowmenu.Append(new CheckMenuItem("Lock Docked Windows"));
			windowmenu.Append(new SeparatorMenuItem());
			windowmenu.Append(new MenuItem("Scene"));
			windowmenu.Append(new MenuItem("Hierarchy"));
			windowmenu.Append(new MenuItem("Inspector"));
			windowmenu.Append(new MenuItem("Assets"));
			windowmenu.Append(new MenuItem("Problems"));
			windowmenu.Append(new MenuItem("Console"));
			windowmenu.Append(new SeparatorMenuItem());
			windowmenu.Append(new MenuItem("Source Control"));
			windowmenu.Append(new MenuItem("Profiler"));
			windowmenu.Append(new MenuItem("Autodocs")); // Gets all the info of types in an assembly and shows the xml docs
			windowmenu.Append(new SeparatorMenuItem());
			windowmenu.Append(new MenuItem("Music Player")); // A mini music player, mostly as an example on how to write custom windows
			windowmenu.Append(new SeparatorMenuItem());
			windowmenu.Append(new MenuItem("Editor Debugger...")); // Will open a mostly isolated editor debugger
			menubar.Append(window);

			var helpmenu = new Menu();
			var help = new MenuItem("Help");
			help.Submenu = helpmenu;
			helpmenu.Append(new MenuItem("Tutorials"));
			helpmenu.Append(new MenuItem("Documentation"));
			helpmenu.Append(new SeparatorMenuItem());
			helpmenu.Append(new MenuItem("Command Palette"));
			helpmenu.Append(new MenuItem("Project Search"));
			helpmenu.Append(new SeparatorMenuItem());
			helpmenu.Append(new MenuItem("Reload Scene"));
			helpmenu.Append(new MenuItem("Reload Assets"));
			var clearCacheMenuItem = new MenuItem("Reload Cache");
			clearCacheMenuItem.Activated += (sender, args) => Editor.ClearCache();
			helpmenu.Append(clearCacheMenuItem);
			helpmenu.Append(new MenuItem("Reload Editor"));
			helpmenu.Append(new SeparatorMenuItem());
			helpmenu.Append(new MenuItem("Request a Feature..."));
			helpmenu.Append(new MenuItem("Report an Issue..."));
			helpmenu.Append(new SeparatorMenuItem());
			helpmenu.Append(new MenuItem("Check for Updates..."));
			helpmenu.Append(new SeparatorMenuItem());
			helpmenu.Append(new MenuItem("Copy Debug Info"));
			helpmenu.Append(new MenuItem("About..."));
			menubar.Append(help);

			var titleBar = new HeaderBar() { ShowCloseButton = true, Title = "MGE Editor", };

			titleBar.Add(new Image("res/images/icons/icon.svg"));
			titleBar.Add(menubar);

			Titlebar = titleBar;

			styleProvider = new CssProvider();

			ReloadStyles();

			StyleContext.AddProviderForScreen(Screen, styleProvider, int.MaxValue);
		}

		private void MainWindow_Destroyed()
		{
			Application.Quit();
		}

		void ReloadStyles()
		{
			SetIconFromFile("res/images/icons/logo.png");

			try
			{
				styleProvider.LoadFromPath("res/styles/styles.css");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}