using AwesomeAssertions;
using TQVaultAE.Application;

namespace TQVaultAE.Tests.Application;

/// <summary>
/// <see cref="SharedStashes.Maps"/>: the maps whose transfer stash and relic vault are read and watched.
/// </summary>
public class SharedStashesTests
{
	private const string ModA = """C:\Mods\moda""";

	[Fact]
	public void Maps_NoImmortalThroneCharacter_GivesNone()
	{
		SharedStashes.Maps([]).Should().BeEmpty();
	}

	[Fact]
	public void Maps_OnlyModdedCharacters_AddsTheBaseGameFirst()
	{
		SharedStashes.Maps([ModA]).Should().Equal(string.Empty, ModA);
	}

	[Fact]
	public void Maps_SameModWrittenTwoWays_IsKeptOnce()
	{
		SharedStashes.Maps([ModA, """C:\MODS\ModA\"""]).Should().Equal(string.Empty, ModA);
	}

	[Fact]
	public void Maps_BaseGameNamedTwoWays_IsKeptOnceWhereItCameFirst()
	{
		SharedStashes.Maps(["main", ModA, string.Empty]).Should().Equal("main", ModA);
	}

	[Fact]
	public void Maps_BaseGameCharacterAfterAModdedOne_KeepsTheCharactersOrder()
	{
		SharedStashes.Maps([ModA, string.Empty]).Should().Equal(ModA, string.Empty);
	}
}
