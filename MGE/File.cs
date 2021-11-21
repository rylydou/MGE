using System;
using System.IO;
using MGE.Serialization;
using FileIO = System.IO.File;

namespace MGE;

public struct File : IEquatable<File>
{
	public static File Create(string path)
	{
		FileIO.Create(path).Dispose();

		return new(path);
	}

	public readonly string path;

	public bool exists => FileIO.Exists(path);

	public File(string path) => this.path = path;

	public FileStream OpenRead() => FileIO.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
	public StreamReader OpenReadText() => FileIO.OpenText(path);

	public FileStream OpenWrite(FileMode mode = FileMode.CreateNew) => FileIO.Open(path, mode, FileAccess.ReadWrite, FileShare.Read);
	public FileStream OpenAppend() => FileIO.Open(path, FileMode.Append, FileAccess.ReadWrite, FileShare.Read);

	public string ReadText() => FileIO.ReadAllText(path);
	public string[] ReadLines() => FileIO.ReadAllLines(path);
	public byte[] ReadBytes() => FileIO.ReadAllBytes(path);

	public void WriteText(string? text) => FileIO.WriteAllText(path, text);
	public void WriteLines(string[] lines) => FileIO.WriteAllLines(path, lines);
	public void WriteBytes(byte[] bytes) => FileIO.WriteAllBytes(path, bytes);

	public void AppendText(string? text) => FileIO.AppendAllText(path, text);
	public void AppendLines(string[] lines) => FileIO.AppendAllLines(path, lines);

	public T ReadObject<T>() => Serializer.Deserialize<T>(ReadText());
	public object? ReadObject(Type type) => Serializer.Deserialize(ReadText(), type);

	public void WriteObject(object obj) => WriteText(Serializer.Serialize(obj));

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
