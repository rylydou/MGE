using System;

namespace MGE;

/// <summary>
/// The Core Audio Module, used for playing sounds
/// </summary>
public abstract class Audio : Module
{
	public string ApiName { get; protected set; } = "Unknown";
	public Version ApiVersion { get; protected set; } = new Version(0, 0, 0);

	protected Audio() : base(400)
	{

	}

	protected internal override void Startup()
	{
		Log.Info($" - Audio {ApiName} {ApiVersion}");
	}
}
