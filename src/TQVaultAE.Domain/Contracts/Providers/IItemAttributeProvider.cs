using System.Collections.ObjectModel;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Contracts.Providers
{
	public interface IItemAttributeProvider
	{
		/// <summary>
		/// Inidicates whether an attribute group has a particular variable name
		/// </summary>
		/// <param name="attributeList">array of attributes</param>
		/// <param name="variableName">name of variable</param>
		/// <returns>true if variable is present in the list</returns>
		bool AttributeGroupHas(Collection<Variable> attributeList, string variableName);
		/// <summary>
		/// Indicates whether an effect type is part of a particular attribute group
		/// </summary>
		/// <param name="attributeList">Array of attributes</param>
		/// <param name="type">Effect type enumeration</param>
		/// <returns>true if attribute effect in group == type</returns>
		bool AttributeGroupIs(Collection<Variable> attributeList, ItemAttributesEffectType type);
		/// <summary>
		/// Indicates whether an effect is part of a particular attribute group
		/// </summary>
		/// <param name="attributeList">Array of attributes</param>
		/// <param name="effect">effect string to be tested</param>
		/// <returns>true if attribute effect in group == effect</returns>
		bool AttributeGroupIs(Collection<Variable> attributeList, string effect);
		/// <summary>
		/// Gets the attribute group type.
		/// </summary>
		/// <param name="attributeList">array of attributes</param>
		/// <returns>Effect type of the attribute list</returns>
		ItemAttributesEffectType AttributeGroupType(Collection<Variable> attributeList);
		/// <summary>
		/// Indicates whether an attibute has a particular variable name
		/// </summary>
		/// <param name="variable">attribute variable</param>
		/// <param name="variableName">string for variable name</param>
		/// <returns>true if the variable == variable name</returns>
		bool AttributeHas(Variable variable, string variableName);
		/// <summary>
		/// Converts format string from TQ format to string.format
		/// </summary>
		/// <param name="formatValue">format string to be parsed</param>
		/// <returns>updated format string</returns>
		string ConvertFormat(string formatValue);
		/// <summary>
		/// Gets data for an attibute string.
		/// </summary>
		/// <param name="attribute">attribute string.  Internally normalized to UpperInvariant.</param>
		/// <returns>ItemAttributesData for the attribute</returns>
		ItemAttributesData GetAttributeData(string attribute);
		/// <summary>
		/// Gets the effect tag string
		/// </summary>
		/// <param name="data">attribute data</param>
		/// <returns>string containing the effect tag</returns>
		string GetAttributeTextTag(ItemAttributesData data);
		/// <summary>
		/// Gets the effect tag string
		/// </summary>
		/// <param name="attribute">attribute string</param>
		/// <returns>effect tag string</returns>
		string GetAttributeTextTag(string attribute);
		/// <summary>
		/// Indicates whether the name is a reagent
		/// </summary>
		/// <param name="name">name to be tested</param>
		/// <returns>true if a reagent</returns>
		bool IsReagent(string name);
	}
}