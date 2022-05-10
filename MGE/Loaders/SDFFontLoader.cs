namespace MGE.Loaders;

public class SDFFontLoader : IContentLoader
{
	public object Load(File file, string filename)
	{
		var tex = new Texture(file.path + ".png");
		tex.filter = TextureFilter.Linear;

		var def = file.ReadJsonRaw();

		return new SDFFont(tex, def);
	}
}
