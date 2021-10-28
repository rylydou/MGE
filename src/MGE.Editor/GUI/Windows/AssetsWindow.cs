using System;
using System.Collections.Generic;
using System.IO;
using Gdk;
using Gtk;

namespace MGE.Editor.GUI.Windows
{
	[EditorWindow("Assets")]
	public class AssetsWindow : EditorWindow
	{
		static EnumerationOptions _enumerationOptions = new()
		{
			IgnoreInaccessible = true,
			AttributesToSkip = FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReadOnly /*  | FileAttributes.Offline | FileAttributes.Encrypted | FileAttributes.Compressed */,
			ReturnSpecialDirectories = false,
		};

		DirectoryInfo _currentFolder = new(Environment.CurrentDirectory);
		DirectoryInfo _topLevel = new(Environment.CurrentDirectory);

		TreeStore folderContents = new(typeof(Pixbuf), typeof(string));
		IconView folderContentsView = new() { SelectionMode = SelectionMode.Multiple, ItemWidth = 80, RowSpacing = 0, ColumnSpacing = 0, };

		public AssetsWindow() : base(false)
		{
			folderContentsView.Model = folderContents;

			folderContentsView.PixbufColumn = 0;
			folderContentsView.TextColumn = 1;
		}

		protected override void Draw()
		{
			var folders = _currentFolder.GetDirectories("*", _enumerationOptions);
			var files = _currentFolder.GetFiles("*", _enumerationOptions);

			folderContents.Clear();

			var folderIcon = EditorGUI.GetIcon("folder");
			foreach (var folder in folders)
			{
				folderContents.AppendValues(folderIcon, folder.Name);
			}

			var fileIcon = EditorGUI.GetIcon("file");
			var imageIcon = EditorGUI.GetIcon("image");
			foreach (var file in files)
			{
				var icon = imageIcon;
				switch (file.Extension)
				{
					case ".png":
					case ".jpg":
					case ".ico":
					case ".svg":
						icon = imageIcon;
						break;
				}
				folderContents.AppendValues(icon, file.Name);
			}

			EditorGUI.HorizontalFlex();

			EditorGUI.VerticalOverflow();
			EditorGUI.StartVertical(0);

			if (_currentFolder.FullName != _topLevel.FullName)
			{
				EditorGUI.horizontalAlign = 0;
				EditorGUI.Button("↩ Back").onPressed += () =>
				{
					_currentFolder = _currentFolder.Parent!;
					Redraw();
				};
			}

			foreach (var folder in folders)
			{
				EditorGUI.horizontalAlign = 0;
				EditorGUI.Button(folder.Name).onPressed += () => { _currentFolder = folder; Redraw(); };
			}

			EditorGUI.End();

			EditorGUI.StartVertical();

			EditorGUI.StartHorizontal(0);

			var parentFolders = new List<DirectoryInfo>();

			{
				var folder = _currentFolder;
				var topLevelParent = _topLevel.Parent!;

				do
				{
					parentFolders.Add(folder);
					folder = folder.Parent!;
				} while (folder.FullName != topLevelParent.FullName);
			}

			parentFolders.Reverse();

			foreach (var folder in parentFolders)
			{
				EditorGUI.horizontalExpand = false;
				EditorGUI.Button(folder.Name).onPressed += () => { _currentFolder = folder; Redraw(); };
			}

			EditorGUI.End();

			EditorGUI.VerticalOverflow();

			folderContentsView.Unparent();
			EditorGUI.Add(folderContentsView);

			EditorGUI.End();

			EditorGUI.End();
		}
	}
}

// foreach (var folder in folders)
// {
// 	EditorGUI.verticalExpand = false;
// 	EditorGUI.horizontalExpand = false;
// 	EditorGUI.ButtonWithContent().onPressed += () => { _currentFolder = folder; Redraw(); };
// 	EditorGUI.verticalExpand = false;
// 	EditorGUI.StartVertical();

// 	EditorGUI.Icon("folder");
// 	EditorGUI.horizontalAlign = 0.5f;
// 	EditorGUI.Text(folder.Name);

// 	EditorGUI.End();
// }

// foreach (var file in files)
// {
// 	EditorGUI.verticalExpand = false;
// 	EditorGUI.horizontalExpand = false;
// 	EditorGUI.ButtonWithContent().onPressed += () => Trace.WriteLine(file.FullName);
// 	EditorGUI.verticalExpand = false;
// 	EditorGUI.StartVertical();

// 	EditorGUI.Icon("file");
// 	EditorGUI.horizontalAlign = 0.5f;
// 	EditorGUI.Text(file.Name);

// 	EditorGUI.End();
// }
