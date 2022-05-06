using System;

namespace MGE;

public class Profiler : AppModule
{
	public double currentFrametime = double.PositiveInfinity;
	public double[] frametime = new double[100];
	public int frametimePosition = 0;

	public long currentMemAvailable = long.MaxValue;
	public long[] memAvailable = new long[100];

	public long currentMemUsage = long.MaxValue;
	public long[] memUsage = new long[100];

	public int memPosition = 0;

	protected internal override void Update(float delta)
	{
		currentFrametime = Time.rawVariableDelta;

		frametimePosition = (frametimePosition + 1) % 100;
		frametime[frametimePosition] = currentFrametime;

		currentMemAvailable = Environment.WorkingSet;
		currentMemUsage = GC.GetTotalMemory(false);

		memPosition = (memPosition + 1) % 100;
		memAvailable[memPosition] = currentMemAvailable;
		memUsage[memPosition] = currentMemUsage;
	}
}
