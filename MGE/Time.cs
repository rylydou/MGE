using Microsoft.Xna.Framework;

namespace MGE
{
	public static class Time
	{
		public static GameTime gameTime { get; private set; }

		public static float timeScale = 1.0f;

		public static float time { get; private set; }
		public static float unscaledTime { get; private set; }

		public static float deltaTime { get; private set; }
		public static float unscaledDeltaTime { get; private set; }

		public static int tick { get; internal set; }
		public static float tickTime { get => Engine.config.tickTime; set => Engine.config.tickTime = value; }

		public static bool runningSlowly { get; private set; }

		internal static void Update(GameTime gameTime)
		{
			Time.gameTime = gameTime;
			time += (float)gameTime.ElapsedGameTime.TotalSeconds * timeScale;
			unscaledTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

			deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds * timeScale;
			unscaledDeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

			runningSlowly = gameTime.IsRunningSlowly;
		}
	}
}