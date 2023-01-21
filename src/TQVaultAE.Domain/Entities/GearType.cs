using EnumsNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TQVaultAE.Domain.Contracts.Providers;

namespace TQVaultAE.Domain.Entities;

/// <summary>
/// Gear classification
/// </summary>
[Flags]
public enum GearType
{
	Undefined = 0,
	[GearTypeDescription(Item.ICLASS_HEAD, "head")]
	Head = 1 << 0,
	[GearTypeDescription(Item.ICLASS_UPPERBODY, "upperBody")]
	Torso = 1 << 1,
	[GearTypeDescription(Item.ICLASS_FOREARM, "forearm")]
	Arm = 1 << 2,
	[GearTypeDescription(Item.ICLASS_LOWERBODY, "lowerBody")]
	Leg = 1 << 3,
	[GearTypeDescription(Item.ICLASS_RING, "ring")]
	Ring = 1 << 4,
	[GearTypeDescription(Item.ICLASS_AMULET, "amulet")]
	Amulet = 1 << 5,
	[GearTypeDescription(Item.ICLASS_ARTIFACT, "")]
	Artifact = 1 << 6,
	[GearTypeDescription(Item.ICLASS_SPEAR, "spear")]
	Spear = 1 << 7,
	[GearTypeDescription(Item.ICLASS_STAFF, "staff")]
	Staff = 1 << 8,
	[GearTypeDescription(Item.ICLASS_RANGEDONEHAND, "bow")]
	Thrown = 1 << 9,
	[GearTypeDescription(Item.ICLASS_BOW, "bow")]
	Bow = 1 << 10,
	[GearTypeDescription(Item.ICLASS_SWORD, "sword")]
	Sword = 1 << 11,
	[GearTypeDescription(Item.ICLASS_MACE, "mace")]
	Mace = 1 << 12,
	[GearTypeDescription(Item.ICLASS_AXE, "axe")]
	Axe = 1 << 13,
	[GearTypeDescription(Item.ICLASS_SHIELD, "shield")]
	Shield = 1 << 14,
	//Unique = 1 << 28,
	MonsterInfrequent = 1 << 29,
	ForMage = 1 << 30,
	ForMelee = 1 << 31,
	Jewellery = Ring | Amulet,
	AllArmor = Head | Torso | Arm | Leg,
	AllWeapons = Spear | Staff | Thrown | Bow | Sword | Mace | Axe,
	AllWearable = AllWeapons | AllArmor | Jewellery | Shield,
}

#region Related Logic

public static class GearTypeExtension
{
	private static Dictionary<GearType, GearTypeDescriptionAttribute> _GearTypeMap;
	public static Dictionary<GearType, GearTypeDescriptionAttribute> GearTypeMap
	{
		get
		{
			if (_GearTypeMap is null)
			{
				_GearTypeMap = Enums.GetMembers<GearType>().Select(m => (m.Value, Attrib: m.Attributes.Get<GearTypeDescriptionAttribute>()))
					.Where(m => m.Attrib is not null)
					.ToDictionary(m => m.Value, m => m.Attrib);
			}
			return _GearTypeMap;
		}
	}

	/// <summary>
	/// Return ItemClass constant for <paramref name="type"/> like <see cref="Item.ICLASS_AMULET"/> or <see cref="string.Empty"/> if none
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public static string GetItemClass(this GearType type)
	{
		if (GearTypeMap.TryGetValue(type, out var attribute))
			return attribute.ICLASS;

		return string.Empty;
	}

	/// <summary>
	/// Return "requirement equation prefix" related to <paramref name="type"/> for use in the requirements equation or <see cref="string.Empty"/> if none
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public static string GetRequirementEquationPrefix(this GearType type)
	{
		if (GearTypeMap.TryGetValue(type, out var attribute))
			return attribute.RequirementEquationPrefix;

		return string.Empty;
	}
}

#endregion

#region Related Attributes

/// <summary>
/// Allowed <see cref="GearType"/> for this element
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
public class GearTypeAttribute : Attribute
{
	public GearType Type { get; }

	public GearTypeAttribute(GearType type)
	{
		Type = type;
	}
}

/// <summary>
/// Allowed <see cref="GearType"/> for this element
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class GearTypeDescriptionAttribute : Attribute
{
	/// <summary>
	/// ICLASS constant for this <see cref="GearType"/> like <see cref="Item.ICLASS_AMULET"/>
	/// </summary>
	public readonly string ICLASS;
	/// <summary>
	/// Requirement equation prefix for use in the requirements equation see <see cref="IItemProvider.GetRequirementEquationPrefix"/>.
	/// </summary>
	public readonly string RequirementEquationPrefix;

	public GearTypeDescriptionAttribute(string ICLASS_CONST, string requirementEquationPrefix)
	{
		this.ICLASS = ICLASS_CONST;
		this.RequirementEquationPrefix = requirementEquationPrefix;
	}
}


#endregion
