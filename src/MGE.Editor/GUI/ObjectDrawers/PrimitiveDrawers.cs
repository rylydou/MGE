namespace MGE.Editor.GUI.ObjectDrawers
{
	public class BoolDrawer : ObjectDrawer<bool>
	{
		public BoolDrawer(bool value) : base(value, false) { }
		protected override void Draw() => EditorGUI.Checkbox(value).onToggled += value => ValueChanged();
	}

	public class StringDrawer : ObjectDrawer<string>
	{
		public StringDrawer(string value) : base(value, false) { }
		protected override void Draw() => EditorGUI.TextFeild(value).onSubmitted += value => ValueChanged();
	}

	public class IntDrawer : ObjectDrawer<int>
	{
		public IntDrawer(int value) : base(value, false) { }
		protected override void Draw() => EditorGUI.NumberFeild(value).onSubmitted += value => ValueChanged();
	}

	public class FloatDrawer : ObjectDrawer<float>
	{
		public FloatDrawer(float value) : base(value, false) { }
		protected override void Draw() => EditorGUI.NumberFeild(value).onSubmitted += value => ValueChanged();
	}

	public class Vector2Drawer : ObjectDrawer<Vector2>
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
