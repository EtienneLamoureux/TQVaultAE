using System.Globalization;
using AwesomeAssertions;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Presentation;

namespace TQVaultAE.Tests.Presentation;

/// <summary>
/// <see cref="ModLinkText"/>: the mod link texts of the link dialog, the character list and the "Modded" menu.
/// </summary>
public class ModLinkTextTests
{
	private static readonly InstalledMod ModA = new("moda", """C:\Mods\moda""", ModSource.Legacy);
	private static readonly InstalledMod ModB = new("modb", """C:\Mods\modb""", ModSource.SteamWorkshop);

	private static string F(string format, params object[] args) => string.Format(CultureInfo.CurrentCulture, format, args);

	[Fact]
	public void State_EachState_HasItsLine()
	{
		ModLinkText.State(CharacterMod.BaseGame).Should().Be(Resources.ModLinkStateBaseGame);
		ModLinkText.State(new CharacterMod(CharacterModState.Linked, ModA, null)).Should().Be(F(Resources.ModLinkStateLinked, ModA.DisplayName));
		ModLinkText.State(new CharacterMod(CharacterModState.LinkedModMissing, null, new ModLinkFile { Mod = "gone" }))
			.Should().Be(F(Resources.ModLinkStateMissing, "gone", Resources.ModLinkBaseGameData));
		ModLinkText.State(CharacterMod.Unlinked).Should().Be(F(Resources.ModLinkStateUnlinked, Resources.ModLinkBaseGameData));
	}

	[Fact]
	public void Suggestion_BaseOnlyNoneBestOrTie()
	{
		ModLinkText.Suggestion(new ModSuggestion(0, [])).Should().Be(Resources.ModLinkSuggestionBaseOnly);
		ModLinkText.Suggestion(new ModSuggestion(4, [])).Should().Be(F(Resources.ModLinkSuggestionNone, 4));
		ModLinkText.Suggestion(new ModSuggestion(4, [new(ModA, 3), new(ModB, 1)]))
			.Should().Be(F(Resources.ModLinkSuggestion, ModA.DisplayName, 3, 4));
		ModLinkText.Suggestion(new ModSuggestion(4, [new(ModA, 3), new(ModB, 3)]))
			.Should().Be(F(Resources.ModLinkSuggestionTie, $"{ModA.DisplayName}, {ModB.DisplayName}"));
	}

	[Fact]
	public void MenuHint_EveryRecordOrPart()
	{
		var suggestion = new ModSuggestion(4, [new(ModA, 4), new(ModB, 1)]);

		ModLinkText.MenuHint(suggestion.Matches[0], suggestion).Shortcut.Should().Be(Resources.ModSuggestedHint);
		var (shortcut, toolTip) = ModLinkText.MenuHint(suggestion.Matches[1], suggestion);
		shortcut.Should().Be(F(Resources.ModSuggestedPartialHint, 1, 4));
		toolTip.Should().Be(ModB.DisplayName + Environment.NewLine + F(Resources.ModSuggestedDetail, 1, 4));
	}

	[Theory]
	[InlineData(CharacterModState.Linked)]
	[InlineData(CharacterModState.LinkedModMissing)]
	[InlineData(CharacterModState.Unlinked)]
	public void CharacterBadge_EachStateHasItsOwnText(CharacterModState state)
		=> Enum.GetValues<CharacterModState>().Where(s => s is not CharacterModState.BaseGame && s != state)
			.Select(ModLinkText.CharacterBadge).Should().NotContain(ModLinkText.CharacterBadge(state));
}
