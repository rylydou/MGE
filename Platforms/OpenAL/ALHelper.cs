using System;
using System.Diagnostics;

namespace MGE.OpenAL;

internal static class ALHelper
{
	[Conditional("DEBUG")]
	[DebuggerHidden]
	internal static void CheckError(string message = "", params object[] args)
	{
		ALError error;
		if ((error = AL10.alGetError()) != ALError.NoError)
		{
			if (args != null && args.Length > 0)
				message = String.Format(message, args);

			throw new InvalidOperationException($"{message} (Reason: {error})");
		}
	}
}

internal static class ALCHelper
{
	[Conditional("DEBUG")]
	[DebuggerHidden]
	internal static void CheckError(IntPtr device, string message = "", params object[] args)
	{
		ALCError error;
		if ((error = ALC10.alcGetError(device)) != ALCError.NoError)
		{
			if (args != null && args.Length > 0)
				message = String.Format(message, args);

			throw new InvalidOperationException(message + " (Reason: " + error.ToString() + ")");
		}
	}
}
