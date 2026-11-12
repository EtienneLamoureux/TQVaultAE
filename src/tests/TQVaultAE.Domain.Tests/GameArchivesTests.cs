using AwesomeAssertions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Domain;

/// <summary>
/// <see cref="GameArchives.ArcFileCandidates"/> lists the archives to search for a resource, in TQVault's fallback order.
/// </summary>
public sealed class GameArchivesTests
{
	[Fact]
	public void Candidates_StartWithTheMappedArchive()
		=> GameArchives.ArcFileCandidates(RecordId.Create("""Items\EquipmentWeapon\Axe\x.tex"""))
			.Should().Equal(
				"""Resources\Items.arc""",
				"""Resources\XPack\Items.arc""",
				"""Resources\XPack2\Items.arc""",
				"""Resources\XPack3\Items.arc""",
				"""Resources\XPack4\Items.arc""");

	[Fact]
	public void BaseGameSound_FallsBackOnResources()
		// The Eternal Embers drop sounds are in Resources\Sounds.arc, not in Audio\Sounds.arc.
		=> GameArchives.ArcFileCandidates(RecordId.Create("""Sounds\item\emberslegendarydrop.mp3"""))
			.Should().StartWith("""Audio\Sounds.arc""")
			.And.EndWith("""Resources\Sounds.arc""");

	[Fact]
	public void ExpansionResource_FallsBackOnTheOtherExpansions()
		// XPack3\Items\artifacts\x2_artifact03.tex is in XPack2\Items.arc.
		=> GameArchives.ArcFileCandidates(RecordId.Create("""XPack3\Items\artifacts\x2_artifact03.tex"""))
			.Should().Equal(
				"""Resources\XPack3\Items.arc""",
				"""Resources\XPack\Items.arc""",
				"""Resources\XPack2\Items.arc""",
				"""Resources\XPack4\Items.arc""",
				"""Resources\Items.arc""");
}
