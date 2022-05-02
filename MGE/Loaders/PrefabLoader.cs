namespace MGE.Loaders;

public class PrefabLoader : IContentLoader
{
	public object Load(File file, string path)
	{
		return new Prefab(file.ReadMemlRaw());
	}
}
