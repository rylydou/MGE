using System;
using System.Linq;

namespace MGE;

public class Profiler : Module
{
	public enum Visibility
	{
		Hidden,
		Summary,
		Full,
	}

	public Visibility visibility;
	public int targetFps = 60;

	public TimeSpan[] deltaHistory = new TimeSpan[256];
	public TimeSpan[] tickHistory = new TimeSpan[256];
	public TimeSpan[] updateHistory = new TimeSpan[256];

	public long availableMemory = 1; // To prevent divide by zero

	public long memoryUsage;
	public long[] memoryUsageHistory = new long[256];

	public long[] memoryUsageHistoryHistorical = new long[256];

	byte _next = 255;
	byte _nextHistorical = 255;

	public Profiler() : base(int.MaxValue - 100)
	{
	}

	protected internal override void Startup()
	{
		base.Startup();

		App.window.onRender += Render;
	}

	protected internal override void Update(float delta)
	{
		if (App.input.keyboard.Pressed(Keys.F1))
		{
			visibility = visibility switch
			{
				Visibility.Hidden => Visibility.Summary,
				Visibility.Summary => Visibility.Full,
				Visibility.Full => Visibility.Hidden,
				_ => Visibility.Full,
			};
		}

		_next++;

		deltaHistory[_next] = Time.diffTime;
		tickHistory[_next] = App.tickDuration;
		updateHistory[_next] = App.updateDuration;

		availableMemory = Environment.WorkingSet;
		memoryUsage = GC.GetTotalMemory(false);
		memoryUsageHistory[_next] = memoryUsage;

		if (_next != 0) return;

		_nextHistorical++;

		memoryUsageHistoryHistorical[_nextHistorical] = (long)memoryUsageHistory.Average();
	}

	void Render(Window window)
	{
		var batch = Batch2D.current;

		switch (visibility)
		{
			case Visibility.Hidden:
				{
					if (Time.duration.TotalSeconds < 5f)
					{
						batch.DrawString(App.content.font, "Profiler enabled, press F1 to show stats", new(8, 8), Color.white, 16);
						batch.Render(window);
					}
				}
				break;

			case Visibility.Summary:
				{
					var text =
						$"{1.0 / deltaHistory.Average(ts => ts.TotalSeconds):F0} / {1.0 / deltaHistory.Max(ts => ts.TotalSeconds):F0}\n" +
						$"{(double)memoryUsage / 1048576:F0}MB";

					for (int y = -1; y <= 1; y++)
					{
						for (int x = -1; x <= 1; x++)
						{
							batch.DrawString(App.content.font, text, new(4 + x, 4 + y), Color.black.translucent, 16);
						}
					}

					batch.DrawString(App.content.font, text, new(4, 4), Color.white, 16);
				}
				break;

			case Visibility.Full:
				{
					var info = GC.GetGCMemoryInfo();

					batch.SetBox(new(0, 0, 512, 128), Color.black.translucent);

					// Frame time Graph
					// var fpsMax = frameTimeHistory.Min() / 2;
					// var fpsMax = 1.0 / Time.tickStepTarget / 2;
					var timeAverage = deltaHistory.Average(ts => ts.TotalSeconds);
					// var timeCeil = timeAverage * 1.1;
					var timeCeil = 1.0 / targetFps;
					var timeMax = deltaHistory.Max();
					for (int i = 0; i < deltaHistory.Length; i++)
					{
						var yOffset = 0f;
						var height = (float)(deltaHistory[i].TotalSeconds / timeCeil * 128);
						batch.SetBox(i, 0, 1, height, Color.clear, Color.clear, Color.blue.translucent, Color.blue.translucent);
						height = 0f;

						height += (float)(updateHistory[i].TotalSeconds / timeCeil * 128);
						batch.SetBox(i, yOffset, 1, height, Color.yellow);
						yOffset += height;

						height += (float)(updateHistory[i].TotalSeconds / timeCeil * 128);
						batch.SetBox(i, yOffset, 1, height, Color.green);
						yOffset += height;
					}
					batch.SetBox(new(0, (float)(timeAverage / timeCeil * 128), 256, 1), Color.cyan.translucent);
					batch.SetBox(_next, 0, 1, 128, Color.white);

					// Memory Graph
					var memCeil = Mathf.Max(memoryUsageHistory.Max(), memoryUsageHistoryHistorical.Max());
					// var memMax = memAvailable;
					for (int i = 0; i < memoryUsageHistory.Length; i++)
					{
						var height = (float)((double)memoryUsageHistory[i] / memCeil * 128);
						batch.SetBox(256 + i, 0, 1, height, Color.blue);

						var memUsageLong = memoryUsageHistoryHistorical[i];
						if (memUsageLong == 0) continue;
						height = (float)((double)memUsageLong / memCeil * 128);
						batch.SetBox(256 + i, height, 1, 4, Color.yellow.translucent);
					}
					batch.SetBox(new(256, (float)((double)info.HeapSizeBytes / memCeil * 128), 256, 1), Color.lime);
					batch.SetBox(256 + _next, 0, 1, 128, Color.white);

					var text =
						$"{1.0 / timeAverage:F0} / {1.0 / timeMax.TotalSeconds:F0}\n" +
						$"{timeAverage * 1000:F2}ms / {timeMax.TotalMilliseconds:F2}ms";

					batch.DrawString(App.content.font, text, new(4, 4), Color.white, 16);

					text =
						$"{(double)memoryUsage / 1048576:F2}MB / {availableMemory / 1048576}MB\n" +
						$"#{info.Index}: {(double)info.HeapSizeBytes / 1048576:F2}MB";
					batch.DrawString(App.content.font, text, new(256 + 4, 4), Color.white, 16);
				}
				break;
		}

		batch.Render(window);
	}
}
