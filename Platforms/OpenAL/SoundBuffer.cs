// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace MGE.OpenAL;

internal class OALSoundBuffer : IDisposable
{
	int openALDataBuffer;
	ALFormat openALFormat;
	int dataSize;
	bool _isDisposed;

	public readonly AL_Audio audio;

	public OALSoundBuffer(AL_Audio audio)
	{
		this.audio = audio;

		AL.alGenBuffers(1, out openALDataBuffer);
		AL.CheckError("Failed to generate OpenAL data buffer.");
	}

	~OALSoundBuffer()
	{
		Dispose(false);
	}

	public int OpenALDataBuffer
	{
		get
		{
			return openALDataBuffer;
		}
	}

	public double Duration
	{
		get;
		set;
	}

	public void BindDataBuffer(byte[] dataBuffer, ALFormat format, int size, int sampleRate, int sampleAlignment = 0)
	{
		if ((format == ALFormat.MonoMSAdpcm || format == ALFormat.StereoMSAdpcm) && !audio.supportsAdpcm)
			throw new InvalidOperationException("MS-ADPCM is not supported by this OpenAL driver");
		if ((format == ALFormat.MonoIma4 || format == ALFormat.StereoIma4) && !audio.supportsIma4)
			throw new InvalidOperationException("IMA/ADPCM is not supported by this OpenAL driver");

		openALFormat = format;
		dataSize = size;
		int unpackedSize = 0;

		if (sampleAlignment > 0)
		{
			AL.alBufferi(openALDataBuffer, ALBufferi.UnpackBlockAlignmentSoft, sampleAlignment);
			AL.CheckError("Failed to fill buffer.");
		}

		AL.alBufferData(openALDataBuffer, openALFormat, dataBuffer, size, sampleRate);
		AL.CheckError("Failed to fill buffer.");

		int bits, channels;
		Duration = -1;
		AL.alGetBufferi(openALDataBuffer, ALGetBufferi.Bits, out bits);
		AL.CheckError("Failed to get buffer bits");
		AL.alGetBufferi(openALDataBuffer, ALGetBufferi.Channels, out channels);
		AL.CheckError("Failed to get buffer channels");
		AL.alGetBufferi(openALDataBuffer, ALGetBufferi.Size, out unpackedSize);
		AL.CheckError("Failed to get buffer size");
		Duration = (float)(unpackedSize / ((bits / 8) * channels)) / (float)sampleRate;
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing)
			{
				// Clean up managed objects
			}
			// Release unmanaged resources
			if (AL.alIsBuffer(openALDataBuffer))
			{
				AL.CheckError("Failed to fetch buffer state.");
				AL.alDeleteBuffers(1, ref openALDataBuffer);
				AL.CheckError("Failed to delete buffer.");
			}

			_isDisposed = true;
		}
	}
}
