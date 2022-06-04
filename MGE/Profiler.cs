using System;
using System.Linq;

namespace MGE;

public class Profiler : Module
{
	public Batch2DRenderInfo? batch2DRenderInfo;

	public bool showStats = false;
	public Color backgroundColor = new(0x00000080);
	public Color foregroundColor = new(0xFFFFFFFF);

	public Color fpsColor = new(0x008751FF);
	public double currentFrameTime;
	public double[] frameTimeHistory = new double[256];

	public Color memColor = new(0x3C6FC8FF);
	public Color memColorLong = new(0xFE9137FF);
	public long memAvailable = 1; // To prevent divide by zero

	public long currentMemUsage;
	public long[] memUsageHistory = new long[256];

	public long[] memUsageHistoryLong = new long[256];

	byte _next = 255;
	byte _nextLong = 255;

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
		if (App.input.keyboard.Pressed(Keys.F1)) showStats = !showStats;

		_next++;

		currentFrameTime = Time.rawVariableDelta;
		frameTimeHistory[_next] = currentFrameTime;

		memAvailable = Environment.WorkingSet;
		currentMemUsage = GC.GetTotalMemory(false);
		memUsageHistory[_next] = currentMemUsage;

		if (_next != 0) return;

		_nextLong++;

		memUsageHistoryLong[_nextLong] = (long)memUsageHistory.Average();
	}

	void Render(Window window)
	{
		var batch = Batch2D.current;

		if (Time.duration.TotalSeconds < 5f)
		{
			batch.DrawString(App.content.font, "Profiler enabled, press F1 to show stats", new(8, 8), Color.white, 16);
			batch.Render(window);
		}

		if (!showStats) return;

		batch.SetBox(new(0, 0, 512, 128), backgroundColor);

		// Frame time Graph
		// var fpsMax = frameTimeHistory.Min() / 2;
		// var fpsMax = 1.0 / Time.tickStepTarget / 2;
		var fpsMax = frameTimeHistory.Average() / 2;
		for (int i = 0; i < frameTimeHistory.Length; i++)
		{
			var height = (float)(fpsMax / frameTimeHistory[i] * 128);
			batch.SetBox(i, 0, 1, height, i == _next ? foregroundColor : fpsColor);
		}

		// Memory Graph
		var memMax = Mathf.Max(memUsageHistory.Max(), memUsageHistoryLong.Max());
		// var memMax = memAvailable;
		for (int i = 0; i < memUsageHistory.Length; i++)
		{
			var height = (float)((double)memUsageHistory[i] / memMax * 128);
			batch.SetBox(256 + i, 0, 1, height, i == _next ? foregroundColor : memColor);

			var memUsageLong = memUsageHistoryLong[i];
			if (memUsageLong == 0) continue;
			height = (float)((double)memUsageLong / memMax * 128);
			batch.SetBox(256 + i, height, 1, 1, i == _next ? foregroundColor : memColorLong);
		}

		batch.DrawString(App.content.font, $"{1.0 / currentFrameTime:F0}fps {1.0 / frameTimeHistory.Average():F0}avg {1.0 / frameTimeHistory.Max():F0}min", new(4, 4), foregroundColor, 16);
		batch.DrawString(App.content.font, $"{(double)currentMemUsage / 1048576:F2}MB / {memAvailable / 1048576}MB", new(256 + 4, 4), foregroundColor, 16);

		if (batch2DRenderInfo is not null)
			batch.DrawString(App.content.font, $"{batch2DRenderInfo.triangleCount} triangles with {batch2DRenderInfo.batchCount} batches in {batch2DRenderInfo.time.TotalMilliseconds:F4}ms", new(512 + 4, 4), foregroundColor, 16);

		batch.Render(window);
	}
}
