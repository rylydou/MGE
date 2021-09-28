using System;

namespace MGE.Editor.Util.Expression
{
	class NodeBinary : Node
	{
		public NodeBinary(Node lhs, Node rhs, Func<double, double, double> op)
		{
			_lhs = lhs;
			_rhs = rhs;
			_op = op;
		}

		Node _lhs;
		Node _rhs;
		Func<double, double, double> _op;

		public override double Eval(IExpressionContext ctx)
		{
			var lhsVal = _lhs.Eval(ctx);
			var rhsVal = _rhs.Eval(ctx);
			var result = _op.Invoke(lhsVal, rhsVal);
			return result;
		}
	}
}
