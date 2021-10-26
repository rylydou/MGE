using System.Collections;

namespace MGE.Editor.GUI.ObjectDrawers
{
	public class ListDrawer : ObjectDrawer<IEnumerable>
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
