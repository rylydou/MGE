using Gtk;
using MGE.Editor.GUI.Data;

namespace MGE.Editor.GUI.Datas
{
	public class CheckboxData : ToggleData<CheckButton>
	{
		public CheckboxData(CheckButton widget) : base(widget) { }
	}
}
