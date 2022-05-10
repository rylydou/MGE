namespace MGE.Loaders;

public class TextureLoader : IContentLoader
{
	public object Load(File file, string filename)
	{
		return new Texture(file);
	}
}
