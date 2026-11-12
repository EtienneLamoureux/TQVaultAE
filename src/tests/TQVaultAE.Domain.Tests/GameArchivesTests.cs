using AwesomeAssertions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GameFiles;

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

	[Theory]
	[InlineData("""Resources\Items.arc""", "x.tex")]
	[InlineData("""Resources\XPack2\Items.arc""", "x.tex")]
	[InlineData("""Resources\XPack4\Item.arc""", "x.tex")]
	[InlineData("""Resources\XPack2\Sounds.arc""", "x.wav")]
	[InlineData("""Resources\XPack3\Dialog.arc""", "x.mp3")]
	[InlineData("""Resources\Sounds.arc""", """xpack\x.wav""")]
	[InlineData("""Resources\Music.arc""", """xpack\x.mp3""")]
	[InlineData("""Audio\Sounds.arc""", "x.wav")]
	[InlineData("""Audio\Dialog.arc""", "x.mp3")]
	public void ArchiveResourceIds_MapBackToTheArchive(string archive, string entry)
	{
		// ArchiveLayout gives the ids of an archive's entries, GameArchives the archive of an id: one rule both ways.
		var resourceId = $"""{ArchiveLayout.ResourcePrefix(archive.Replace('\\', Path.DirectorySeparatorChar))}\{entry}""";

		GameArchives.ArcFileOf(RecordId.Create(resourceId)).Should().Be(archive);
	}

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
