using System.IO;

namespace MGE;

public interface ContentLoader
{
	void Load(Stream fileStream);
}
