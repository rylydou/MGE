using System;
using System.IO;
using System.Linq;

using System.Threading.Tasks;
using MGE.Serialization;
using Newtonsoft.Json;

namespace MGE
{
	public class FolderJsonConverter : JsonConverter<Folder>
	{
		public override Folder ReadJson(JsonReader reader, Type objectType, Folder existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			return new Folder(reader.ReadAsString());
		}

		public override void WriteJson(JsonWriter writer, Folder folder, JsonSerializer serializer)
		{
			writer.WriteValue(folder.path);
		}
	}

	public class Folder : IEquatable<Folder>
	{
		public static Folder root = string.Empty;
		public static Folder cwd = Environment.CurrentDirectory;
		public static Folder roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		public static Folder assets = cwd + "Assets";

		static Folder _saveData;
		public static Folder saveData
		{
			get
			{
				if (_saveData is null)
				{
#if DEBUG
					_saveData = cwd / "Data";
#else
					if (string.IsNullOrEmpty(Engine.config.companyName))
					{
						_saveData = roamingAppData / Engine.config.gameName;
					}
					else
					{
						_saveData = roamingAppData / Engine.config.companyName / Engine.config.gameName;
					}
#endif
				}
				return _saveData;
			}
		}

		public readonly string path;

		public Folder(string path)
		{
			this.path = path.Replace('\\', '/');
		}

		public string GetAbsolutePath(string path)
		{
			if (this.path.Length == 0)
				return path.Replace('\\', '/');
			return this.path + '/' + path.Replace('\\', '/');
		}

		public string GetRelitivePath(string path)
		{
			return path.Replace('\\', '/').Replace(this.path + "/", null);
		}

		#region File

		public bool FileExsits(string path)
		{
			return File.Exists(GetAbsolutePath(path));
		}

		public bool FileDelete(string path)
		{
			if (FileExsits(path))
			{
				File.Delete(GetAbsolutePath(path));
				return true;
			}
			return false;
		}

		public FileStream FileCreate(string path)
		{
			return File.Create(GetAbsolutePath(path));
		}

		public FileStream FileOpen(string path, FileAccess access = FileAccess.ReadWrite, FileMode mode = FileMode.OpenOrCreate, FileShare share = FileShare.Read)
		{
			return File.Open(GetAbsolutePath(path), mode, access, share);
		}

		public FileStream FileOpenRead(string path, FileMode mode = FileMode.OpenOrCreate, FileShare share = FileShare.Read)
		{
			return FileOpen(path, FileAccess.Read, mode, share);
		}

		public FileStream FileOpenWrite(string path, FileMode mode = FileMode.OpenOrCreate, FileShare share = FileShare.Read)
		{
			// if (FileExsits(path)) FileDelete(path);
			return FileOpen(path, FileAccess.ReadWrite, mode, share);
		}

		public StreamWriter FileWriter(string path, bool append = false)
		{
			return new StreamWriter(GetAbsolutePath(path), append);
		}

		public StreamReader FileReader(string path)
		{
			return new StreamReader(GetAbsolutePath(path));
		}

		public string FileReadText(string path)
		{
			return File.ReadAllText(GetAbsolutePath(path));
		}

		public Task<string> FileReadTextAsync(string path)
		{
			return File.ReadAllTextAsync(GetAbsolutePath(path));
		}

		public string[] FileReadLines(string path)
		{
			return File.ReadAllLines(GetAbsolutePath(path));
		}

		public Task<string[]> FileReadLinesAsync(string path)
		{
			return File.ReadAllLinesAsync(GetAbsolutePath(path));
		}

		public byte[] FileReadBytes(string path)
		{
			return File.ReadAllBytes(GetAbsolutePath(path));
		}

		public Task<byte[]> FileReadBytesAsync(string path)
		{
			return File.ReadAllBytesAsync(GetAbsolutePath(path));
		}

		public void FileWriteText(string path, string text)
		{
			File.WriteAllText(GetAbsolutePath(path), text);
		}

		public Task FileWriteTextAsync(string path, string text)
		{
			return File.WriteAllTextAsync(GetAbsolutePath(path), text);
		}

		public void FileWriteLines(string path, string[] lines)
		{
			File.WriteAllLines(GetAbsolutePath(path), lines);
		}

		public Task FileWriteLinesAsync(string path, string[] lines)
		{
			return File.WriteAllLinesAsync(GetAbsolutePath(path), lines);
		}

		public void FileWriteBytes(string path, byte[] bytes)
		{
			File.WriteAllBytes(GetAbsolutePath(path), bytes);
		}

		public Task FileWriteBytesAsync(string path, byte[] bytes)
		{
			return File.WriteAllBytesAsync(GetAbsolutePath(path), bytes);
		}

		public bool FileWriteReadable(string path, object obj)
		{
			var createdNew = !FileExsits(path);
			using (var writer = FileWriter(path))
				Serializer.SerializeReadable(writer, obj);
			return createdNew;
		}

		public T FileReadReadable<T>(string path)
		{
			var reader = FileReader(path);
			var obj = Serializer.DeserializeReadable<T>(reader);
			reader.Close();
			return obj;
		}

		public bool FileWriteBinary(string path, object obj)
		{
			var createdNew = !FileExsits(path);
			using (var writer = FileOpenWrite(path))
				Serializer.SerializeBinary(writer, obj);
			return createdNew;
		}

		public T FileReadBinary<T>(string path)
		{
			var reader = FileOpenRead(path);
			var obj = Serializer.DeserializeBinary<T>(reader);
			reader.Close();
			return obj;
		}

		#endregion

		#region Folder

		public bool FolderExsits(string path)
		{
			return Directory.Exists(GetAbsolutePath(path));
		}

		public bool FolderDelete(string path)
		{
			if (FolderExsits(path))
			{
				Directory.Delete(GetAbsolutePath(path));
				return true;
			}
			return false;
		}

		public Folder FolderCreate(string path)
		{
			return new Folder(Directory.CreateDirectory(GetAbsolutePath(path)).FullName);
		}

		public Folder FolderOpen(string path)
		{
			return new Folder(GetAbsolutePath(path));
		}

		public Folder[] FolderGetFolders(string path, bool recursive = false, string searchPattern = "*")
		{
			return Directory.GetDirectories(GetAbsolutePath(path), searchPattern, new EnumerationOptions() { RecurseSubdirectories = recursive }).Select(x => new Folder(x)).ToArray();
		}

		public string[] FolderGetFiles(string path, bool recursive = false, string searchPattern = "*")
		{
			return Directory.GetFiles(GetAbsolutePath(path), searchPattern, new EnumerationOptions() { RecurseSubdirectories = recursive }).Select(x => GetRelitivePath(x)).ToArray();
		}

		#endregion

		public static Folder operator +(Folder left, string right) => left.path + '/' + right;
		public static Folder operator /(Folder left, string right) => left.FolderCreate(right);

		public static bool operator ==(Folder left, Folder right) => left.path == right.path;
		public static bool operator !=(Folder left, Folder right) => left.path != right.path;

		public static implicit operator string(Folder folder) => folder.path;
		public static implicit operator Folder(string str) => new Folder(str);

		public override string ToString() => path;

		public override int GetHashCode() => path.GetHashCode();

		public bool Equals(Folder other) => path == other.path;
		public override bool Equals(object other)
		{
			if (other is Folder folder)
				return Equals(folder);
			return false;
		}
	}
}
