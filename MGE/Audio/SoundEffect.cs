using System;
using System.IO;

namespace MGE;

public class SoundEffect : IDisposable
{
	public abstract class Platform
	{
		public abstract void Init(SoundEffect soundEffect, Stream stream);
		public abstract void Dispose();
	}

	public TimeSpan duration = TimeSpan.Zero;

	public readonly Platform implementation;
	public readonly Audio audio;

	public SoundEffect(Audio audio, Stream stream)
	{
		this.audio = audio;
		implementation = audio.CreateSoundEffect();
		implementation.Init(this, stream);
	}

	public void Dispose()
	{
		implementation.Dispose();
	}

	public SoundEffectInstance Play(float volume, float pitch, float pan)
	{
		var sfx = new SoundEffectInstance(audio);
		sfx.SetSoundEffect(this);
		sfx.volume = volume;
		sfx.pitch = pitch;
		sfx.pan = pan;
		sfx.Play();
		return sfx;
	}
}
