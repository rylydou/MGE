using System;
using System.Collections.Generic;
using Gtk;
using MGE.Editor.GUI.Events;
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

		#region Menubar

		// TODO
		public static ButtonEvents MenubarButton(string title)
		{
			var events = new ButtonEvents();

			var segments = title.Split('/');

			foreach (var segment in segments)
			{

			}

			return events;
		}

		#endregion

		#region Properties

		public static Optional<bool> sensitive = new();
		public static Optional<string?> tooltip = new();
		public static Optional<int> width = new();
		public static Optional<int> height = new();
		public static Optional<bool> xExpand = new();
		public static Optional<bool> yExpand = new();

		public static Optional<float> xAlign = new();
		public static Optional<int> widthInChars = new();
		public static Optional<int> maxWidthInChars = new();
		public static Optional<EllipsizeMode> ellipsizeMode = new();

		public static string? placeholderText;

		static void ResetProperties()
		{
			sensitive.Unset();
			tooltip.Unset();
			width.Unset();
			height.Unset();
			xExpand.Unset();
			yExpand.Unset();

			xAlign.Unset();
			widthInChars.Unset();
			maxWidthInChars.Unset();
			ellipsizeMode.Unset();

			placeholderText = null;
		}

		#endregion

		public static Container container
		{
			get
			{
				if (_container is null) throw new NullReferenceException("No active container");
				return _container;
			}
		}
		// TODO
		public static bool isHorizontalContainer;
		public static bool inLabel;
		static Container? _container;
		static Stack<Container> _containerStack = new Stack<Container>();

		public static void PushContainer(Container container)
		{
			if (_container is not null)
				_containerStack.Push(_container);
			_container = container;
		}

		public static void PopContainer()
		{
			if (_containerStack.TryPop(out var container))
				_container = container;
			else
				_container = null;
		}

		internal static void SetContainer(Container container)
		{
			_containerStack.Clear();
			_container = container;
		}

		public static Widget Add(Widget widget)
		{
			sensitive.TrySetValue(value => widget.Sensitive = value);
			tooltip.TrySetValue(value => widget.TooltipText = value);
			width.TrySetValue(value => widget.WidthRequest = value);
			xExpand.TrySetValue(value => widget.Hexpand = value);
			yExpand.TrySetValue(value => widget.Vexpand = value);

			container.Add(widget);

			ResetProperties();

			return widget;
		}

		#region Widgets

		public static void Text(string text) => Add(new Label(text)
		{
			Xalign = xAlign.GetValueOnDefualt(0),
			WidthChars = widthInChars.GetValueOnDefualt(-1),
			MaxWidthChars = maxWidthInChars.GetValueOnDefualt(-1),
			Ellipsize = ellipsizeMode.GetValueOnDefualt(EllipsizeMode.End),
		});

		public static void Header(string text) => Add(new Label(text)
		{
			Xalign = xAlign.GetValueOnDefualt(0.5f),
			WidthChars = widthInChars.GetValueOnDefualt(-1),
			MaxWidthChars = maxWidthInChars.GetValueOnDefualt(-1),
			Ellipsize = ellipsizeMode.GetValueOnDefualt(EllipsizeMode.End),
		});

		public static void Label(string text) => Add(new Label(text)
		{
			Xalign = xAlign.GetValueOnDefualt(0),
			WidthChars = widthInChars.GetValueOnDefualt(32),
			MaxWidthChars = maxWidthInChars.GetValueOnDefualt(32),
			Ellipsize = ellipsizeMode.GetValueOnDefualt(EllipsizeMode.End),
			TooltipText = text.Length > 32 ? text : null,
		});

		#region Buttons

		public static ButtonEvents Button(string text)
		{
			var button = new Button(text) { Hexpand = true };
			Add(button);

			var events = new ButtonEvents();
			button.Clicked += (sender, args) => events.onPressed.Invoke();
			return events;
		}

		public static ButtonEvents IconButton(string icon)
		{
			var button = new Button(icon, IconSize.Button);
			Add(button);

			var events = new ButtonEvents();
			button.Clicked += (sender, args) => events.onPressed.Invoke();
			return events;
		}

		public static ToggleEvents Checkbox(bool? value)
		{
			var button = new CheckButton();
			if (value.HasValue) button.Active = value.Value;
			else button.Inconsistent = true;
			Add(button);

			var events = new ToggleEvents();
			button.Clicked += (sender, args) => events.onToggled.Invoke(button.Active);
			return events;
		}

		#endregion

		#region Feilds

		static Entry MakeEntry(string text, Func<string, string> onTextSubmitted)
		{
			var entry = new Entry(text) { Hexpand = true };
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
				// entry.SelectRegion(0, 0);
				// Works well enough
				entry.Sensitive = false;
				entry.Sensitive = true;

				if (originalText != entry.Text)
				{
					try
					{
						entry.Text = onTextSubmitted.Invoke(entry.Text);
					}
					catch (System.Exception)
					{
						entry.Text = originalText;
					}
				}
			};

			return entry;
		}

		public static TextFeildEvents TextFeild(string text)
		{
			var events = new TextFeildEvents();
			var entry = MakeEntry(text, text =>
			{
				events.onSubmitted.Invoke(text);
				return text;
			});

			entry.PlaceholderText = placeholderText ?? "Enter Text...";

			Add(entry);

			return events;
		}

		public static NumberFeildEvents<float> NumberFeild(float number)
		{
			var events = new NumberFeildEvents<float>();
			var entry = MakeEntry(number.ToString(), text =>
			{
				var val = 0f;
				if (!string.IsNullOrEmpty(text))
					val = (float)Eval(text);
				events.onSubmitted.Invoke(val);
				return val.ToString();
			});

			entry.PlaceholderText = placeholderText ?? "Enter Float...";

			Add(entry);

			return events;
		}

		public static NumberFeildEvents<double> NumberFeild(double number)
		{
			var events = new NumberFeildEvents<double>();
			var entry = MakeEntry(number.ToString(), text =>
			{
				var val = 0d;
				if (!string.IsNullOrEmpty(text))
					val = Eval(text);
				events.onSubmitted.Invoke(val);
				return val.ToString();
			});

			entry.PlaceholderText = placeholderText ?? "Enter Double...";

			Add(entry);

			return events;
		}

		public static NumberFeildEvents<int> NumberFeild(int number)
		{
			var events = new NumberFeildEvents<int>();
			var entry = MakeEntry(number.ToString(), text =>
			{
				var val = 0;
				if (!string.IsNullOrEmpty(text))
					val = (int)Math.Round(Eval(text));
				events.onSubmitted.Invoke(val);
				return val.ToString();
			});

			entry.PlaceholderText = placeholderText ?? "Enter Integer...";

			Add(entry);

			return events;
		}

		#endregion

		// public ListEvents<T> ListEntry<T>(IEnumerable<T> list)
		// {
		// 	var events = new ListEvents<T>();

		// 	var model = new ListStore();
		// 	var tree = new TreeView();

		// 	return events;
		// }

		#endregion

		#region Layout

		public static void StartHorizontal(int spacing = 0)
		{
			var box = new Box(Orientation.Horizontal, spacing);
			Add(box);
			PushContainer(box);
		}

		public static void StartProperty(string label, bool inline = true)
		{
			if (inline)
			{
				StartHorizontal();

				Label(label);
			}
			else
			{
				Label(label);

				var lb = new ListBox();
				Add(lb);
				PushContainer(lb);
			}

		}

		public static void End() => PopContainer();

		public static void Separator() => Add(isHorizontalContainer ? new VSeparator() : new HSeparator());

		#endregion

		static double Eval(string expr) => ExpressionParser.Parse(expr).Eval(_dictionaryContext);
	}
}
