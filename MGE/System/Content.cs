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
	public readonly Folder contentPacks = Folder.data / "Content Packs";

	readonly Dictionary<string, object?> preloadedAssets = new();

	protected internal override void ApplicationStarted()
	{
		var files = contentFolder.GetFilesRecursive();

		foreach (var file in files)
		{
			// var obj = ;
			// key_2_asset.Add(resourcesFolder.GetRelativePath(file), );
		}
	}
}
