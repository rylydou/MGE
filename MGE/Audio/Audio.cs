using System;

namespace MGE;

public abstract class Audio : AppModule
{
	public string apiName { get; protected set; } = "Unknown";
	public Version apiVersion { get; protected set; } = new Version(0, 0, 0);

	protected Audio() : base(400) { }
}
