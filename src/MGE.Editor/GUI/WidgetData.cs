namespace MGE.Editor.GUI.Data
{
	public abstract class WidgetData<T>
	{
		public readonly T widget;

		protected WidgetData(T widget)
		{
			this.widget = widget;
		}
	}
}
