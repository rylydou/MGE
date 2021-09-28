using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Gdk;
using Gtk;
using MGE.Editor.GUI.Events;
using MGE.Editor.Util;
using Key = Gdk.Key;

namespace MGE.Editor.GUI
{
	public abstract class EditorWindow
	{
		static ExpressionDictionaryContext dictionaryContext = new ExpressionDictionaryContext(new Dictionary<string, double>()
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

			{ "round", (args) => Math.Round(args[0]) },
			{ "floor", (args) => Math.Floor(args[0]) },
			{ "ceil", (args) => Math.Ceiling(args[0]) },
			{ "trunc", (args) => Math.Truncate(args[0]) },

			{ "min", (args) => Math.Min(args[0], args[1]) },
			{ "max", (args) => Math.Max(args[0], args[1]) },
			{ "clamp", (args) => Math.Clamp(args[0], args[1], args[2]) },
		});

		public ListBox root;

		#region Container Management

		Stack<Container> _containerStack = new Stack<Container>();
		Container _container;
		int _uiDepth;
		bool _isHorizontal;
		bool _inLabel;

		[MemberNotNull(nameof(_container))]
		void PushContainer(Container container)
		{
			_uiDepth++;

			_containerStack.Push(container);
			_container = container;

			Trace.WriteLine($"Pushed '{container}' to container stack\tdepth: {_uiDepth}");

			ContainerChanged();
		}

		Container PopContainer()
		{
			if (_uiDepth < 1) throw new Exception("Can not pop any further");
			_uiDepth--;

			var container = _containerStack.Pop();

			_container = _containerStack.Peek();

			ContainerChanged();

			Trace.WriteLine($"Poped '{container}' from container stack\tdepth: {_uiDepth}");

			return container;
		}

		void ContainerChanged()
		{
			_isHorizontal = _container is Box box && box.Orientation == Orientation.Horizontal;
		}

		#endregion

		protected EditorWindow()
		{
			root = new ListBox() { SelectionMode = SelectionMode.None, FocusOnClick = false, ActivateOnSingleClick = false };
			PushContainer(root);
			_uiDepth = 0;

			Update();
		}

		#region Widgets

		Widget Add(Widget widget)
		{
			Trace.WriteLine($"Adding {widget} to {_container}\t\tdepth: {_uiDepth}");

			_container.Add(widget);

			if (widget is Container container && (widget is ListBox || widget is Box))
			{
				PushContainer(container);
			}
			else if (_inLabel)
			{
				_inLabel = false;
				PopContainer();
			}

			return widget;
		}

		public void Text(string text) => Add(new Label(text) { Xalign = 0 });

		public void Label(string text)
		{
			var wasHorizontal = _isHorizontal;
			StartHorizontal();
			Add(new Label(text) { Xalign = 0, WidthChars = wasHorizontal ? -1 : 18, });

			_inLabel = true;
		}

		#region Buttons

		public ButtonEvents Button(string text)
		{
			var button = new Button(text) { Vexpand = true };
			Add(button);

			var events = new ButtonEvents();
			button.Clicked += (sender, args) => events.onPressed.Invoke();
			return events;
		}

		public ToggleEvents Checkbox(bool value)
		{
			var button = new CheckButton() { Active = value };
			Add(button);

			var events = new ToggleEvents();
			button.Clicked += (sender, args) => events.onToggled.Invoke(button.Active);
			return events;
		}

		#endregion

		#region Feilds

		public TextFeildEvents TextFeild(string text)
		{
			var events = new TextFeildEvents();
			var entry = MakeEntry(text, text =>
			{
				events.onSubmitted.Invoke(text);
				return text;
			});

			Add(entry);

			return events;
		}

		public NumberFeildEvents<float> NumberFeild(float number)
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

			Add(entry);

			return events;
		}

		public NumberFeildEvents<double> NumberFeild(double number)
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

			Add(entry);

			return events;
		}

		public NumberFeildEvents<int> NumberFeild(int number)
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

		public void StartHorizontal(int spacing = 0) => Add(new Box(Orientation.Horizontal, spacing));

		public void End() => PopContainer();

		public void Separator() => Add(_isHorizontal ? new VSeparator() : new HSeparator());

		#endregion

		#region Internal Utils

		Entry MakeEntry(string text, Func<string, string> onTextSubmitted)
		{
			var entry = new Entry(text) { Hexpand = true, PlaceholderText = "Enter Text...", Sensitive = true, };
			var originalText = string.Empty;

			// FIXME Select everything when focused
			entry.FocusInEvent += (sender, args) =>
			{
				originalText = entry.Text;
				// entry.SelectRegion(0, -1);
				entry.GrabFocus();
			};

			// Finish when escape is pressed
			entry.KeyPressEvent += (sender, args) =>
			{
				switch (args.Event.Key)
				{
					// Reset the text feild when escape is pressed
					case Key.Escape:
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
				entry.SelectRegion(0, 0);
				entry.UnsetStateFlags(StateFlags.Selected);
				entry.UnsetStateFlags(StateFlags.Focused);
				entry.UnsetStateFlags(StateFlags.Active);

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

		public double Eval(string expr)
		{
			return ExpressionParser.Parse(expr).Eval(dictionaryContext);
		}

		#endregion

		protected virtual void Update() { }
	}
}
