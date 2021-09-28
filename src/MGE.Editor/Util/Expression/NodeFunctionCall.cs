namespace MGE.Editor.Util.Expression
{
	public class NodeFunctionCall : Node
	{
		public NodeFunctionCall(string functionName, Node[] arguments)
		{
			_functionName = functionName;
			_arguments = arguments;
		}

		string _functionName;
		Node[] _arguments;

		public override double Eval(IExpressionContext ctx)
		{
			var argVals = new double[_arguments.Length];
			for (int i = 0; i < _arguments.Length; i++)
			{
				argVals[i] = _arguments[i].Eval(ctx);
			}

			return ctx.CallFunction(_functionName, argVals);
		}
	}
}
