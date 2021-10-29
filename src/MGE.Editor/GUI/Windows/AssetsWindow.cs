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
			AttributesToSkip = FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReadOnly /* | FileAttributes.Offline | FileAttributes.Encrypted | FileAttributes.Compressed */,
			ReturnSpecialDirectories = false,
		};

		DirectoryInfo _currentFolder = new(Environment.CurrentDirectory);
		DirectoryInfo _topLevel = new(Environment.CurrentDirectory);

		TreeStore folderContents = new(typeof(Pixbuf), typeof(string), typeof(string), typeof(bool));
		IconView folderContentsView = new() { SelectionMode = SelectionMode.Multiple, ItemWidth = 80, RowSpacing = 0, ColumnSpacing = 0, Reorderable = true, };

		List<DirectoryInfo> folders = new();
		List<FileInfo> files = new();

		public AssetsWindow() : base(false)
		{
			folderContentsView.Model = folderContents;

			folderContentsView.ItemActivated += (sender, args) =>
			{
				folderContents.GetIter(out var iter, args.Path);
				var path = (string)folderContents.GetValue(iter, 2);
				var isFolder = (bool)folderContents.GetValue(iter, 3);

				if (isFolder)
				{
					_currentFolder = new(path);
					Reload();
				}
				else
				{
					Editor.OpenInApp(path);
				}
			};

			folderContentsView.PixbufColumn = 0;
			folderContentsView.TextColumn = 1;

			Reload();
		}

		void Reload()
		{
			folders = _currentFolder.GetDirectories("*", _enumerationOptions).ToList();
			folders.Sort((left, right) => left.Name.CompareTo(right.Name));

			files = _currentFolder.GetFiles("*", _enumerationOptions).ToList();
			files.Sort((left, right) => left.Name.CompareTo(right.Name));

			folderContents.Clear();

			foreach (var folder in folders)
			{
				folderContents.AppendValues(EditorGUI.GetIcon("folder"), folder.Name, folder.FullName, true);
			}

			foreach (var file in files)
			{
				var icon = EditorGUI.GetIcon("file");
				switch (file.Extension)
				{
					case ".png":
					case ".jpg":
					case ".ico":
					case ".svg":
						try { icon = new Pixbuf(file.FullName, 64, 64, true); break; }
						catch { icon = EditorGUI.GetIcon("image"); break; }
					case ".cs":
						icon = EditorGUI.GetIcon("csharp"); break;
					case ".txt":
					case ".md":
						icon = EditorGUI.GetIcon("textfile"); break;
				}
				folderContents.AppendValues(icon, file.Name, file.FullName, false);
			}

			Redraw();
		}

		protected override void Draw()
		{
			EditorGUI.HorizontalFlex();

			EditorGUI.VerticalOverflow();
			EditorGUI.StartVertical(0);

			if (_currentFolder.FullName != _topLevel.FullName)
			{
				EditorGUI.horizontalAlign = 0;
				EditorGUI.Button("↩ Back").onPressed += () =>
				{
					_currentFolder = _currentFolder.Parent!;
					Reload();
				};
			}

			foreach (var folder in folders)
			{
				EditorGUI.horizontalAlign = 0;
				EditorGUI.Button(folder.Name).onPressed += () => { _currentFolder = folder; Reload(); };
			}

			EditorGUI.End();

			EditorGUI.StartVertical();

			EditorGUI.StartHorizontal();

			EditorGUI.HorizontalOverflow();
			EditorGUI.StartHorizontal(0);

			var parentFolders = new List<DirectoryInfo>();

			{
				var folder = _currentFolder;
				var topLevelParent = _topLevel.Parent!;

				do
				{
					parentFolders.Add(folder);
					if (folder.Parent is null) break;
					folder = folder.Parent;
				} while (folder.FullName != topLevelParent.FullName);
			}

			parentFolders.Reverse();

			foreach (var folder in parentFolders)
			{
				EditorGUI.horizontalExpand = false;
				EditorGUI.Button(folder.Name).onPressed += () => { _currentFolder = folder; Reload(); };
			}

			EditorGUI.End();

			EditorGUI.IconButton("redo").onPressed += Reload;

			EditorGUI.End();

			EditorGUI.VerticalOverflow();

			folderContentsView.Unparent();
			EditorGUI.Add(folderContentsView);

			EditorGUI.End();

			EditorGUI.End();
		}
	}
}
