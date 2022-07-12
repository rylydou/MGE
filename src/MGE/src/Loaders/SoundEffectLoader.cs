namespace MGE.Loaders;

public class SoundEffectLoader : IContentLoader
{
	public object Load(File file, string filename)
	{
		using var fs = file.OpenRead();
		return new SoundEffect(App.audio, fs);
	}
}
