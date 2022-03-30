using System;
using System.IO;
using System.Linq;
using System.Reflection;
using MEML;
using FileIO = System.IO.File;

namespace MGE;

public struct File : IEquatable<File>
{
	static StructureConverter converter = new StructureConverter()
	{
		memberFinder = (type) => type.GetMembers(StructureConverter.suggestedBindingFlags).Where(m => m.GetCustomAttribute<SaveAttribute>() is not null)
	};

	static File()
	{
		converter.RegisterConverter<Vector2>(_ => new StructureArray(_.x, _.y), _ => new Vector2(_[0].Float, _[1].Float));
		converter.RegisterConverter<Vector2Int>(_ => new StructureArray(_.x, _.y), _ => new Vector2Int(_[0].Int, _[1].Int));

		converter.RegisterConverter<Vector3>(_ => new StructureArray(_.x, _.y, _.z), _ => new Vector3(_[0].Float, _[1].Float, _[2].Float));

		converter.RegisterConverter<Rect>(_ => new StructureArray(_.x, _.y, _.width, _.height), _ => new Rect(_[0].Float, _[1].Float, _[2].Float, _[3].Float));
		converter.RegisterConverter<RectInt>(_ => new StructureArray(_.x, _.y, _.width, _.height), _ => new RectInt(_[0].Int, _[1].Int, _[2].Int, _[3].Int));

		converter.RegisterConverter<Color>(_ => "#" + _.ToHexStringRGBA(), _ => Color.FromHexStringRGBA(_.String));
	}

	public static File Create(string path)
	{
		FileIO.Create(path).Dispose();

		return new(path);
	}

	public readonly string path;

	public string extension
	{
		get
		{
			var length = path.Length;
			for (int i = length; --i >= 0;)
			{
				var ch = path[i];
				if (ch == '.') return path.Substring(i + 1, length - i - 1);
				if (ch == '/') break;
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

	public T ReadMeml<T>()
	{
		var type = typeof(T);
		var value = ReadMeml(type);
		return (T)(value ?? throw new NotImplementedException());
	}
	public object? ReadMeml(Type? impliedType = null) => converter.CreateObjectFromStructure(new MemlTextReader(OpenReadText()).ReadObject(), impliedType);

	public StructureValue ReadMemlStructure()
	{
		return new MemlTextReader(OpenReadText()).ReadObject();
	}

	public void WriteText(string? text) => FileIO.WriteAllText(path, text);
	public void WriteLines(string[] lines) => FileIO.WriteAllLines(path, lines);
	public void WriteBytes(byte[] bytes) => FileIO.WriteAllBytes(path, bytes);

	public void WriteMeml(object? obj, Type? impliedType = null) => new MemlTextWriter(OpenWrite()).Write(converter.CreateStructureFromObject(obj, impliedType));

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
