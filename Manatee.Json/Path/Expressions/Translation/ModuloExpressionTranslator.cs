using System;
using System.Linq.Expressions;

namespace Manatee.Json.Path.Expressions.Translation
{
	internal class ModuloExpressionTranslator : IExpressionTranslator
	{
		public ExpressionTreeNode<T> Translate<T>(Expression body)
		{
			var add = body as BinaryExpression;
			if (add == null)
				throw new InvalidOperationException();
			return new ModuloExpression<T>(ExpressionTranslator.TranslateNode<T>(add.Left),
			                               ExpressionTranslator.TranslateNode<T>(add.Right));
		}
	}
}