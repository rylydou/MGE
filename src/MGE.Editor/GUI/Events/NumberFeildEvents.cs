using System;
using Gtk;

namespace MGE.Editor.GUI.Events
{
	public class NumberFeildEvents<T>
	{
		public Action<T> onSubmitted = n => { };
	}
}
