namespace TQVaultAE.Domain.Forge;

/// <summary>What a relic slot of the Forge accepts.</summary>
public enum ForgeRelicSource
{
	/// <summary>A relic or a charm.</summary>
	RelicOrCharm,

	/// <summary>A relic, a charm, or a weapon, shield, armor or jewellery with a socketed relic.</summary>
	RelicOrSocketedGear,

	/// <summary>Any item.</summary>
	Anything,
}

/// <summary>
/// What a <see cref="ForgeMode"/> allows and requires. <see cref="ForgeRules.CheckDrop"/> applies exactly these
/// checks, and the mode cards of the Forge describe them.
/// </summary>
/// <param name="GearOnly">The base item and the prefix and suffix items must be a weapon, shield, armor or jewellery.</param>
/// <param name="EpicLegendaryAllowed">Epic and Legendary items may be the base item or give a prefix or suffix.</param>
/// <param name="AffixFromSameBaseItem">The prefix and suffix items must have the base item's record.</param>
/// <param name="AffixLevelNotAboveBase">The prefix and suffix items must not have a higher level requirement than the base item.</param>
/// <param name="AffixFromSameGearType">The prefix and suffix items must be of the base item's class.</param>
/// <param name="AffixNotRarerThanBase">The prefix and suffix items must not be rarer than the base item.</param>
/// <param name="AffixMustBePresent">The prefix (suffix) item must have a prefix (suffix).</param>
/// <param name="AnyPartOfMaterial">Any part of a material item can be merged (base, prefix, suffix or relics).</param>
/// <param name="RelicSource">What the relic slots accept.</param>
/// <param name="RelicMustFitBase">A relic must be allowed on the base item.</param>
/// <param name="SecondRelicNeedsTinkerer">The second relic needs the "of the Tinkerer" suffix.</param>
/// <param name="StricterModeExists">A stricter mode exists: choosing it empties the slots (<see cref="ForgeRules.IsDowngrade"/>).</param>
public sealed record ForgeCapabilities(
	bool GearOnly,
	bool EpicLegendaryAllowed,
	bool AffixFromSameBaseItem,
	bool AffixLevelNotAboveBase,
	bool AffixFromSameGearType,
	bool AffixNotRarerThanBase,
	bool AffixMustBePresent,
	bool AnyPartOfMaterial,
	ForgeRelicSource RelicSource,
	bool RelicMustFitBase,
	bool SecondRelicNeedsTinkerer,
	bool StricterModeExists);
