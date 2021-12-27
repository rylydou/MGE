namespace MGE.Editor.GUI.Data
{
	public abstract class ToggleData<T> : WidgetData<T>
	{
		public System.Action<bool> onToggled = state => { };

		protected ToggleData(T widget) : base(widget) { }
	}
}
