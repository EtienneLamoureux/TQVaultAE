//-----------------------------------------------------------------------
// <copyright file="ItemAttributesData.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Domain.Entities;

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

	/// <summary>
	/// The group of an attribute: one per effect (its min, max, chance, duration...), the damage qualifiers together, an
	/// unknown variable alone. The item text lists one group per line, and the item cost equations count one per group.
	/// </summary>
	/// <param name="data">The attribute data, or <c>null</c> for an unknown variable.</param>
	/// <param name="variableName">The variable name.</param>
	public static string EffectGroupOf(ItemAttributesData? data, string variableName)
		=> data is null ? string.Concat(nameof(ItemAttributesEffectType.Other), ":", variableName)
			: data.EffectType == ItemAttributesEffectType.DamageQualifierEffect ? string.Concat(data.EffectType.ToString(), ":DamageQualifier")
			: string.Concat(data.EffectType.ToString(), ":", data.Effect);
}