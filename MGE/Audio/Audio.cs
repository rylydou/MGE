using System;

namespace MGE;

/// <summary>
/// The Core Audio Module, used for playing sounds
/// </summary>
public abstract class Audio : AppModule
{
	public string apiName { get; protected set; } = "Unknown";
	public Version apiVersion { get; protected set; } = new Version(0, 0, 0);

	public abstract int maxSoundInstances { get; }

	public abstract SoundEffect.Platform CreateSoundEffect();
	public abstract SoundEffectInstance.Platform CreateSoundEffectInstance();

	protected Audio() : base(400)
	{
	}

	protected internal override void Startup()
	{
		Log.System($"{apiName} {apiVersion}");
	}

	protected internal sealed override void Update()
	{
	}
}
