using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Gtk;
using MGE.Editor.GUI.Data;
using MGE.Editor.GUI.Datas;
using MGE.Editor.Util;
using Pango;

namespace MGE.Editor.GUI
{
	public static class EditorGUI
	{
		static ExpressionDictionaryContext _dictionaryContext = new ExpressionDictionaryContext(new Dictionary<string, double>()
		{
			{ "pi", Math.PI },
			{ "tau", Math.Tau },
			{ "e", Math.E },
			{ "inf", double.PositiveInfinity },
			{ "ninf", double.NegativeInfinity },
			{ "nan", double.NaN },
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
			var label = new Label(text)
			{
				Xalign = horizontalAlign.TryGetValue(0),
				WidthChars = widthInChars.TryGetValue(-1),
				MaxWidthChars = maxWidthInChars.TryGetValue(-1),
				Ellipsize = ellipsizeMode.TryGetValue(EllipsizeMode.End),
			};
			var data = new TextData(label);

			Add(label);

			return data;
		}

		public static TextData Label(string text)
		{
			var label = new Label(text)
			{
				Xalign = horizontalAlign.TryGetValue(0),
				WidthChars = widthInChars.TryGetValue(-1),
				MaxWidthChars = maxWidthInChars.TryGetValue(-1),
				Ellipsize = ellipsizeMode.TryGetValue(EllipsizeMode.End),
			};
			var data = new TextData(label);

			Add(label);

			return data;
		}

		public static TextData Header(string text)
		{
			var label = new Label(text)
			{
				Xalign = horizontalAlign.TryGetValue(0.5f),
				WidthChars = widthInChars.TryGetValue(-1),
				MaxWidthChars = maxWidthInChars.TryGetValue(-1),
				Ellipsize = ellipsizeMode.TryGetValue(EllipsizeMode.End),
			};
			var data = new TextData(label);

			label.StyleContext.AddClass("header");
			Add(label);

			return data;
		}

		public static TextBoxData TextBox(string text)
		{
			var textView = new TextView() { Editable = false, WrapMode = Gtk.WrapMode.Word, };
			var data = new TextBoxData(textView);

			textView.Buffer.Text = text;
			Add(textView);

			return data;
		}

		#region Buttons

		public static ButtonData Button(string text)
		{
			var button = new Button(text) { Hexpand = true };
			var data = new ButtonData(button);
			button.Clicked += (sender, args) => data.onPressed.Invoke();

			Add(button);

			return data;
		}

		public static ButtonData IconButton(string icon)
		{
			var button = new Button(icon, IconSize.Button);
			var data = new ButtonData(button);

			button.StyleContext.AddClass("icon");
			Add(button);

			button.Clicked += (sender, args) => data.onPressed.Invoke();
			return data;
		}

		public static CheckboxData Checkbox(bool? value)
		{
			var checkbox = new CheckButton();
			var data = new CheckboxData(checkbox);
			checkbox.Clicked += (sender, args) => data.onToggled.Invoke(checkbox.Active);

			if (value.HasValue) checkbox.Active = value.Value;
			else checkbox.Inconsistent = true;
			Add(checkbox);

			return data;
		}

		#endregion

		#region Feilds

		static InternalEntryData MakeEntry(string text)
		{
			var entry = new Entry(text) { Hexpand = true, WidthRequest = 0, WidthChars = 0, MaxWidthChars = 0, };
			var entryEvent = new InternalEntryData(entry);
			var originalText = string.Empty;

			// FIXME Select everything when focused
			entry.FocusInEvent += (sender, args) =>
			{
				originalText = entry.Text;
				entry.SelectRegion(0, -1);
			};

			entry.KeyPressEvent += (sender, args) =>
			{
				switch (args.Event.Key)
				{
					// Reset the text feild when escape is pressed
					case Gdk.Key.Escape:
						entry.Text = originalText;
						entry.FinishEditing();
						break;
				}
			};

			// Finish when enter is pressed
			entry.Activated += (sender, args) => entry.FinishEditing();
			// Finish when the feild is unfocused
			entry.FocusOutEvent += (sender, args) => entry.FinishEditing();

			entry.EditingDone += (sender, args) =>
			{
				// HACK Works well enough, but is not good for keyboard users
				entry.Sensitive = false;
				entry.Sensitive = true;

				if (originalText != entry.Text)
				{
					try
					{
						entry.Text = entryEvent.onTextSubmitted.Invoke(entry.Text);
					}
					catch (System.Exception)
					{
						entry.Text = originalText;
					}
				}
			};

			return entryEvent;
		}

		public static EntryData<string> TextFeild(string text)
		{
			var entry = MakeEntry(text);
			var events = new EntryData<string>(entry.entry);

			entry.onTextSubmitted = text =>
			{
				events.onSubmitted.Invoke(text);
				return text;
			};

			entry.entry.PlaceholderText = placeholderText.TryGetValue("Enter Text...");

			Add(entry.entry);

			return events;
		}

		public static EntryData<int> NumberFeild(int number)
		{
			var entry = MakeEntry(number.ToString());
			var events = new EntryData<int>(entry.entry);

			entry.onTextSubmitted = text =>
			{
				var val = 0;
				if (!string.IsNullOrEmpty(text))
					val = (int)Math.Round(Eval(text));
				events.onSubmitted.Invoke(val);
				return val.ToString();
			};

			Add(entry.entry);

			return events;
		}

		public static EntryData<float> NumberFeild(float number)
		{
			var entry = MakeEntry(number.ToString());
			var events = new EntryData<float>(entry.entry);

			entry.onTextSubmitted = text =>
			{
				var val = 0f;
				if (!string.IsNullOrEmpty(text))
					val = (float)Eval(text);
				events.onSubmitted.Invoke(val);
				return val.ToString();
			};

			Add(entry.entry);

			return events;
		}

		public static EntryData<double> NumberFeild(double number)
		{
			var entry = MakeEntry(number.ToString());
			var events = new EntryData<double>(entry.entry);

			entry.onTextSubmitted = text =>
			{
				var val = 0d;
				if (!string.IsNullOrEmpty(text))
					val = Eval(text);
				events.onSubmitted.Invoke(val);
				return val.ToString();
			};

			Add(entry.entry);

			return events;
		}

		#endregion

		public static ComboboxData Combobox(string[] options, string? current = null) => Combobox(options, Array.IndexOf(options, current));
		public static ComboboxData Combobox(string[] options, int current = -1)
		{
			var combobox = new ComboBox(options) { Hexpand = true };
			var events = new ComboboxData(combobox);

			combobox.Active = current;

			if (combobox.Active < 0) combobox.Active = 0;

			combobox.Changed += (sender, args) =>
			{
				events.onItemIndexChanged.Invoke(combobox.Active);
				events.onItemChanged.Invoke(options[combobox.Active]);
			};

			Add(combobox);

			return events;
		}

		// public ListEvents<T> ListEntry<T>(IEnumerable<T> list)
		// {
		// 	var events = new ListEvents<T>();

		// 	var model = new ListStore();
		// 	var tree = new TreeView();

		// 	return events;
		// }

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

		static double Eval(string expression) => ExpressionParser.Parse(expression).Eval(_dictionaryContext);
	}
}
