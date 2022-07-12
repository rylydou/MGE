#region License
/* OpenAL# - C# Wrapper for OpenAL Soft
 *
 * Copyright (c) 2014-2015 Ethan Lee
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

public static class ALC
{
	[Conditional("DEBUG")]
	[DebuggerHidden]
	internal static void CheckError(IntPtr device, string message = "", params object[] args)
	{
		ALCError error;
		if ((error = ALC.alcGetError(device)) != ALCError.NoError)
		{
			if (args is not null && args.Length > 0)
				message = String.Format(message, args);

			throw new InvalidOperationException(message + " (Reason: " + error.ToString() + ")");
		}
	}

	private const string nativeLibName = "OpenAL";

	/* typedef int ALenum; */
	public const int ALC_FALSE = 0x0000;
	public const int ALC_TRUE = 0x0001;
	public const int ALC_FREQUENCY = 0x1007;
	public const int ALC_REFRESH = 0x1008;
	public const int ALC_SYNC = 0x1009;

	public const int ALC_NO_ERROR = 0x0000;
	public const int ALC_INVALID_DEVICE = 0xA001;
	public const int ALC_INVALID_CONTEXT = 0xA002;
	public const int ALC_INVALID_ENUM = 0xA003;
	public const int ALC_INVALID_VALUE = 0xA004;
	public const int ALC_OUT_OF_MEMORY = 0xA005;

	public const int ALC_MAJOR_VERSION = 0x1000;
	public const int ALC_MINOR_VERSION = 0x1001;
	public const int ALC_ATTRIBUTES_SIZE = 0x1002;
	public const int ALC_ALL_ATTRIBUTES = 0x1003;
	public const int ALC_DEFAULT_DEVICE_SPECIFIER = 0x1004;
	public const int ALC_DEVICE_SPECIFIER = 0x1005;
	public const int ALC_EXTENSIONS = 0x1006;

	/* IntPtr refers to an ALCcontext*, device to an ALCdevice* */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr alcCreateContext(IntPtr device, int[] attrList);

	/* context refers to an ALCcontext* */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern bool alcMakeContextCurrent(IntPtr context);

	/* context refers to an ALCcontext* */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alcDestroyContext(IntPtr context);

	/* IntPtr refers to an ALCcontext* */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr alcGetCurrentContext();

	/* IntPtr refers to an ALCdevice*, context to an ALCcontext* */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr alcGetContextsDevice(IntPtr context);

	/* IntPtr refers to an ALCdevice* */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr alcOpenDevice([In()][MarshalAs(UnmanagedType.LPStr)] string devicename);

	/* device refers to an ALCdevice* */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern bool alcCloseDevice(IntPtr device);

	/* device refers to an ALCdevice* */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern ALCError alcGetError(IntPtr device);

	/* IntPtr refers to a function pointer, device to an ALCdevice* */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr alcGetProcAddress(
		IntPtr device,
		[In()] [MarshalAs(UnmanagedType.LPStr)]
				string funcname
	);

	/* device refers to an ALCdevice* */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern int alcGetEnumValue(
		IntPtr device,
		[In()] [MarshalAs(UnmanagedType.LPStr)]
				string enumname
	);

	/* device refers to an ALCdevice* */
	[DllImport(nativeLibName, EntryPoint = "alcGetString", CallingConvention = CallingConvention.Cdecl)]
	private static extern IntPtr INTERNAL_alcGetString(IntPtr device, ALCGetString param);
	public static string alcGetString(IntPtr device, ALCGetString param)
	{
		return Marshal.PtrToStringAnsi(INTERNAL_alcGetString(device, param)) ?? "";
	}

	/* device refers to an ALCdevice*, size to an ALCsizei */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alcGetIntegerv(
		IntPtr device,
		int param,
		int size,
		int[] values
	);

	// ALC11

	/* typedef int ALenum */
	public const int ALC_MONO_SOURCES = 0x1010;
	public const int ALC_STEREO_SOURCES = 0x1011;

	public const int ALC_CAPTURE_DEVICE_SPECIFIER = 0x0310;
	public const int ALC_CAPTURE_DEFAULT_DEVICE_SPECIFIER = 0x0311;
	public const int ALC_CAPTURE_SAMPLES = 0x0312;
	public const int ALC_DEFAULT_ALL_DEVICES_SPECIFIER = 0x1012;
	public const int ALC_ALL_DEVICES_SPECIFIER = 0x1013;

	/* context refers to an ALCcontext* */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alcProcessContext(IntPtr context);

	/* context refers to an ALCcontext* */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alcSuspendContext(IntPtr context);

	/* device refers to an ALCdevice* */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern bool alcIsExtensionPresent(
		IntPtr device,
		[In()] [MarshalAs(UnmanagedType.LPStr)]
				string extname
	);

	/* IntPtr refers to an ALCdevice*, buffersize to an ALCsizei */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr alcCaptureOpenDevice(
		[In()] [MarshalAs(UnmanagedType.LPStr)]
				string devicename,
		uint frequency,
		int format,
		int buffersize
	);

	/* device refers to an ALCdevice* */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern bool alcCaptureCloseDevice(IntPtr device);

	/* device refers to an ALCdevice* */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alcCaptureStart(IntPtr device);

	/* device refers to an ALCdevice* */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alcCaptureStop(IntPtr device);

	/* device refers to an ALCdevice*, samples to an ALCsizei */
	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern void alcCaptureSamples(
		IntPtr device,
		IntPtr buffer,
		int samples
	);
}
