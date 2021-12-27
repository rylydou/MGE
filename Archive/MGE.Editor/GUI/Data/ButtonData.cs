using Gtk;

namespace MGE.Editor.GUI.Data
{
	public class ButtonData : WidgetData<Button>
	{
		public System.Action onPressed = () => { };

		public ButtonData(Button widget) : base(widget) { }
	}
}
