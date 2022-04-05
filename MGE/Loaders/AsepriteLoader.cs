namespace MGE.Loaders;

public class AsepriteTextureLoader : IContentLoader
{
	public object Load(File file, string path)
	{
		var ase = new Aseprite(file);
		var tex = new Texture(ase.frames[0].bitmap);
		return tex;
	}
}
