namespace MGE.Editor.GUI.Drawers
{
	public class BoolDrawer : Drawer<bool>
	{
		public BoolDrawer(bool value) : base(value, false) { }
		protected override void Draw() => EditorGUI.Checkbox(value).onToggled += value => ValueChanged();
	}

	public class StringDrawer : Drawer<string>
	{
		public StringDrawer(string value) : base(value, false) { }
		protected override void Draw() => EditorGUI.TextFeild(value).onSubmitted += value => ValueChanged();
	}

	public class IntDrawer : Drawer<int>
	{
		public IntDrawer(int value) : base(value, false) { }
		protected override void Draw() => EditorGUI.NumberFeild(value).onSubmitted += value => ValueChanged();
	}

	public class FloatDrawer : Drawer<float>
	{
		public FloatDrawer(float value) : base(value, false) { }
		protected override void Draw() => EditorGUI.NumberFeild(value).onSubmitted += value => ValueChanged();
	}

	public class Vector2Drawer : Drawer<Vector2>
	{
		public Vector2Drawer(Vector2 value) : base(value, false) { }
		protected override void Draw()
		{
			EditorGUI.Label("X");
			EditorGUI.NumberFeild(value.x).onSubmitted += value => { this.value.x = value; ValueChanged(); };
			EditorGUI.Label("Y");
			EditorGUI.NumberFeild(value.y).onSubmitted += value => { this.value.x = value; ValueChanged(); };
		}
	}
}