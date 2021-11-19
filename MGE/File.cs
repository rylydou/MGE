using System.IO;
using FileIO = System.IO.File;

namespace MGE;

public class File
{
	public readonly string path;

	public bool exists => FileIO.Exists(path);

	public File(string path) => this.path = path;

	public FileStream OpenRead() => FileIO.OpenRead(path);
	public FileStream OpenWrite() => FileIO.OpenWrite(path);

	public string ReadText() => FileIO.ReadAllText(path);
	public string[] ReadLines() => FileIO.ReadAllLines(path);
	public byte[] ReadBytes() => FileIO.ReadAllBytes(path);

	public void WriteText(string? text) => FileIO.WriteAllText(path, text);
	public void WriteLines(string[] lines) => FileIO.WriteAllLines(path, lines);
	public void WriteBytes(byte[] bytes) => FileIO.WriteAllBytes(path, bytes);

	public void AppendText(string? text) => FileIO.AppendAllText(path, text);
	public void AppendLines(string[] lines) => FileIO.AppendAllLines(path, lines);
}
