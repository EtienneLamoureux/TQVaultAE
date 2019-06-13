﻿//-----------------------------------------------------------------------
// <copyright file="Variable.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.DAL
{
	using System;
	using System.Globalization;
	using System.Text;

	/// <summary>
	/// The different data-types a variable can be.
	/// </summary>
	public enum VariableDataType
	{
		/// <summary>
		/// int
		/// values will be Int32
		/// </summary>
		Integer = 0,

		/// <summary>
		/// float
		/// values will be Single
		/// </summary>
		Float = 1,

		/// <summary>
		/// string
		/// Values will be string
		/// </summary>
		StringVar = 2,

		/// <summary>
		/// bool
		/// Values will be Int32
		/// </summary>
		Boolean = 3,

		/// <summary>
		/// unknown type
		/// values will be Int32
		/// </summary>
		Unknown = 4
	}

	/// <summary>
	/// A variable within a DB Record
	/// </summary>
	public class Variable
	{
		/// <summary>
		/// the variable values.
		/// </summary>
		private object[] values;

		/// <summary>
		/// Initializes a new instance of the Variable class.
		/// </summary>
		/// <param name="variableName">string name of the variable.</param>
		/// <param name="dataType">string type of data for variable.</param>
		/// <param name="numberOfValues">int number for values that the variable contains.</param>
		public Variable(string variableName, VariableDataType dataType, int numberOfValues)
		{
			this.Name = variableName;
			this.DataType = dataType;
			this.values = new object[numberOfValues];
		}

		/// <summary>
		/// Gets the name of the variable.
		/// </summary>
		public string Name { get; private set; }

		public Variable clone()
		{
			Variable newVariable = (Variable) this.MemberwiseClone();
			newVariable.values = (object []) this.values.Clone();
			return newVariable;
		}

		/// <summary>
		/// Gets the Datatype of the variable.
		/// </summary>
		public VariableDataType DataType { get; private set; }

		/// <summary>
		/// Gets the number of values that the variable contains.
		/// </summary>
		public int NumberOfValues
		{
			get
			{
				return this.values.Length;
			}
		}

		/// <summary>
		/// Gets or sets the generic object for a particular value.
		/// </summary>
		/// <param name="index">Index of the value.</param>
		/// <returns>object containing the value.</returns>
		public object this[int index]
		{
			get
			{
				return this.values[index];
			}

			set
			{
				this.values[index] = value;
			}
		}

		/// <summary>
		/// Gets the integer for a value.
		/// Throws exception if value is not the correct type
		/// </summary>
		/// <param name="index">Index of the value.</param>
		/// <returns>Returns the integer for the value.</returns>
		public int GetInt32(int index)
		{
			return Convert.ToInt32(this.values[index], CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Gets the float for a value.
		/// </summary>
		/// <param name="index">Index of the value.</param>
		/// <returns>Single of the value.</returns>
		public float GetSingle(int index)
		{
			return Convert.ToSingle(this.values[index], CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Gets a string for a particular value.
		/// </summary>
		/// <param name="index">Index of the value.</param>
		/// <returns>
		/// string of value.
		/// </returns>
		public string GetString(int index)
		{
			return Convert.ToString(this.values[index], CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Converts the variable to a string.
		/// Format is name,val1;val2;val3;val4;...;valn,
		/// </summary>
		/// <returns>Returns converted string for the values including the variable name.</returns>
		public override string ToString()
		{
			// First set our val format string based on the data type
			string formatSpec = "{0}";
			if (this.DataType == VariableDataType.Float)
			{
				formatSpec = "{0:f6}";
			}

			StringBuilder ans = new StringBuilder(64);
			ans.Append(this.Name);
			ans.Append(",");

			for (int i = 0; i < this.NumberOfValues; ++i)
			{
				if (i > 0)
				{
					ans.Append(";");
				}

				ans.AppendFormat(CultureInfo.InvariantCulture, formatSpec, this.values[i]);
			}

			ans.Append(",");
			return ans.ToString();
		}

		/// <summary>
		/// Converts the values to a string.
		/// Format is name,val1;val2;val3;val4;...;valn,
		/// </summary>
		/// <returns>Returns converted string for the values.</returns>
		public string ToStringValue()
		{
			// First set our val format string based on the data type
			string formatSpec = "{0}";
			if (this.DataType == VariableDataType.Float)
			{
				formatSpec = "{0:f6}";
			}

			StringBuilder ans = new StringBuilder(64);
			for (int i = 0; i < this.NumberOfValues; ++i)
			{
				if (i > 0)
				{
					ans.Append(", ");
				}

				ans.AppendFormat(CultureInfo.InvariantCulture, formatSpec, this.values[i]);
			}

			return ans.ToString();
		}
	}
}