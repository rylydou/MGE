using System;
using System.Linq;

namespace MGE;

public class Profiler : Module
{
	public Batch2DRenderInfo? batch2DRenderInfo;

	public bool showStats = true;
	public IFont font = App.content.Get<IFont>("Fonts/Regular.json");
	public Color backgroundColor = new(0x000000, 127);
	public Color foregroundColor = new(0xFFFFFF);

	public Color fpsColor = new(0x6BA841);
	public double currentFrametime;
	public double[] frametimeHistory = new double[256];

	public Color memColor = new(0x3385B8);
	public Color memColorLong = new(0x99c2dc);
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

		currentFrametime = Time.rawVariableDelta;
		frametimeHistory[_next] = currentFrametime;

		memAvailable = Environment.WorkingSet;
		currentMemUsage = GC.GetTotalMemory(false);
		memUsageHistory[_next] = currentMemUsage;

		if (_next != 0) return;

		_nextLong++;

		memUsageHistoryLong[_nextLong] = (long)memUsageHistory.Average();
	}

	void Render(Window window)
	{
		if (!showStats) return;

		var batch = Batch2D.current;

		batch.Rect(new(0, 0, 512, 128), backgroundColor);

		// Frametime Graph
		// var fpsMax = frametimeHistory.Min() / 2;
		// var fpsMax = 1.0 / Time.tickStepTarget / 2;
		var fpsMax = frametimeHistory.Average() / 2;
		for (int i = 0; i < frametimeHistory.Length; i++)
		{
			var height = (float)(fpsMax / frametimeHistory[i] * 128);
			batch.Rect(i, 0, 1, height, i == _next ? foregroundColor : fpsColor);
		}

		// Memory Graph
		var memMax = Mathf.Max(memUsageHistory.Max(), memUsageHistoryLong.Max());
		// var memMax = memAvailable;
		for (int i = 0; i < memUsageHistory.Length; i++)
		{
			var height = (float)((double)memUsageHistory[i] / memMax * 128);
			batch.Rect(256 + i, 0, 1, height, i == _next ? foregroundColor : memColor);

			height = (float)((double)memUsageHistoryLong[i] / memMax * 128);
			batch.Rect(256 + i, height, 1, 1, i == _next ? foregroundColor : memColorLong);
		}

		batch.DrawString(font, $"{1.0 / currentFrametime:F0}fps {1.0 / frametimeHistory.Average():F0}avg {1.0 / frametimeHistory.Max():F0}min", new(4, 4), foregroundColor, 16);
		batch.DrawString(font, $"{(double)currentMemUsage / 1048576:F2}MB / {memAvailable / 1048576}MB", new(256 + 4, 4), foregroundColor, 16);

		if (batch2DRenderInfo is not null)
			batch.DrawString(font, $"{batch2DRenderInfo.triangleCount} triangles with {batch2DRenderInfo.batchCount} batches in {batch2DRenderInfo.time.TotalMilliseconds:F4}ms", new(512 + 4, 4), foregroundColor, 16);

		batch.Render(window);
	}
}
