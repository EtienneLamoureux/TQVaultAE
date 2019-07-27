//-----------------------------------------------------------------------
// <copyright file="ItemAttributeListCompare.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Data
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using TQVaultAE.Domain.Contracts.Providers;
	using TQVaultAE.Domain.Entities;

	/// <summary>
	/// Used to sort AttributeEffect groups so that effects that belong together stay together
	/// </summary>
	public class ItemAttributeListCompare : IComparer<List<Variable>>
	{
		/// <summary>
		/// Flag to show whether the item is a piece of armor
		/// </summary>
		private bool isArmor;
		private readonly IItemAttributeProvider ItemAttributeProvider;

		/// <summary>
		/// Initializes a new instance of the ItemAttributeListCompare class.
		/// </summary>
		/// <param name="isArmor">flag to show that the item is a piece of armor</param>
		public ItemAttributeListCompare(bool isArmor, IItemAttributeProvider itemAttributeProvider)
		{
			this.isArmor = isArmor;
			this.ItemAttributeProvider = itemAttributeProvider;
		}

		/// <summary>
		/// Compares 2 values
		/// </summary>
		/// <param name="value1">First object to compare</param>
		/// <param name="value2">Second object to compare</param>
		/// <returns>-1 if value2 is larger, 1 if value1 is larger and 0 if equal</returns>
		int IComparer<List<Variable>>.Compare(List<Variable> value1, List<Variable> value2)
		{
			// Calculate an "order" number for each
			int ordera = this.CalcOrder(value1);
			int orderb = this.CalcOrder(value2);

			return (ordera < orderb) ? -1 : (ordera > orderb) ? 1 : 0;
		}

		/// <summary>
		/// Makes the order global
		/// </summary>
		/// <param name="order">current order number of the parameter</param>
		/// <returns>globalized order number</returns>
		private static int MakeGlobal(int order) => order + 10000000;

		/// <summary>
		/// Calculates the base order of the attribute
		/// </summary>
		/// <param name="effectType">Effect type</param>
		/// <param name="subOrder">suborder used for grouping same types</param>
		/// <returns>order value of attribute</returns>
		private int CalcBaseOrder(ItemAttributesEffectType effectType, int subOrder)
		{
			// If it is armor then make sure Defense comes out as 1
			// Shield effects still come first at 0
			int typeOrder = (int)effectType;
			if (this.isArmor)
			{
				if (effectType == ItemAttributesEffectType.ShieldEffect)
					typeOrder = 0;
				else if (effectType == ItemAttributesEffectType.Defense)
					typeOrder = 1;
				else if (typeOrder < ((int)ItemAttributesEffectType.Defense))
					++typeOrder;
			}

			return ((1000 * (1 + typeOrder)) + subOrder) * 10;
		}

		/// <summary>
		/// Calculates the order for an array of attributes
		/// </summary>
		/// <param name="attributes">List holding the attributes</param>
		/// <returns>value of the attribute</returns>
		private int CalcOrder(List<Variable> attributes)
		{
			// Get the first item to use as a reference.
			Variable v = attributes[0];
			ItemAttributesData aa = ItemAttributeProvider.GetAttributeData(v.Name);

			// put granted skills at the end
			if (v.Name.Equals("itemSkillName"))
				return 4000000;

			if (aa == null)
				return 3000000;

			// This is a good first estimate.
			int order = this.CalcBaseOrder(aa.EffectType, aa.Suborder);

			// now some special cases
			if (aa.FullAttribute.Equals("characterBaseAttackSpeedTag"))
			{
				// put it right after the base piercing stat
				ItemAttributesData piercing = ItemAttributeProvider.GetAttributeData("offensivePierceRatioMin");
				order = this.CalcBaseOrder(piercing.EffectType, piercing.Suborder) + 1;
			}
			else if (aa.FullAttribute.Equals("retaliationGlobalChance"))
			{
				// Put this guy at the beginning of the retaliation global group
				order = MakeGlobal(this.CalcBaseOrder(ItemAttributesEffectType.Retaliation, 0) - 1);
			}
			else if (aa.FullAttribute.Equals("offensiveGlobalChance"))
			{
				// put this guy at the beginning of the offensive global group
				order = MakeGlobal(this.CalcBaseOrder(ItemAttributesEffectType.Offense, 0) - 1);
			}
			else if (this.isArmor && aa.FullAttribute.Equals("offensivePhysicalMin"))
			{
				// put it right after the block recovery time stat
				ItemAttributesData blockRecovery = ItemAttributeProvider.GetAttributeData("blockRecoveryTime");
				order = this.CalcBaseOrder(blockRecovery.EffectType, blockRecovery.Suborder) + 1;
			}

			// Now see if the variable is global and move it to the global group if it is
			if (ItemAttributeProvider.AttributeGroupHas(new Collection<Variable>(attributes), "Global"))
				order = MakeGlobal(order);

			return order;
		}

	}
}