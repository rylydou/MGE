// using System;
// using System.Collections.Generic;

// namespace Demo;

// public class MenuData
// {
// 	public class MenuButtonData
// 	{


// 		internal MenuButtonData(MenuData menu, string text)
// 		{
// 			this.menu = menu;
// 			this.text = text;
// 		}

// 		internal bool Press()
// 		{
// 			onPress?.Invoke(this);
// 			return onPress is not null;
// 		}
// 		public Func<MenuButtonData, bool>? onPress;

// 		internal bool Increment()
// 		{
// 			onIncrement?.Invoke(this);
// 			return onIncrement is not null;
// 		}
// 		public Func<MenuButtonData, bool> onIncrement = item => false;

// 		internal bool Decrement()
// 		{
// 			onDecrement?.Invoke(this);
// 			return onDecrement is not null;
// 		}
// 		public Func<MenuButtonData, bool> onDecrement = item => false;

// 		internal void Enter() => onEnter(this);
// 		public Action<MenuButtonData> onExit = item => { };
// 		internal void Exit() => onExit(this);
// 		public Action<MenuButtonData> onEnter = item => { };
// 	}

// 	public abstract class MenuItem
// 	{
// 		public string text;
// 		public MenuData menu { get; init; }

// 		protected MenuItem(MenuData menu, string text)
// 		{
// 			this.menu = menu;
// 			this.text = text;
// 		}
// 	}

// 	public bool isHorizontal;

// 	public MenuButtonData currentItem => _items[selection];

// 	int _selection;
// 	public int selection
// 	{
// 		get => _selection;
// 		set
// 		{
// 			if (value < 0 || value >= _items.Count)
// 				throw new ArgumentOutOfRangeException(nameof(value));

// 			if (_selection == value) return;
// 			_selection = value;

// 			_items[_selection].Exit();
// 			_items[value].Enter();

// 			_selection = value;
// 		}
// 	}

// 	List<MenuButtonData> _items = new();

// 	public MenuButtonData AddButton(string text)
// 	{
// 		var item = new MenuButtonData(this, text);
// 		_items.Add(item);
// 		return item;
// 	}

// 	bool MoveForward()
// 	{
// 		if (selection <= 0) return false;
// 		selection++;
// 		return true;
// 	}

// 	bool MoveBackward()
// 	{
// 		if (selection >= _items.Count) return false;
// 		selection--;
// 		return true;
// 	}

// 	public bool MoveUp()
// 	{
// 		if (isHorizontal)
// 			return currentItem.Increment();
// 		return MoveForward();
// 	}

// 	public bool MoveDown()
// 	{
// 		if (isHorizontal)
// 			return currentItem.Decrement();
// 		return MoveBackward();
// 	}

// 	public bool MoveLeft()
// 	{
// 		if (isHorizontal)
// 			return MoveBackward();
// 		return currentItem.Decrement();
// 	}

// 	public bool MoveRight()
// 	{
// 		if (isHorizontal)
// 			return MoveForward();
// 		return currentItem.Increment();
// 	}

// 	public bool Press()
// 	{
// 		return currentItem.Press();
// 	}

// 	public delegate void RenderMenuItemDelegate(MenuButtonData item, int index, bool selected);
// 	public void Render(RenderMenuItemDelegate drawMenuItem)
// 	{
// 		int i = 0;
// 		foreach (var item in _items)
// 		{
// 			drawMenuItem(item, i, i == selection);
// 			i++;
// 		}
// 	}
// }
