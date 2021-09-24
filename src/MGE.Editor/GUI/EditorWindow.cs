using System;
using System.Collections.Generic;
using Gtk;
using Action = System.Action;

namespace MGE.Editor.GUI
{
	public abstract class EditorWindow
	{
		#region Containers

		Stack<Container> _containerStack = new Stack<Container>();
		Container _container;
		int _uiDepth;
		bool _isHorizontal;
		bool _inLabel;

		void PushContainer(Container container)
		{
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

			return container;
		}

		void ContainerChanged()
		{
			_isHorizontal = _container is HBox;
		}

		#endregion

		public EditorWindow()
		{
			_container = new ListBox();
		}

		#region Widgets

		Widget Add(Widget widget)
		{
			_container.Add(widget);

			if (widget is Container container)
			{
				PushContainer(container);
			}

			if (_inLabel)
			{
				_inLabel = false;
				PopContainer();
			}

			return widget;
		}

		public void Text(string text) => Add(new Label(text));

		public void Label(string text)
		{
			_inLabel = true;
			StartHorizontal();
			Add(new Label(text));
		}

		public Action<string> TextFeild(string text)
		{
			var textFeild = new Entry(text);
			Add(textFeild);

			Action<string> onClicked = text => { };
			textFeild.Changed += (sender, args) => onClicked.Invoke(textFeild.Text);
			return onClicked;
		}

		public Action Button(string text)
		{
			var button = new Button(text);
			Add(button);

			Action onClicked = () => { };
			button.Clicked += (sender, args) => onClicked.Invoke();
			return onClicked;
		}

		#endregion

		#region Layout

		public void StartHorizontal(bool homogeneous = false, int spacing = 0) => Add(new HBox(homogeneous, spacing));

		public void End() => PopContainer();

		public void Separator() => Add(_isHorizontal ? new VSeparator() : new HSeparator());

		#endregion
	}
}
