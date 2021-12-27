using Gtk;

namespace MGE.Editor.GUI.Data
{
	public class FlexData : WidgetData<Paned>
	{
		public System.Action<int> onPositionChanged = position => { };

		public FlexData(Paned widget) : base(widget) { }
	}
}
