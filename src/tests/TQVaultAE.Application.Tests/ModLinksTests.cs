using AwesomeAssertions;
using TQVaultAE.Application.DTOs;

namespace TQVaultAE.Tests.Application;

/// <summary>
/// <see cref="ModSuggestion"/> and <see cref="CharacterMod"/>: the tied mods and the can-link rule.
/// </summary>
public class ModLinksTests
{
	private static readonly InstalledMod ModA = new("moda", """C:\Mods\moda""", ModSource.Legacy);
	private static readonly InstalledMod ModB = new("modb", """C:\Mods\modb""", ModSource.SteamWorkshop);
	private static readonly InstalledMod ModC = new("modc", """C:\Mods\modc""", ModSource.Legacy);

	[Fact]
	public void Tied_KeepsTheMatchesWithAsManyRecordsAsTheFirst()
	{
		var suggestion = new ModSuggestion(10, [new(ModA, 6), new(ModB, 6), new(ModC, 2)]);

		suggestion.Tied.Select(m => m.Mod).Should().Equal(ModA, ModB);
		suggestion.Best.Should().BeNull();
	}

	[Fact]
	public void Tied_NoMatch_IsEmpty()
		=> new ModSuggestion(10, []).Tied.Should().BeEmpty();

	[Fact]
	public void CanLinkTo_ACustomCharacterAndAnotherMod_Only()
	{
		var linkedToA = new CharacterMod(CharacterModState.Linked, ModA, new ModLinkFile { Mod = "moda" });

		linkedToA.CanLinkTo(true, ModB).Should().BeTrue();
		linkedToA.CanLinkTo(true, ModA).Should().BeFalse();
		linkedToA.CanLinkTo(true, null).Should().BeFalse();
		CharacterMod.BaseGame.CanLinkTo(false, ModB).Should().BeFalse();
		CharacterMod.Unlinked.CanLinkTo(true, ModA).Should().BeTrue();
	}
}
