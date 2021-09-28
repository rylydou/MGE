using System;
using System.Collections.Generic;
using System.IO;
using MGE.Editor.Util.Expression;

namespace MGE.Editor.Util
{
	public class ExpressionDictionaryContext : IExpressionContext
	{
		Dictionary<string, double> vars;
		Dictionary<string, Func<double[], double>> funcs;

		public ExpressionDictionaryContext()
		{
			this.vars = new Dictionary<string, double>();
			this.funcs = new Dictionary<string, Func<double[], double>>();
		}

		public ExpressionDictionaryContext(Dictionary<string, double> vars, Dictionary<string, Func<double[], double>> funcs)
		{
			this.vars = vars;
			this.funcs = funcs;
		}

		public void AddVariable(string name, double value)
		{
			if (!vars.TryAdd(name, value)) vars[name] = value;
		}

		public void AddFunction(string name, Func<double[], double> func)
		{
			if (!funcs.TryAdd(name, func)) funcs[name] = func;
		}

		public double ResolveVariable(string name)
		{
			if (vars.TryGetValue(name, out var variable)) return variable;
			throw new InvalidDataException($"Unknown variable: '{name}'");
		}

		public double CallFunction(string name, double[] arguments)
		{
			if (funcs.TryGetValue(name, out var func)) return func.Invoke(arguments);
			throw new InvalidDataException($"Unknown function: '{name}'");
		}
	}
}
