using System;
using Gtk;

[AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
sealed class EditorWindowAttribute : Attribute
{
	public readonly string name;
	public readonly Orientation orientation;

	public EditorWindowAttribute(string name, Orientation orientation = Orientation.Vertical)
	{
		this.name = name;
		this.orientation = orientation;
	}
}
