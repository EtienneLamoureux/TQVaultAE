using System.Globalization;
using TQVaultAE.Domain.Forge;

namespace TQVaultAE.Presentation;

/// <summary>One line of a forge mode card: something allowed, or a restriction.</summary>
public sealed record ForgeCardLine(string Text, bool Allowed);

/// <summary>A titled section of a forge mode card.</summary>
public sealed record ForgeCardSection(string Title, IReadOnlyList<ForgeCardLine> Lines);

/// <summary>The text of a forge mode card: its title, summary, the base item, affix and relic sections, and a footer.</summary>
public sealed record ForgeModeCard(string Title, string Summary, IReadOnlyList<ForgeCardSection> Sections, string? Footer);

/// <summary>
/// The card shown on a forge mode radio button, written from <see cref="ForgeRules.Capabilities"/>: one line per check.
/// </summary>
public static class ForgeModeCardText
{
	/// <summary>Builds the card of <paramref name="mode"/>.</summary>
	/// <param name="mode">The forge mode.</param>
	/// <param name="ofTheTinkerer">The "of the Tinkerer" suffix, translated in the game language.</param>
	public static ForgeModeCard Build(ForgeMode mode, string ofTheTinkerer)
	{
		var culture = CultureInfo.CurrentCulture;
		var can = ForgeRules.Capabilities(mode);

		static ForgeCardLine Allow(string text) => new(text, true);
		static ForgeCardLine Limit(string text) => new(text, false);
		var epicLegendary = can.EpicLegendaryAllowed ? Allow(Resources.ForgeCapEpicLegendary) : Limit(Resources.ForgeCapNoEpicLegendary);

		var baseLines = can.GearOnly
			? new List<ForgeCardLine> { Limit(Resources.ForgeCapBaseGear), epicLegendary }
			: [Allow(Resources.ForgeCapAnyItem)];

		var affixLines = new List<ForgeCardLine>();
		if (can.AnyPartOfMaterial)
			affixLines.Add(Allow(Resources.ForgeCapAnyPart));
		else
		{
			if (can.GearOnly)
				affixLines.Add(Limit(Resources.ForgeCapBaseGear));
			if (can.AffixFromSameBaseItem)
				affixLines.Add(Limit(Resources.ForgeCapSameBaseItem));
			if (can.AffixLevelNotAboveBase)
				affixLines.Add(Limit(Resources.ForgeCapLevelRequirement));
			affixLines.Add(can.AffixFromSameGearType ? Limit(Resources.ForgeCapSameGearType) : Allow(Resources.ForgeCapAnyGearType));
			if (can.AffixNotRarerThanBase)
				affixLines.Add(Limit(Resources.ForgeCapNotRarerThanBase));
			affixLines.Add(epicLegendary);
			if (can.AffixMustBePresent)
				affixLines.Add(Limit(Resources.ForgeCapNeedsAffix));
		}

		var relicLines = new List<ForgeCardLine>
		{
			can.RelicSource switch
			{
				ForgeRelicSource.RelicOrCharm => Limit(Resources.ForgeCapRelicOnly),
				ForgeRelicSource.RelicOrSocketedGear => Allow(Resources.ForgeCapRelicOrSocketed),
				_ => Allow(Resources.ForgeCapAnyRelicSlot),
			},
		};
		if (can.RelicMustFitBase)
			relicLines.Add(Limit(Resources.ForgeCapRelicGear));
		relicLines.Add(can.SecondRelicNeedsTinkerer
			? Limit(string.Format(culture, Resources.ForgeCapSecondRelicTinkerer, ofTheTinkerer))
			: Allow(Resources.ForgeCapSecondRelicFree));

		var (title, summary) = mode switch
		{
			ForgeMode.God => (Resources.ForgeGodMode, Resources.ForgeGodModeTT),
			ForgeMode.Relax => (Resources.ForgeRelaxMode, string.Format(culture, Resources.ForgeRelaxModeTT, ofTheTinkerer)),
			ForgeMode.Strict => (Resources.ForgeStrictMode, Resources.ForgeStrictModeTT),
			_ => (Resources.ForgeGameMode, Resources.ForgeGameModeTT),
		};

		return new(
			title,
			summary,
			[
				new(Resources.ForgeCapSectionBase, baseLines),
				new(Resources.ForgeCapSectionAffixes, affixLines),
				new(Resources.ForgeCapSectionRelics, relicLines),
			],
			can.StricterModeExists ? Resources.ForgeCapResetOnStricter : null);
	}
}
