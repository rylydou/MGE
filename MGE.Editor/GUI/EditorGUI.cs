using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Gdk;
using Gtk;
using MGE.Editor.GUI.Data;
using MGE.Editor.GUI.Drawers;
using MGE.Editor.Util;
using Pango;

namespace MGE.Editor.GUI
{
	public static class EditorGUI
	{
		public class ContainerInfo
		{
			public Container container;

			public readonly bool isHorizontal;
			public readonly bool isBin;

			public ContainerInfo(Container container)
			{
				this.container = container;

				if (container is Box box)
				{
					isHorizontal = box.Orientation == Orientation.Horizontal;
				}

				isBin = container is Bin;
			}
		}

		static EditorGUI()
		{
			RegisterDrawer<IEnumerable, ListDrawer>();

			RegisterDrawer<bool, BoolDrawer>();
			RegisterDrawer<string, StringDrawer>();
			RegisterDrawer<int, IntDrawer>();
			RegisterDrawer<float, FloatDrawer>();

			RegisterDrawer<Vector2, Vector2Drawer>();
		}

		#region Icons

		static Dictionary<string, Gdk.Pixbuf> _icons = new();
		readonly static string _iconsPath = $"{Editor.assets.FullName}/Icons";
		static Pixbuf _fallbackIcon = new($"{_iconsPath}/Unknown.symbolic.png");

		static Dictionary<string, Gdk.Pixbuf> _fileIcons = new();
		readonly static string _fileIconsPath = $"{Editor.assets.FullName}/Files";
		static Pixbuf _fallbackFileIcon = new($"{Editor.assets.FullName}/Files/File.svg", 64, 64, true);

		internal static void ReloadIcons()
		{
			foreach (var icon in _icons.Values) icon.Dispose();
			foreach (var icon in _fileIcons.Values) icon.Dispose();

			_icons.Clear();
			_fileIcons.Clear();

			foreach (var item in new DirectoryInfo(_iconsPath).GetFiles("*.symbolic.png"))
			{
				var icon = new Gdk.Pixbuf(item.FullName);

				_icons.Add(item.Name.Substring(0, item.Name.Length - 13), icon);
			}

			foreach (var item in new DirectoryInfo(_fileIconsPath).GetFiles("*.svg"))
			{
				var icon = new Gdk.Pixbuf(item.FullName, 64, 64, true);

				_fileIcons.Add(item.Name.Substring(0, item.Name.Length - 4), icon);
			}
		}

		public static Pixbuf GetIcon(string name)
		{
			if (_icons.TryGetValue(name, out var icon)) return icon;
			return _fallbackIcon;
		}

		public static Pixbuf GetFileIcon(string name)
		{
			if (_fileIcons.TryGetValue(name, out var icon)) return icon;
			return _fallbackFileIcon;
		}

		#endregion

		#region Drawers

		static Dictionary<Type, Type> _typeToDrawer = new();
		static List<(Type, Type)> _objectDrawers = new();

		static Type _defaultDrawer = typeof(DefaultDrawer);
		static Type _enumDrawer = typeof(EnumDrawer);
		static Type _flagsDrawer = typeof(FlagsDrawer);

		public static void RegisterDrawer<Type, Drawer>()
		{
			_objectDrawers.Insert(0, (typeof(Type), typeof(Drawer)));
			_typeToDrawer.Add(typeof(Type), typeof(Drawer));
		}

		public static Drawer GetDrawer(object value)
		{
			var type = value.GetType();

			// Try to see if the drawer fro this specific type has been found already
			if (_typeToDrawer.TryGetValue(type, out var drawer)) goto Return;

			// If not then try to find the drawer for the specific type
			foreach (var item in _objectDrawers)
			{
				if (type.Equals(item.Item1))
				{
					drawer = item.Item2;
					break;
				}
			}

			if (type.IsEnum) drawer = type.GetCustomAttribute<FlagsAttribute>() is null ? _enumDrawer : _flagsDrawer;
			else if (drawer is null) drawer = _defaultDrawer;

			// Add the drawer and type combo to the cache so its faster to find next time
			_typeToDrawer.Add(type, drawer);

		Return:
			return (Drawer)Activator.CreateInstance(drawer, value)!;
		}

		#endregion Drawers

		public static ExpressionDictionaryContext _dictionaryContext = new(new()
		{
			{ "pi", () => Math.pi },
			{ "tau", () => Math.tau },
			{ "e", () => Math.e },
			{ "inf", () => double.PositiveInfinity },
			{ "ninf", () => double.NegativeInfinity },
			{ "nan", () => double.NaN },

			{ "rand", () => new Random().NextDouble() },
		}, new()
		{
			// { "sin", (args) => Math.Sin(args[0]) },
			// { "cos", (args) => Math.Cos(args[0]) },
			// { "tan", (args) => Math.Tan(args[0]) },

			// { "asin", (args) => Math.Asin(args[0]) },
			// { "acos", (args) => Math.Acos(args[0]) },
			// { "atan", (args) => Math.Atan(args[0]) },

			// { "pow", (args) => Math.Pow(args[0], args[1]) },

			// { "abs", (args) => Math.Abs(args[0]) },
			// { "sign", (args) => Math.Sign(args[0]) },

			// { "round", (args) => Math.Round(args[0]) },
			// { "floor", (args) => Math.Floor(args[0]) },
			// { "ceil", (args) => Math.Ceil(args[0]) },
			// { "trunc", (args) => Math.Trunc(args[0]) },

			// { "min", (args) => Math.Min(args[0], args[1]) },
			// { "max", (args) => Math.Max(args[0], args[1]) },
			// { "clamp", (args) => Math.Clamp(args[0], args[1], args[2]) },
		});

		#region Widget Properties

		public static Optional<bool> sensitive = new();
		public static Optional<string?> tooltip = new();
		public static Optional<int> width = new();
		public static Optional<int> height = new();
		public static Optional<bool> horizontalExpand = new();
		public static Optional<bool> verticalExpand = new();

		public static Optional<int> margin = new();
		public static Optional<int> marginTop = new();
		public static Optional<int> marginBottom = new();
		public static Optional<int> marginStart = new();
		public static Optional<int> marginEnd = new();

		public static Optional<float> horizontalAlign = new();
		public static Optional<int> widthInChars = new();
		public static Optional<int> maxWidthInChars = new();
		public static Optional<EllipsizeMode> ellipsizeMode = new();

		public static Optional<string> placeholderText = new();

		public static readonly List<string> classes = new();

		public static void ResetProperties()
		{
			sensitive.Unset();
			tooltip.Unset();
			width.Unset();
			height.Unset();
			horizontalExpand.Unset();
			verticalExpand.Unset();

			margin.Unset();
			marginTop.Unset();
			marginBottom.Unset();
			marginStart.Unset();
			marginEnd.Unset();

			horizontalAlign.Unset();
			widthInChars.Unset();
			maxWidthInChars.Unset();
			ellipsizeMode.Unset();

			classes.Clear();

			placeholderText.Unset();
		}

		#endregion Widget Properties

		static Stack<ContainerInfo?> _containerStack = new();
		static ContainerInfo? _containerInfo;
		public static ContainerInfo containerInfo { get => _containerInfo is not null ? _containerInfo : throw new NullReferenceException("No active container"); }
		public static Container container { get => containerInfo.container; }
		public static bool isHorizontal { get => containerInfo.isHorizontal; }
		public static bool isBin { get => containerInfo.isBin; }

		public static void PushContainer(Container container)
		{
			_containerStack.Push(_containerInfo);
			_containerInfo = new ContainerInfo(container);
		}

		public static void End() => PopContainer();
		public static void PopContainer()
		{
			_containerInfo = _containerStack.Pop();
		}

		public static Widget Add(Widget widget)
		{
			sensitive.TryGetValue(value => widget.Sensitive = value);
			tooltip.TryGetValue(value => widget.TooltipText = value);
			width.TryGetValue(value => widget.WidthRequest = value);
			horizontalExpand.TryGetValue(value => widget.Hexpand = value);
			verticalExpand.TryGetValue(value => widget.Vexpand = value);

			margin.TryGetValue(value => widget.Margin = value);
			marginTop.TryGetValue(value => widget.MarginTop = value);
			marginBottom.TryGetValue(value => widget.MarginBottom = value);
			marginStart.TryGetValue(value => widget.MarginStart = value);
			marginEnd.TryGetValue(value => widget.MarginEnd = value);

			var styleContext = widget.StyleContext;
			foreach (var c in classes)
			{
				styleContext.AddClass(c);
			}

			ResetProperties();

			container.Add(widget);

			if (containerInfo.isBin) End();

			return widget;
		}

		public static Container AddContainer(Container container)
		{
			Add(container);
			PushContainer(container);

			return container;
		}

		#region Widgets

		public static TextData Text(string text)
		{
			var widget = new Label(text)
			{
				Xalign = horizontalAlign.TryGetValue(0),
				WidthChars = widthInChars.TryGetValue(-1),
				MaxWidthChars = maxWidthInChars.TryGetValue(-1),
				Ellipsize = ellipsizeMode.TryGetValue(EllipsizeMode.End),
			};
			var data = new TextData(widget);

			Add(widget);

			return data;
		}

		public static TextData Label(string text)
		{
			var widget = new Label(text)
			{
				Xalign = horizontalAlign.TryGetValue(0),
				WidthChars = widthInChars.TryGetValue(-1),
				MaxWidthChars = maxWidthInChars.TryGetValue(-1),
				Ellipsize = ellipsizeMode.TryGetValue(EllipsizeMode.End),
			};
			var data = new TextData(widget);

			Add(widget);

			return data;
		}

		public static TextData Header(string text)
		{
			var widget = new Label(text)
			{
				Xalign = horizontalAlign.TryGetValue(0.5f),
				WidthChars = widthInChars.TryGetValue(-1),
				MaxWidthChars = maxWidthInChars.TryGetValue(-1),
				Ellipsize = ellipsizeMode.TryGetValue(EllipsizeMode.End),
			};
			var data = new TextData(widget);

			widget.StyleContext.AddClass("header");
			Add(widget);

			return data;
		}

		public static TextBoxData TextBox(string text)
		{
			var widget = new TextView() { Editable = false, WrapMode = Gtk.WrapMode.Word, };
			var data = new TextBoxData(widget);

			widget.Buffer.Text = text;
			Add(widget);

			return data;
		}

		#region Controls

		#region Buttons

		public static ButtonData Button(string text)
		{
			var widget = new Button(text) { Hexpand = true, Xalign = horizontalAlign.TryGetValue(0.5f), };
			var data = new ButtonData(widget);
			widget.Clicked += (sender, args) => data.onPressed();

			Add(widget);

			return data;
		}

		public static ButtonData Link(string text)
		{
			var widget = new LinkButton(text);
			var data = new ButtonData(widget);
			widget.Clicked += (sender, args) => data.onPressed();

			Add(widget);

			return data;
		}

		public static ButtonData ButtonWithContent()
		{
			var widget = new Button() { Hexpand = true, };
			var data = new ButtonData(widget);
			widget.Clicked += (sender, args) => data.onPressed();

			AddContainer(widget);

			return data;
		}

		public static ButtonData IconButton(string icon)
		{
			var widget = new Button(icon, IconSize.Button);
			var data = new ButtonData(widget);

			var styleContext = widget.StyleContext;
			styleContext.AddClass("flat");
			styleContext.AddClass("icon");
			Add(widget);

			widget.Clicked += (sender, args) => data.onPressed();
			return data;
		}

		public static ToggleButtonData ToggleButton(string text, bool? state)
		{
			var widget = new ToggleButton(text) { Inconsistent = !state.HasValue, Active = state.HasValue ? state.Value : false, Xalign = horizontalAlign.TryGetValue(0.5f), };
			var data = new ToggleButtonData(widget);

			var styleContext = widget.StyleContext;
			Add(widget);

			widget.Clicked += (sender, args) => data.onToggled(widget.Active);
			return data;
		}

		public static ToggleButtonData IconToggleButton(string icon, bool? state)
		{
			var widget = new ToggleButton() { Inconsistent = !state.HasValue, Active = state.HasValue ? state.Value : false };
			var data = new ToggleButtonData(widget);

			widget.Add(new Image(icon, IconSize.Button));
			var styleContext = widget.StyleContext;
			styleContext.AddClass("icon");
			Add(widget);

			widget.Clicked += (sender, args) => data.onToggled(widget.Active);
			return data;
		}

		#endregion Buttons

		#region Fields

		static InternalEntryData MakeEntry(string text)
		{
			var widget = new Entry(text) { Hexpand = true, WidthRequest = 0, WidthChars = 0, MaxWidthChars = 0, };
			var data = new InternalEntryData(widget);
			var originalText = string.Empty;

			// FIXME Select everything when focused
			widget.FocusInEvent += (sender, args) =>
			{
				originalText = widget.Text;
				widget.SelectRegion(0, -1);
			};

			widget.KeyPressEvent += (sender, args) =>
			{
				switch (args.Event.Key)
				{
					// Reset the text field when escape is pressed
					case Gdk.Key.Escape:
						widget.Text = originalText;
						widget.FinishEditing();
						break;
				}
			};

			// Finish when enter is pressed
			widget.Activated += (sender, args) => widget.FinishEditing();
			// Finish when the field is unfocused
			widget.FocusOutEvent += (sender, args) => widget.FinishEditing();
			// Finish when the field is destroyed
			widget.Destroyed += (sender, args) => widget.FinishEditing();

			widget.EditingDone += (sender, args) =>
			{
				// HACK Works well enough, but is not good for keyboard users
				widget.Sensitive = false;
				widget.Sensitive = true;

				if (originalText != widget.Text)
				{
					try
					{
						widget.Text = data.onTextSubmitted(widget.Text);
					}
					catch (System.Exception)
					{
						widget.Text = originalText;
					}
				}
			};

			return data;
		}

		public static EntryData<string> TextField(string text)
		{
			var widget = MakeEntry(text);
			var data = new EntryData<string>(widget.entry);

			widget.onTextSubmitted = text =>
			{
				data.onSubmitted(text);
				return text;
			};

			widget.entry.PlaceholderText = placeholderText.TryGetValue("Enter Text...");

			Add(widget.entry);

			return data;
		}

		public static EntryData<int> NumberField(int number)
		{
			var widget = MakeEntry(number.ToString());
			var data = new EntryData<int>(widget.entry);

			widget.onTextSubmitted = text =>
			{
				var val = 0;
				if (!string.IsNullOrEmpty(text))
					val = (int)Math.Round((float)Eval(text));
				data.onSubmitted(val);
				return val.ToString();
			};

			Add(widget.entry);

			return data;
		}

		public static EntryData<float> NumberField(float number)
		{
			var widget = MakeEntry(number.ToString());
			var data = new EntryData<float>(widget.entry);

			widget.onTextSubmitted = text =>
			{
				var val = 0f;
				if (!string.IsNullOrEmpty(text))
					val = (float)Eval(text);
				data.onSubmitted(val);
				return val.ToString();
			};

			Add(widget.entry);

			return data;
		}

		public static EntryData<double> NumberField(double number)
		{
			var widget = MakeEntry(number.ToString());
			var data = new EntryData<double>(widget.entry);

			widget.onTextSubmitted = text =>
			{
				var val = 0d;
				if (!string.IsNullOrEmpty(text))
					val = Eval(text);
				data.onSubmitted(val);
				return val.ToString();
			};

			Add(widget.entry);

			return data;
		}

		#endregion Fields

		public static CheckboxData Checkbox(bool? value)
		{
			var widget = new CheckButton();
			var data = new CheckboxData(widget);
			widget.Clicked += (sender, args) => data.onToggled(widget.Active);

			if (value.HasValue) widget.Active = value.Value;
			else widget.Inconsistent = true;
			Add(widget);

			return data;
		}

		public static ComboboxData Combobox(string[] options, string? current = null) => Combobox(options, Array.IndexOf(options, current));
		public static ComboboxData Combobox(string[] options, int current = -1)
		{
			var widget = new ComboBox(options) { Hexpand = true };
			var data = new ComboboxData(widget);

			widget.Active = current;

			if (widget.Active < 0) widget.Active = 0;

			widget.Changed += (sender, args) =>
			{
				data.onItemIndexChanged(widget.Active);
				data.onItemChanged(options[widget.Active]);
			};

			Add(widget);

			return data;
		}

		#endregion Controls

		#endregion

		public static void Icon(string name) => Add(new Image(IconTheme.Default.LoadIcon(name, 64, IconLookupFlags.ForceSymbolic)));

		public static void Image(string path) => Add(new Image($"{Environment.CurrentDirectory}/{path}"));

		#region Layout

		#region Containers

		public static void StartHorizontal(int spacing = 4, bool homogeneous = false)
		{
			var box = new Box(Orientation.Horizontal, spacing) { Homogeneous = homogeneous, };
			AddContainer(box);
		}

		public static void StartVertical(int spacing = 4, bool homogeneous = false)
		{
			var box = new Box(Orientation.Vertical, spacing) { Homogeneous = homogeneous };
			AddContainer(box);
		}

		public static void StartHorizontalFlow(int spacing = 4)
		{
			var widget = new FlowBox()
			{
				Orientation = Orientation.Horizontal,
				RowSpacing = (uint)spacing,
				ColumnSpacing = (uint)spacing,
				Homogeneous = true,
			};

			AddContainer(widget);
		}

		public static void StartProperty(string label, bool inline = true)
		{
			if (inline)
			{
				StartHorizontal();

				widthInChars.SetIfUnset(18);
				maxWidthInChars.SetIfUnset(18);

				tooltip.SetIfUnset(label);

				Label(label);
			}
			else
			{
				Label(label);

				var box = new Box(Orientation.Vertical, 4);
				AddContainer(box);
			}

		}

		#endregion Containers

		#region Bins

		public static ScrolledData Overflow()
		{
			var widget = new ScrolledWindow() { Hexpand = true, Vexpand = true, HscrollbarPolicy = PolicyType.Automatic, VscrollbarPolicy = PolicyType.Automatic, };
			var data = new ScrolledData(widget);

			AddContainer(widget);

			return data;
		}

		public static ScrolledData HorizontalOverflow()
		{
			var widget = new ScrolledWindow() { Hexpand = true, Vexpand = false, HscrollbarPolicy = PolicyType.Automatic, VscrollbarPolicy = PolicyType.Never, };
			var data = new ScrolledData(widget);

			AddContainer(widget);

			return data;
		}

		public static ScrolledData VerticalOverflow()
		{
			var widget = new ScrolledWindow() { Hexpand = false, Vexpand = true, HscrollbarPolicy = PolicyType.Never, VscrollbarPolicy = PolicyType.Automatic, };
			var data = new ScrolledData(widget);

			AddContainer(widget);

			return data;
		}

		public static FlexData HorizontalFlex(int position = 240)
		{
			var widget = new Paned(Orientation.Horizontal) { Vexpand = true, Hexpand = true, Position = position, PositionSet = true, };
			var data = new FlexData(widget);

			widget.MoveHandle += (sender, args) => data.onPositionChanged.Invoke(widget.Position);

			AddContainer(widget);

			return data;
		}

		#endregion Bins

		public static void Separator() => Add(new Separator(isHorizontal ? Orientation.Horizontal : Orientation.Vertical));

		public static void Flex() => Add(new Label() { Hexpand = isHorizontal, Vexpand = !isHorizontal, });

		#endregion Layout

		#region Popup Window

		static Gtk.Window? _window;

		public static void StartWindow(string title)
		{
			if (_window is not null) throw new Exception("EndWindow has not been called");

			_window = new(title);
			_window.ParentWindow = MGEEditorWindow.current.Window;
			_window.TypeHint = WindowTypeHint.Dialog;
			AddContainer(_window);
		}

		public static void EndWindow()
		{
			if (_window is null) throw new Exception("StartWindow has not been called");

			_window.ShowAll();
			PopContainer();
		}

		#endregion

		#region Menu

		static Stack<MenuShell?> _menuStack = new();
		static MenuShell? _menu;
		static MenuItem? _currentMenuItem;

		public static void AddToMenu(MenuItem widget)
		{
			if (_menu is null) throw new InvalidOperationException("There is no active menu");

			_menu.Append(widget);
			_currentMenuItem = widget;
		}

		public static void PushMenu(MenuShell menu)
		{
			_menuStack.Push(_menu);

			if (_currentMenuItem is not null)
			{
				_currentMenuItem.Submenu = menu;
				_currentMenuItem = null;
			}

			_menu = menu;
		}

		public static void StartMenu() => PushMenu(new Menu());
		public static void StartMenubar() => PushMenu(new MenuBar());

		public static MenuShell EndMenu()
		{
			var poppedMenu = _menu;
			_menu = _menuStack.Pop();
			_currentMenuItem = null;

			return poppedMenu ?? throw new InvalidOperationException("Start menu has not been called yet");
		}

		public static void MenuPopup(Event e)
		{
			var menu = (Menu)EndMenu();
			menu.ShowAll();
			menu.PopupAtPointer(e);
		}

		public static void MenuSeparator() => AddToMenu(new SeparatorMenuItem());

		public static MenuItemData MenuButton(string label)
		{
			var widget = new MenuItem(label);
			var data = new MenuItemData(widget);

			widget.Activated += (sender, args) => data.onPressed();
			AddToMenu(widget);

			return data;
		}

		public static CheckboxMenuItemData MenuCheckbox(string label, bool state)
		{
			var widget = new CheckMenuItem(label) { Active = state };
			var data = new CheckboxMenuItemData(widget);

			widget.Activated += (sender, args) => data.onToggled(widget.Active);
			AddToMenu(widget);

			return data;
		}

		#endregion Menu

		#region Utils

		public static void Value<T>(T obj, Action<T> onObjectChanged) where T : notnull
		{
			var drawer = EditorGUI.GetDrawer(obj);

			Add(drawer);
		}

		public static void Inspector<T>(T obj, Action<T> onObjectChanged)
		{
			// Todo properly support null
			if (obj is null)
			{
				EditorGUI.Text("(null)");
				return;
			}

			var props = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
			Inspector(obj, props.GetEnumerator(), onObjectChanged);
		}

		public static void Inspector<T>(T obj, System.Collections.IEnumerator props, Action<T> onObjectChanged)
		{
			if (obj is null)
			{
				EditorGUI.Text("(null)");
				return;
			}

			while (props.MoveNext())
			{
				var prop = (PropertyInfo)props.Current;

				EditorGUI.tooltip = prop.Name;
				EditorGUI.StartProperty(Editor.GetPropertyName(prop.Name));

				var value = prop.GetValue(obj);

				if (value is null)
				{
					EditorGUI.Text("(null)");
					return;
				}

				Value(value, val => { prop.SetValue(obj, val); onObjectChanged(obj); });

				EditorGUI.End();
			}
		}

		static double Eval(string expression) => ExpressionParser.Parse(expression).Eval(_dictionaryContext);

		#endregion Utils
	}
}
