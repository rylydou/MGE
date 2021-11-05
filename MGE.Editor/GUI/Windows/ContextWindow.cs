using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MGE.Editor.GUI.Windows
{
	public abstract class ContextWindow : EditorWindow
	{
		int _contextIndex;
		public int contextIndex
		{
			get => _contextIndex;
			[MemberNotNull(nameof(context))]
			protected set
			{
				_contextIndex = value;
				context = Editor.contexts[_contextIndex];
			}
		}
		public EditorContext context { get; private set; } = Editor.contexts[0];

		protected ContextWindow(bool scrolled = true) : base(scrolled) { }
	}
}
