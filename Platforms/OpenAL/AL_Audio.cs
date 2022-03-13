using System;
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

	public override int maxSoundInstances => 256;

	protected override void ApplicationStarted()
	{
		apiName = "OpenGL";
	}

	protected override void FirstWindowCreated()
	{
		// Get the defualt audio device
		device = ALC10.alcOpenDevice(""); ALCHelper.CheckError(device, "Could not open default audio device");

		// Create a context using this device
		context = ALC10.alcCreateContext(device, new int[0]); ALCHelper.CheckError(device, "Could not create a context from defualt device");
		ALC10.alcMakeContextCurrent(context); ALCHelper.CheckError(device, "Could not bind the current context");

		supportsIeee = AL10.alIsExtensionPresent("AL_EXT_float32");
		supportsAdpcm = AL10.alIsExtensionPresent("AL_SOFT_MSADPCM");
		supportsIma4 = AL10.alIsExtensionPresent("AL_EXT_IMA4");
		supportsEfx = AL10.alIsExtensionPresent("AL_EXT_EFX");

		Log.System(ALC10.alcGetString(device, ALCGetString.CaptureDeviceSpecifier));
		Log.System(ALC10.alcGetString(device, ALCGetString.Extensions));
		Log.System(AL10.alGetString(ALGetString.Extensions));
	}

	public override SoundEffect.Platform CreateSoundEffect()
	{
		return new AL_SoundEffect(this);
	}

	public override SoundEffectInstance.Platform CreateSoundEffectInstance()
	{
		return new AL_SoundEffectInstance(this);
	}
}
