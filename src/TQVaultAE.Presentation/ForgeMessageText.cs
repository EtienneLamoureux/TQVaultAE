using System.Globalization;
using TQVaultAE.Domain.Forge;

namespace TQVaultAE.Presentation;

/// <summary>
/// Texts of the forge panel: the message shown when <see cref="ForgeRules"/> refuses an item, and the God mode part
/// choices. Written next to <see cref="ForgeModeCardText"/>.
/// </summary>
public static class ForgeMessageText
{
	/// <summary>The text of a forge refusal.</summary>
	/// <param name="message">The refusal.</param>
	/// <param name="ofTheTinkerer">The "of the Tinkerer" suffix, translated in the game language.</param>
	/// <returns>The localized text; the message name when it has no text.</returns>
	public static string Of(ForgeMessage message, string ofTheTinkerer) => message switch
	{
		ForgeMessage.ItemMustBeStoredFirst => Resources.ForgeItemMustBeStoredFirst,
		ForgeMessage.MustSelectBaseItemFirst => Resources.ForgeMustSelectBaseItemFirst,
		ForgeMessage.MustSelectArmorOrWeaponOrJewellery => Resources.ForgeMustSelectArmorOrWeaponOrJewellery,
		ForgeMessage.MustHavePrefix => Resources.ForgeMustHavePrefixOutsideOfGodMode,
		ForgeMessage.MustHaveSuffix => Resources.ForgeMustHaveSuffixOutsideOfGodMode,
		ForgeMessage.MustSelectRelicOrItemWithRelic => Resources.ForgeMustSelectRelicOrItemWithRelic,
		ForgeMessage.NoEpicLegendary => Resources.ForgeNoEpicLegendaryAllowed,
		ForgeMessage.PrefixSuffixOfAnotherBaseItem => Resources.ForgeCantUsePrefixSuffixOfAnotherBaseItem,
		ForgeMessage.PrefixSuffixHigherLevelRequirement => Resources.ForgeCantUsePrefixSuffixHavingGreaterReqLevelThanBaseItem,
		ForgeMessage.PrefixSuffixOfAnotherGearType => Resources.ForgeCantUsePrefixSuffixOfAnotherGearType,
		ForgeMessage.PrefixSuffixBetterThanBase => Resources.ForgeCantUsePrefixSuffixBetterThanBase,
		ForgeMessage.MustBeRelic => Resources.ForgeMustBeRelicInStrictMode,
		ForgeMessage.MustHaveRelic => Resources.ForgeMustHaveRelicInRelaxMode,
		ForgeMessage.RelicNotAllowed => Resources.ForgeRelicNotAllowed,
		ForgeMessage.MustHaveTinkeredSuffix => string.Format(CultureInfo.CurrentCulture, Resources.ForgeMustHaveTinkeredSuffix, ofTheTinkerer),
		ForgeMessage.MustSelectBaseItem => Resources.ForgeMustSelectBaseItem,
		ForgeMessage.MustSelectItemToMerge => Resources.ForgeMustSelectItemToMerge,
		ForgeMessage.PartOfAnotherMod => Resources.ForgePartOfAnotherMod,
		_ => message.ToString(),
	};

	/// <summary>The God mode choice of the part a material item gives.</summary>
	/// <param name="part">The part.</param>
	/// <returns>The localized choice; the base item for any other part.</returns>
	public static string PartOf(ForgePart part) => part switch
	{
		ForgePart.Prefix => Resources.ForgeUsePrefix,
		ForgePart.Suffix => Resources.ForgeUseSuffix,
		ForgePart.Relic1 => Resources.ForgeUseRelic1,
		ForgePart.Relic2 => Resources.ForgeUseRelic2,
		_ => Resources.ForgeUseBase,
	};
}
