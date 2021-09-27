using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Gdk;
using Gtk;
using MGE.Editor.GUI.Events;
using Key = Gdk.Key;

namespace MGE.Editor.GUI
{
	public abstract class EditorWindow
	{
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
			StartHorizontal(16);
			Add(new Label(text) { Xalign = 0, WidthRequest = wasHorizontal ? 0 : 128 });

			_inLabel = true;
		}

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

		// TODO Select all the text when the feild is selected
		public TextFeildEvents TextFeild(string text)
		{
			var textFeild = new Entry(text) { Hexpand = true, PlaceholderText = "Enter Text...", Sensitive = true, };

			var events = new TextFeildEvents();

			// Select everything when focused
			textFeild.FocusInEvent += (sender, args) =>
			{
				textFeild.SelectRegion(0, -1);
			};

			textFeild.ButtonPressEvent += (sender, args) =>
			{
				Trace.WriteLine("Pressed");
				textFeild.SelectRegion(0, -1);
			};

			textFeild.KeyPressEvent += (sender, args) =>
			{
				// Trace.WriteLine("Key " + args.Event.Key);
				switch (args.Event.Key)
				{
					case Key.Return:
					case Key.ISO_Enter:
					case Key.KP_Enter:
						textFeild.HasFocus = false;
						textFeild.SelectRegion(0, 0);
						events.onTextSubmitted.Invoke(textFeild.Text);
						break;
				}
			};

			textFeild.EditingDone += (sender, args) =>
			{
				Trace.WriteLine("Done");
				textFeild.SelectRegion(0, 0);
				// textFeild.IsFocus = false;
			};

			Add(textFeild);

			return events;
		}

		#endregion

		#region Layout

		public void StartHorizontal(int spacing = 0) => Add(new Box(Orientation.Horizontal, spacing));

		public void End() => PopContainer();

		public void Separator() => Add(_isHorizontal ? new VSeparator() : new HSeparator());

		#endregion

		protected virtual void Update() { }
	}
}
