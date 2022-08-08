using System;

namespace TQVaultAE.Domain.Entities;

/// <summary>
/// Gear rarity
/// </summary>
public enum Rarity
{
	/// <summary>
	/// Item is no gear
	/// </summary>
	NoGear = 0,
	Broken = 1,
	Mundane = 2,
	Common = 3,
	Rare = 4,
	Epic = 5,
	Legendary = 6,
}

#region Related logic

public static class RarityExtension
{
	public static ItemStyle? GetItemStyle(this Rarity level)
	{
		var style = level switch
		{
			Rarity.Broken => ItemStyle.Broken,
			Rarity.Mundane => ItemStyle.Mundane,
			Rarity.Common => ItemStyle.Common,
			Rarity.Rare => ItemStyle.Rare,
			Rarity.Epic => ItemStyle.Epic,
			Rarity.Legendary => ItemStyle.Legendary,
			_ => ItemStyle.Quest
		};

		if (style == ItemStyle.Quest) return null;

		return style;
	}

	public static string GetTranslationTag(this Rarity level)
	{
		var style = GetItemStyle(level);

		if (style is null) return null;

		return EnumsNET.Enums.AsString(style.Value, EnumsNET.EnumFormat.Description, EnumsNET.EnumFormat.Name);
	}
}

#endregion
