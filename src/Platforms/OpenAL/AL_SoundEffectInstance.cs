// namespace MGE.OpenAL;

// internal class AL_SoundEffectInstance : SoundEffectInstance.Platform
// {
// 	static float ConvertPitch(float pitch)
// 	{
// 		return (float)Math.Pow(2, pitch);
// 	}

// 	public readonly AL_Audio audio;

// 	public SoundEffectInstance? soundEffectInstance;
// 	public int? handle;

// 	public override SoundEffectState state
// 	{
// 		get
// 		{
// 			if (!handle.HasValue) return SoundEffectState.Stopped;
// 			var state = AL.alGetSourceState(handle.Value); AL.CheckError("Failed get source state");
// 			switch (state)
// 			{
// 				case ALSourceState.Paused: return SoundEffectState.Paused;
// 				case ALSourceState.Playing: return SoundEffectState.Playing;
// 				default: return SoundEffectState.Stopped;
// 			}
// 		}
// 	}

// 	public AL_SoundEffectInstance(AL_Audio audio)
// 	{
// 		this.audio = audio;
// 	}

// 	~AL_SoundEffectInstance()
// 	{
// 		Dispose();
// 	}

// 	public override void Init(SoundEffectInstance soundEffectInstance)
// 	{
// 		this.soundEffectInstance = soundEffectInstance;
// 	}

// 	public override void Play()
// 	{
// 		handle = audio.GetSource();

// 		var al_soundEffect = (AL_SoundEffect)soundEffectInstance!.soundEffect!.implementation;

// 		// Bind buffer to source
// 		AL.alSourcei(handle.Value, ALSourcei.Buffer, al_soundEffect.handle); AL.CheckError("Failed bind buffer to source");

// 		// Set source properties
// 		AL.alSourcei(handle.Value, ALSourcei.SourceRelative, 1); AL.CheckError("Failed set source relative");
// 		AL.alSourcef(handle.Value, ALSourcef.Gain, soundEffectInstance.volume); AL.CheckError("Failed set volume of source");
// 		AL.alSourcef(handle.Value, ALSourcef.Pitch, soundEffectInstance.pitch); AL.CheckError("Failed set pitch of source");
// 		AL.alSource3f(handle.Value, ALSource3f.Position, soundEffectInstance.pan, 0.0f, 0.0f); AL.CheckError("Failed set pan of source");

// 		AL.alSourcePlay(handle.Value); AL.CheckError("Failed play source");
// 	}

// 	public override void Pause()
// 	{
// 		if (!handle.HasValue) return;
// 		if (state != SoundEffectState.Playing) return;

// 		AL.alSourcePause(handle.Value); AL.CheckError("Failed pause source");
// 	}

// 	public override void Resume()
// 	{
// 		if (!handle.HasValue)
// 		{
// 			Play();
// 			return;
// 		}

// 		if (state == SoundEffectState.Paused)
// 		{
// 			AL.alSourcePlay(handle.Value); AL.CheckError("Failed resume source");
// 		}
// 	}

// 	public override void Stop()
// 	{
// 		if (!handle.HasValue) return;
// 		AL.alSourceStop(handle.Value); AL.CheckError("Failed stop source");
// 		Dispose();
// 	}

// 	public override void SetVolume(float volume)
// 	{
// 		if (!handle.HasValue) return;
// 		AL.alSourcef(handle.Value, ALSourcef.Gain, volume); AL.CheckError("Failed set volume of source", volume);
// 	}

// 	public override void SetPitch(float pitch)
// 	{
// 		if (!handle.HasValue) return;
// 		pitch = ConvertPitch(pitch);
// 		AL.alSourcef(handle.Value, ALSourcef.Pitch, pitch); AL.CheckError("Failed set pitch of source", pitch);
// 	}

// 	public override void SetPan(float pan)
// 	{
// 		if (!handle.HasValue) return;
// 		AL.alSource3f(handle.Value, ALSource3f.Position, pan, 0.0f, 0.0f); AL.CheckError("Failed set pan of source", pan);
// 	}

// 	public override void Dispose()
// 	{
// 		audio.ReturnSource(ref handle);
// 	}
// }
