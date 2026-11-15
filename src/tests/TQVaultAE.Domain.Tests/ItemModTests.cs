using AwesomeAssertions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Tests.Shared;

namespace TQVaultAE.Tests.Domain;

/// <summary>
/// <see cref="Item.Mod"/>: computed from the databases the item's records were read from, never saved.
/// </summary>
public class ItemModTests
{
	private const string Shadow = "shadowchampions|workshop:1949340853";

	private static Item Sword() => new()
	{
		BaseItemId = RecordId.Create("records/item/sword.dbr"),
		prefixID = RecordId.Create("records/affix/prefix.dbr"),
		RelicBonus2Id = RecordId.Create("records/affix/bonus.dbr"),
	};

	[Fact]
	public void Mod_AnyRecordReadFromTheModsDatabase_GivesTheMod()
	{
		// Only the bonus of its second relic is the mod's: a record of its own, or one it overrides.
		var sword = Sword();
		sword.baseItemInfo = TestRecords.InfoFrom(sword.BaseItemId, null);
		sword.prefixInfo = TestRecords.InfoFrom(sword.prefixID!, null);
		sword.RelicBonus2Info = TestRecords.InfoFrom(sword.RelicBonus2Id!, Shadow);

		sword.Mod.Should().Be(Shadow);
	}

	[Fact]
	public void Mod_EveryRecordFromTheGame_IsNull_EvenReadWithAModsMap()
	{
		// In a vault linked to the mod, the records the mod does not have are read from the game's databases.
		var sword = Sword();
		sword.SetContainerMod(Shadow);
		sword.baseItemInfo = TestRecords.InfoFrom(sword.BaseItemId, null);
		sword.prefixInfo = TestRecords.InfoFrom(sword.prefixID!, null);
		sword.RelicBonus2Info = TestRecords.InfoFrom(sword.RelicBonus2Id!, null);

		sword.Mod.Should().BeNull();
	}

	[Fact]
	public void Mod_RecordsNotFound_IsNull_AndTheItemIsIncomplete()
	{
		// A mod's item read with the base game: its records are missing, it comes from no mod it can tell.
		var sword = Sword();

		(sword.Mod, sword.IsIncomplete, sword.HasWarning).Should().Be(((string?)null, true, true));
	}

	[Fact]
	public void SetContainerMod_TellsWhetherTheMapChanged_IgnoringCase()
	{
		var sword = Sword();

		sword.SetContainerMod(null).Should().BeTrue("a first container always gives a map");
		sword.SetContainerMod("  ").Should().BeFalse("blank is the base game");
		sword.SetContainerMod(Shadow).Should().BeTrue();
		sword.SetContainerMod(Shadow.ToUpperInvariant()).Should().BeFalse();
	}

	[Fact]
	public void ShowNoRoom_ShowsOneCell_KeepsTheRealSize_AndClearsBack()
	{
		var lance = new Item { BaseItemId = RecordId.Create("records/item/lance.dbr"), Width = 1, Height = 5 }.ReadFrom(null);

		lance.ShowNoRoom();
		var shown = (lance.Width, lance.Height, lance.RealHeight, lance.HasWarning, lance.IsIncomplete);
		lance.ClearNoRoom();

		shown.Should().Be((1, 1, 5, true, false));
		(lance.Width, lance.Height, lance.HasWarning).Should().Be((1, 5, false));
	}

	[Fact]
	public void Copies_KeepTheContainersMap_NotTheLackOfRoom()
	{
		var lance = new Item { BaseItemId = RecordId.Create("records/item/lance.dbr"), Width = 1, Height = 5 };
		lance.SetContainerMod(Shadow);
		lance.ShowNoRoom();

		var copy = lance.Duplicate(true);
		copy.ClearNoRoom();
		var relic = lance.MakeEmptyCopy(RecordId.Create("records/item/relic.dbr"));

		(copy.ContainerMod, copy.Height).Should().Be((Shadow, 5));
		(relic.ContainerMod, relic.HasContainerMod, relic.LacksRoom).Should().Be((Shadow, true, false));
	}
}
