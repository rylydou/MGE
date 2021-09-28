namespace MGE.Editor.Util.Expression
{
	class NodeNumber : Node
	{
		double _number;

		public NodeNumber(double number) => _number = number;

		public override double Eval(IExpressionContext ctx) => _number;
	}
}
