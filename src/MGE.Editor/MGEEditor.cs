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

			ReloadStyles();
			StyleContext.AddProviderForScreen(Screen, styleProvider, int.MaxValue);

			// Idk which one I should use
			IconTheme.Default.AppendSearchPath($"{Editor.assets.FullName}/Icons");
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
			EditorGUI.IconButton("Icon");
			MakeMenubar();
			EditorGUI.Add(menubar);

			EditorGUI.PopContainer();

			Titlebar = titlebar;
			Title = "MGE Editor";

			MakeStatusbar();

			mainLayout.Add(statusbar);

			Add(mainLayout);
		}

		void UpdatePanedSizes()
		{
			topPaned.Position = Allocation.Width / 5;
			leftPaned.Position = Allocation.Height / 3 * 2;
			mainPaned.Position = Allocation.Width / 4 * 3;
		}

		void MakeMenubar()
		{
			EditorGUI.StartMenubar();

			EditorGUI.MenuButton("File");
			EditorGUI.StartMenu();
			EditorGUI.MenuButton("Projects...");
			EditorGUI.MenuSeparator();
			EditorGUI.MenuButton("Save");
			EditorGUI.MenuButton("Save As...");
			EditorGUI.MenuSeparator();
			EditorGUI.MenuButton("Backups...");
			EditorGUI.MenuSeparator();
			EditorGUI.MenuButton("Exit...").onClicked += () => Application.Quit();
			EditorGUI.EndMenu();

			EditorGUI.MenuButton("Edit");
			EditorGUI.StartMenu();
			EditorGUI.MenuButton("Undo");
			EditorGUI.MenuButton("Redo");
			EditorGUI.MenuSeparator();
			EditorGUI.MenuButton("Cut");
			EditorGUI.MenuButton("Copy");
			EditorGUI.MenuButton("Paste");
			EditorGUI.MenuSeparator();
			EditorGUI.MenuButton("Editor Settings...");
			EditorGUI.MenuButton("Project Settings...");
			EditorGUI.EndMenu();

			EditorGUI.MenuButton("View");
			EditorGUI.StartMenu();
			EditorGUI.MenuCheckbox("Large Interface", false);
			EditorGUI.MenuSeparator();
			EditorGUI.MenuButton("Reset Dock Sizes").onClicked += () => UpdatePanedSizes();
			EditorGUI.MenuSeparator();
			EditorGUI.MenuButton("Reload Theme").onClicked += () => { ReloadStyles(); EditorGUI.ReloadIcons(); };
			EditorGUI.EndMenu();

			EditorGUI.MenuButton("Window");
			EditorGUI.StartMenu();
			EditorGUI.MenuCheckbox("Freeze Docks", false);
			EditorGUI.MenuSeparator();
			EditorGUI.MenuButton("All Windows...");
			EditorGUI.MenuSeparator();
			EditorGUI.MenuButton("Scene");
			EditorGUI.MenuButton("Hierarchy");
			EditorGUI.MenuButton("Inspector");
			EditorGUI.MenuButton("Assets");
			EditorGUI.MenuButton("Problems");
			EditorGUI.MenuButton("Console");
			EditorGUI.MenuSeparator();
			EditorGUI.MenuButton("Source Control");
			EditorGUI.MenuButton("Profiler");
			EditorGUI.MenuButton("Autodocs");
			EditorGUI.MenuButton("Music Player");
			EditorGUI.MenuSeparator();
			EditorGUI.MenuButton("GUI Debugger...");
			EditorGUI.EndMenu();

			EditorGUI.MenuButton("Help");
			EditorGUI.StartMenu();
			EditorGUI.MenuButton("Get Started");
			EditorGUI.MenuButton("Documentation...");
			EditorGUI.MenuButton("Tutorials...");
			EditorGUI.MenuSeparator();
			EditorGUI.MenuButton("Command Palette");
			EditorGUI.MenuButton("Super Search");
			EditorGUI.MenuSeparator();
			EditorGUI.MenuButton("Report an Issue...");
			EditorGUI.MenuButton("Request a Feature...");
			EditorGUI.MenuSeparator();
			EditorGUI.MenuButton("Copy Info").onClicked += () =>
			{
				var asmName = typeof(MGEEditor).Assembly.GetName();
				Clipboard.GetDefault(Display).Text = $"`{asmName.Name} {asmName.Version} OS={Environment.OSVersion.ToString()}; CLR={Environment.Version}`";
			};
			EditorGUI.MenuButton("Check for Updates...");
			EditorGUI.MenuButton("About...");
			EditorGUI.EndMenu();

			menubar = (MenuBar)EditorGUI.EndMenu();

			// var helpmenu = new Menu();
			// var help = new MenuItem("Help");
			// help.Submenu = helpmenu;
			// helpmenu.Append(new MenuItem("Tutorials"));
			// helpmenu.Append(new MenuItem("Documentation"));
			// helpmenu.Append(new SeparatorMenuItem());
			// helpmenu.Append(new MenuItem("Command Palette"));
			// helpmenu.Append(new MenuItem("Project Search"));
			// helpmenu.Append(new SeparatorMenuItem());
			// helpmenu.Append(new MenuItem("Reload Assets"));
			// var clearCacheMenuItem = new MenuItem("Reload Cache");
			// clearCacheMenuItem.Activated += (sender, args) => Editor.ClearCache();
			// helpmenu.Append(clearCacheMenuItem);
			// helpmenu.Append(new MenuItem("Reload Editor"));
			// helpmenu.Append(new SeparatorMenuItem());
			// helpmenu.Append(new MenuItem("Request a Feature..."));
			// helpmenu.Append(new MenuItem("Report an Issue..."));
			// helpmenu.Append(new SeparatorMenuItem());
			// helpmenu.Append(new MenuItem("Check for Updates..."));
			// helpmenu.Append(new SeparatorMenuItem());
			// helpmenu.Append(new MenuItem("About..."));
			// menubar.Append(help);
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

			SetIconFromFile($"{Editor.assets.FullName}/Favicons/Normal.svg");

			EditorGUI.ReloadIcons();

			try
			{
				styleProvider.LoadFromPath($"{Editor.assets.FullName}/Styles/Styles.css");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
