using XNA_Window = Microsoft.Xna.Framework.GameWindow;

namespace MGE
{
	public static class Window
	{
		public static XNA_Window window { get => Engine.game.Window; }
		public static string title { get => window.Title; set => window.Title = value; }
		public static Vector2Int size { get => window.ClientBounds.Size; }
		public static bool fullscreen { get => GFX.gfxManager.IsFullScreen; set => GFX.gfxManager.IsFullScreen = value; }

		static Vector2Int floatingWindowSize;
		static Vector2Int position;

		public static Event onResize = new Event();

		internal static void Init()
		{
			window.ClientSizeChanged += (sender, args) => onResize.Invoke();

#if DEBUG
			Window.title = Engine.config.gameName + " (DEBUG)";
#else
			Window.title = Engine.config.gameName;
#endif
		}

		public static void ToggleFullscreen()
		{
			Logger.Log($"Toggling Fullscreen {!GFX.gfxManager.IsFullScreen}");

			if (GFX.gfxManager.IsFullScreen)
			{
				window.Position = position;
				GFX.gfxManager.PreferredBackBufferWidth = floatingWindowSize.x;
				GFX.gfxManager.PreferredBackBufferHeight = floatingWindowSize.y;
			}
			else
			{
				position = window.Position;
				floatingWindowSize = GFX.gfxManager.GraphicsDevice.Viewport.Bounds.Size;
				GFX.gfxManager.PreferredBackBufferWidth = GFX.gfxManager.GraphicsDevice.Adapter.CurrentDisplayMode.Width;
				GFX.gfxManager.PreferredBackBufferHeight = GFX.gfxManager.GraphicsDevice.Adapter.CurrentDisplayMode.Height;
			}

			GFX.gfxManager.IsFullScreen = !GFX.gfxManager.IsFullScreen;
			GFX.gfxManager.ApplyChanges();
		}
	}
}
