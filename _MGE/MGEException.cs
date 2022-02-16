using System;

namespace MGE;

public class MGEException : Exception
{
	public MGEException() : base() { }
	public MGEException(string message) : base(message) { }
	public MGEException(string intendedAction, string reason) : base($"Cannot {intendedAction} - {reason}") { }
	public MGEException(string message, Exception innerException) : base(message, innerException) { }
}
