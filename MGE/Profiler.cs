using System;
using System.Collections.Generic;
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

	public Color backgroundColor = new(0x00000080);
	public Color foregroundColor = new(0xFFFFFFFF);

	public Color timeGraphColor = new(0x26A269FF);
	public double currentTime;
	public double[] timeHistory = new double[256];

	public Color memoryGraphColor = new(0x3584E4FF);
	public Color memoryDotColor = new(0xE01B24FF);
	public long availableMemory = 1; // To prevent divide by zero

	public long memoryUsage;
	public long[] memoryUsageHistory = new long[256];
	public Queue<int> ticksPerFrameHistory = new();

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

		currentTime = Time.rawVariableDelta;
		timeHistory[_next] = currentTime;

		availableMemory = Environment.WorkingSet;
		memoryUsage = GC.GetTotalMemory(false);
		memoryUsageHistory[_next] = memoryUsage;

		ticksPerFrameHistory.Enqueue(App.ticksThisFrame);
		while (ticksPerFrameHistory.Count > 10)
		{
			ticksPerFrameHistory.Dequeue();
		}

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
						$"{1.0 / currentTime:F0}fps {1.0 / timeHistory.Average():F0}avg {1.0 / timeHistory.Max():F0}min\n" +
						$"{(double)memoryUsage / 1048576:F2}MB / {availableMemory / 1048576}MB";
					batch.DrawString(App.content.font, text, new(4, 4), foregroundColor, 16);
				}
				break;

			case Visibility.Full:
				{
					// batch.SetBox(new(0, 0, 512, 128), backgroundColor);

					// Frame time Graph
					// var fpsMax = frameTimeHistory.Min() / 2;
					// var fpsMax = 1.0 / Time.tickStepTarget / 2;
					var fpsMax = timeHistory.Average() / 2;
					for (int i = 0; i < timeHistory.Length; i++)
					{
						var height = (float)(fpsMax / timeHistory[i] * 128);
						batch.SetBox(i, 0, 1, height, i == _next ? foregroundColor : timeGraphColor);
					}

					// Memory Graph
					var memMax = Mathf.Max(memoryUsageHistory.Max(), memoryUsageHistoryHistorical.Max());
					// var memMax = memAvailable;
					for (int i = 0; i < memoryUsageHistory.Length; i++)
					{
						var height = (float)((double)memoryUsageHistory[i] / memMax * 128);
						batch.SetBox(256 + i, 0, 1, height, i == _next ? foregroundColor : memoryGraphColor);

						var memUsageLong = memoryUsageHistoryHistorical[i];
						if (memUsageLong == 0) continue;
						height = (float)((double)memUsageLong / memMax * 128);
						batch.SetBox(256 + i, height, 1, 1, memoryDotColor);
					}

					var text =
						$"{1.0 / currentTime:F0}fps {1.0 / timeHistory.Average():F0}avg {1.0 / timeHistory.Max():F0}min\n" +
						$"ttf: {string.Join(' ', ticksPerFrameHistory.Select(n => n.ToString()))}";

					batch.DrawString(App.content.font, text, new(4, 4), foregroundColor, 16);
					batch.DrawString(App.content.font, $"{(double)memoryUsage / 1048576:F2}MB / {availableMemory / 1048576}MB", new(256 + 4, 4), foregroundColor, 16);
				}
				break;
		}


		batch.Render(window);
	}
}
