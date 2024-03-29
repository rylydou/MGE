namespace MGE;

public class MGException : System.Exception
{
	public MGException()
	: base("An error occurred")
	{ }

	public MGException(string message)
	: base(message)
	{ }

	/// <summary>
	/// An exception with a reason.
	/// </summary>
	/// <remarks>
	/// For the result of "Failed to Create player - Save data is corrupt" use the following arguments:
	/// failedAction: "Create player"
	/// reason: "Save data is corrupt"
	/// </remarks>
	/// <param name="failedAction">The action that failed. ex: "Create player"</param>
	/// <param name="reason">The reason the action failed. ex: "Save data is corrupt"</param>
	public MGException(string failedAction, string reason)
	: base($"Failed to {failedAction} - {reason}")
	{ }

	public MGException(string failedAction, string reason, string possibleCauseses)
	: base($"Failed to {failedAction} - {reason}\n\t{possibleCauseses}")
	{ }
}
