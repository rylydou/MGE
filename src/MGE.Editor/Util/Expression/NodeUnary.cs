using System;

namespace MGE.Editor.Util.Expression
{
	// NodeUnary for unary operations such as Negate
	class NodeUnary : Node
	{
		public NodeUnary(Node rhs, Func<double, double> op)
		{
			_rhs = rhs;
			_op = op;
		}

		Node _rhs;
		Func<double, double> _op;

		public override double Eval(IExpressionContext ctx)
		{
			var rhsVal = _rhs.Eval(ctx);
			var result = _op.Invoke(rhsVal);
			return result;
		}
	}
}
