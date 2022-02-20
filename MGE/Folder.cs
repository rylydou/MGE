using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MGE;

public struct Folder : IEquatable<Folder>
{
	#region Static

	static Folder()
	{
		root = "";
		user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

		data = Environment.GetEnvironmentVariable("appdata") ?? new Folder(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)) / "MGE Game";
		here = Assembly.GetEntryAssembly()!.Location;

		content = Environment.GetEnvironmentVariable("content") ?? here / "Content";
		contentPacks = data / "Content Packs";
	}

	public static readonly Folder root;
	public static readonly Folder user;

	public static readonly Folder data;
	public static readonly Folder here;

	public static readonly Folder content;
	public static readonly Folder contentPacks;

	#endregion

	#region Instance

	public readonly string path;

	public bool exists => Directory.Exists(path);

	public Folder(string path) => this.path = CleanPath(path);

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

	public File FileCreate(string path) => File.Create(GetAbsolutePath(path));

	public File GetFile(string path) => new(GetAbsolutePath(path));

	public File[] GetFiles(string search = "*") => Directory.GetFiles(path, search).Select(f => new File(f)).ToArray();

	static readonly EnumerationOptions recursiveEnumeration = new() { RecurseSubdirectories = true, };
	public File[] GetFilesRecursive(string search = "*") => Directory.GetFiles(path, search, recursiveEnumeration).Select(f => new File(f)).ToArray();

	// public FileStream FileOpenRead(string path) => new FileStream(GetAbsolutePath(path), FileMode.Open, FileAccess.Read, FileShare.Read);

	// public FileStream FileOpenWrite(string path) => new FileStream(GetAbsolutePath(path), FileMode.Open, FileAccess.ReadWrite, FileShare.Read);

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

	public override string ToString() => path.StartsWith(user.path) ? "~" + path.Remove(0, user.path.Length) : path;

	public override int GetHashCode() => path.GetHashCode();

	public bool Equals(Folder other) => path == other.path;
	public override bool Equals(object? other) => other is Folder folder && Equals(folder);

	#endregion
}
