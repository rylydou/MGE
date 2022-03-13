using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MGE.OpenAL;

public class AL_Audio : Audio
{
	internal IntPtr device;
	internal IntPtr context;

	public bool supportsIeee;
	public bool supportsAdpcm;
	public bool supportsIma4;
	public bool supportsEfx;

	const int MAX_SOURCES = 256;

	int[]? _allSources;
	Queue<int>? _availableSources;
	List<int>? _inUseSources;

	protected override void ApplicationStarted()
	{
		apiName = "OpenGL";
	}

	protected override void FirstWindowCreated()
	{
		// Get the defualt audio device
		device = ALC.alcOpenDevice(""); ALC.CheckError(device, "Failed open default audio device");

		// Create a context using this device
		context = ALC.alcCreateContext(device, new int[0]); ALC.CheckError(device, "Failed create a context from defualt device");
		ALC.alcMakeContextCurrent(context); ALC.CheckError(device, "Failed bind the current context");

		supportsIeee = AL.alIsExtensionPresent("AL_EXT_float32");
		supportsAdpcm = AL.alIsExtensionPresent("AL_SOFT_MSADPCM");
		supportsIma4 = AL.alIsExtensionPresent("AL_EXT_IMA4");
		supportsEfx = AL.alIsExtensionPresent("AL_EXT_EFX");

		Log.System(ALC.alcGetString(device, ALCGetString.CaptureDeviceSpecifier));
		Log.System(ALC.alcGetString(device, ALCGetString.Extensions));
		Log.System(AL.alGetString(ALGetString.Extensions));

		_allSources = new int[MAX_SOURCES];
		AL.alGenSources(MAX_SOURCES, _allSources); AL.CheckError("Failed generate sources");
		_availableSources = new(_allSources);
		_inUseSources = new();

		AL.alListener3f(ALListener3f.Position, 0, 0, 0);
	}

	public override SoundEffect.Platform CreateSoundEffect()
	{
		return new AL_SoundEffect(this);
	}

	// public override SoundEffectInstance.Platform CreateSoundEffectInstance()
	// {
	// 	return new AL_SoundEffectInstance(this);
	// }

	internal static float ConvertPitch(float pitch)
	{
		return (float)Math.Pow(2, pitch);
	}

	public override void PlaySoundEffect(SoundEffect soundEffect, float volume, float pitch, float pan)
	{
		var source = BorrowSource();

		var al_soundEffect = (AL_SoundEffect)soundEffect.implementation;

		// Bind buffer to source
		AL.alSourcei(source, ALSourcei.Buffer, al_soundEffect.handle); AL.CheckError("Failed bind buffer to source");

		AL.alDistanceModel(ALDistanceModel.InverseDistanceClamped); AL.CheckError("Failed set source distance");

		// Set source properties
		AL.alSourcei(source, ALSourcei.SourceRelative, 1); AL.CheckError("Failed set source relative");
		AL.alSourcef(source, ALSourcef.MaxDistance, 1f); AL.CheckError("Failed set max distance");
		AL.alSourcef(source, ALSourcef.ReferenceDistance, 0.5f); AL.CheckError("Failed set reference distance");

		AL.alSourcef(source, ALSourcef.Gain, volume); AL.CheckError("Failed set volume of source");
		AL.alSourcef(source, ALSourcef.Pitch, ConvertPitch(pitch)); AL.CheckError("Failed set pitch of source");
		AL.alSource3f(source, ALSource3f.Position, pan, 0.0f, 0.1f); AL.CheckError("Failed set pan of source");

		AL.alSourcePlay(source);
	}

	internal int BorrowSource()
	{
		int source;

		lock (_availableSources!)
		{
			if (_availableSources.Count == 0) throw new Exception($"No sources to borrow");
			source = _availableSources.Dequeue();
			_availableSources.Enqueue(source);
		}

		return source;
	}

	internal int GetSource()
	{
		int source;
		lock (_availableSources!)
		{
			if (_availableSources.Count == 0) throw new Exception($"Reached source limit of {MAX_SOURCES}");

			source = _availableSources.Dequeue();
			_inUseSources!.Add(source);
		}

		return source;
	}

	internal void ReturnSource(int source)
	{
		AL.alSourcei(source, ALSourcei.Buffer, 0); AL.CheckError("Failed free source from buffers");

		lock (_availableSources!)
		{
			if (_inUseSources!.Remove(source))
			{
				_availableSources.Enqueue(source);
			}
		}
	}
}
