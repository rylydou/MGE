using System.Runtime;
using MGE.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MGE
{
	public static class Engine
	{
		public static Game game { get; private set; }
		public static Config config;

		static float timeSinceLastTick;

		public static void Setup(Game game, Config settings)
		{
			if (game is null)
				throw new System.ArgumentNullException("game", "`game` cannot be null!");
			Engine.game = game;

			if (settings is null)
				throw new System.ArgumentNullException("settings", "`settings` cannot be null!");
			Engine.config = settings;

			Logger.Log(System.DateTime.Now.ToString(@"yyyy-MM-dd hh\:mm\:ss"));
			Logger.Log($"{settings.gameName} {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}");
			Logger.Log($"Mangrove Game Engine {System.Reflection.Assembly.GetAssembly(typeof(Engine))?.GetName()?.Version}");

			Logger.StartHeader("Enviorment");
			Logger.LogVar("CWD", System.Environment.CurrentDirectory);
			Logger.LogVar("Language Runtime", System.Environment.Version);
			Logger.LogVar("64bit Process?", System.Environment.Is64BitProcess);
			Logger.LogVar("Command Line Args", System.Environment.GetCommandLineArgs());

			Logger.StartHeader("Computation");
			Logger.LogVar("OS", System.Environment.OSVersion + (System.Environment.Is64BitOperatingSystem ? " 64bit" : " 32bit"));
			Logger.LogVar("Processor", $"{System.Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER")} {System.Environment.ProcessorCount} threads");

			game.IsMouseVisible = true;

			GFX.gfxManager = new GraphicsDeviceManager(game);

			GCSettings.LatencyMode = GCLatencyMode.LowLatency;

			Input.GamepadInit();
		}

		public static void Init()
		{
			GFX.sb = new SpriteBatch(game.GraphicsDevice);

			Logger.StartHeader("Graphics");
			Logger.LogVar("GPU", game.GraphicsDevice.Adapter.Description);
			Logger.LogVar("Current Display Mode", game.GraphicsDevice.Adapter.CurrentDisplayMode);
			Logger.LogVar("Supported Display Modes", game.GraphicsDevice.Adapter.SupportedDisplayModes);
			Logger.LogVar("Wide Screen?", game.GraphicsDevice.Adapter.IsWideScreen);

			GFX.gfxManager.PreferredBackBufferWidth = 1280;
			GFX.gfxManager.PreferredBackBufferHeight = 720;
			GFX.gfxManager.PreferMultiSampling = true;
			GFX.gfxManager.HardwareModeSwitch = false;
			GFX.gfxManager.SynchronizeWithVerticalRetrace = true;
			GFX.gfxManager.ApplyChanges();

			game.IsFixedTimeStep = false;

			Window.Init();
			Window.window.AllowUserResizing = true;
			Window.window.TextInput += (sender, args) => Input.WhenTextInput(args);

			Assets.ReloadAssets();

			Terminal.RegisterCommand<Commands>();

			Logger.StartHeader("Initialize Hierarchy");
			Hierarchy.root.DoInit();
		}

		public static void Load() { }

		public static void Unload()
		{
			Assets.UnloadAssets();
		}

		public static void Update(GameTime gameTime)
		{
			Time.Update(gameTime);
			Input.Update();
			GFX.Update();
			Terminal.Update();

			timeSinceLastTick += Time.deltaTime;
			while (timeSinceLastTick > config.tickTime)
			{
				Time.tick++;
				timeSinceLastTick -= config.tickTime;
				Tick();
			}

			Hierarchy.root.DoUpdate();

			if (Input.IsButtonPressed(Buttons.KB_F11) || (Input.IsButtonHeld(Buttons.KB_RightAlt) && Input.IsButtonPressed(Buttons.KB_Enter)))
				Window.ToggleFullscreen();

			Stats.Update();
		}

		static void Tick()
		{
			Time.tick++;

			Hierarchy.root.DoTick();
		}

		public static void Draw(GameTime gameTime)
		{
			if (config.clearScreen)
				game.GraphicsDevice.Clear(config.screenClearColor);

			Hierarchy.root.DoDraw();

			GFX.StartBatch();
			{
				Stats.Draw();

				Terminal.Draw();
			}
			GFX.EndBatch();
		}
	}
}