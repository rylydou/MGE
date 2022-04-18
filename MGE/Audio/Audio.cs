using System;

namespace MGE;

/// <summary>
/// The Core Audio Module, used for playing sounds
/// </summary>
public abstract class Audio : AppModule
{
	public string apiName { get; protected set; } = "Unknown";
	public Version apiVersion { get; protected set; } = new Version(0, 0, 0);

	public abstract SoundEffect.Platform CreateSoundEffect();
	// public abstract SoundEffectInstance.Platform CreateSoundEffectInstance();
	public abstract void PlaySoundEffect(SoundEffect soundEffect, float volume, float pitch, float pan);

	float _masterVolume;
	public float masterVolume
	{
		get => _masterVolume;
		set
		{
			if (_masterVolume == value) return;
			if (_masterVolume < 0 || _masterVolume > 1) throw new ArgumentOutOfRangeException(nameof(value));
			_masterVolume = value;
		}
	}

	public bool audioEnabled { get; protected set; }

	protected Audio() : base(400) { }

	protected internal override void Startup()
	{
		Log.System($"{apiName} {apiVersion}");
	}
}
