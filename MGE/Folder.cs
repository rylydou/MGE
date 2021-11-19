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
		public string GetAbsolutePath(string path) => $"{this.path}/{CleanPath(path)}";

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

		public FileStream FileCreate(string path) => File.Create(GetAbsolutePath(path));

		public FileStream FileOpenRead(string path) => new FileStream(GetAbsolutePath(path), FileMode.Open, FileAccess.Read, FileShare.Read);

		public FileStream FileOpenWrite(string path) => new FileStream(GetAbsolutePath(path), FileMode.Open, FileAccess.ReadWrite, FileShare.Read);

		#endregion

		#region Folder

		public bool FolderExists(string path) => Directory.Exists(GetAbsolutePath(path));

		public Folder FolderCreate(string path) => new(Directory.CreateDirectory(GetAbsolutePath(path)).FullName);

		public Folder FolderOpen(string path) => new(GetAbsolutePath(path));

		#endregion

		public static Folder operator +(Folder folder, string extention) => folder.FolderOpen(extention);

		/// <summary>
		/// Appends the paths and creates the folder.
		/// </summary>
		/// <param name="folder"></param>
		/// <param name="extention"></param>
		/// <returns></returns>
		public static Folder operator /(Folder folder, string extention) => folder.FolderCreate(extention);

		public static bool operator ==(Folder left, Folder right) => left.path == right.path;
		public static bool operator !=(Folder left, Folder right) => left.path != right.path;

		public static implicit operator string(Folder folder) => folder.path;
		public static implicit operator Folder(string str) => new Folder(str);

		public override string ToString() => path;

		public override int GetHashCode() => path.GetHashCode();

		public bool Equals(Folder other) => path == other.path;
		public override bool Equals(object? other) => other is Folder folder && Equals(folder);
	}
}
