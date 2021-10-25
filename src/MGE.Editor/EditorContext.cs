using System;

namespace MGE.Editor
{
	public class EditorContext
	{
		public TestNode root = new()
		{
			nodes = new()
			{
				new(),
				new TestChildNode(),
				new()
				{
					nodes = new()
					{
						new(),
						new TestChildNode(),
						new(),
						new(),
					}
				},
				new(),
				new TestChildNode(),
				new(),
			},
		};

		// TODO Support multiple objects
		public object? selection;
		public Action onSelectionChanged = () => { };

		public EditorContext()
		{
			selection = root;
		}
	}
}
