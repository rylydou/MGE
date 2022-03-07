namespace MGE.Loaders;

public class SpriteFontLoader : IContentLoader
{
	public float resolutionFactor = 1;
	public int kernelSize = 1;

	public object Load(File file, string path)
	{
		return new Font(App.graphics, file);
	}
}
