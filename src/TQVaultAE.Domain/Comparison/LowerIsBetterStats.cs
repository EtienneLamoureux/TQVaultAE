using System.Text;
using System.Text.RegularExpressions;

namespace TQVaultAE.Domain.Comparison;

/// <summary>
/// The stats where a lower number is better, outside the requirements group: they are colored the other way in the
/// item comparison. They are listed by game attribute name; the comparison works on displayed lines, so each name is
/// matched through the <see cref="Signature"/> of its label in the current language.
/// </summary>
public static partial class LowerIsBetterStats
{
	/// <summary>
	/// The game attributes where lower is better. The reductions are displayed negative ("-10% Recharge",
	/// "-15% Strength Requirement for all Weapons"): a bigger reduction is a lower number.
	/// </summary>
	public static readonly IReadOnlyList<string> AttributeNames =
	[
		// Granted skill costs and times.
		"skillManaCost",
		"skillActiveManaCost",
		"skillActiveLifeCost",
		"skillCooldownTime",

		// Energy an active skill keeps reserved while on.
		"characterManaLimitReserve",
		"characterManaLimitReserveModifier",

		// A shield's time between two blocks.
		"blockRecoveryTime",

		// Reductions, written with a minus sign.
		"refreshTime",
		"skillCooldownReduction",
		"skillCooldownReductionModifier",
		"skillManaCostReduction",
		"skillManaCostReductionModifier",
		"characterDefensiveBlockRecoveryReduction",
		"characterGlobalReqReduction",
		"characterLevelReqReduction",
		"characterWeaponStrengthReqReduction",
		"characterWeaponDexterityReqReduction",
		"characterWeaponIntelligenceReqReduction",
		"characterMeleeStrengthReqReduction",
		"characterMeleeDexterityReqReduction",
		"characterMeleeIntelligenceReqReduction",
		"characterHuntingStrengthReqReduction",
		"characterHuntingDexterityReqReduction",
		"characterHuntingIntelligenceReqReduction",
		"characterStaffStrengthReqReduction",
		"characterStaffDexterityReqReduction",
		"characterStaffIntelligenceReqReduction",
		"characterShieldStrengthReqReduction",
		"characterShieldDexterityReqReduction",
		"characterShieldIntelligenceReqReduction",
		"characterArmorStrengthReqReduction",
		"characterArmorDexterityReqReduction",
		"characterArmorIntelligenceReqReduction",
		"characterJewelryStrengthReqReduction",
		"characterJewelryDexterityReqReduction",
		"characterJewelryIntelligenceReqReduction",
	];

	/// <summary>A game format item (<c>{%.1f0}</c>, <c>{0:#0}</c>), a TQ format spec (<c>%+.0f0</c>) or a color tag (<c>{^a}</c>, <c>^a</c>).</summary>
	[GeneratedRegex(@"\{[^}]*\}|%[+\-]?(?:\.\d)?[sdft]\d|\^\w")]
	private static partial Regex FormatRegex();

	/// <summary>
	/// What identifies a stat whatever its numbers and formatting: the letters of its text, lower case, without format
	/// items and color tags. "1.5 Second(s) Recharge" and the label <c>^a{%.1f0} Second(s) Recharge</c> both give
	/// "secondsrecharge".
	/// </summary>
	/// <param name="text">A label of the game's text, or the <see cref="StatLine.Template"/> of a displayed line.</param>
	public static string Signature(string? text)
	{
		if (string.IsNullOrEmpty(text))
			return string.Empty;

		var plain = FormatRegex().Replace(text, " ");
		var signature = new StringBuilder(plain.Length);
		foreach (var c in plain)
		{
			if (char.IsLetter(c))
				signature.Append(char.ToLowerInvariant(c));
		}

		return signature.ToString();
	}

	/// <summary>The signatures of <paramref name="labels"/>, the empty ones left out.</summary>
	public static IReadOnlySet<string> Signatures(IEnumerable<string?> labels)
		=> labels.Select(Signature).Where(s => s.Length != 0).ToHashSet(StringComparer.Ordinal);
}
