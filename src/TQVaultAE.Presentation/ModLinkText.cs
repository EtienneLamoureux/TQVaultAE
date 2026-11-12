using System.Globalization;
using TQVaultAE.Application.DTOs;

namespace TQVaultAE.Presentation;

/// <summary>
/// Texts of a character's mod link: the link dialog's state and suggestion lines, the character list's badge and the
/// hints of the "Modded" menu.
/// </summary>
public static class ModLinkText
{
	/// <summary>The link dialog's state line: base game, linked, linked to a mod not installed, or not linked.</summary>
	/// <param name="mod">The character's mod link.</param>
	public static string State(CharacterMod mod)
	{
		// Without a link to an installed mod, a character is loaded with the base game data.
		var loadedWith = Resources.ModLinkBaseGameData;

		return mod.State switch
		{
			CharacterModState.BaseGame => Resources.ModLinkStateBaseGame,
			CharacterModState.Linked => string.Format(CultureInfo.CurrentCulture, Resources.ModLinkStateLinked, mod.Mod!.DisplayName),
			CharacterModState.LinkedModMissing => string.Format(CultureInfo.CurrentCulture, Resources.ModLinkStateMissing, mod.Link!.Mod, loadedWith),
			_ => string.Format(CultureInfo.CurrentCulture, Resources.ModLinkStateUnlinked, loadedWith),
		};
	}

	/// <summary>
	/// The link dialog's suggestion line: base game records only, no mod found, the best mod, or the mods tied first.
	/// </summary>
	/// <param name="suggestion">The suggestion read from the save.</param>
	public static string Suggestion(ModSuggestion suggestion)
	{
		var culture = CultureInfo.CurrentCulture;
		if (suggestion.OnlyBaseGameRecords)
			return Resources.ModLinkSuggestionBaseOnly;

		if (suggestion.Matches.Count == 0)
			return string.Format(culture, Resources.ModLinkSuggestionNone, suggestion.ModOnlyRecords);

		if (suggestion.Best is { } best)
			return string.Format(culture, Resources.ModLinkSuggestion, best.Mod.DisplayName, best.Matched, suggestion.ModOnlyRecords);

		return string.Format(culture, Resources.ModLinkSuggestionTie, string.Join(", ", suggestion.Tied.Select(m => m.Mod.DisplayName)));
	}

	/// <summary>The character list's badge: linked, mod not installed, or not linked.</summary>
	/// <param name="state">The state of a custom map character other than the base game.</param>
	public static string CharacterBadge(CharacterModState state) => state switch
	{
		CharacterModState.Linked => Resources.CharacterModLinked,
		CharacterModState.LinkedModMissing => Resources.CharacterModNotInstalled,
		_ => Resources.CharacterModNotLinked,
	};

	/// <summary>
	/// The hint of a suggested mod in the "Modded" menu: "suggested" when it has every mod-only record, else how many,
	/// and a tool tip with the mod's name and the count.
	/// </summary>
	/// <param name="match">The mod and the records it has.</param>
	/// <param name="suggestion">The suggestion it belongs to.</param>
	public static (string Shortcut, string ToolTip) MenuHint(ModMatch match, ModSuggestion suggestion)
	{
		var culture = CultureInfo.CurrentCulture;
		var shortcut = match.Matched == suggestion.ModOnlyRecords
			? Resources.ModSuggestedHint
			: string.Format(culture, Resources.ModSuggestedPartialHint, match.Matched, suggestion.ModOnlyRecords);
		var toolTip = $"{match.Mod.DisplayName}{Environment.NewLine}"
			+ string.Format(culture, Resources.ModSuggestedDetail, match.Matched, suggestion.ModOnlyRecords);
		return (shortcut, toolTip);
	}
}
