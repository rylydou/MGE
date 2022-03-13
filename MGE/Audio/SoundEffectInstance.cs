using System;

namespace MGE;

public enum SoundEffectState
{
	Unknown,
	Playing,
	Paused,
	Stopped,
}

public class SoundEffectInstance
{
	public abstract class Platform
	{
		public abstract void SetVolume(float volume);
		public abstract void SetPitch(float pitch);
		public abstract void SetPan(float pan);

		public abstract SoundEffectState state { get; }

		public abstract void Init(SoundEffectInstance soundEffectInstance);

		public abstract void SetSoundEffect(SoundEffect soundEffect);

		public abstract void Play();
		public abstract void Pause();
		public abstract void Stop();
	}

	public readonly Platform implementation;
	public readonly Audio audio;

	float _volume;
	public float volume
	{
		get => _volume;
		set
		{
			if (_volume == value) return;
			if (_volume < 0 || volume > 1) throw new ArgumentOutOfRangeException(nameof(value));
			_volume = value;
			implementation.SetVolume(value);
		}
	}

	float _pitch;
	public float pitch
	{
		get => _pitch;
		set
		{
			if (_pitch == value) return;
			if (_pitch < -1 || pitch > 1) throw new ArgumentOutOfRangeException(nameof(value));
			_pitch = value;
			implementation.SetPitch(value);
		}
	}

	float _pan;
	public float pan
	{
		get => _pan;
		set
		{
			if (_pan == value) return;
			if (_pan < -1 || pan > 1) throw new ArgumentOutOfRangeException(nameof(value));
			_pan = value;
			implementation.SetPan(value);
		}
	}

	public SoundEffect? soundEffect;

	public SoundEffectState state => implementation.state;

	internal SoundEffectInstance(Audio audio)
	{
		this.audio = audio;
		implementation = audio.CreateSoundEffectInstance();
		implementation.Init(this);
	}

	public void SetSoundEffect(SoundEffect soundEffect)
	{
		this.soundEffect = soundEffect;
		implementation.SetSoundEffect(soundEffect);
	}

	public void Play() => implementation.Play();
	public void Pause() => implementation.Pause();
	public void Stop() => implementation.Stop();
}
