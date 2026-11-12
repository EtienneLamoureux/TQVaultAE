using ArzExplorer.Models;
using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Win32.ArzExplorer;

/// <summary>
/// <see cref="ArcPaths"/> converts the tree keys of ARC entries to the ids of <c>IArcFileProvider</c>.
/// </summary>
public sealed class ArcPathsTests
{
	[Theory]
	[InlineData("""e:\game\Resources\Items.arc""", "Items")]
	[InlineData("""e:\game\Audio\Dialog.arc""", "Dialog")]
	[InlineData("""e:\game\Resources\XPack2\Items.arc""", """XPack2\Items""")]
	[InlineData("""e:\game\Resources\xpack\Creatures.arc""", """xpack\Creatures""")]
	public void TreePrefix_IsTheArcNameAfterItsExpansionFolder(string arcFile, string expected)
		=> ArcPaths.TreePrefix(RecordId.Create(arcFile)).Should().Be(expected);

	[Fact]
	public void DataId_OfExpansionEntry_DropsTheExpansionFolder()
		=> ArcPaths.DataId(RecordId.Create("""XPack2\Items\artifacts\recipe01a.tex"""))
			.Should().Be(RecordId.Create("""Items\artifacts\recipe01a.tex"""));

	[Fact]
	public void DataId_OfEntryInAnXPackSubfolder_KeepsTheArcName()
	{
		// Resources\Music.arc holds entries under "xpack/": only a leading expansion folder is dropped.
		var treeKey = RecordId.Create("""Music\xpack\ambient\elysium\mus_amb_elysium_01.mp3""");

		ArcPaths.DataId(treeKey).Should().Be(treeKey);
	}

	[Theory]
	[InlineData("""Items\animalrelics\animalpart01a.tex""", """animalrelics\animalpart01a.tex""")]
	[InlineData("""XPack2\Items\artifacts\recipe01a.tex""", """artifacts\recipe01a.tex""")]
	[InlineData("""Fonts\albertus mt light.fnt""", "albertus mt light.fnt")]
	public void EntryId_IsThePathInsideTheArc(string treeKey, string expected)
		=> ArcPaths.EntryId(RecordId.Create(treeKey)).Should().Be(RecordId.Create(expected));

	[Theory]
	[InlineData("""Items\animalrelics\animalpart01a.tex""", """RESOURCES\ITEMS.ARC""")]
	[InlineData("""XPack\Items\artifacts\recipe01.msh""", """RESOURCES\XPACK\ITEMS.ARC""")]
	[InlineData("""XPack2\Creatures\monster\skin.tex""", """RESOURCES\XPACK2\CREATURES.ARC""")]
	[InlineData("""Sounds\ambience\loops\loopbeach01.mp3""", """AUDIO\SOUNDS.ARC""")]
	[InlineData("""Dialog\egypt\camper.WAV""", """AUDIO\DIALOG.ARC""")]
	// Immortal Throne sounds are in Resources\Sounds.arc, under "xpack/"
	[InlineData("""Sounds\XPack\Monster\Machae\machae02_death_04.wav""", """RESOURCES\SOUNDS.ARC""")]
	public void ArcFileOf_FindsTheArcOfADatabaseReference(string resource, string expected)
		=> ArcPaths.ArcFileOf(RecordId.Create(resource)).Should().Be(RecordId.Create(expected));
}
