using System;
using System.Collections.Generic;

namespace MGE.Editor
{
	public class EditorContext
	{
		public GameNode root = new(
			new(),
			new GameNodePlus(
				new(),
				new GameNodePlus(),
				new(
					new(),
					new GameNodePlus(),
					new()
				),
				new()
			),
			new(),
			new GameNodePlus(),
			new()
		);

		public readonly List<object> selection = new();
		public Action onSelectionChanged = () => { };

		public object? selectedObject;
		public bool multipleSelectedObjects { get; private set; }

		public void SetSelection(IEnumerable<object> objects)
		{
			selection.Clear();
			foreach (var obj in objects) selection.Add(obj);

			UpdateSelection();
		}

		public void ClearSelection()
		{
			selection.Clear();

			UpdateSelection();
		}

		void UpdateSelection()
		{
			selectedObject = selection.Count == 1 ? selection[0] : null;
			multipleSelectedObjects = selection.Count > 1;

			onSelectionChanged();
		}
	}
}
