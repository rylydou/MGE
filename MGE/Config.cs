using Microsoft.Xna.Framework.Graphics;

namespace MGE
{
	public class Config
	{
		public string gameName;
		public string companyName;

		public int tps { get => Math.RoundToInt(1f / tickTime); set => tickTime = 1f / value; }
		public float tickTime = 1f / 60;

		public string metaFileExtention = ".meta";
		public string indexedAssetPrefix = "~ ";

		public bool clearScreen = true;
		public Color screenClearColor = new Color("#122020");
		// Good screen clear colors: #ff7504 #048EFF #472d3c #a93b3b #fa2178 #17191e #122020 #273840 #24523b #70942F #453967 #394778 #2974a9 #3978a8 #0099db #4da6ff #0fdabd #eeeee4

		public int fpsHistorySize = 60 * 2;

		public BlendState opaqueBlend = BlendState.Opaque;
		public BlendState transparentBlend = BlendState.NonPremultiplied;

		public Config(string gameName = "", string companyName = "")
		{
			if (string.IsNullOrEmpty(gameName))
				gameName = System.Reflection.Assembly.GetCallingAssembly().GetName().Name;
			this.gameName = gameName;
			this.companyName = companyName;
		}
	}
}