namespace MGE.Editor.Util
{
	public interface IExpressionContext
	{
		double ResolveVariable(string name);
		double CallFunction(string name, double[] arguments);
	}
}
