using System.IO;

namespace MGE.Editor
{
	public class Project
	{
		public readonly DirectoryInfo root;
		public readonly DirectoryInfo assets;

		public string name;

		public Project(string path)
		{
			this.root = new(path);
			this.assets = new(path + "/Assets");

			this.name = "Untitled Game";
		}
	}
}
