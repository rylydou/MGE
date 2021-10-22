using System.Collections.Generic;
using GLib;
using Gtk;

namespace MGE.Editor.GUI.Widgets
{
	// TODO

	// Layout: [0] 0 [1] 1 [2] 2 [3]
	// Count:   1     2     3     4
	//
	// If separator 1 is moved then resize item 1 and 2
	// If separator x is moved then resize item x and x + 1
	//
	// If an item is added before or after item 1 then resize item 1

	public class EditorPaned : Container
	{
		public readonly Orientation orientation;
		readonly Orientation separatorOrientation;

		public List<Widget> items = new();
		public List<Separator> separators = new();

		public EditorPaned(Orientation orientation)
		{
			this.orientation = orientation;
			this.separatorOrientation = orientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Vertical;
		}

		public void GetSeprators(int position, out Separator? before, out Separator? after)
		{
			// First item
			if (position <= 0)
			{
				before = separators[0];
				after = null;
				return;
			}

			// Last item
			if (position >= items.Count - 1)
			{
				before = null;
				after = separators[separators.Count - 1];
				return;
			}

			// Middle items
			before = separators[position - 1];
			after = separators[position];
		}

		public void GetItems(int position, out Widget? before, out Widget? after)
		{
			// First item
			if (position <= 0)
			{
				before = items[0];
				after = null;
				return;
			}

			// Last item
			if (position >= separators.Count - 1)
			{
				before = null;
				after = items[items.Count - 1];
				return;
			}

			// Middle items
			before = items[position - 1];
			after = items[position];
		}

		public void Insert(int position, Widget widget)
		{
			if (separators.Count == 0)
			{
				// [0] No separators needed!

				items.Add(widget);

				Add(widget);
			}
			else
			{
				// [0] 0 [1] Add a separator before

				var separator = new Separator(separatorOrientation);

				separators.Add(separator);
				items.Add(widget);

				Add(separator);
				Add(widget);
			}
		}
	}
}
