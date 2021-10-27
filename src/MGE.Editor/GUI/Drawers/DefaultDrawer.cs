namespace MGE.Editor.GUI.Drawers
{
	public class DefaultDrawer : Drawer<object>
	{
		public DefaultDrawer(object value) : base(value, true) { }

		protected override void Draw()
		{
			EditorGUI.Inspector(value, value => { this.value = value; ValueChanged(); });
		}
	}
}
