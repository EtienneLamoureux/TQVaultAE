using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Domain;

/// <summary>
/// <see cref="ModIdentity"/>: a mod is its folder name and where it is installed from, kept apart in files.
/// </summary>
public class ModIdentityTests
{
	[Theory]
	[InlineData("shadowchampions", "workshop:1949340853", "shadowchampions|workshop:1949340853")]
	[InlineData("shadowchampions", "custommaps", "shadowchampions|custommaps")]
	[InlineData("shadowchampions", null, "shadowchampions")]
	[InlineData("shadowchampions", " ", "shadowchampions")]
	[InlineData(null, "custommaps", null)]
	[InlineData("", null, null)]
	public void Compose_ThenSplit_GivesBackTheNameAndSource(string? name, string? source, string? expected)
	{
		string? key = ModIdentity.Compose(name, source);

		key.Should().Be(expected);
		ModIdentity.NameOf(key).Should().Be(string.IsNullOrWhiteSpace(name) ? null : name);
		ModIdentity.SourceOf(key).Should().Be(string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(name) ? null : source);
	}

	[Fact]
	public void NameOf_KeyWithoutSource_IsTheKey()
	{
		// A file written before the source was kept names the mod alone; a folder name never holds the separator.
		ModIdentity.NameOf("Diablo 2 Lilith").Should().Be("Diablo 2 Lilith");
		ModIdentity.SourceOf("Diablo 2 Lilith").Should().BeNull();
	}

	[Theory]
	[InlineData("shadowchampions|custommaps", "shadowchampions|custommaps", true)]
	[InlineData("shadowchampions|custommaps", "SHADOWCHAMPIONS", true)]
	[InlineData("shadowchampions|custommaps", "shadowchampions|workshop:1949340853", false)]
	[InlineData("shadowchampions", "Lilith", false)]
	[InlineData(null, null, true)]
	[InlineData(null, "shadowchampions", false)]
	public void SameData_HomonymsOfAnotherSourceDiffer_ANameAloneMatchesEither(string? a, string? b, bool expected)
		=> ModIdentity.SameData(a, b).Should().Be(expected);
}
