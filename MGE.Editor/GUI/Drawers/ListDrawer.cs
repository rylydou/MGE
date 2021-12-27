using System.Collections;

namespace MGE.Editor.GUI.Drawers
{
	public class ListDrawer : Drawer<IEnumerable>
	{
		public ListDrawer(IEnumerable value) : base(value, true) { }

		protected override void Draw()
		{
			var index = 0;
			foreach (var item in value)
			{
				EditorGUI.Value(item, value => { });
				index++;
			}
		}
	}
}
