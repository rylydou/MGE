namespace MGE.Loaders;

public class SpriteFontLoader : IContentLoader
{
	[Save] public float resolutionFactor = 1;
	[Save] public int kernelSize = 1;

	public object Load(File file, string path)
	{
		return new Font(App.graphics, file, resolutionFactor, kernelSize);
	}
}
