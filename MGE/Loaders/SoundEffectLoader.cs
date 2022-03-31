namespace MGE.Loaders;

public class SoundEffectLoader : IContentLoader
{
	public object Load(File file, string path)
	{
		using var fs = file.OpenRead();
		return new SoundEffect(App.audio, fs);
	}
}
