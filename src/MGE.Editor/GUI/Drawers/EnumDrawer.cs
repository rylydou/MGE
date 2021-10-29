using System;

namespace MGE.Editor.GUI.Drawers
{
	public class EnumDrawer : Drawer<Enum>
	{
		public EnumDrawer(Enum value) : base(value, false) { }

		protected override void Draw()
		{
			var type = value.GetType();
			var options = Enum.GetNames(type);

			EditorGUI.Combobox(options, Enum.GetName(type, value)).onItemChanged += value => { this.value = (Enum)Enum.Parse(type, value); ValueChanged(); };
		}
	}
}