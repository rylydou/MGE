using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Gtk;
using Action = System.Action;

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
			Trace.WriteLine($"Pushing '{container}' to container stack - depth: {_uiDepth}");

			_uiDepth++;

			_containerStack.Push(container);
			_container = container;

			ContainerChanged();
		}

		Container PopContainer()
		{

			if (_uiDepth < 1) throw new Exception("Can not pop any further");
			_uiDepth--;

			var container = _containerStack.Pop();

			_container = _containerStack.Peek();

			ContainerChanged();

			Trace.WriteLine($"Poped '{container}' from container stack - depth: {_uiDepth}");

			return container;
		}

		void ContainerChanged()
		{
			_isHorizontal = _container is HBox;
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
			Trace.WriteLine($"Adding {widget} to {_container} - depth: {_uiDepth}");

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

		public void Text(string text) => Add(new Label(text));

		public void Label(string text)
		{
			StartHorizontal(16);
			Add(new Label(text) { Xalign = 0, WidthRequest = 128 });
			_inLabel = true;
		}

		public Action<string> TextFeild(string text)
		{
			var textFeild = new Entry(text) { Hexpand = true, PlaceholderText = "Enter Text..." };
			Add(textFeild);

			Action<string> onClicked = text => { };
			textFeild.Changed += (sender, args) => onClicked.Invoke(textFeild.Text);
			return onClicked;
		}

		public Action Button(string text)
		{
			var button = new Button(text) { Vexpand = true };
			Add(button);

			Action onClicked = () => { };
			button.Clicked += (sender, args) => onClicked.Invoke();
			return onClicked;
		}

		public Action<bool> Checkbox(bool value)
		{
			var button = new CheckButton() { Active = value };
			Add(button);

			Action<bool> onClicked = state => { };
			button.StateChanged += (sender, args) => onClicked.Invoke(button.Active);
			return onClicked;
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
