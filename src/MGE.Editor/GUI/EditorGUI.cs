using System;
using System.Collections.Generic;
using Gtk;
using MGE.Editor.GUI.Data;
using MGE.Editor.Util;
using Pango;

namespace MGE.Editor.GUI
{
	public static class EditorGUI
	{
		static ExpressionDictionaryContext _dictionaryContext = new ExpressionDictionaryContext(new()
		{
			{ "pi", () => Math.PI },
			{ "tau", () => Math.Tau },
			{ "e", () => Math.E },
			{ "inf", () => double.PositiveInfinity },
			{ "ninf", () => double.NegativeInfinity },
			{ "nan", () => double.NaN },

			{ "rand", () => new Random().NextDouble() },
		}, new Dictionary<string, Func<double[], double>>()
		{
			{ "sin", (args) => Math.Sin(args[0]) },
			{ "cos", (args) => Math.Cos(args[0]) },
			{ "tan", (args) => Math.Tan(args[0]) },

			{ "asin", (args) => Math.Asin(args[0]) },
			{ "acos", (args) => Math.Acos(args[0]) },
			{ "atan", (args) => Math.Atan(args[0]) },

			{ "pow", (args) => Math.Pow(args[0], args[1]) },

			{ "abs", (args) => Math.Abs(args[0]) },
			{ "sign", (args) => Math.Sign(args[0]) },

			{ "round", (args) => Math.Round(args[0]) },
			{ "floor", (args) => Math.Floor(args[0]) },
			{ "ceil", (args) => Math.Ceiling(args[0]) },
			{ "trunc", (args) => Math.Truncate(args[0]) },

			{ "min", (args) => Math.Min(args[0], args[1]) },
			{ "max", (args) => Math.Max(args[0], args[1]) },
			{ "clamp", (args) => Math.Clamp(args[0], args[1], args[2]) },
		});

		#region Properties

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

		static void ResetProperties()
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

		#endregion

		static Stack<Container> _containerStack = new();
		static Container? _container;
		public static Container container
		{
			get
			{
				if (_container is null) throw new NullReferenceException("No active container");
				return _container;
			}
		}
		public static bool isHorizontal { get; private set; }

		public static void PushContainer(Container container)
		{
			if (_container is not null)
				_containerStack.Push(_container);
			_container = container;
			UpdateContainer();
		}

		public static void PopContainer()
		{
			if (_containerStack.TryPop(out var container))
				_container = container;
			else
				_container = null;
			UpdateContainer();
		}

		static void UpdateContainer()
		{
			isHorizontal = _container is Box box && box.Orientation == Orientation.Horizontal;
		}

		internal static void SetContainer(Container container)
		{
			_containerStack.Clear();
			_container = container;
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

			container.Add(widget);

			ResetProperties();

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

		#region Buttons

		public static ButtonData Button(string text)
		{
			var widget = new Button(text) { Hexpand = true };
			var data = new ButtonData(widget);
			widget.Clicked += (sender, args) => data.onPressed();

			Add(widget);

			return data;
		}

		public static ButtonData IconButton(string icon)
		{
			var widget = new Button(icon, IconSize.Button);
			var data = new ButtonData(widget);

			widget.StyleContext.AddClass("icon");
			Add(widget);

			widget.Clicked += (sender, args) => data.onPressed();
			return data;
		}

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

		#endregion

		#region Feilds

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
					// Reset the text feild when escape is pressed
					case Gdk.Key.Escape:
						widget.Text = originalText;
						widget.FinishEditing();
						break;
				}
			};

			// Finish when enter is pressed
			widget.Activated += (sender, args) => widget.FinishEditing();
			// Finish when the feild is unfocused
			widget.FocusOutEvent += (sender, args) => widget.FinishEditing();

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

		public static EntryData<string> TextFeild(string text)
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

		public static EntryData<int> NumberFeild(int number)
		{
			var widget = MakeEntry(number.ToString());
			var data = new EntryData<int>(widget.entry);

			widget.onTextSubmitted = text =>
			{
				var val = 0;
				if (!string.IsNullOrEmpty(text))
					val = (int)Math.Round(Eval(text));
				data.onSubmitted(val);
				return val.ToString();
			};

			Add(widget.entry);

			return data;
		}

		public static EntryData<float> NumberFeild(float number)
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

		public static EntryData<double> NumberFeild(double number)
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

		#endregion

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

		#endregion

		#region Layout

		public static void StartHorizontal(int spacing = 4, bool homogeneous = false)
		{
			var box = new Box(Orientation.Horizontal, spacing) { Homogeneous = homogeneous };
			Add(box);
			PushContainer(box);
		}

		public static void StartHorizonalFlow(int rowspacing = 8, int columnSpacing = 8)
		{
			var flow = new FlowBox()
			{
				Orientation = Orientation.Horizontal,
				RowSpacing = (uint)rowspacing,
				ColumnSpacing = (uint)columnSpacing,
			};
			Add(flow);
			PushContainer(flow);
		}

		public static void StartNotebook(bool tabsOnSide)
		{
			var notebook = new Notebook() { TabPos = tabsOnSide ? PositionType.Left : PositionType.Top };

			Add(notebook);
			PushContainer(notebook);
		}

		public static ScrolledData StartScrolled()
		{
			var scrolled = new ScrolledWindow();
			var data = new ScrolledData(scrolled);

			return data;
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
				Add(box);
				PushContainer(box);
			}

		}

		public static void End() => PopContainer();

		public static void Separator() => Add(isHorizontal ? new VSeparator() : new HSeparator());

		public static void Expand() => Add(new Box(Orientation.Horizontal, 0) { Hexpand = isHorizontal, Vexpand = !isHorizontal, });

		#endregion

		#region Menu

		static Menu? _menu;

		public static void StartMenu()
		{
			_menu = new Menu();
		}

		public static void EndMenu()
		{
			if (_menu is null) throw new Exception($"{nameof(StartMenu)} has not been called yet");

			_menu.ShowAll();
			_menu.Popup();
		}

		public static void MenuSeparator()
		{
			if (_menu is null) throw new Exception($"{nameof(StartMenu)} has not been called yet");

			_menu.Add(new SeparatorMenuItem());
		}

		public static MenuItemData MenuButton(string label)
		{
			if (_menu is null) throw new Exception($"{nameof(StartMenu)} has not been called yet");

			var widget = new MenuItem(label);
			var data = new MenuItemData(widget);

			widget.Activated += (sender, args) => data.onClicked();
			_menu.Add(widget);

			return data;
		}

		public static CheckboxMenuItemData MenuCheckbox(string label, bool state)
		{
			if (_menu is null) throw new Exception($"{nameof(StartMenu)} has not been called yet");

			var widget = new CheckMenuItem(label) { Active = state };
			var data = new CheckboxMenuItemData(widget);

			widget.Activated += (sender, args) => data.onToggled(widget.Active);
			_menu.Add(widget);

			return data;
		}

		#endregion

		static double Eval(string expression) => ExpressionParser.Parse(expression).Eval(_dictionaryContext);
	}
}
