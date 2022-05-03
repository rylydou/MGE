namespace MGE.Loaders;

public class SDFFontLoader : IContentLoader
{
	public object Load(File file, string path)
	{
		var tex = new Texture(file.path + ".png");
		tex.filter = TextureFilter.Linear;

		var def = file.ReadJson<SDFFontDef>();

		return new Font(tex, def);
	}
}
