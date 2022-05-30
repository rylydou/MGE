using System;
using System.IO;
using System.Linq;
using System.Reflection;
using MEML;
using FileIO = System.IO.File;

namespace MGE;

public struct File : IEquatable<File>
{
	static MemlConverter _converter;

	static File()
	{
		_converter = Util.GetStructureConverter();
		_converter.memberFinder = (type) => type.GetMembers(MemlConverter.suggestedBindingFlags).Where(m => m.GetCustomAttribute<SaveAttribute>() is not null);
	}

	public static File Create(string path)
	{
		FileIO.Create(path).Dispose();

		return new(path);
	}

	public readonly string path;

	public string name
	{
		get
		{
			var length = path.Length;
			var start = 0;
			var end = length;
			for (int i = length; --i >= 0;)
			{
				var ch = path[i];
				if (ch == '.')
				{
					end = i;
					continue;
				}
				if (ch == '/' || ch == '\\')
				{
					start = i + 1;
					break;
				}
			}
			return path.Substring(start, end - start);
		}
	}

	public string extension
	{
		get
		{
			var length = path.Length;
			for (int i = length; --i >= 0;)
			{
				var ch = path[i];
				if (ch == '.') return path.Substring(i + 1, length - i - 1);
				if (ch == '/' || ch == '\\') break;
			}
			return string.Empty;
		}
	}

	public bool exists => FileIO.Exists(path);

	public File(string path) => this.path = path;

	public FileStream OpenRead() => FileIO.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
	public StreamReader OpenReadText() => FileIO.OpenText(path);

	public FileStream OpenWrite(FileMode mode = FileMode.CreateNew) => FileIO.Open(path, mode, FileAccess.ReadWrite, FileShare.Read);
	public StreamWriter OpenWriteText(FileMode mode = FileMode.CreateNew) => new StreamWriter(FileIO.Open(path, mode, FileAccess.ReadWrite, FileShare.Read));
	public FileStream OpenAppend() => FileIO.Open(path, FileMode.Append, FileAccess.ReadWrite, FileShare.Read);

	public string ReadText() => FileIO.ReadAllText(path);
	public string[] ReadLines() => FileIO.ReadAllLines(path);
	public byte[] ReadBytes() => FileIO.ReadAllBytes(path);

	public T ReadMeml<T>() => (T)(ReadMeml(typeof(T)) ?? throw new NotImplementedException());
	public object? ReadMeml(Type? impliedType = null) => _converter.CreateObjectFromStructure(ReadMemlRaw(), impliedType);
	public MemlValue ReadMemlRaw() => new MemlTextReader(OpenReadText()).ReadObject();

	public T ReadJson<T>() => (T)(ReadJson(typeof(T)) ?? throw new NotImplementedException());
	public object? ReadJson(Type? impliedType = null) => _converter.CreateObjectFromStructure(new JsonTextReader(OpenReadText()).ReadObject(), impliedType);
	public MemlValue ReadJsonRaw() => new JsonTextReader(OpenReadText()).ReadObject();

	public T ReadBinary<T>() => (T)(ReadJson(typeof(T)) ?? throw new NotImplementedException());
	public object? ReadBinary(Type? impliedType = null) => _converter.CreateObjectFromStructure(new MemlBinaryReader(OpenRead()).ReadObject(), impliedType);
	public MemlValue ReadBinaryRaw() => new JsonTextReader(OpenReadText()).ReadObject();

	public void WriteText(string? text) => FileIO.WriteAllText(path, text);
	public void WriteLines(string[] lines) => FileIO.WriteAllLines(path, lines);
	public void WriteBytes(byte[] bytes) => FileIO.WriteAllBytes(path, bytes);

	public void WriteMeml(object? obj, Type? impliedType = null) => new MemlTextWriter(OpenWrite()).Write(_converter.CreateMemlFromObject(obj, impliedType));
	public void WriteMeml(MemlValue value) => new MemlTextWriter(OpenWriteText()).Write(value);

	public void WriteJson(object? obj, Type? impliedType = null) => new JsonTextWriter(OpenWrite()).Write(_converter.CreateMemlFromObject(obj, impliedType));
	public void WriteJson(MemlValue value) => new JsonTextWriter(OpenWriteText()).Write(value);

	public void WriteBinary(object? obj, Type? impliedType = null) => new MemlBinaryWriter(OpenWrite()).Write(_converter.CreateMemlFromObject(obj, impliedType));
	public void WriteBinary(MemlValue value) => new MemlBinaryWriter(OpenWrite()).Write(value);

	public void AppendText(string? text) => FileIO.AppendAllText(path, text);
	public void AppendLines(string[] lines) => FileIO.AppendAllLines(path, lines);

	public FileStream Create() => FileIO.Create(path);

	public void Delete() => FileIO.Delete(path);

	public static bool operator ==(File left, File right) => left.path == right.path;
	public static bool operator !=(File left, File right) => left.path != right.path;

	public static implicit operator string(File folder) => folder.path;
	public static implicit operator File(string str) => new(str);

	public override string ToString() => path;

	public override int GetHashCode() => path.GetHashCode();

	public bool Equals(File other) => path == other.path;
	public override bool Equals(object? other) => other is Folder folder && Equals(folder);
}
