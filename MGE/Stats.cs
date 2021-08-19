using System;
using System.Collections.Generic;
using System.Linq;

namespace MGE
{
	public static class Stats
	{
		public static bool enabled = false;

		public static double frametime { get; private set; }
		public static float rawFPS { get; private set; } = 0;
		public static int fps { get => Math.RoundToInt(rawFPS); }

		public static Queue<double> frametimeHistory { get; private set; } = new Queue<double>();

		public static double averageFrametime { get => frametimeHistory.Average(); }
		public static float rawAverageFPS { get => (float)(1.0 / averageFrametime); }
		public static int averageFPS { get => Math.RoundToInt(rawAverageFPS); }

		public static double maxFrametime { get => frametimeHistory.Max(); }
		public static float rawMinFPS { get => (float)(1.0 / maxFrametime); }
		public static int minFPS { get => Math.RoundToInt(rawMinFPS); }

		public static double minFrametime { get => frametimeHistory.Min(); }
		public static float rawMaxFPS { get => (float)(1.0 / minFrametime); }
		public static int maxFPS { get => Math.RoundToInt(rawMaxFPS); }

		public static long memUsed { get; private set; }
		public static long memAllocated { get; private set; }

		internal static void Update()
		{
			if (!enabled) return;

			rawFPS = 1.0f / Time.deltaTime;
			frametime = Time.gameTime.ElapsedGameTime.TotalSeconds;

			frametimeHistory.Enqueue(frametime);
			if (frametimeHistory.Count > Engine.config.fpsHistorySize)
				frametimeHistory.Dequeue();

			if ((int)Time.tick % Engine.config.tps == 0)
			{
				memUsed = GC.GetTotalMemory(false);
				memAllocated = GC.GetTotalAllocatedBytes(false);
			}
			// memAllocated = Environment.WorkingSet;
		}

		internal static void Draw()
		{
			if (!enabled) return;

			Assets.font.Draw(
				$"{Stats.averageFPS}fps ({Stats.averageFrametime.ToString("F6")}ms)   " +
				$"{Stats.minFPS}min ({Stats.maxFrametime.ToString("F6")}ms)   " +
				$"{Util.BytesToString(Stats.memUsed)}MB / {Util.BytesToString(Stats.memAllocated)}MB   " +
				$"{GFX.realDraws}/{GFX.attemptedDraws} draws in {GFX.batches} batches",
			new Vector2(8), new Color(0.75f, 0.75f));

			var index = 0;
			const int barWidth = 4;
			foreach (var frametime in frametimeHistory)
			{
				var fps = (float)(1.0 / frametime);
				var scaledFrametime = (float)(frametime * 10000);
				GFX.DrawBox(
					new Rect(index * barWidth, Window.size.y - scaledFrametime, barWidth, scaledFrametime),
					frametime > Engine.config.tickTime ? frametime > Engine.config.tickTime * 2 ? new Color(1, 0, 0, 0.25f) : new Color(1, 1, 0, 0.25f) : new Color(0, 1, 0, 0.25f));
				index++;
			}
		}
	}
}