using System;

namespace MGE;

public class Profiler : AppModule
{
	public double[] frametime = new double[100];
	public int ftPosition = 0;

	public long[] memTotal = new long[100];
	public long[] memUsed = new long[100];
	public int memPosition = 0;

	protected internal override void Update(float delta)
	{
		ftPosition = (ftPosition + 1) % 100;
		frametime[ftPosition] = Time.rawVariableDelta;

		memPosition = (memPosition + 1) % 100;
		memTotal[memPosition] = GC.GetTotalMemory(false);
		memUsed[memPosition] = GC.GetTotalAllocatedBytes();
	}
}
