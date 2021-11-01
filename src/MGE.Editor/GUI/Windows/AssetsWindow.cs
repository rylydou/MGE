using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Gdk;
using Gtk;

namespace MGE.Editor.GUI.Windows
{
	[EditorWindow("Assets")]
	public class AssetsWindow : EditorWindow
	{
		class FilePreviewCache
		{
			public readonly FileInfo file;

			Pixbuf _image;
			public Pixbuf image
			{
				get
				{
					if (this.lastWriteTime != file.LastWriteTimeUtc) ReloadImage();

					return _image!;
				}
			}
			public DateTime lastWriteTime;

			void ReloadImage()
			{
				this.lastWriteTime = file.LastWriteTimeUtc;

				_image = new(file.FullName, 64, 64, true);

				// var image = new Pixbuf(file.FullName);
				// image.Scale(_image, 0, 0, 64, 64, 0, 0, 64.0 / image.Width, 64.0 / image.Height, InterpType.Nearest);
			}

			public FilePreviewCache(string path)
			{
				if (!File.Exists(path)) throw new System.Exception("File does not exsist");

				file = new(path);

				if (file.Extension == ".svg") _image = EditorGUI.GetFileIcon("SVG");
				else _image = EditorGUI.GetFileIcon("Image");

				ReloadImage();
			}
		}

		static EnumerationOptions _enumerationOptions = new()
		{
			IgnoreInaccessible = true,
			AttributesToSkip = FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReadOnly /* | FileAttributes.Offline | FileAttributes.Encrypted | FileAttributes.Compressed */,
			ReturnSpecialDirectories = false,
		};

		Dictionary<string, FilePreviewCache> _filePreviewCache = new();

		DirectoryInfo _currentFolder = Editor.project.assets;

		ListStore _folderContents = new(typeof(Pixbuf), typeof(string), typeof(string), typeof(bool));
		IconView _folderContentsView = new() { SelectionMode = SelectionMode.Multiple, ItemWidth = 64, RowSpacing = 0, ColumnSpacing = 0, ItemPadding = 4, Spacing = 0, };

		List<DirectoryInfo> _folders = new();
		List<FileInfo> _files = new();

		public AssetsWindow() : base(false)
		{
			_folderContentsView.Model = _folderContents;

			_folderContentsView.ItemActivated += (sender, args) =>
			{
				_folderContents.GetIter(out var iter, args.Path);
				var path = (string)_folderContents.GetValue(iter, 2);
				var isFolder = (bool)_folderContents.GetValue(iter, 3);

				if (isFolder)
				{
					_currentFolder = new(path);
					Reload();
				}
				else
				{
					Editor.OpenFile(path);
				}
			};

			_folderContentsView.PixbufColumn = 0;
			_folderContentsView.TextColumn = 1;

			Reload();
		}

		void Reload()
		{
			_folders = _currentFolder.GetDirectories("*", _enumerationOptions).ToList();
			_folders.Sort((left, right) => left.Name.CompareTo(right.Name));

			_files = _currentFolder.GetFiles("*", _enumerationOptions).ToList();
			_files.Sort((left, right) => left.Name.CompareTo(right.Name));

			_folderContents.Clear();

			// _folderContentsView.DragDataGet += (sender, args) =>
			// {
			// 	_folderContents.GetIter(out var iter, _folderContentsView.SelectedItems[0]);
			// 	var path = (string)_folderContents.GetValue(iter, 2);
			// 	args.SelectionData.Text = path;
			// 	Trace.WriteLine(path);
			// };
			// var target = new TargetEntry();
			// _folderContentsView.EnableModelDragSource(ModifierType.None, new[] { target }, DragAction.Default);
			// _folderContentsView.EnableModelDragDest(new[] { target }, DragAction.Default);
			// _folderContentsView.DragDataReceived += (sender, args) =>
			// {
			// 	Trace.WriteLine("Rec " + args.SelectionData.Text);
			// };

			foreach (var folder in _folders)
			{
				var iter = _folderContents.AppendValues(EditorGUI.GetFileIcon("Folder"), folder.Name, folder.FullName, true);
				_folderContentsView.SetDragDestItem(_folderContents.GetPath(iter), IconViewDropPosition.DropInto);
			}

			foreach (var file in _files)
			{
				var icon = EditorGUI.GetFileIcon("File");

				switch (file.Extension)
				{
					case ".png":
					case ".jpg":
					case ".ico":
					case ".bmp":
					case ".svg":
						if (!_filePreviewCache.TryGetValue(file.FullName, out var cache))
						{
							cache = new(file.FullName);
							_filePreviewCache.Add(file.FullName, cache);
						}
						icon = cache.image;
						break;
					case ".txt":
					case ".md":
						icon = EditorGUI.GetFileIcon("Text"); break;
					case ".wav":
					case ".ogg":
						icon = EditorGUI.GetFileIcon("SFX"); break;
					case ".mp3":
						icon = EditorGUI.GetFileIcon("Music"); break;
					case ".zip":
					case ".7z":
					case ".rar":
						icon = EditorGUI.GetFileIcon("Archive"); break;
					case ".dll":
					case ".so":
						icon = EditorGUI.GetFileIcon("Library"); break;
					case ".cs":
						icon = EditorGUI.GetFileIcon("CSharp"); break;
					case ".node":
						icon = EditorGUI.GetFileIcon("Node"); break;
					case ".yml":
						icon = EditorGUI.GetFileIcon("Object"); break;
				}
				_folderContents.AppendValues(icon, file.Name, file.FullName, false);
			}

			Redraw();
		}

		protected override void Draw()
		{
			EditorGUI.StartVertical();

			EditorGUI.StartHorizontal();

			EditorGUI.sensitive = !_currentFolder.Equals(Editor.project.assets);
			EditorGUI.IconButton("Back").onPressed += () => { _currentFolder = _currentFolder.Parent!; Reload(); };

			EditorGUI.HorizontalOverflow();
			EditorGUI.StartHorizontal(0);

			var parentFolders = new List<DirectoryInfo>();

			{
				var folder = _currentFolder;
				var topLevelParent = Editor.project.assets.Parent!;

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

			EditorGUI.Text($"{_files.Count + _folders.Count} Items");

			EditorGUI.IconButton("Redo").onPressed += Reload;

			EditorGUI.End();

			EditorGUI.VerticalOverflow();

			_folderContentsView.Unparent();
			EditorGUI.Add(_folderContentsView);

			EditorGUI.End();
		}
	}
}
