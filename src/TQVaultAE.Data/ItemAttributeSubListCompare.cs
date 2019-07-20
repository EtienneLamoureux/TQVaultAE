//-----------------------------------------------------------------------
// <copyright file="ItemAttributeSubListCompare.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Data
{
	using System.Collections.Generic;
	using TQVaultAE.Domain.Contracts.Providers;
	using TQVaultAE.Domain.Entities;

	/// <summary>
	/// Used for autosorting of items within a group
	/// </summary>
	public class ItemAttributeSubListCompare : IComparer<Variable>
	{
		private readonly IItemAttributeProvider ItemAttributeProvider;

		/// <summary>
		/// Initializes a new instance of the ItemAttributeSubListCompare class.
		/// </summary>
		public ItemAttributeSubListCompare(IItemAttributeProvider itemAttributeProvider)
		{
			this.ItemAttributeProvider = itemAttributeProvider;
		}

		/// <summary>
		/// Compares 2 values
		/// </summary>
		/// <param name="value1">First object to compare</param>
		/// <param name="value2">Second object to compare</param>
		/// <returns>-1 if value2 is larger, 1 if value1 is larger and 0 if equal</returns>
		int IComparer<Variable>.Compare(Variable value1, Variable value2)
		{
			// Calculate an "order" number for each
			int ordera = CalcOrder(value1);
			int orderb = CalcOrder(value2);

			return (ordera < orderb) ? -1 : (ordera > orderb) ? 1 : 0;
		}

		/// <summary>
		/// Calculates the base order of the attribute
		/// </summary>
		/// <param name="effectType">Effect type</param>
		/// <param name="subOrder">suborder used for grouping same types</param>
		/// <returns>order value of attribute</returns>
		private int CalcBaseOrder(ItemAttributesEffectType effectType, int subOrder)
		{
			int typeOrder = (int)effectType;
			return ((1000 * (1 + typeOrder)) + subOrder) * 10;
		}

		/// <summary>
		/// Calculates the order of the attribute based on type.
		/// </summary>
		/// <param name="variable">variable to be checked</param>
		/// <returns>order number of the variable</returns>
		private int CalcOrder(Variable variable)
		{
			ItemAttributesData aa = ItemAttributeProvider.GetAttributeData(variable.Name);
			if (aa == null)
				return 3000000;

			return CalcBaseOrder(aa.EffectType, aa.Suborder);
		}


	}
}