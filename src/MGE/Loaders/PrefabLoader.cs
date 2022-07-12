namespace MGE.Loaders;

public class PrefabLoader : IContentLoader
{
	public object Load(File file, string filename)
	{
		return new Prefab(file.ReadMemlRaw());
	}
}
