//-----------------------------------------------------------------------
// <copyright file="ExpressionEvaluate.cs" company="BlueLaser505">
//     Copyright (c) BlueLaser505. All rights reserved.
// </copyright>
// Taken from http://sourceforge.net/projects/expression-eval/
// Ported to C# by VillageIdiot
//-----------------------------------------------------------------------
namespace TQVaultAE.Data.ExpressionEvaluator
{
	using System;

	/// <summary>
	/// Class which evaluates a string expression into a value.
	/// </summary>
	public static class ExpressionEvaluate
	{
		/// <summary>
		/// Creates the expression instance from a string.
		/// </summary>
		/// <param name="expression">expression string we want to evaluate</param>
		/// <returns>Expression instance based on the expression string.</returns>
		public static Expression CreateExpression(string expression)
		{
			int i, j, m;
			float temp;
			string left;
			string right;

			// Take out all spaces and copy to string2
			string string2 = expression.Replace(" ", string.Empty);

			// Take out parenthesis that surround the entire statement
			for (;;)
			{
				j = 1;

				// Look for a beginning open parenthesis
				if (string2.StartsWith("(", StringComparison.Ordinal))
				{
					for (i = 1; i < string2.Length; i++)
					{
						if (string2[i] == '(')
						{
							// We have found a nested parenthesis
							// so increment the count
							j++;
						}
						else if (string2[i] == ')')
						{
							// We have found a closing parenthesis
							// so we decrement the count
							j--;
							if (j == 0)
							{
								// We have found the last closing parenthesis
								// so we stop looking.
								break;
							}
						}
					}

					// Check to see if we found our parenthesis at the end
					// of the string.  The whole string is enclosed.
					if (i == string2.Length - 1)
					{
						// Extract the string without the enclosed parenthesis.
						string2 = string2.Substring(1, string2.Length - 2);
					}
					else
					{
						// Otherwise we move on.
						break;
					}
				}
				else
				{
					// No open parenthesis at the beginning then we skip it.
					break;
				}
			}

			// Begin tree building
			// Find the outermost parenthesis pair.
			// Previously each operation checked for parenthesis.
			// Now we do it only once in the beginning.
			bool noParenthesis = false;
			int leftParenthesis = -1;
			int rightParenthesis = -1;
			m = 0;
			int startPosition = string2.LastIndexOf(")", StringComparison.Ordinal);
			if (startPosition >= 0)
			{
				for (i = startPosition; i >= 0; i--)
				{
					if (string2[i] == ')')
					{
						if (m == 0)
						{
							rightParenthesis = i;
						}

						m++;
					}
					else if (string2[i] == '(')
					{
						m--;
						if (m == 0)
						{
							leftParenthesis = i;
							break;
						}
					}
				}
			}

			if (leftParenthesis == -1 && rightParenthesis == -1)
			{
				noParenthesis = true;
			}

			// Or
			int arithmeticOrPosition;
			if (noParenthesis)
			{
				// We don't have to deal with parenthesis so
				// just look for the first || occurrence.
				arithmeticOrPosition = string2.LastIndexOf("||", StringComparison.Ordinal);
			}
			else
			{
				// We have some parenthesis so we check outside of the parenthesis
				arithmeticOrPosition = string2.LastIndexOf("||", string2.Length - 1, string2.Length - rightParenthesis - 1, StringComparison.Ordinal);
				if (arithmeticOrPosition == -1)
				{
					// We didn't find an OR operator.
					// so we check the left side.
					arithmeticOrPosition = string2.LastIndexOf("||", leftParenthesis, leftParenthesis, StringComparison.Ordinal);
				}
			}

			if (arithmeticOrPosition >= 0)
			{
				// We found something so we can create an expression
				left = string2.Substring(0, arithmeticOrPosition - 1);
				right = string2.Substring(arithmeticOrPosition + 1);
				return new Or(CreateExpression(left), CreateExpression(right));
			}

			// &&
			int arithmeticAndPosition;
			if (noParenthesis)
			{
				// We don't have to deal with parenthesis so
				// just look for the first && occurrence.
				arithmeticAndPosition = string2.LastIndexOf("&&", StringComparison.Ordinal);
			}
			else
			{
				// We have some parenthesis so we check outside of the parenthesis
				arithmeticAndPosition = string2.LastIndexOf("&&", string2.Length - 1, string2.Length - rightParenthesis - 1, StringComparison.Ordinal);
				if (arithmeticAndPosition == -1)
				{
					// We didn't find an AND operator.
					// so we check the left side.
					arithmeticAndPosition = string2.LastIndexOf("&&", leftParenthesis, leftParenthesis, StringComparison.Ordinal);
				}
			}

			if (arithmeticAndPosition >= 0)
			{
				left = string2.Substring(0, arithmeticAndPosition - 1);
				right = string2.Substring(arithmeticAndPosition + 1);
				return new And(CreateExpression(left), CreateExpression(right));
			}

			// == and !=
			int equalSignPosition;
			int notEqualsPosition;
			if (noParenthesis)
			{
				// We don't have to deal with parenthesis so
				// just look for the first == and != occurrence.
				equalSignPosition = string2.LastIndexOf("==", StringComparison.Ordinal);
				notEqualsPosition = string2.LastIndexOf("!=", StringComparison.Ordinal);
			}
			else
			{
				// We have some parenthesis so we check outside of the parenthesis
				equalSignPosition = string2.LastIndexOf("==", string2.Length - 1, string2.Length - rightParenthesis - 1, StringComparison.Ordinal);
				notEqualsPosition = string2.LastIndexOf("!=", string2.Length - 1, string2.Length - rightParenthesis - 1, StringComparison.Ordinal);
				if (equalSignPosition == -1)
				{
					// We didn't find an EQUALS operator.
					// so we check the left side.
					equalSignPosition = string2.LastIndexOf("==", leftParenthesis, leftParenthesis, StringComparison.Ordinal);
				}

				if (notEqualsPosition == -1)
				{
					// We didn't find a NOT EQUAL operator.
					// so we check the left side.
					notEqualsPosition = string2.LastIndexOf("!=", leftParenthesis, leftParenthesis, StringComparison.Ordinal);
				}
			}

			// See which one comes first.
			if (equalSignPosition > notEqualsPosition)
			{
				if (equalSignPosition >= 0)
				{
					left = string2.Substring(0, equalSignPosition - 1);
					right = string2.Substring(equalSignPosition + 1);
					return new EqualEqual(CreateExpression(left), CreateExpression(right));
				}
			}
			else
			{
				if (notEqualsPosition >= 0)
				{
					left = string2.Substring(0, notEqualsPosition - 1);
					right = string2.Substring(notEqualsPosition + 1);
					return new NotEqual(CreateExpression(left), CreateExpression(right));
				}
			}

			// < > <= and >=
			int greaterThanPosition;
			int lessThanPosition;
			if (noParenthesis)
			{
				// We don't have to deal with parenthesis so
				// just look for the first > and < occurrence.
				greaterThanPosition = string2.LastIndexOf(">", StringComparison.Ordinal);
				lessThanPosition = string2.LastIndexOf("<", StringComparison.Ordinal);
			}
			else
			{
				// We have some parenthesis so we check outside of the parenthesis
				greaterThanPosition = string2.LastIndexOf(">", string2.Length - 1, string2.Length - rightParenthesis - 1, StringComparison.Ordinal);
				lessThanPosition = string2.LastIndexOf("<", string2.Length - 1, string2.Length - rightParenthesis - 1, StringComparison.Ordinal);
				if (greaterThanPosition == -1)
				{
					// We didn't find an Greater Than operator.
					// so we check the left side.
					greaterThanPosition = string2.LastIndexOf(">", leftParenthesis, leftParenthesis, StringComparison.Ordinal);
				}

				if (lessThanPosition == -1)
				{
					// We didn't find a Less Than operator.
					// so we check the left side.
					lessThanPosition = string2.LastIndexOf("<", leftParenthesis, leftParenthesis, StringComparison.Ordinal);
				}
			}

			// We found the greater than symbol first
			if (greaterThanPosition > lessThanPosition)
			{
				// Make sure we really found it.
				if (greaterThanPosition >= 0 && greaterThanPosition < string2.Length - 1)
				{
					// Look to see if we found >= instead.
					if (string2[greaterThanPosition + 1] == '=')
					{
						left = string2.Substring(0, greaterThanPosition - 1);
						right = string2.Substring(greaterThanPosition + 1);
						return new GreaterThanEqual(CreateExpression(left), CreateExpression(right));
					}
					else
					{
						left = string2.Substring(0, greaterThanPosition);
						right = string2.Substring(greaterThanPosition + 1);
						return new GreaterThan(CreateExpression(left), CreateExpression(right));
					}
				}
			}
			else
			{
				// Make sure we really found it.
				if (lessThanPosition >= 0 && lessThanPosition < string2.Length - 1)
				{
					// Look to see if we found >= instead.
					if (string2[greaterThanPosition + 1] == '=')
					{
						left = string2.Substring(0, lessThanPosition - 1);
						right = string2.Substring(lessThanPosition + 1);
						return new LessThanEqual(CreateExpression(left), CreateExpression(right));
					}
					else
					{
						left = string2.Substring(0, lessThanPosition);
						right = string2.Substring(lessThanPosition + 1);
						return new LessThan(CreateExpression(left), CreateExpression(right));
					}
				}
			}

			// Add and Subtract
			int addPos;
			int subPos;
			if (noParenthesis)
			{
				// We don't have to deal with parenthesis so
				// just look for the first + and - occurrence.
				addPos = string2.LastIndexOf("+", StringComparison.Ordinal);
				subPos = string2.LastIndexOf("-", StringComparison.Ordinal);
			}
			else
			{
				// We have some parenthesis so we check outside of the parenthesis
				addPos = string2.LastIndexOf("+", string2.Length - 1, string2.Length - rightParenthesis - 1, StringComparison.Ordinal);
				subPos = string2.LastIndexOf("-", string2.Length - 1, string2.Length - rightParenthesis - 1, StringComparison.Ordinal);
				if (addPos == -1)
				{
					// We didn't find a plus operator.
					// so we check the left side.
					addPos = string2.LastIndexOf("+", leftParenthesis, leftParenthesis, StringComparison.Ordinal);
				}

				if (subPos == -1)
				{
					// We didn't find a minus operator.
					// so we check the left side.
					subPos = string2.LastIndexOf("-", leftParenthesis, leftParenthesis, StringComparison.Ordinal);
				}
			}

			// See which one comes first.
			if (addPos > subPos)
			{
				if (addPos >= 0)
				{
					left = string2.Substring(0, addPos);
					right = string2.Substring(addPos + 1);
					return new Add(CreateExpression(left), CreateExpression(right));
				}
			}
			else
			{
				if (subPos >= 0)
				{
					left = string2.Substring(0, subPos);
					right = string2.Substring(subPos + 1);
					return new Subtract(CreateExpression(left), CreateExpression(right));
				}
			}

			// Multiply and divide
			int mulPos;
			int divPos;
			if (noParenthesis)
			{
				// We don't have to deal with parenthesis so
				// just look for the first * and / occurrence.
				mulPos = string2.LastIndexOf("*", StringComparison.Ordinal);
				divPos = string2.LastIndexOf("/", StringComparison.Ordinal);
			}
			else
			{
				// We have some parenthesis so we check outside of the parenthesis
				mulPos = string2.LastIndexOf("*", string2.Length - 1, string2.Length - rightParenthesis - 1, StringComparison.Ordinal);
				divPos = string2.LastIndexOf("/", string2.Length - 1, string2.Length - rightParenthesis - 1, StringComparison.Ordinal);
				if (mulPos == -1)
				{
					// We didn't find a multiplication operator.
					// so we check the left side.
					mulPos = string2.LastIndexOf("*", leftParenthesis, leftParenthesis, StringComparison.Ordinal);
				}

				if (divPos == -1)
				{
					// We didn't find a division operator.
					// so we check the left side.
					divPos = string2.LastIndexOf("/", leftParenthesis, leftParenthesis, StringComparison.Ordinal);
				}
			}

			// See which one comes first.
			if (mulPos > divPos)
			{
				if (mulPos >= 0)
				{
					left = string2.Substring(0, mulPos);
					right = string2.Substring(mulPos + 1);
					return new Multiply(CreateExpression(left), CreateExpression(right));
				}
			}
			else
			{
				if (divPos >= 0)
				{
					left = string2.Substring(0, divPos);
					right = string2.Substring(divPos + 1);
					return new Divide(CreateExpression(left), CreateExpression(right));
				}
			}

			// Exponent
			int expPos;
			if (noParenthesis)
			{
				// We don't have to deal with parenthesis so
				// just look for the first ^ occurrence.
				expPos = string2.LastIndexOf("^", StringComparison.Ordinal);
			}
			else
			{
				// We have some parenthesis so we check outside of the parenthesis
				expPos = string2.LastIndexOf("^", string2.Length - 1, string2.Length - rightParenthesis - 1, StringComparison.Ordinal);
				if (expPos == -1)
				{
					// We didn't find an AND operator.
					// so we check the left side.
					expPos = string2.LastIndexOf("^", leftParenthesis, leftParenthesis, StringComparison.Ordinal);
				}
			}

			if (expPos >= 0)
			{
				left = string2.Substring(0, expPos);
				right = string2.Substring(expPos + 1);
				return new Power(CreateExpression(left), CreateExpression(right));
			}

			// Make a constant
			temp = Convert.ToSingle(string2, System.Globalization.CultureInfo.InvariantCulture);
			return new Constant(temp);
		}

		/// <summary>
		/// Constant class, returns a constant.
		/// </summary>
		internal class Constant : Expression
		{
			/// <summary>
			/// constant value
			/// </summary>
			private float constant;

			/// <summary>
			/// Initializes a new instance of the Constant class.
			/// </summary>
			/// <param name="input">constant value we are assigning</param>
			public Constant(float input)
			{
				this.constant = input;
			}

			/// <summary>
			/// Evaluates the constant value.
			/// </summary>
			/// <returns>float value of the constant</returns>
			public override float Evaluate()
			{
				return this.constant;
			}
		}

		/// <summary>
		/// Add class.  Encapsulates adding two expressions.
		/// </summary>
		internal class Add : Expression
		{
			/// <summary>
			/// Expression that represents the right hand side of the equation
			/// </summary>
			private Expression rightHandSide;

			/// <summary>
			/// Expression that represents the left hand side of the equation
			/// </summary>
			private Expression leftHandSide;

			/// <summary>
			/// Initializes a new instance of the Add class.
			/// </summary>
			/// <param name="left">Left side of the add equation</param>
			/// <param name="right">Right side of the add equation</param>
			public Add(Expression left, Expression right)
			{
				this.leftHandSide = left;
				this.rightHandSide = right;
			}

			/// <summary>
			/// Evaluates adding the left hand side and right hand side
			/// </summary>
			/// <returns>float value containg the results of the add</returns>
			public override float Evaluate()
			{
				return this.leftHandSide.Evaluate() + this.rightHandSide.Evaluate();
			}
		}

		/// <summary>
		/// Subtract class.  Encapsulates subtracting two expressions.
		/// </summary>
		internal class Subtract : Expression
		{
			/// <summary>
			/// Expression that represents the right hand side of the equation
			/// </summary>
			private Expression rightHandSide;

			/// <summary>
			/// Expression that represents the left hand side of the equation
			/// </summary>
			private Expression leftHandSide;

			/// <summary>
			/// Initializes a new instance of the Subtract class.
			/// </summary>
			/// <param name="left">Left side of the subtraction equation</param>
			/// <param name="right">Right side of the subtraction equation</param>
			public Subtract(Expression left, Expression right)
			{
				this.leftHandSide = left;
				this.rightHandSide = right;
			}

			/// <summary>
			/// Evaluates subtracting the right hand side from the left hand side
			/// </summary>
			/// <returns>float value containg the results of the subtraction</returns>
			public override float Evaluate()
			{
				return this.leftHandSide.Evaluate() - this.rightHandSide.Evaluate();
			}
		}

		/// <summary>
		/// Multiply class.  Encapsulates multiplying two expressions.
		/// </summary>
		internal class Multiply : Expression
		{
			/// <summary>
			/// Expression that represents the right hand side of the equation
			/// </summary>
			private Expression rightHandSide;

			/// <summary>
			/// Expression that represents the left hand side of the equation
			/// </summary>
			private Expression leftHandSide;

			/// <summary>
			/// Initializes a new instance of the Multiply class.
			/// </summary>
			/// <param name="left">Left side of the multiplication equation</param>
			/// <param name="right">Right side of the multiplication equation</param>
			public Multiply(Expression left, Expression right)
			{
				this.leftHandSide = left;
				this.rightHandSide = right;
			}

			/// <summary>
			/// Evaluates multiplying the right hand side and the left hand side
			/// </summary>
			/// <returns>float value containg the results of the multiplication</returns>
			public override float Evaluate()
			{
				return this.leftHandSide.Evaluate() * this.rightHandSide.Evaluate();
			}
		}

		/// <summary>
		/// Divide class.  Encapsulates dividing two expressions.
		/// </summary>
		internal class Divide : Expression
		{
			/// <summary>
			/// Expression that represents the right hand side of the equation
			/// </summary>
			private Expression rightHandSide;

			/// <summary>
			/// Expression that represents the left hand side of the equation
			/// </summary>
			private Expression leftHandSide;

			/// <summary>
			/// Initializes a new instance of the Divide class.
			/// </summary>
			/// <param name="left">Left side of the division equation</param>
			/// <param name="right">Right side of the division equation</param>
			public Divide(Expression left, Expression right)
			{
				this.leftHandSide = left;
				this.rightHandSide = right;
			}

			/// <summary>
			/// Evaluates dividing the left hand side by the right hand side
			/// </summary>
			/// <returns>float value containg the results of the multiplication</returns>
			public override float Evaluate()
			{
				return this.leftHandSide.Evaluate() / this.rightHandSide.Evaluate();
			}
		}

		/// <summary>
		/// Exponent class.  Encapsulates an exponent of two expressions.
		/// </summary>
		internal class Power : Expression
		{
			/// <summary>
			/// Expression that represents the right hand side of the equation
			/// </summary>
			private Expression rightHandSide;

			/// <summary>
			/// Expression that represents the left hand side of the equation
			/// </summary>
			private Expression leftHandSide;

			/// <summary>
			/// Initializes a new instance of the Power class.
			/// </summary>
			/// <param name="left">Left side of the exponent equation</param>
			/// <param name="right">Right side of the exponent equation</param>
			public Power(Expression left, Expression right)
			{
				this.leftHandSide = left;
				this.rightHandSide = right;
			}

			/// <summary>
			/// Evaluates the left hand side to the power of the right hand side.
			/// </summary>
			/// <returns>float value containg the results of the exponent</returns>
			public override float Evaluate()
			{
				return (float)Math.Pow((double)this.leftHandSide.Evaluate(), (double)this.rightHandSide.Evaluate());
			}
		}

		/// <summary>
		/// And class.  Encapsulates anding two expressions.
		/// </summary>
		internal class And : Expression
		{
			/// <summary>
			/// Expression that represents the right hand side of the equation
			/// </summary>
			private Expression rightHandSide;

			/// <summary>
			/// Expression that represents the left hand side of the equation
			/// </summary>
			private Expression leftHandSide;

			/// <summary>
			/// Initializes a new instance of the And class.
			/// </summary>
			/// <param name="left">Left side of the and equation</param>
			/// <param name="right">Right side of the and equation</param>
			public And(Expression left, Expression right)
			{
				this.leftHandSide = left;
				this.rightHandSide = right;
			}

			/// <summary>
			/// Evaluates anding the left hand side and the right hand side.
			/// </summary>
			/// <returns>float value containg the results of the and</returns>
			public override float Evaluate()
			{
				if (((this.leftHandSide.Evaluate() != 0) ? true : false) && ((this.rightHandSide.Evaluate() != 0) ? true : false))
				{
					return EXPRESSIONTRUE;
				}
				else
				{
					return EXPRESSIONFALSE;
				}
			}
		}

		/// <summary>
		/// Or class.  Encapsulates Oring two expressions.
		/// </summary>
		internal class Or : Expression
		{
			/// <summary>
			/// Expression that represents the right hand side of the equation
			/// </summary>
			private Expression rightHandSide;

			/// <summary>
			/// Expression that represents the left hand side of the equation
			/// </summary>
			private Expression leftHandSide;

			/// <summary>
			/// Initializes a new instance of the Or class.
			/// </summary>
			/// <param name="left">Left side of the or equation</param>
			/// <param name="right">Right side of the or equation</param>
			public Or(Expression left, Expression right)
			{
				this.leftHandSide = left;
				this.rightHandSide = right;
			}

			/// <summary>
			/// Evaluates oring the left hand side and the right hand side.
			/// </summary>
			/// <returns>float value containg the results of the or</returns>
			public override float Evaluate()
			{
				if (((this.leftHandSide.Evaluate() != 0) ? true : false) || ((this.rightHandSide.Evaluate() != 0) ? true : false))
				{
					return EXPRESSIONTRUE;
				}
				else
				{
					return EXPRESSIONFALSE;
				}
			}
		}

		/// <summary>
		/// LessThan class.  Encapsulates a less than comparison of two expressions.
		/// </summary>
		internal class LessThan : Expression
		{
			/// <summary>
			/// Expression that represents the right hand side of the equation
			/// </summary>
			private Expression rightHandSide;

			/// <summary>
			/// Expression that represents the left hand side of the equation
			/// </summary>
			private Expression leftHandSide;

			/// <summary>
			/// Initializes a new instance of the LessThan class.
			/// </summary>
			/// <param name="left">Left side of the less than comparison</param>
			/// <param name="right">Right side of the less than comparison</param>
			public LessThan(Expression left, Expression right)
			{
				this.leftHandSide = left;
				this.rightHandSide = right;
			}

			/// <summary>
			/// Evaluates whether the left hand side is less than the right hand side.
			/// </summary>
			/// <returns>float value containg true if the left hand side is less than the right hand side.</returns>
			public override float Evaluate()
			{
				if (this.leftHandSide.Evaluate() < this.rightHandSide.Evaluate())
				{
					return EXPRESSIONTRUE;
				}
				else
				{
					return EXPRESSIONFALSE;
				}
			}
		}

		/// <summary>
		/// GreaterThan class.  Encapsulates a greater than comparison of two expressions.
		/// </summary>
		internal class GreaterThan : Expression
		{
			/// <summary>
			/// Expression that represents the right hand side of the equation
			/// </summary>
			private Expression rightHandSide;

			/// <summary>
			/// Expression that represents the left hand side of the equation
			/// </summary>
			private Expression leftHandSide;

			/// <summary>
			/// Initializes a new instance of the GreaterThan class.
			/// </summary>
			/// <param name="left">Left side of the greater than comparison</param>
			/// <param name="right">Right side of the greater than comparison</param>
			public GreaterThan(Expression left, Expression right)
			{
				this.leftHandSide = left;
				this.rightHandSide = right;
			}

			/// <summary>
			/// Evaluates whether the left hand side is greater than the right hand side.
			/// </summary>
			/// <returns>float value containg true if the left hand side is greater than the right hand side.</returns>
			public override float Evaluate()
			{
				if (this.leftHandSide.Evaluate() > this.rightHandSide.Evaluate())
				{
					return EXPRESSIONTRUE;
				}
				else
				{
					return EXPRESSIONFALSE;
				}
			}
		}

		/// <summary>
		/// LessThanEqual class.  Encapsulates a comparison if two expressions are less than or equal to one another.
		/// </summary>
		internal class LessThanEqual : Expression
		{
			/// <summary>
			/// Expression that represents the right hand side of the equation
			/// </summary>
			private Expression rightHandSide;

			/// <summary>
			/// Expression that represents the left hand side of the equation
			/// </summary>
			private Expression leftHandSide;

			/// <summary>
			/// Initializes a new instance of the LessThanEqual class.
			/// </summary>
			/// <param name="left">Left side of the less than or equal comparison</param>
			/// <param name="right">Right side of the less than or equal comparison</param>
			public LessThanEqual(Expression left, Expression right)
			{
				this.leftHandSide = left;
				this.rightHandSide = right;
			}

			/// <summary>
			/// Evaluates whether the left hand side is less than or equal to the right hand side.
			/// </summary>
			/// <returns>float value containg true if the left hand side is less than or equal to the right hand side.</returns>
			public override float Evaluate()
			{
				if (this.leftHandSide.Evaluate() <= this.rightHandSide.Evaluate())
				{
					return EXPRESSIONTRUE;
				}
				else
				{
					return EXPRESSIONFALSE;
				}
			}
		}

		/// <summary>
		/// GreaterThanEqual class.  Encapsulates a comparison if two expressions are greater than or equal to one another.
		/// </summary>
		internal class GreaterThanEqual : Expression
		{
			/// <summary>
			/// Expression that represents the right hand side of the equation
			/// </summary>
			private Expression rightHandSide;

			/// <summary>
			/// Expression that represents the left hand side of the equation
			/// </summary>
			private Expression leftHandSide;

			/// <summary>
			/// Initializes a new instance of the GreaterThanEqual class.
			/// </summary>
			/// <param name="left">Left side of the greater than or equal comparison</param>
			/// <param name="right">Right side of the greater than or equal comparison</param>
			public GreaterThanEqual(Expression left, Expression right)
			{
				this.leftHandSide = left;
				this.rightHandSide = right;
			}

			/// <summary>
			/// Evaluates whether the left hand side is greater than or equal to the right hand side.
			/// </summary>
			/// <returns>float value containg true if the left hand side is greater than or equal to the right hand side.</returns>
			public override float Evaluate()
			{
				if (this.leftHandSide.Evaluate() >= this.rightHandSide.Evaluate())
				{
					return EXPRESSIONTRUE;
				}
				else
				{
					return EXPRESSIONFALSE;
				}
			}
		}

		/// <summary>
		/// EqualEqual class.  Encapsulates a comparison if two expressions are equal to one another.
		/// </summary>
		internal class EqualEqual : Expression
		{
			/// <summary>
			/// Expression that represents the right hand side of the equation
			/// </summary>
			private Expression rightHandSide;

			/// <summary>
			/// Expression that represents the left hand side of the equation
			/// </summary>
			private Expression leftHandSide;

			/// <summary>
			/// Initializes a new instance of the EqualEqual class.
			/// </summary>
			/// <param name="left">Left side of the equality comparison</param>
			/// <param name="right">Right side of the equality comparison</param>
			public EqualEqual(Expression left, Expression right)
			{
				this.leftHandSide = left;
				this.rightHandSide = right;
			}

			/// <summary>
			/// Evaluates whether the left hand side is equal to the right hand side.
			/// </summary>
			/// <returns>float value containg true if the left hand side is equal to the right hand side.</returns>
			public override float Evaluate()
			{
				if (this.leftHandSide.Evaluate() == this.rightHandSide.Evaluate())
				{
					return EXPRESSIONTRUE;
				}
				else
				{
					return EXPRESSIONFALSE;
				}
			}
		}

		/// <summary>
		/// NotEqual class.  Encapsulates a comparison if two expressions are not equal to one another.
		/// </summary>
		internal class NotEqual : Expression
		{
			/// <summary>
			/// Expression that represents the right hand side of the equation
			/// </summary>
			private Expression rightHandSide;

			/// <summary>
			/// Expression that represents the left hand side of the equation
			/// </summary>
			private Expression leftHandSide;

			/// <summary>
			/// Initializes a new instance of the NotEqual class.
			/// </summary>
			/// <param name="left">Left side of the equality comparison</param>
			/// <param name="right">Right side of the equality comparison</param>
			public NotEqual(Expression left, Expression right)
			{
				this.leftHandSide = left;
				this.rightHandSide = right;
			}

			/// <summary>
			/// Evaluates whether the left hand side is not equal to the right hand side.
			/// </summary>
			/// <returns>float value containg true if the left hand side is not equal to the right hand side.</returns>
			public override float Evaluate()
			{
				if (this.leftHandSide.Evaluate() != this.rightHandSide.Evaluate())
				{
					return EXPRESSIONTRUE;
				}
				else
				{
					return EXPRESSIONFALSE;
				}
			}
		}
	}
}