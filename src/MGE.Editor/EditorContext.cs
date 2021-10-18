using System;

namespace MGE.Editor
{
	public class EditorContext
	{
		public object? root;

		// TODO Support multiple objects
		public object? selection;
		public Action onSelectionChanged = () => { };
	}
}
