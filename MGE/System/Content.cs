using System;
using System.Collections.Generic;

namespace MGE;

public class Content : AppModule
{
	public delegate T Load<T>(T obj, File file, string path);

	abstract class ContentLoader
	{
		public abstract Type loaderFor { get; }

		public abstract object Load(object obj, File file, string path);
	}

	sealed class ContentLoader<T> : ContentLoader where T : class
	{
		public override Type loaderFor => typeof(T);

		public readonly Load<T> load;

		public ContentLoader(Load<T> load)
		{
			this.load = load;
		}

		public override object Load(object obj, File file, string path)
		{
			return load((T)obj, file, path);
		}
	}

	public readonly Folder contentFolder = new Folder(Environment.CurrentDirectory) / "Content";
	public readonly Folder contentPacksFolder = Folder.data / "Content Packs";

	readonly Dictionary<string, string> contentIndex = new();

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

		IndexContent(contentFolder, contentIndex);

		var contentPacks = contentPacksFolder.GetFolders();
		foreach (var contentPack in contentPacks)
		{
			IndexContent(contentPack, contentIndex);
		}

		Log.EndStopwatch();

		void IndexContent(Folder folder, Dictionary<string, string> contentIndex)
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

		}

		Log.EndStopwatch();
	}
}
