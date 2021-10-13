namespace MGE.Editor
{
	public class EditorSettings
	{
		public static readonly EditorSettings settings = new();

		public bool fullscreen = true;
		public string? recentProjectPath = null;

		EditorSettings()
		{

		}
	}
}
