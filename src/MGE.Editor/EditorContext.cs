using System;

namespace MGE.Editor
{
	public class EditorContext
	{
		public GameNode root = new(
			new(),
			new GameNodePlus(
				new(),
				new GameNodePlus(),
				// new(
				// 	new(),
				// 	new GameNodePlus(),
				// 	new()
				// ),
				new()
			),
			new(),
			new GameNodePlus(),
			new()
		);

		// TODO Support multiple objects
		public object? selection;
		public Action onSelectionChanged = () => { };

		public EditorContext()
		{
			selection = root;
		}
	}
}
