using Gtk;

namespace MGE.Editor
{
	public static class EditorUtil
	{
		public static Widget GenerateInspector(object obj)
		{
			// var model = new ListStore(typeof(string), typeof(Widget));
			// model.AppendValues(new object[] { "Name", new Entry() });
			// model.AppendValues(new object[] { "Enabled", new CheckButton() });
			// model.AppendValues(new object[] { "Visible", new CheckButton() });

			var inspector = new ScrolledWindow(null, null);
			var list = new ListBox();
			list.SelectionMode = SelectionMode.None;

			list.Add(GenerateEntry("Name", new Entry() { Expand = true, PlaceholderText = "Enter Text...", TooltipText = "Click to enter text!" }));
			list.Add(GenerateEntry("Description", new Entry() { Expand = true, TruncateMultiline = true, CapsLockWarning = true, PlaceholderText = "Enter Text..." }));
			list.Add(GenerateEntry("Enabled", new CheckButton()));
			list.Add(GenerateEntry("Visible", new CheckButton()));
			list.Add(GenerateEntry("Attack Type", new ComboBox(new[] { "Fire", "Air", "Water", "Earth" }) { Expand = true }));

			inspector.Add(list);

			// var inspector = new Grid();

			// inspector.ColumnSpacing = 32;
			// inspector.RowSpacing = 8;

			return inspector;
		}

		public static Widget GenerateEntry(string name, Widget widget)
		{
			var box = new Box(Orientation.Horizontal, 32);
			box.Spacing = 16;

			var lable = new Label(name);
			lable.Xalign = 0;
			lable.WidthRequest = 96;

			box.Add(lable);
			box.Add(widget);

			return box;
		}
	}
}
