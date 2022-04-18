using System;
using System.IO;

namespace MGE.OpenAL;

internal class AL_SoundEffect : SoundEffect.Platform
{
	public readonly AL_Audio audio;
	public SoundEffect? soundEffect;

	public int handle;

	internal AL_SoundEffect(AL_Audio audio)
	{
		this.audio = audio;
	}

	public override void Init(SoundEffect soundEffect, Stream stream)
	{
		this.soundEffect = soundEffect;

		if (audio.audioEnabled)
		{
			AL.alGenBuffers(1, out handle); AL.CheckError("Failed generate buffer.");
		}

		var buffer = AudioLoader.Load(stream, out var format, out var frequency, out var channels, out var blockAlignment, out var bitsPerSample, out var samplesPerBlock, out var sampleCount);

		soundEffect.duration = TimeSpan.FromSeconds((float)sampleCount / (float)frequency);

		PlatformInitializeBuffer(buffer, buffer.Length, format, channels, frequency, blockAlignment, bitsPerSample, 0, 0);
	}

	private void PlatformInitializePcm(byte[] buffer, int offset, int count, int sampleBits, int sampleRate, AudioChannel channels, int loopStart, int loopLength)
	{
		if (sampleBits == 24)
		{
			// Convert 24-bit signed PCM to 16-bit signed PCM
			buffer = AudioLoader.Convert24To16(buffer, offset, count);
			offset = 0;
			count = buffer.Length;
			sampleBits = 16;
		}

		var format = AudioLoader.GetSoundFormat(AudioLoader.FormatPcm, (int)channels, sampleBits);

		// bind buffer
		BindDataBuffer(buffer, format, count, sampleRate);
	}

	private void PlatformInitializeIeeeFloat(byte[] buffer, int offset, int count, int sampleRate, AudioChannel channels, int loopStart, int loopLength)
	{
		if (!audio.supportsIeee)
		{
			// If 32-bit IEEE float is not supported, convert to 16-bit signed PCM
			buffer = AudioLoader.ConvertFloatTo16(buffer, offset, count);
			PlatformInitializePcm(buffer, 0, buffer.Length, 16, sampleRate, channels, loopStart, loopLength);
			return;
		}

		var format = AudioLoader.GetSoundFormat(AudioLoader.FormatIeee, (int)channels, 32);

		// bind buffer
		BindDataBuffer(buffer, format, count, sampleRate);
	}

	private void PlatformInitializeAdpcm(byte[] buffer, int offset, int count, int sampleRate, AudioChannel channels, int blockAlignment, int loopStart, int loopLength)
	{
		if (!audio.supportsAdpcm)
		{
			// If MS-ADPCM is not supported, convert to 16-bit signed PCM
			buffer = AudioLoader.ConvertMsAdpcmToPcm(buffer, offset, count, (int)channels, blockAlignment);
			PlatformInitializePcm(buffer, 0, buffer.Length, 16, sampleRate, channels, loopStart, loopLength);
			return;
		}

		var format = AudioLoader.GetSoundFormat(AudioLoader.FormatMsAdpcm, (int)channels, 0);
		int sampleAlignment = AudioLoader.SampleAlignment(format, blockAlignment);

		// bind buffer
		// Buffer length must be aligned with the block alignment
		int alignedCount = count - (count % blockAlignment);
		BindDataBuffer(buffer, format, alignedCount, sampleRate, sampleAlignment);
	}

	private void PlatformInitializeIma4(byte[] buffer, int offset, int count, int sampleRate, AudioChannel channels, int blockAlignment, int loopStart, int loopLength)
	{
		if (!audio.supportsIma4)
		{
			// If IMA/ADPCM is not supported, convert to 16-bit signed PCM
			buffer = AudioLoader.ConvertIma4ToPcm(buffer, offset, count, (int)channels, blockAlignment);
			PlatformInitializePcm(buffer, 0, buffer.Length, 16, sampleRate, channels, loopStart, loopLength);
			return;
		}

		var format = AudioLoader.GetSoundFormat(AudioLoader.FormatIma4, (int)channels, 0);
		int sampleAlignment = AudioLoader.SampleAlignment(format, blockAlignment);

		// bind buffer
		BindDataBuffer(buffer, format, count, sampleRate, sampleAlignment);
	}

	private void PlatformInitializeFormat(byte[] header, byte[] buffer, int bufferSize, int loopStart, int loopLength)
	{
		var wavFormat = BitConverter.ToInt16(header, 0);
		var channels = BitConverter.ToInt16(header, 2);
		var sampleRate = BitConverter.ToInt32(header, 4);
		var blockAlignment = BitConverter.ToInt16(header, 12);
		var bitsPerSample = BitConverter.ToInt16(header, 14);

		var format = AudioLoader.GetSoundFormat(wavFormat, channels, bitsPerSample);
		PlatformInitializeBuffer(buffer, bufferSize, format, channels, sampleRate, blockAlignment, bitsPerSample, loopStart, loopLength);
	}

	private void PlatformInitializeBuffer(byte[] buffer, int bufferSize, ALFormat format, int channels, int sampleRate, int blockAlignment, int bitsPerSample, int loopStart, int loopLength)
	{
		switch (format)
		{
			case ALFormat.Mono8:
			case ALFormat.Mono16:
			case ALFormat.Stereo8:
			case ALFormat.Stereo16:
				PlatformInitializePcm(buffer, 0, bufferSize, bitsPerSample, sampleRate, (AudioChannel)channels, loopStart, loopLength);
				break;
			case ALFormat.MonoMSAdpcm:
			case ALFormat.StereoMSAdpcm:
				PlatformInitializeAdpcm(buffer, 0, bufferSize, sampleRate, (AudioChannel)channels, blockAlignment, loopStart, loopLength);
				break;
			case ALFormat.MonoFloat32:
			case ALFormat.StereoFloat32:
				PlatformInitializeIeeeFloat(buffer, 0, bufferSize, sampleRate, (AudioChannel)channels, loopStart, loopLength);
				break;
			case ALFormat.MonoIma4:
			case ALFormat.StereoIma4:
				PlatformInitializeIma4(buffer, 0, bufferSize, sampleRate, (AudioChannel)channels, blockAlignment, loopStart, loopLength);
				break;
			default:
				throw new NotSupportedException("Unsupported wave format!");
		}
	}

	void BindDataBuffer(byte[] dataBuffer, ALFormat format, int size, int sampleRate, int sampleAlignment = 0)
	{
		if (!audio.audioEnabled) return;

		if ((format == ALFormat.MonoMSAdpcm || format == ALFormat.StereoMSAdpcm) && !audio.supportsAdpcm)
			throw new InvalidOperationException("MS-ADPCM is not supported by this OpenAL driver");
		if ((format == ALFormat.MonoIma4 || format == ALFormat.StereoIma4) && !audio.supportsIma4)
			throw new InvalidOperationException("IMA/ADPCM is not supported by this OpenAL driver");

		var openALFormat = format;
		var dataSize = size;
		// int unpackedSize = 0;

		if (sampleAlignment > 0)
		{
			AL.alBufferi(handle, ALBufferi.UnpackBlockAlignmentSoft, sampleAlignment);
			AL.CheckError("Failed fill buffer.");
		}

		AL.alBufferData(handle, openALFormat, dataBuffer, size, sampleRate);
		AL.CheckError("Failed fill buffer.");

		// int bits, channels;
		// Duration = -1;
		// AL.GetBuffer(openALDataBuffer, ALGetBufferi.Bits, out bits);
		// ALHelper.CheckError("Failed get buffer bits");
		// AL.GetBuffer(openALDataBuffer, ALGetBufferi.Channels, out channels);
		// ALHelper.CheckError("Failed get buffer channels");
		// AL.GetBuffer(openALDataBuffer, ALGetBufferi.Size, out unpackedSize);
		// ALHelper.CheckError("Failed get buffer size");
		// Duration = (float)(unpackedSize / ((bits / 8) * channels)) / (float)sampleRate;
	}

	public override void Dispose()
	{
		AL.alDeleteBuffers(1, ref handle); AL.CheckError("Failed delete buffer", handle);
	}
}
