namespace MGE.Loaders;

public class SpriteFontLoader : IContentLoader
{
	[Save] public int size = 18;
	[Save] public string charset = @" !""#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

	public object Load(File file, string path)
	{
		return new SpriteFont(file, size, charset, TextureFilter.Nearest);
	}
}
