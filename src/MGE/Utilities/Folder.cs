using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MGE;

public struct Folder : IEquatable<Folder>
{
	#region Static

	static Folder()
	{
		root = "";
		user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		parentFolder = AppDomain.CurrentDomain.BaseDirectory ?? throw new MGException();

#if DEBUG
		here = Environment.CurrentDirectory;
		data = new Folder(Environment.CurrentDirectory) / "appdata";
#else
		here = parentFolder;
		data = new Folder(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)) / App.name;
#endif
	}

	/// <summary>
	/// A folder with an empty path.
	/// </summary>
	/// <remarks>
	/// Does not include the drive letter if on Windows.
	/// </remarks>
	public static readonly Folder root;
	/// <summary>
	/// The user folder.
	/// </summary>
	public static readonly Folder user;

	///	<summary>
	/// The folder where user related app data should be stored.
	/// </summary>
	/// <remarks>
	/// <list type="table">
	///		<listheader>
	///			<term>Platform</term>
	///			<description>Location</description>
	///		</listheader>
	///		<item>
	///			<term>Windows</term>
	///			<description><c>%USERPROFILE%\AppData\Roaming\Application_Name</c></description>
	///		</item>
	///		<item>
	///			<term>Linux and MacOS</term>
	///			<description><c>~/.config/Application_Name</c></description>
	///		</item>
	///	</list>
	/// </remarks>
	public static readonly Folder data;
	/// <summary>
	/// The folder where the application is located or parentFolder if in debug mode.
	/// </summary>
	public static readonly Folder here;

	/// <summary>
	/// The folder where the application is located, even in debug mode.
	/// </summary>
	public static readonly Folder parentFolder;

	#endregion

	#region Instance

	public readonly string path;

	public bool exists => Directory.Exists(path);

	public Folder(string path)
	{
		this.path = CleanPath(path);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string CleanPath(string path)
	{
		// return path.Replace('\\', '/');
		path = path.Replace('\\', '/');
		// var startsWithSlash = path.StartsWith('/');
		var startsWithSlash = false;
		var endsWithSlash = path.EndsWith('/');
		var start = startsWithSlash ? 1 : 0;
		var length = path.Length - (startsWithSlash ? 1 : 0) - (endsWithSlash ? 1 : 0);
		return path.Substring(start, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string GetAbsolutePath(string relitivePath) => $"{this.path}/{CleanPath(relitivePath)}";

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string GetRelativePath(string absolutePath)
	{
		absolutePath = CleanPath(absolutePath);
		if (absolutePath.StartsWith(this.path))
		{
			return absolutePath.Remove(0, this.path.Length + 1);
		}
		return absolutePath;
	}

	#region File

	public File FileCreate(string path) => File.Create(GetAbsolutePath(path));

	public File GetFile(string path) => new(GetAbsolutePath(path));

	public File[] GetFiles(string search = "*") => Directory.GetFiles(path, search).Select(f => new File(f)).ToArray();

	static readonly EnumerationOptions recursiveEnumeration = new() { RecurseSubdirectories = true, };
	/// <summary>
	///
	/// </summary>
	/// <param name="search">
	/// The search string to match against the names in path. This
	///	parameter can contain a combination of valid literal and wildcard characters,
	///	but it doesn't support regular expressions.
	/// </param>
	/// <returns></returns>
	public IEnumerable<File> GetFilesRecursive(string search = "*") => Directory.GetFiles(path, search, recursiveEnumeration).Select(f => new File(f));

	#endregion

	#region Folder

	public bool FolderExists(string path) => Directory.Exists(GetAbsolutePath(path));

	public Folder FolderCreate(string path)
	{
		path = GetAbsolutePath(path);
		if (!Directory.Exists(path))
			Directory.CreateDirectory(path);
		return new(path);
	}

	public Folder FolderOpen(string path) => new(GetAbsolutePath(path));

	public IEnumerable<Folder> GetFolders() => Directory.GetDirectories(path).Select(f => new Folder(f));

	#endregion

	public static Folder operator +(Folder folder, string relitivePath) => folder.FolderOpen(relitivePath);

	/// <summary>
	/// Appends the paths and creates the folder on disk.
	/// </summary>
	/// <param name="folder"></param>
	/// <param name="relitivePath"></param>
	/// <returns></returns>
	public static Folder operator /(Folder folder, string relitivePath) => folder.FolderCreate(relitivePath);

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
