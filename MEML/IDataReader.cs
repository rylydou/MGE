namespace MEML;

// Make an interface
public interface IMemlReader
{
	/// <summary>
	/// The current Token
	/// </summary>
	MemlToken token { get; }

	/// <summary>
	/// The current Value
	/// </summary>
	object? value { get; }

	/// <summary>
	/// Skips the current Value
	/// </summary>
	void SkipValue();

	/// <summary>
	/// Reads the next Token in the Stream
	/// </summary>
	bool Read();
}
