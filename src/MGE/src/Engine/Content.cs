using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using MEML;

namespace MGE;

public interface IContentLoader
{
	object Load(File file, string filename);
}

public class Content : AppModule
{
	public delegate T Load<T>(File file, string filename);

	public readonly Folder contentFolder;
	public readonly Folder contentPacksFolder;

	// Location to file
	readonly Dictionary<string, File> contentIndex = new();

	// Location to asset
	readonly Dictionary<string, object?> preloadedAssets = new();

	// Location to file
	readonly Dictionary<string, File> unloadedAssets = new();

	public IFont font = null!;
	void LoadEmbeddedFont()
	{
		var fontJson = Util.EmbeddedResource("res/Inter.json");
		var fontPng = Util.EmbeddedResource("res/Inter.json.png");

		font = new SDFFont(new Texture(fontPng, "png"), new JsonTextReader(fontJson).ReadObject());
	}

	public Content()
	{
		contentFolder = Folder.here / "Content";
		contentPacksFolder = Folder.data / "Content Packs";
	}

	protected internal override void Startup()
	{
		LoadEmbeddedFont();

		ScanContent();
		LoadContent();
	}

	public void ScanContent()
	{
		contentIndex.Clear();

		Log.StartStopwatch("Scanning content");

		IndexContent(contentFolder);

		var contentPacks = contentPacksFolder.GetFolders();
		foreach (var contentPack in contentPacks)
		{
			IndexContent(contentPack);
		}

		Log.EndStopwatch();

		void IndexContent(Folder folder)
		{
			var files = folder.GetFilesRecursive();
			foreach (var file in files)
			{
				contentIndex.Set(folder.GetRelativePath(file), file);
			}
		}
	}

	public void LoadContent()
	{
		foreach (var item in preloadedAssets)
		{
			if (item.Value is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		preloadedAssets.Clear();
		unloadedAssets.Clear();

		Log.StartStopwatch("Loading content");

		foreach (var item in contentIndex)
		{
			var filePath = item.Value;
			var filename = item.Key;

			if (filePath.path.EndsWith(".load")) continue;

			var loadFile = new File(filePath + ".load");

			if (!loadFile.exists) continue;

			var loader = loadFile.ReadMeml<IContentLoader>();

			var sw = new Stopwatch();
			sw.Start();
			var value = loader.Load(filePath, filename);
			sw.Stop();
			// {loader.GetType().FullName,-42}
			Log.Trace($"{filename,-64} {sw.ElapsedMilliseconds}ms" + (sw.ElapsedMilliseconds >= 100 ? " <-- Slow" : ""));

			preloadedAssets.Add(filename, value);
		}

		Log.EndStopwatch();
	}

	public T Get<T>(string filename) where T : class
	{
		return (T)(preloadedAssets[filename] ?? throw new MGException());
	}

	public bool Get<T>(string filename, [MaybeNullWhen(false)] out T asset) where T : class
	{
		if (!preloadedAssets.TryGetValue(filename, out var obj))
		{
			asset = null;
			return false;
		}

		asset = (T)(obj ?? throw new MGException());
		return true;
	}
}
