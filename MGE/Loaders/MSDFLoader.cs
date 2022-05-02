namespace MGE.Loaders;

public class MSDFLoader : IContentLoader
{
	public object Load(File file, string path)
	{
		var tex = new Texture(file.path + ".png");
		tex.filter = TextureFilter.Linear;

		var def = file.ReadJson<MSDFFontDef>();

		return new MSDFFont(tex, def);
	}
}
