using System;
using System.Collections.Generic;
using Gtk;

namespace MGE.Editor.GUI
{
	public abstract class EditorWindow
	{
		#region Containers

		Stack<Container> _containerStack = new Stack<Container>();
		Container _container;
		int _uiDepth;

		void PushContainer(Container container)
		{
			_containerStack.Push(container);
			_container = container;
			_uiDepth++;
		}

		Container PopContainer()
		{
			if (_uiDepth < 1) throw new Exception("Can not pop any further");

			var container = _containerStack.Pop();
			_container = _containerStack.Peek();
			_uiDepth--;

			return container;
		}

		#endregion

		public EditorWindow()
		{
			_container = new ListBox();
		}

		Widget AddWidget(Widget widget)
		{
			_container.Add(widget);

			if (widget is Container container)
			{
				PushContainer(container);
			}

			return widget;
		}

		public void Lable(string text) => _container.Add(new Label(text));

		public void TextInput(string text, Action<string> onTextChanged)
		{
			var textInput = new Entry(text);
			// textInput.Changed += (sender, args) => ;
			_container.Add(textInput);
		}

		public void StartHBox(bool homogeneous = false, int spacing = 0) => AddWidget(new HBox(homogeneous, spacing));
		public void StartVBox(bool homogeneous = false, int spacing = 0) => AddWidget(new VBox(homogeneous, spacing));

		public void End() => PopContainer();
	}
}
