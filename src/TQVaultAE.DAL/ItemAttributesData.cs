//-----------------------------------------------------------------------
// <copyright file="ItemAttributesData.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.DAL
{
	/// <summary>
	/// Used to hold the data for the item attributes
	/// </summary>
	public class ItemAttributesData
	{
		/// <summary>
		/// Initializes a new instance of the ItemAttributesData class.
		/// </summary>
		/// <param name="effectType">effect type enumeration</param>
		/// <param name="attribute">attribute string</param>
		/// <param name="effect">effect string</param>
		/// <param name="variable">variable string</param>
		/// <param name="suborder">attribute suborder</param>
		public ItemAttributesData(ItemAttributesEffectType effectType, string attribute, string effect, string variable, int suborder)
		{
			this.EffectType = effectType;
			this.FullAttribute = attribute;
			this.Effect = effect;
			this.Variable = variable;
			this.Suborder = suborder;
		}

		/// <summary>
		/// Gets or sets the Effect Type
		/// </summary>
		public ItemAttributesEffectType EffectType { get; set; }

		/// <summary>
		/// Gets or sets the effect name
		/// </summary>
		public string Effect { get; set; }

		/// <summary>
		/// Gets or sets the variable string
		/// </summary>
		public string Variable { get; set; }

		/// <summary>
		/// Gets or sets the full attribute string
		/// </summary>
		public string FullAttribute { get; set; }

		/// <summary>
		/// Gets or sets the suborder
		/// </summary>
		public int Suborder { get; set; }
	}
}