namespace MGE.UI;

public enum UIDirection
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

public enum UIResizing
{
	/// <summary>
	/// Widget should have size determined by <see cref="UIWidget.fixedWidth"/> and <see cref="UIWidget.fixedHeight"/>.
	/// </summary>
	Fixed,

	/// <summary>
	/// The widget tries to be as small as possible determined by its contents.
	/// </summary>
	/// <remarks>
	/// If the widget doesn't have contents then defaults to <see cref="UIResizing.Fixed"/>.
	/// Makes <see cref="UIResizing.FillContainer"/> widgets to behave like <see cref="UIResizing.HugContents"/>.
	/// </remarks>
	HugContents,

	/// <summary>
	/// The widget fills all the availble space inside its parent distributed among other <see cref="UIResizing.FillContainer"/> widgets.
	/// </summary>
	/// <remarks>
	/// Defaults to <see cref="UIResizing.HugContents"/> if parent is set to <see cref="UIResizing.HugContents"/> or the widget is placed absolutely.
	/// </remarks>
	FillContainer,
}
