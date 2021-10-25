namespace MGE.Editor.GUI.ObjectDrawers
{
	public class DefaultDrawer : ObjectDrawer<object>
	{
		public DefaultDrawer(object value) : base(value, true) { }

		protected override void Draw()
		{
			EditorGUI.Inspector(value, value => { this.value = value; ValueChanged(); });
		}
	}
}
