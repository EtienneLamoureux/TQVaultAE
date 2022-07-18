using System;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Helpers;

public static class GearLevelExtension
{
	public static ItemStyle? GetItemStyle(this GearLevel level)
	{
		var style = level switch
		{
			GearLevel.Broken => ItemStyle.Broken,
			GearLevel.Mundane => ItemStyle.Mundane,
			GearLevel.Common => ItemStyle.Common,
			GearLevel.Rare => ItemStyle.Rare,
			GearLevel.Epic => ItemStyle.Epic,
			GearLevel.Legendary => ItemStyle.Legendary,
			_ => ItemStyle.Quest
		};

		if (style == ItemStyle.Quest) return null;

		return style;
	}

	public static string GetTranslationTag(this GearLevel level)
	{
		var style = GetItemStyle(level);

		if (style is null) return null;

		return EnumsNET.Enums.AsString(style.Value, EnumsNET.EnumFormat.Description, EnumsNET.EnumFormat.Name);
	}
}