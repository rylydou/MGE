using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MGE;

public class Content : AppModule
{
	public delegate T Load<T>(T obj, File file, string location);

	abstract class ContentLoader
	{
		public abstract object Load(object obj, File file, string path);
	}

	sealed class ContentLoader<T> : ContentLoader where T : class
	{
		public readonly Load<T> load;

		public ContentLoader(Load<T> load)
		{
			this.load = load;
		}

		public sealed override object Load(object obj, File file, string location)
		{
			return load((T)obj, file, location);
		}
	}

	public readonly Folder contentFolder = new Folder(Environment.CurrentDirectory) / "Content";
	public readonly Folder contentPacksFolder = Folder.data / "Content Packs";

	// Location to file
	readonly Dictionary<string, File> contentIndex = new();

	readonly Dictionary<string, object?> preloadedAssets = new();
	readonly Dictionary<string, object?> unloadedAssets = new();

	protected internal override void ApplicationStarted()
	{
		ScanContent();

		LoadContent();
	}

	public void ScanContent()
	{
		Log.StartStopwatch("Scan content");

		contentIndex.Clear();

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
		Log.StartStopwatch("Load content");

		preloadedAssets.Clear();
		unloadedAssets.Clear();

		foreach (var item in contentIndex)
		{
			if (item.Value.path.EndsWith(".load"))
			{

			}
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
