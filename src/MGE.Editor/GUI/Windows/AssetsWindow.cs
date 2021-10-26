using System;
using System.IO;

namespace MGE.Editor.GUI.Windows
{
	public class AssetsWindow : EditorWindow
	{
		static EnumerationOptions enumerationOptions = new()
		{
			IgnoreInaccessible = true,
			AttributesToSkip = FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReadOnly /*  | FileAttributes.Offline | FileAttributes.Encrypted | FileAttributes.Compressed */,
			ReturnSpecialDirectories = false,
		};

		public override string title => "Assets";

		public DirectoryInfo currentFolder = new(Environment.CurrentDirectory);
		public DirectoryInfo topLevel = new(Environment.CurrentDirectory);

		public AssetsWindow() : base() { }

		protected override void Draw()
		{
			// TODO Switch to panned
			EditorGUI.StartHorizontal(homogeneous: true);

			EditorGUI.StartHorizonalFlow();

			var folders = currentFolder.GetDirectories("*", enumerationOptions);
			var files = currentFolder.GetFiles("*", enumerationOptions);

			if (currentFolder.FullName != topLevel.FullName)
			{
				EditorGUI.Button("...").onPressed += () =>
				{
					currentFolder = currentFolder.Parent!;
					Redraw();
				};
			}

			foreach (var folder in folders)
			{
				EditorGUI.Button(folder.Name).onPressed += () =>
				{
					currentFolder = folder;
					Redraw();
				};
			}

			EditorGUI.End();

			EditorGUI.StartVertical();

			foreach (var file in files)
			{
				EditorGUI.Button(file.Name);
			}

			EditorGUI.End();

			EditorGUI.End();
		}
	}
}
