using System.Globalization;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;

namespace TQVaultAE.Presentation;

/// <summary>
/// Texts of the player information panel shown next to the equipment.
/// </summary>
public static class PlayerInfoText
{
	/// <summary>
	/// The masteries of a character for the character drop down: their translated names joined by " + ", in the order
	/// of <see cref="Domain.Entities.Masteries"/>.
	/// </summary>
	/// <param name="info">The player information; <see langword="null"/> when the save is not loaded.</param>
	/// <param name="characterEdit">Gives the name tag of a mastery.</param>
	/// <param name="translations">Translates the name tags.</param>
	/// <returns>An empty text when the character has no mastery.</returns>
	public static string MasteryNames(PlayerInfo? info, ICharacterEditService characterEdit, ITranslationService translations)
		=> string.Join(" + ", (info?.ActiveMasteriesRecordNames ?? [])
			.Select(recId => translations.TranslateXTag(characterEdit.GetMasteryDescriptionTag(recId))));

	/// <summary>
	/// A line of the character editor's mastery list: the mastery name, and its skills and points, each in the singular
	/// or the plural ("Storm", "4 skills, 1 point").
	/// </summary>
	/// <param name="summary">The mastery summary of a slot.</param>
	/// <param name="translations">Translates the mastery name; the mastery's enum name without a translation.</param>
	public static (string Name, string Summary) MasterySummaryLine(MasterySummary summary, ITranslationService translations)
	{
		var culture = CultureInfo.CurrentCulture;
		var skills = string.Format(culture, summary.SkillCount == 1 ? Resources.CEMasterySkill : Resources.CEMasterySkills, summary.SkillCount);
		var points = string.Format(culture, summary.PointsSpent == 1 ? Resources.CEMasteryPoint : Resources.CEMasteryPoints, summary.PointsSpent);
		return (translations.TranslateOr(summary.Mastery.NameTag, summary.Mastery.Mastery.ToString()),
			string.Format(culture, Resources.CEMasterySummary, skills, points));
	}

	/// <summary>
	/// Builds the lines of the player information panel: a localized label and its value, in display order.
	/// </summary>
	/// <param name="info">The player information.</param>
	/// <param name="translations">Translates the class, the mastery and the difficulty.</param>
	/// <returns>The lines keyed by label. The Electrum line is left out when the save has no Electrum.</returns>
	public static IReadOnlyDictionary<string, string> BuildLines(PlayerInfo info, ITranslationService translations)
	{
		var pclass = translations.TranslateXTag(info.Class ?? string.Empty, true, true) ?? string.Empty;
		var mclass = translations.TranslateMastery(info.Class ?? string.Empty) ?? string.Empty;
		// No mastery found: the translation falls back to the label itself, shown as nothing.
		mclass = mclass == Resources.Masteries ? string.Empty : mclass;

		(string Label, string? Value)[] lines =
		[
			(Resources.CurrentLevel, info.CurrentLevel.ToString()),
			(Resources.Class, pclass),
			(Resources.Masteries, mclass),
			(Resources.CurrentXP, info.CurrentXP.ToString()),
			(Resources.DifficultyUnlocked, translations.TranslateDifficulty(info.DifficultyUnlocked) ?? "unknown"),
			(Resources.Money, info.Money.ToString()),
			// A character without Eternal Embers has no Electrum at all: no line rather than a misleading 0.
			(Resources.Electrum, info.Electrum?.ToString()),
			(Resources.SkillPoints, info.SkillPoints.ToString()),
			(Resources.AttributesPoints, info.AttributesPoints.ToString()),
			(Resources.CEStrength, info.CalculatedStrength.ToString()),
			(Resources.CEDexterity, info.CalculatedDexterity.ToString()),
			(Resources.CEIntelligence, info.CalculatedIntelligence.ToString()),
			(Resources.CEHealth, info.CalculatedHealth.ToString()),
			(Resources.CEMana, info.CalculatedMana.ToString()),
			(Resources.PlayTimeInSeconds, info.PlayTimeInSeconds.ToString()),
			(Resources.NumberOfDeaths, info.NumberOfDeaths.ToString()),
			(Resources.NumberOfKills, info.NumberOfKills.ToString()),
			(Resources.ExperienceFromKills, info.ExperienceFromKills.ToString()),
			(Resources.HealthPotionsUsed, info.HealthPotionsUsed.ToString()),
			(Resources.ManaPotionsUsed, info.ManaPotionsUsed.ToString()),
			(Resources.NumHitsReceived, info.NumHitsReceived.ToString()),
			(Resources.NumHitsInflicted, info.NumHitsInflicted.ToString()),
			(Resources.CriticalHitsInflicted, info.CriticalHitsInflicted.ToString()),
			(Resources.CriticalHitsReceived, info.CriticalHitsReceived.ToString()),
		];

		var result = new Dictionary<string, string>(lines.Length);
		foreach (var (label, value) in lines)
		{
			if (value is not null)
				result[label] = value;
		}

		return result;
	}
}
