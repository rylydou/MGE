namespace MGE.Loaders;

public class AsepriteTextureLoader : IContentLoader
{
	public object Load(File file, string filename)
	{
		var ase = new Aseprite(file);
		var tex = new Texture(ase.frames[0].bitmap);
		return tex;
	}
}
