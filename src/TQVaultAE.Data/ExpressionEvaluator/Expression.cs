//-----------------------------------------------------------------------
// <copyright file="Expression.cs" company="BlueLaser505">
//     Copyright (c) BlueLaser505. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Data.ExpressionEvaluator
{
	/// <summary>
	/// Parent class for the expression tree
	/// </summary>
	public class Expression
	{
		/// <summary>
		/// Constant value for boolean true.
		/// </summary>
		public const float EXPRESSIONTRUE = 1.0F;

		/// <summary>
		/// Constant value for boolean false.
		/// </summary>
		public const float EXPRESSIONFALSE = 0.0F;

		/// <summary>
		/// Initializes a new instance of the Expression class.
		/// </summary>
		public Expression()
		{
		}

		/// <summary>
		/// Evaluates the current expression into a value.
		/// </summary>
		/// <returns>float with the evaluation results</returns>
		public virtual float Evaluate()
		{
			return 0.0F;
		}
	}
}