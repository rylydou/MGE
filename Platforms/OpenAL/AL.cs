#region License
/* OpenAL# - C# Wrapper for OpenAL Soft
 *
 * Copyright (c) 2014-2015 Ethan Lee.
 *
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from
 * the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 * claim that you wrote the original software. If you use this software in a
 * product, an acknowledgment in the product documentation would be
 * appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not be
 * misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source distribution.
 *
 * Ethan "flibitijibibo" Lee <flibitijibibo@flibitijibibo.com>
 *
 */
#endregion

#region Using Statements
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
#endregion

namespace MGE.OpenAL;

public static class AL
{
	[Conditional("DEBUG")]
	[DebuggerHidden]
	internal static void CheckError(string message = "", params object[] args)
	{
		ALError error;
		if ((error = AL.alGetError()) != ALError.NoError)
		{
			if (args != null && args.Length > 0)
				message = String.Format(message, args);

			throw new InvalidOperationException($"{message} (Reason: {error})");
		}
	}

	private const string nativeLibName = "OpenAL";

	/* typedef int ALenum; */
	public const int AL_NONE = 0x0000;
	public const int AL_FALSE = 0x0000;
	public const int AL_TRUE = 0x0001;

	public const int AL_SOURCE_RELATIVE = 0x0202;

	public const int AL_CONE_INNER_ANGLE = 0x1001;
	public const int AL_CONE_OUTER_ANGLE = 0x1002;

	public const int AL_PITCH = 0x1003;
	public const int AL_POSITION = 0x1004;
	public const int AL_DIRECTION = 0x1005;
	public const int AL_VELOCITY = 0x1006;
	public const int AL_LOOPING = 0x1007;
	public const int AL_BUFFER = 0x1009;
	public const int AL_GAIN = 0x100A;
	public const int AL_MIN_GAIN = 0x100D;
	public const int AL_MAX_GAIN = 0x100E;
	public const int AL_ORIENTATION = 0x100F;

	public const int AL_SOURCE_STATE = 0x1010;
	public const int AL_INITIAL = 0x1011;
	public const int AL_PLAYING = 0x1012;
	public const int AL_PAUSED = 0x1013;
	public const int AL_STOPPED = 0x1014;

	public const int AL_BUFFERS_QUEUED = 0x1015;
	public const int AL_BUFFERS_PROCESSED = 0x1016;

	public const int AL_REFERENCE_DISTANCE = 0x1020;
	public const int AL_ROLLOFF_FACTOR = 0x1021;
	public const int AL_CONE_OUTER_GAIN = 0x1022;

	public const int AL_MAX_DISTANCE = 0x1023;

	public const int AL_SOURCE_TYPE = 0x1027;
	public const int AL_STATIC = 0x1028;
	public const int AL_STREAMING = 0x1029;
	public const int AL_UNDETERMINED = 0x1030;

	public const int AL_FORMAT_MONO8 = 0x1100;
	public const int AL_FORMAT_MONO16 = 0x1101;
	public const int AL_FORMAT_STEREO8 = 0x1102;
	public const int AL_FORMAT_STEREO16 = 0x1103;

	public const int AL_FREQUENCY = 0x2001;
	public const int AL_BITS = 0x2002;
	public const int AL_CHANNELS = 0x2003;
	public const int AL_SIZE = 0x2004;

	public const int AL_NO_ERROR = 0x0000;
	public const int AL_INVALID_NAME = 0xA001;
	public const int AL_INVALID_ENUM = 0xA002;
	public const int AL_INVALID_VALUE = 0xA003;
	public const int AL_INVALID_OPERATION = 0xA004;
	public const int AL_OUT_OF_MEMORY = 0xA005;

	public const int AL_VENDOR = 0xB001;
	public const int AL_VERSION = 0xB002;
	public const int AL_RENDERER = 0xB003;
	public const int AL_EXTENSIONS = 0xB004;

	public const int AL_DOPPLER_FACTOR = 0xC000;
	/* Deprecated! */
	public const int AL_DOPPLER_VELOCITY = 0xC001;

	public const int AL_DISTANCE_MODEL = 0xD000;
	public const int AL_INVERSE_DISTANCE = 0xD001;
	public const int AL_INVERSE_DISTANCE_CLAMPED = 0xD002;

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alDopplerFactor(float value);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alDistanceModel(ALDistanceModel distanceModel);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alEnable(int capability);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alDisable(int capability);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern bool alIsEnabled(int capability);

	[DllImport(nativeLibName, EntryPoint = "alGetString", CallingConvention = CallingConvention.Cdecl)]
	private static extern IntPtr INTERNAL_alGetString(ALGetString param);
	public static string alGetString(ALGetString param)
	{
		return Marshal.PtrToStringAnsi(INTERNAL_alGetString(param)) ?? "";
	}

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGetBooleanv(int param, bool[] values);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGetIntegerv(int param, int[] values);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGetFloatv(int param, float[] values);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGetDoublev(int param, double[] values);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern bool alGetBoolean(int param);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern int alGetInteger(int param);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern float alGetFloat(int param);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern double alGetDouble(int param);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern ALError alGetError();

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern bool alIsExtensionPresent(
		[In()] [MarshalAs(UnmanagedType.LPStr)]
				string extname
	);

	/* IntPtr refers to a function pointer */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr alGetProcAddress(
		[In()] [MarshalAs(UnmanagedType.LPStr)]
				string fname
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern int alGetEnumValue(
		[In()] [MarshalAs(UnmanagedType.LPStr)]
				string ename
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alListenerf(
		int param,
		float value
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alListener3f(
		ALListener3f param,
		float value1,
		float value2,
		float value3
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alListenerfv(
		int param,
		float[] values
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alListeneri(
		int param,
		int value
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alListener3i(
		int param,
		int value1,
		int value2,
		int value3
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alListeneriv(
		int param,
		int[] values
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGetListenerf(
		int param,
		out float value
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGetListener3f(
		int param,
		out float value1,
		out float value2,
		out float value3
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGetListenerfv(
		int param,
		float[] values
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGetListeneri(
		int param,
		out int value
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGetListener3i(
		int param,
		out int value1,
		out int value2,
		out int value3
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGetListeneriv(
		int param,
		int[] values
	);

	/* n refers to a ALsizei */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGenSources(int n, int[] sources);

	/* n refers to an ALsizei. Overload provided to avoid uint[] alloc. */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGenSources(int n, out int sources);

	/* n refers to a ALsizei */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alDeleteSources(int n, int[] sources);

	/* n refers to an ALsizei. Overload provided to avoid uint[] alloc. */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alDeleteSources(int n, ref int sources);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern bool alIsSource(int source);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alSourcef(
		int source,
		ALSourcef param,
		float value
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alSource3f(
		int source,
		ALSource3f param,
		float value1,
		float value2,
		float value3
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alSourcefv(
		int source,
		int param,
		float[] values
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alSourcei(
		int source,
		ALSourcei param,
		int value
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alSource3i(
		int source,
		int param,
		int value1,
		int value2,
		int value3
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alSourceiv(
		int source,
		int param,
		int[] values
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGetSourcef(
		int source,
		int param,
		out float value
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGetSource3f(
		int source,
		int param,
		out float value1,
		out float value2,
		out float value3
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGetSourcefv(
		int source,
		int param,
		float[] values
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGetSourcei(
		int source,
		ALGetSourcei param,
		out int value
	);

	public static ALSourceState alGetSourceState(int source)
	{
		alGetSourcei(source, ALGetSourcei.SourceState, out var state);
		return (ALSourceState)state;
	}

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGetSource3i(
		int source,
		int param,
		out int value1,
		out int value2,
		out int value3
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGetSourceiv(
		int source,
		int param,
		int[] values
	);

	/* n refers to an ALsizei */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alSourcePlayv(
		int n,
		uint[] sources
	);

	/* n refers to an ALsizei */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alSourceStopv(
		int n,
		uint[] sources
	);

	/* n refers to an ALsizei */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alSourceRewindv(
		int n,
		uint[] sources
	);

	/* n refers to an ALsizei */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alSourcePausev(
		int n,
		uint[] sources
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alSourcePlay(int source);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alSourceStop(int source);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alSourceRewind(int source);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alSourcePause(int source);

	/* nb refers to an ALsizei */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alSourceQueueBuffers(
		int source,
		int nb,
		uint[] buffers
	);

	/* nb refers to an ALsizei. Overload provided to avoid uint[] alloc. */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alSourceQueueBuffers(
		int source,
		int nb,
		ref uint buffers
	);

	/* nb refers to an ALsizei */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alSourceUnqueueBuffers(
		int source,
		int nb,
		uint[] buffers
	);

	/* nb refers to an ALsizei. Overload provided to avoid uint[] alloc. */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alSourceUnqueueBuffers(
		int source,
		int nb,
		ref uint buffers
	);

	/* n refers to an ALsizei */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGenBuffers(int n, int[] buffers);

	/* n refers to an ALsizei. Overload provided to avoid uint[] alloc. */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGenBuffers(int n, out int buffers);

	/* n refers to an ALsizei */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alDeleteBuffers(int n, int[] buffers);

	/* n refers to an ALsizei. Overload provided to avoid uint[] alloc. */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alDeleteBuffers(int n, ref int buffers);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern bool alIsBuffer(int buffer);

	/* data refers to a void*, size and freq to an ALsizei */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alBufferData(
		int buffer,
		ALFormat format,
		IntPtr data,
		int size,
		int freq
	);

	/* size and freq refer to an ALsizei */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alBufferData(
		int buffer,
		ALFormat format,
		byte[] data,
		int size,
		int freq
	);

	/* size and freq refer to an ALsizei */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alBufferData(
		int buffer,
		ALFormat format,
		short[] data,
		int size,
		int freq
	);

	/* size and freq refer to an ALsizei */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alBufferData(
		int buffer,
		ALFormat format,
		float[] data,
		int size,
		int freq
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alBufferf(
		int buffer,
		int param,
		float value
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alBuffer3f(
		int buffer,
		int param,
		float value1,
		float value2,
		float value3
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alBufferfv(
		int buffer,
		int param,
		float[] values
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alBufferi(
		int buffer,
		ALBufferi param,
		int value
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alBuffer3i(
		int buffer,
		int param,
		int value1,
		int value2,
		int value3
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alBufferiv(
		int buffer,
		int param,
		int[] values
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGetBufferf(
		int buffer,
		int param,
		out float value
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGetBuffer3f(
		int buffer,
		int param,
		out float value1,
		out float value2,
		out float value3
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGetBufferfv(
		int buffer,
		int param,
		float[] values
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGetBufferi(
		int buffer,
		ALGetBufferi param,
		out int value
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGetBuffer3i(
		int buffer,
		int param,
		out int value1,
		out int value2,
		out int value3
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGetBufferiv(
		int buffer,
		int param,
		int[] values
	);

	// AL11

	/* typedef int ALenum; */
	public const int AL_SEC_OFFSET = 0x1024;
	public const int AL_SAMPLE_OFFSET = 0x1025;
	public const int AL_BYTE_OFFSET = 0x1026;

	public const int AL_SPEED_OF_SOUND = 0xC003;

	public const int AL_LINEAR_DISTANCE = 0xD003;
	public const int AL_LINEAR_DISTANCE_CLAMPED = 0xD004;
	public const int AL_EXPONENT_DISTANCE = 0xD005;
	public const int AL_EXPONENT_DISTANCE_CLAMED = 0xD006;

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alSpeedOfSound(float value);

	// ALEXT

	/* TODO: All OpenAL Soft extensions! Complete as needed. */

	/* typedef int ALenum */
	public const int AL_FORMAT_MONO_FLOAT32 = 0x10010;
	public const int AL_FORMAT_STEREO_FLOAT32 = 0x10011;

	public const int AL_LOOP_POINTS_SOFT = 0x2015;

	public const int AL_UNPACK_BLOCK_ALIGNMENT_SOFT = 0x200C;
	public const int AL_PACK_BLOCK_ALIGNMENT_SOFT = 0x200D;

	public const int AL_FORMAT_MONO_MSADPCM_SOFT = 0x1302;
	public const int AL_FORMAT_STEREO_MSADPCM_SOFT = 0x1303;

	public const int AL_BYTE_SOFT = 0x1400;
	public const int AL_SHORT_SOFT = 0x1402;
	public const int AL_FLOAT_SOFT = 0x1406;

	public const int AL_MONO_SOFT = 0x1500;
	public const int AL_STEREO_SOFT = 0x1501;

	public const int AL_GAIN_LIMIT_SOFT = 0x200E;

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alGetBufferSamplesSOFT(
		uint buffer,
		int offset,
		int samples,
		int channels,
		int type,
		IntPtr data
	);
}
