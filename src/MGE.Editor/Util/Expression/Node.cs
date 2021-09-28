namespace MGE.Editor.Util.Expression
{
	public abstract class Node
	{
		public abstract double Eval(IExpressionContext ctx);
	}
}
