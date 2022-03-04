namespace MGE.Loaders;

public class TextureLoader : IContentLoader
{
	public object Load(File file, string path)
	{
		return new Texture(file);
	}
}
