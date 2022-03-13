using System;

namespace MGE.OpenAL;

internal class AL_SoundEffectInstance : SoundEffectInstance.Platform
{
	static float ConvertPitch(float pitch)
	{
		return (float)Math.Pow(2, pitch);
	}

	public readonly AL_Audio audio;

	public SoundEffectInstance? soundEffectInstance;
	public uint handle;

	public override SoundEffectState state
	{
		get
		{
			AL10.alGetSourcei(handle, ALGetSourcei.SourceState, out var state); ALHelper.CheckError("Could not get source state");
			return (SoundEffectState)(state - ALSourceState.Initial);
		}
	}

	public AL_SoundEffectInstance(AL_Audio audio)
	{
		this.audio = audio;
	}

	public override void Init(SoundEffectInstance soundEffectInstance)
	{
		this.soundEffectInstance = soundEffectInstance;

		AL10.alGenSources(1, out handle); ALHelper.CheckError("Could not generate source");
	}

	public override void SetSoundEffect(SoundEffect soundEffect)
	{
		if (!(soundEffect.implementation is AL_SoundEffect alSoundEffect)) throw new Exception();
		// Stop();
		AL10.alSourcei(handle, ALSourcei.Buffer, (int)alSoundEffect.handle); ALHelper.CheckError("Could not bind buffer to source");
	}

	public override void Play()
	{
		AL10.alSourcePlay(handle); ALHelper.CheckError("Could not play source");
	}

	public override void Pause()
	{
		AL10.alSourcePause(handle); ALHelper.CheckError("Could not pause source");
	}

	public override void Stop()
	{
		AL10.alSourceStop(handle); ALHelper.CheckError("Could not stop source");
	}

	public override void SetVolume(float volume)
	{
		AL10.alSourcef(handle, ALSourcef.Gain, volume); ALHelper.CheckError("Could not set volume of source", volume);
	}

	public override void SetPitch(float pitch)
	{
		pitch = ConvertPitch(pitch);
		AL10.alSourcef(handle, ALSourcef.Pitch, pitch); ALHelper.CheckError("Could not set pitch of source", pitch);
	}

	public override void SetPan(float pan)
	{
		AL10.alSource3f(handle, ALSource3f.Position, pan, 0.0f, 0.0f); ALHelper.CheckError("Could not set pan of source", pan);
	}
}
