using System.IO;
using System.Runtime.CompilerServices;

namespace MGE
{
	public struct Folder
	{
		public readonly string path;

		public Folder(string path)
		{
			this.path = CleanPath(path);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string CleanPath(string path) => path.Replace('\\', '/');

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string GetFullPath(string path) => $"{this.path}/{CleanPath(path)}";

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string GetRelativePath(string path)
		{
			path = CleanPath(path);
			if (path.StartsWith(this.path))
			{
				return path.Remove(0, this.path.Length);
			}
			return path;
		}

		#region File

		public FileStream FileCreate(string path) => File.Create(GetFullPath(path));

		public FileStream FileOpenRead(string path) => new FileStream(GetFullPath(path), FileMode.Open, FileAccess.Read, FileShare.Read);

		public FileStream FileOpenWrite(string path) => new FileStream(GetFullPath(path), FileMode.Open, FileAccess.ReadWrite, FileShare.Read);

		#endregion

		#region Folder

		public Folder CreateFolder(string path) => new(Directory.CreateDirectory(GetFullPath(path)).FullName);

		public Folder OpenFolder(string path) => new(GetFullPath(path));

		#endregion

	}
}
