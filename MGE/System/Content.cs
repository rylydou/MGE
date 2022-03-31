using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MGE;

public interface IContentLoader
{
	object Load(File file, string path);
}

public class Content : AppModule
{
	public delegate T Load<T>(File file, string location);

	public readonly Folder contentFolder;
	public readonly Folder contentPacksFolder;

	// Location to file
	readonly Dictionary<string, File> contentIndex = new();

	// Location to asset
	readonly Dictionary<string, object?> preloadedAssets = new();

	// Location to file
	readonly Dictionary<string, File> unloadedAssets = new();

	public Content()
	{
		contentFolder = Environment.GetEnvironmentVariable("MGE_CONTENT") ?? Folder.here / "Content";
		contentPacksFolder = Folder.data / "Content Packs";
	}

	protected internal override void Startup()
	{
		ScanContent();

		LoadContent();
	}

	public void ScanContent()
	{
		contentIndex.Clear();

		Log.StartStopwatch("Scan content");

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

		Log.StartStopwatch("Load content");

		foreach (var item in contentIndex)
		{
			if (item.Value.path.EndsWith(".load")) continue;

			var loadFile = new File(item.Value + ".load");

			if (!loadFile.exists) continue;

			var loader = loadFile.ReadMeml<IContentLoader>();

			preloadedAssets.Add(item.Key, loader.Load(item.Value, item.Key));

			Log.Info($"Loaded {item.Key,-42} {loader.GetType().FullName}");
		}

		Log.EndStopwatch();
	}

	public T Get<T>(string location) where T : class
	{
		return (T)(preloadedAssets[location] ?? throw new Exception());
	}

	public bool Get<T>(string location, [MaybeNullWhen(false)] out T asset) where T : class
	{
		if (!preloadedAssets.TryGetValue(location, out var obj))
		{
			asset = null;
			return false;
		}

		asset = (T)(obj ?? throw new Exception());
		return true;
	}
}
