namespace MGE.Loaders;

public class AsepriteTextureLoader : IContentLoader
{
	public object Load(File file, string filename)
	{
		var ase = new Aseprite(file);
		return new Texture(ase.frames[0].bitmap);
	}
}

public class AsepriteSpriteSheetLoader : IContentLoader
{
	public object Load(File file, string filename)
	{
		var ase = new Aseprite(file);
		return ase.ToSpriteSheet();
	}
}
