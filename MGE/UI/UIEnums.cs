namespace MGE.UI;

public enum UIFlow
{
	Horizontal,
	Vertical,
}

public enum UIAlignment
{
	Start,
	Center,
	End,
}

public enum UISizing
{
	/// <summary>
	/// Widget should have size determined by <see cref="UIWidget.fixedWidth"/> and <see cref="UIWidget.fixedHeight"/>.
	/// </summary>
	Fix,

	/// <summary>
	/// The widget tries to be as small as possible determined by its contents.
	/// </summary>
	/// <remarks>
	/// If the widget doesn't have contents then defaults to <see cref="UISizing.Fix"/>.
	/// Makes <see cref="UISizing.Fill"/> widgets to behave like <see cref="UISizing.Hug"/>.
	/// </remarks>
	Hug,

	/// <summary>
	/// The widget fills all the availble space inside its parent distributed among other <see cref="UISizing.Fill"/> widgets.
	/// </summary>
	/// <remarks>
	/// Defaults to <see cref="UISizing.Hug"/> if parent is set to <see cref="UISizing.Hug"/> or the widget is placed absolutely.
	/// </remarks>
	Fill,
}
