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
	public void CanLinkTo_AnotherMod_Only()
	{
		var linkedToA = new CharacterMod(CharacterModState.Linked, ModA, new ModLinkFile { Mod = "moda" });

		linkedToA.CanLinkTo(ModB).Should().BeTrue();
		linkedToA.CanLinkTo(ModA).Should().BeFalse();
		linkedToA.CanLinkTo(null).Should().BeFalse();
		// A main campaign character can be linked too (a bounce mod).
		CharacterMod.BaseGame.CanLinkTo(ModB).Should().BeTrue();
		CharacterMod.Unlinked.CanLinkTo(ModA).Should().BeTrue();
	}

	[Fact]
	public void LabelOf_InstalledModByItsName_HomonymsWithTheirSource_OthersByTheNameTheyKeep()
	{
		var legacy = new InstalledMod("same", """C:\Mods\same""", ModSource.Legacy);
		var workshop = new InstalledMod("same", """C:\Workshop\475150\42\same""", ModSource.SteamWorkshop);
		InstalledMod[] installed = [ModA, legacy, workshop];

		InstalledMod.LabelOf(ModA.Id.ToUpperInvariant(), installed).Should().Be("moda");
		InstalledMod.LabelOf(workshop.Id, installed).Should().Be(workshop.DisplayName, "another installed mod has its name");
		InstalledMod.LabelOf("gone|custommaps", installed).Should().Be("gone");
		InstalledMod.LabelOf(null, installed).Should().BeNull();
	}
}
