using System;
using System.Collections.Generic;

namespace MGE;

public class Content : AppModule
{
	readonly Dictionary<string, object?> key_2_asset = new();
	public readonly Folder contentFolder = new Folder(Environment.CurrentDirectory) / "Content";
	public readonly Folder contentPacks = Folder.data / "Content Packs";

	protected internal override void ApplicationStarted()
	{
		var files = contentFolder.GetFilesRecursive();

		foreach (var file in files)
		{
			// key_2_asset.Add(resourcesFolder.GetRelativePath(file), );
		}
	}
}
