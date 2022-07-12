using System;

namespace MGE;

public class Batch2DRenderInfo
{
	public TimeSpan time;

	public int triangleCount;
	public int vertexCount;
	public int indexCount;
	public int batchCount;

	public Batch2DRenderInfo(int triangleCount, int vertexCount, int indexCount, int batchCount)
	{
		this.triangleCount = triangleCount;
		this.vertexCount = vertexCount;
		this.indexCount = indexCount;
		this.batchCount = batchCount;
	}
}
