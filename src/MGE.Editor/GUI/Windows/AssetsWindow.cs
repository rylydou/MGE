using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
			var folders = _currentFolder.GetDirectories("*", _enumerationOptions).ToList();
			folders.Sort((left, right) => left.Name.CompareTo(right.Name));

			var files = _currentFolder.GetFiles("*", _enumerationOptions).ToList();
			files.Sort((left, right) => left.Name.CompareTo(right.Name));

			folderContents.Clear();

			foreach (var folder in folders)
			{
				folderContents.AppendValues(EditorGUI.GetIcon("folder"), folder.Name);
			}

			foreach (var file in files)
			{
				var icon = EditorGUI.GetIcon("file");
				switch (file.Extension)
				{
					case ".png":
					case ".jpg":
					case ".ico":
						icon = EditorGUI.GetIcon("image"); break;
					case ".svg":
						icon = EditorGUI.GetIcon("svg"); break;
					case ".cs":
						icon = EditorGUI.GetIcon("csharp"); break;
					case ".txt":
					case ".md":
						icon = EditorGUI.GetIcon("textfile"); break;
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
