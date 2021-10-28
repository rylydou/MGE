using System;
using System.Diagnostics;
using Gtk;
using MGE.Editor.GUI;
using MGE.Editor.GUI.Widgets;
using MGE.Editor.GUI.Windows;

namespace MGE.Editor
{
	class MGEEditor : Gtk.Window
	{
		public static MGEEditor current { get { if (_current is null) throw new NullReferenceException("_current is null"); return _current; } }
		static MGEEditor? _current;

		public HeaderBar titlebar = new() { ShowCloseButton = true };
		public MenuBar menubar = new();
		public Box statusbar = new(Orientation.Horizontal, 8);

		CssProvider styleProvider = new();

		Box mainLayout = new(Orientation.Vertical, 0);
		Dock leftDock = new();
		Dock rightDock = new();
		Dock centerDock = new();
		Dock bottomDock = new();

		Paned topPaned = new(Orientation.Horizontal);
		Paned leftPaned = new(Orientation.Vertical);
		Paned mainPaned = new(Orientation.Horizontal);

		public MGEEditor() : base("MGE EDITOR")
		{
			if (_current is not null) throw new Exception("A MGE editor window already exists");

			_current = this;

			SetPosition(WindowPosition.Center);
			SetDefaultSize(1280, 720);
			// Maximize();

			// FocusInEvent += (sender, args) => ReloadStyles();
			Destroyed += (sender, args) => Application.Quit();
			Shown += (sender, args) => UpdatePanedSizes();

			// Idk which one I should use
			IconTheme.Default.PrependSearchPath(Environment.CurrentDirectory + "/assets/icons");
			IconTheme.Default.AppendSearchPath(Environment.CurrentDirectory + "/assets/icons");
			IconTheme.Default.AddResourcePath(Environment.CurrentDirectory + "/assets/icons");
			IconTheme.Default.AppendSearchPath(Environment.CurrentDirectory + "/assets/icons");
			IconTheme.Default.RescanIfNeeded();

			leftDock.AddWindow(new HierarchyWindow());

			rightDock.AddWindow(new InspectorWindow());
			rightDock.AddWindow(new RealHierarchyWindow());

			centerDock.AddWindow(new SceneWindow());
			centerDock.AddWindow(new SettingsWindow());

			bottomDock.AddWindow(new AssetsWindow());
			bottomDock.AddWindow(new ConsoleWindow());

			topPaned.Add(leftDock);
			topPaned.Add(centerDock);

			leftPaned.Add(topPaned);
			leftPaned.Add(bottomDock);

			mainPaned.Add(leftPaned);
			mainPaned.Add(rightDock);

			mainLayout.Add(mainPaned);

			EditorGUI.PushContainer(titlebar);

			EditorGUI.classes.Add("favicon");
			EditorGUI.IconButton("icon");
			MakeMenubar();
			EditorGUI.Add(menubar);

			EditorGUI.PopContainer();

			Titlebar = titlebar;
			Title = "MGE Editor";

			MakeStatusbar();

			mainLayout.Add(statusbar);

			Add(mainLayout);

			ReloadStyles();

			StyleContext.AddProviderForScreen(Screen, styleProvider, int.MaxValue);
		}

		void UpdatePanedSizes()
		{
			topPaned.Position = Allocation.Width / 4;
			leftPaned.Position = Allocation.Height / 3 * 2;
			mainPaned.Position = Allocation.Width / 4 * 3;
		}

		void MakeMenubar()
		{
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
			editmenu.Append(new MenuItem("Manage 2 Contexts..."));
			editmenu.Append(new SeparatorMenuItem());
			editmenu.Append(new MenuItem("Editor Settings..."));
			editmenu.Append(new MenuItem("Project Settings..."));
			menubar.Append(edit);

			var viewmenu = new Menu();
			var view = new MenuItem("View");
			view.Submenu = viewmenu;
			viewmenu.Append(new CheckMenuItem("Large GUI"));
			viewmenu.Append(new CheckMenuItem("Static GUI"));
			viewmenu.Append(new CheckMenuItem("Focus Outlines"));
			viewmenu.Append(new SeparatorMenuItem());
			var resetDocksSizesMenuItem = new MenuItem("Reset Docks Sizes");
			resetDocksSizesMenuItem.Activated += (sender, args) => UpdatePanedSizes();
			viewmenu.Append(resetDocksSizesMenuItem);
			var reloadStylesMenuItem = new MenuItem("Reload Styles");
			reloadStylesMenuItem.Activated += (sender, args) => ReloadStyles();
			viewmenu.Append(reloadStylesMenuItem);
			menubar.Append(view);

			var windowmenu = new Menu();
			var window = new MenuItem("Window");
			window.Submenu = windowmenu;
			windowmenu.Append(new CheckMenuItem("Lock Docked Windows"));
			windowmenu.Append(new SeparatorMenuItem());
			windowmenu.Append(new MenuItem("All Windows..."));
			windowmenu.Append(new SeparatorMenuItem()); // Pinned windows (windows assigned to hotkeys)
			windowmenu.Append(new MenuItem("Scene")); // alt 1, goes to the last focused context of that window
			windowmenu.Append(new MenuItem("Hierarchy")); // alt 2
			windowmenu.Append(new MenuItem("Inspector")); // alt 3
			windowmenu.Append(new MenuItem("Assets")); // alt 4
			windowmenu.Append(new MenuItem("Problems")); // alt 5
			windowmenu.Append(new MenuItem("Console")); // alt
			windowmenu.Append(new SeparatorMenuItem()); // Recenty used windows
			windowmenu.Append(new MenuItem("Source Control"));
			windowmenu.Append(new MenuItem("Profiler"));
			windowmenu.Append(new MenuItem("Autodocs")); // Gets all the info of types in an assembly and shows the xml docs
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
			helpmenu.Append(new MenuItem("About..."));
			menubar.Append(help);
		}

		void MakeStatusbar()
		{
			statusbar.StyleContext.AddClass("statusbar");

			statusbar.Add(new Label("main*"));
			statusbar.Add(new Label("2⬇ 1⬆"));
			statusbar.Add(new Label("0 Errors 0 Warnings"));
			statusbar.Add(new Label("0 hrs 0 mins"));
		}

		void ReloadStyles()
		{
			Trace.WriteLine("Reloading styles...");

			SetIconFromFile("assets/favicons/normal.svg");

			EditorGUI.ReloadIcons();

			try
			{
				styleProvider.LoadFromPath("assets/styles/styles.css");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
