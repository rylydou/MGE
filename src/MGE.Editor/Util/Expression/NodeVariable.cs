namespace MGE.Editor.Util.Expression
{
	public class NodeVariable : Node
	{
		string _variableName;

		public NodeVariable(string variableName) => _variableName = variableName;

		public override double Eval(IExpressionContext ctx) => ctx.ResolveVariable(_variableName);
	}
}
