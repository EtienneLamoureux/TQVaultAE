using AwesomeAssertions;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;
using TQVaultAE.Tests.Shared;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// <see cref="ItemEditService"/>: the item edits of the sack context menu.
/// </summary>
public class ItemEditServiceTests
{
	private static readonly RecordId OldAffix = RecordId.Create("records/affix/old.dbr");
	private static readonly RecordId NewAffix = RecordId.Create("records/affix/new.dbr");

	private readonly Mock<IDatabase> _database = new();
	private readonly Mock<IItemProvider> _itemProvider = new();
	private readonly SackCollection _sack = new();
	private readonly ItemEditService _service;

	public ItemEditServiceTests()
	{
		this._service = new ItemEditService(this._database.Object, this._itemProvider.Object, Mock.Of<IItemModService>());
	}

	#region Helpers

	private static Variable Text(string name, string value)
	{
		var v = new Variable(name, VariableDataType.StringVar, 1);
		v[0] = value;
		return v;
	}

	private static Info MakeInfo(string id, string itemClass, int completedRelicLevel = 0)
	{
		var record = new DBRecordCollection(RecordId.Create(id), itemClass);
		record.Set(Text("Class", itemClass));
		if (completedRelicLevel > 0)
		{
			var level = new Variable("completedRelicLevel", VariableDataType.Integer, 1);
			level[0] = completedRelicLevel;
			record.Set(level);
		}

		return new Info(record);
	}

	private static Item MakeItem(string id, string itemClass, int completedRelicLevel = 0)
		=> new()
		{
			BaseItemId = RecordId.Create(id),
			baseItemInfo = MakeInfo(id, itemClass, completedRelicLevel),
		};

	private static Item Relic(int shards = 1, int completedLevel = 5)
	{
		var relic = MakeItem("records/relic.dbr", Item.ICLASS_RELIC, completedLevel);
		relic.Var1 = shards;
		return relic;
	}

	/// <summary>A one entry table: the roll always picks <paramref name="bonusId"/>.</summary>
	private static LootTableCollection SingleBonusTable(RecordId bonusId)
		=> new(RecordId.Create("table"), new Dictionary<RecordId, (float, LootRandomizerItem)>
		{
			[bonusId] = (1f, LootRandomizerItem.Default(bonusId)),
		});

	private Info StubInfo(RecordId id)
	{
		var info = MakeInfo(id.Raw, "ItemRelic");
		this._database.Setup(d => d.GetInfo(id)).Returns(info);
		return info;
	}

	#endregion

	#region Affixes

	[Fact]
	public void ChangePrefix_NewAffix_SetsIdAndInfoAndMarksItemAndSack()
	{
		// Arrange
		var item = MakeItem("records/sword.dbr", "WeaponMelee_Sword");
		item.prefixID = OldAffix;
		var info = this.StubInfo(NewAffix);

		// Act
		var changed = this._service.ChangePrefix(this._sack, item, NewAffix);

		// Assert
		changed.Should().BeTrue();
		item.prefixID.Should().Be(NewAffix);
		item.prefixInfo.Should().BeSameAs(info);
		item.IsModified.Should().BeTrue();
		this._sack.IsModified.Should().BeTrue();
	}

	[Fact]
	public void ChangeSuffix_SameAffix_ChangesNothing()
	{
		var item = MakeItem("records/sword.dbr", "WeaponMelee_Sword");
		item.suffixID = OldAffix;

		this._service.ChangeSuffix(this._sack, item, OldAffix).Should().BeFalse();

		item.IsModified.Should().BeFalse();
		this._sack.IsModified.Should().BeFalse();
		this._database.Verify(d => d.GetInfo(It.IsAny<RecordId>()), Times.Never);
	}

	[Fact]
	public void ChangeSuffix_NewAffix_LeavesThePrefixAlone()
	{
		var item = MakeItem("records/sword.dbr", "WeaponMelee_Sword");
		item.prefixID = OldAffix;
		var info = this.StubInfo(NewAffix);

		this._service.ChangeSuffix(this._sack, item, NewAffix).Should().BeTrue();

		item.suffixID.Should().Be(NewAffix);
		item.suffixInfo.Should().BeSameAs(info);
		item.prefixID.Should().Be(OldAffix);
	}

	[Fact]
	public void RemovePrefixAndSuffix_ClearTheirIdAndInfo()
	{
		// Arrange
		var item = MakeItem("records/sword.dbr", "WeaponMelee_Sword");
		item.prefixID = OldAffix;
		item.prefixInfo = MakeInfo(OldAffix.Raw, "LootRandomizer");
		item.suffixID = NewAffix;
		item.suffixInfo = MakeInfo(NewAffix.Raw, "LootRandomizer");

		// Act
		this._service.RemovePrefix(this._sack, item);

		// Assert
		item.HasPrefix.Should().BeFalse();
		item.prefixInfo.Should().BeNull();
		item.suffixID.Should().Be(NewAffix, "removing the prefix keeps the suffix");
		this._sack.IsModified.Should().BeTrue();

		this._service.RemoveSuffix(this._sack, item);
		item.HasSuffix.Should().BeFalse();
		item.suffixInfo.Should().BeNull();
	}

	#endregion

	#region Completion bonus

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void ChangeCompletionBonus_ChangesOnlyTheChosenSlot(bool secondRelic)
	{
		// Arrange
		var item = MakeItem("records/sword.dbr", "WeaponMelee_Sword");
		item.RelicBonusId = OldAffix;
		item.RelicBonus2Id = OldAffix;
		var info = this.StubInfo(NewAffix);

		// Act
		var changed = this._service.ChangeCompletionBonus(this._sack, item, NewAffix, secondRelic);

		// Assert
		changed.Should().BeTrue();
		(secondRelic ? item.RelicBonus2Id : item.RelicBonusId).Should().Be(NewAffix);
		(secondRelic ? item.RelicBonus2Info : item.RelicBonusInfo).Should().BeSameAs(info);
		(secondRelic ? item.RelicBonusId : item.RelicBonus2Id).Should().Be(OldAffix);
		item.IsModified.Should().BeTrue();
		this._sack.IsModified.Should().BeTrue();
	}

	[Fact]
	public void ChangeCompletionBonus_SameBonus_ChangesNothing()
	{
		var item = MakeItem("records/sword.dbr", "WeaponMelee_Sword");
		item.RelicBonus2Id = OldAffix;

		this._service.ChangeCompletionBonus(this._sack, item, OldAffix, secondRelic: true).Should().BeFalse();

		this._sack.IsModified.Should().BeFalse();
	}

	#endregion

	#region Relics and artifacts

	[Fact]
	public void CompleteRelic_FillsTheShardsAndRollsTheBonus()
	{
		// Arrange
		var relic = Relic(shards: 2, completedLevel: 5);
		var bonus = RecordId.Create("records/bonus/b.dbr");
		this._itemProvider.Setup(p => p.BonusTableRelicOrArtifact(relic)).Returns(SingleBonusTable(bonus));

		// Act
		this._service.CompleteRelic(this._sack, relic);

		// Assert
		relic.Var1.Should().Be(5, "the shards are capped to the completion level");
		relic.IsRelicComplete.Should().BeTrue();
		relic.RelicBonusId.Should().Be(bonus);
		this._itemProvider.Verify(p => p.GetDBData(relic), Times.Once);
		relic.IsModified.Should().BeTrue();
		this._sack.IsModified.Should().BeTrue();
	}

	[Fact]
	public void CompleteRelic_NoBonusTable_KeepsTheBonus()
	{
		var relic = Relic();
		relic.RelicBonusId = OldAffix;
		this._itemProvider.Setup(p => p.BonusTableRelicOrArtifact(relic)).Returns((LootTableCollection?)null);

		this._service.CompleteRelic(this._sack, relic);

		relic.IsRelicComplete.Should().BeTrue();
		relic.RelicBonusId.Should().Be(OldAffix);
	}

	[Fact]
	public void CraftArtifact_ReturnsTheArtifactWithARolledBonus()
	{
		// Arrange
		var formula = MakeItem("records/formula.dbr", Item.ICLASS_FORMULA);
		var artifact = MakeItem("records/artifact.dbr", Item.ICLASS_ARTIFACT);
		var bonus = RecordId.Create("records/bonus/b.dbr");
		var info = this.StubInfo(bonus);
		this._itemProvider.Setup(p => p.CraftArtifact(formula)).Returns(artifact);
		this._itemProvider.Setup(p => p.BonusTableRelicOrArtifact(artifact)).Returns(SingleBonusTable(bonus));

		// Act
		var crafted = this._service.CraftArtifact(formula);

		// Assert
		crafted.Should().BeSameAs(artifact);
		artifact.RelicBonusId.Should().Be(bonus);
		artifact.RelicBonusInfo.Should().BeSameAs(info);
		artifact.IsModified.Should().BeTrue();
		formula.IsModified.Should().BeFalse("the formula stays where it is until the artifact is placed");
	}

	[Fact]
	public void CraftArtifact_NoArtifact_ReturnsNull()
	{
		var formula = MakeItem("records/formula.dbr", Item.ICLASS_FORMULA);
		this._itemProvider.Setup(p => p.CraftArtifact(formula)).Returns((Item?)null);

		this._service.CraftArtifact(formula).Should().BeNull();
		this._itemProvider.Verify(p => p.BonusTableRelicOrArtifact(It.IsAny<Item>()), Times.Never);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void RemoveRelic_RelicInTheSlot_ReturnsItFromThatSlot(bool secondSlot)
	{
		// Arrange
		var sword = MakeItem("records/sword.dbr", "WeaponMelee_Sword");
		var relic = Relic();
		if (secondSlot)
			this._itemProvider.Setup(p => p.RemoveRelic2(sword)).Returns(relic);
		else
			this._itemProvider.Setup(p => p.RemoveRelic1(sword)).Returns(relic);

		// Act
		var removed = this._service.RemoveRelic(this._sack, sword, secondSlot);

		// Assert
		removed.Should().BeSameAs(relic);
		this._sack.IsModified.Should().BeTrue("the item lost its relic");
		this._itemProvider.Verify(p => p.RemoveRelic1(sword), secondSlot ? Times.Never() : Times.Once());
		this._itemProvider.Verify(p => p.RemoveRelic2(sword), secondSlot ? Times.Once() : Times.Never());
	}

	[Fact]
	public void RemoveRelic_SlotHeldSomethingElse_ReturnsNullAndMarksTheSack()
	{
		// God mode in the Forge can socket a weapon: it is removed, not placed.
		var sword = MakeItem("records/sword.dbr", "WeaponMelee_Sword");
		this._itemProvider.Setup(p => p.RemoveRelic1(sword)).Returns(MakeItem("records/axe.dbr", "WeaponMelee_Axe"));

		this._service.RemoveRelic(this._sack, sword, secondSlot: false).Should().BeNull();

		this._sack.IsModified.Should().BeTrue();
	}

	#endregion

	#region Copies and seed

	[Fact]
	public void Copy_GetsANewSeed_DuplicateKeepsIt()
	{
		var sword = MakeItem("records/sword.dbr", "WeaponMelee_Sword");
		sword.Seed = 1234;

		var duplicate = this._service.Duplicate(sword);
		duplicate.Should().NotBeSameAs(sword);
		duplicate.Seed.Should().Be(1234);
		duplicate.BaseItemId.Should().Be(sword.BaseItemId);

		// A new seed may equal the old one by chance: try a few times.
		Enumerable.Range(0, 20).Select(_ => this._service.Copy(sword).Seed).Should().Contain(s => s != 1234);
	}

	[Fact]
	public void Copies_LeaveTheSourceAndItsSackUnchanged()
	{
		// Upstream #267: a copy alone must not make its vault or character saved again.
		var sword = MakeItem("records/sword.dbr", "WeaponMelee_Sword");
		sword.Seed = 1234;
		var formula = MakeItem("records/formula.dbr", Item.ICLASS_FORMULA);
		this._itemProvider.Setup(p => p.CraftArtifact(formula)).Returns(MakeItem("records/artifact.dbr", Item.ICLASS_ARTIFACT));
		this._itemProvider.Setup(p => p.BonusTableRelicOrArtifact(It.IsAny<Item>())).Returns((LootTableCollection?)null);

		this._service.Copy(sword);
		this._service.Duplicate(sword);
		this._service.NewSetMember(sword, RecordId.Create("records/other.dbr"));
		this._service.CraftArtifact(formula);

		sword.IsModified.Should().BeFalse();
		sword.Seed.Should().Be(1234);
		formula.IsModified.Should().BeFalse();
		this._sack.IsModified.Should().BeFalse();
	}

	[Fact]
	public void Split_KeepsOneAndReturnsTheRest()
	{
		var potions = MakeItem("records/potion.dbr", Item.ICLASS_POTIONHEALTH);
		potions.StackSize = 5;

		var rest = this._service.Split(this._sack, potions);

		rest.Should().NotBeNull();
		rest!.StackSize.Should().Be(4);
		potions.StackSize.Should().Be(1);
		this._sack.IsModified.Should().BeTrue("the stack in the sack changed");
	}

	[Fact]
	public void Split_SingleItem_ReturnsNullAndLeavesTheSack()
	{
		var potion = MakeItem("records/potion.dbr", Item.ICLASS_POTIONHEALTH);

		this._service.Split(this._sack, potion).Should().BeNull("a single item cannot be split");
		this._sack.IsModified.Should().BeFalse();
	}

	[Fact]
	public void ForceSeed_GivesAValidSeedAndMarksTheSackWhenItChanged()
	{
		var sword = MakeItem("records/sword.dbr", "WeaponMelee_Sword");
		sword.Seed = 1234;

		var changed = this._service.ForceSeed(this._sack, sword);

		Item.IsValidSeed(sword.Seed).Should().BeTrue();
		changed.Should().Be(sword.Seed != 1234);
		this._sack.IsModified.Should().Be(changed);
		sword.IsModified.Should().BeTrue();
	}

	[Fact]
	public void SetSeed_WithANewValidSeed_ChangesItAndMarksTheItemAndTheSack()
	{
		var sword = MakeItem("records/sword.dbr", "WeaponMelee_Sword");
		sword.Seed = 1234;

		this._service.SetSeed(this._sack, sword, Item.MaxSeed).Should().BeTrue();

		sword.Seed.Should().Be(Item.MaxSeed);
		sword.IsModified.Should().BeTrue();
		this._sack.IsModified.Should().BeTrue();
	}

	[Theory]
	[InlineData(1234)]
	[InlineData(0)]
	[InlineData(-5)]
	[InlineData(Item.MaxSeed + 1)]
	public void SetSeed_WithTheSameOrAnInvalidSeed_ChangesNothing(int seed)
	{
		var sword = MakeItem("records/sword.dbr", "WeaponMelee_Sword");
		sword.Seed = 1234;

		this._service.SetSeed(this._sack, sword, seed).Should().BeFalse();

		sword.Seed.Should().Be(1234);
		sword.IsModified.Should().BeFalse();
		this._sack.IsModified.Should().BeFalse();
	}

	#endregion

	[Fact]
	public void NewSetMember_KeepsTheBlockDataAndReadsTheMemberRecord()
	{
		var setItem = MakeItem("records/set/helm.dbr", Item.ICLASS_HEAD);
		setItem.beginBlockCrap1 = 7;
		var memberId = RecordId.Create("records/set/armor.dbr");

		var member = this._service.NewSetMember(setItem, memberId);

		member.Should().NotBeSameAs(setItem);
		member.BaseItemId.Should().Be(memberId);
		member.beginBlockCrap1.Should().Be(7);
		this._itemProvider.Verify(p => p.GetDBData(member), Times.Once);
	}
	#region Maps

	private const string Shadow = "shadowchampions|custommaps";
	private static readonly RecordId Lance = RecordId.Create("records/customitems/lance.dbr");

	/// <summary>
	/// The databases, read with the map of the item's container: the mod adds a 1x5 lance; every other record is the
	/// base game's, 1x1. Without the mod, the lance is missing (a 1x1 warning tile).
	/// </summary>
	private void UseTheMaps()
		=> this._itemProvider.Setup(p => p.GetDBData(It.IsAny<Item>())).Callback<Item>(Read);

	private static void Read(Item item)
	{
		bool modded = item.ContainerMod == Shadow;
		bool lance = item.BaseItemId == Lance;
		item.baseItemInfo = lance
			? (modded ? TestRecords.InfoFrom(Lance, Shadow, "WeaponMelee_Spear") : null)
			: TestRecords.InfoFrom(item.BaseItemId, null);
		item.Width = 1;
		item.Height = lance && modded ? 5 : 1;
	}

	/// <summary>An item a container of the map <paramref name="containerMod"/> holds, read with that map.</summary>
	private static Item Placed(RecordId record, string? containerMod, int x = 0, int y = 0)
	{
		var item = new Item { BaseItemId = record, PositionX = x, PositionY = y };
		item.SetContainerMod(containerMod);
		Read(item);
		return item;
	}

	private static TQVaultAE.Application.Session.PlayerCollection Vault(string? mod, params Item[] items)
	{
		var sack = new SackCollection();
		foreach (var item in items)
			sack.AddItem(item);

		return new TQVaultAE.Application.Session.PlayerCollection("vault", "vault.json") { IsVault = true, Mod = mod, Sacks = [sack] };
	}

	#endregion

	[Fact]
	public void ChangeVaultMod_EveryItemTakesTheNewMap_AndIsReadAgain()
	{
		this.UseTheMaps();
		var ring = Placed(RecordId.Create("records/ring.dbr"), "Lilith|custommaps");
		var sword = Placed(RecordId.Create("records/sword.dbr"), "Lilith|custommaps", x: 1);
		var vault = Vault("Lilith|custommaps", ring, sword);

		var change = this._service.ChangeVaultMod(vault, Shadow);

		vault.Mod.Should().Be(Shadow);
		change.Changed.Should().Equal(ring, sword);
		change.NoRoom.Should().BeEmpty();
		(ring.ContainerMod, sword.ContainerMod).Should().Be((Shadow, Shadow));
		vault.IsModified.Should().BeTrue();
	}

	[Fact]
	public void ChangeVaultMod_TheModCompletesAnItem_AndUnlinkingMakesItIncompleteAgain()
	{
		// Arrange: an incomplete lance in a vault without link; the mod has it
		this.UseTheMaps();
		var lance = Placed(Lance, null);
		var vault = Vault(null, lance);

		// Act: linked to the mod, then unlinked
		this._service.ChangeVaultMod(vault, Shadow, (sack, item) => true);
		var linked = (lance.Mod, lance.IsIncomplete);
		this._service.ChangeVaultMod(vault, null);

		// Assert: its mod is the database its records were read from, each time
		linked.Should().Be((Shadow, false));
		(lance.Mod, lance.IsIncomplete).Should().Be(((string?)null, true));
	}

	[Fact]
	public void ChangeVaultMod_ItemWhoseNewSizeHasNoRoom_IsShown1x1_UntilTheLinkGivesItRoomAgain()
	{
		// Arrange: a 2x2 bag; read with the mod, the 1x1 tile of the lance becomes 1x5
		this.UseTheMaps();
		var lance = Placed(Lance, null);
		var vault = Vault(null, lance);
		var bag = new System.Drawing.Size(2, 2);

		// Act
		var change = this._service.ChangeVaultMod(vault, Shadow, (sack, item) => TQVaultAE.Domain.Actions.SackLayout.FitsOrMoves(sack, bag, item));

		// Assert: shown 1x1 where it lies, read with the mod (whole records), its real size kept
		change.NoRoom.Should().Equal(lance);
		change.Changed.Should().BeEmpty();
		(lance.LacksRoom, lance.Width, lance.Height, lance.RealHeight).Should().Be((true, 1, 1, 5));
		(lance.Mod, lance.IsIncomplete, lance.HasWarning).Should().Be((Shadow, false, true));

		// Unlinked again, it is 1x1 for real (a missing record): no lack of room any more
		this._service.ChangeVaultMod(vault, null, (sack, item) => TQVaultAE.Domain.Actions.SackLayout.FitsOrMoves(sack, bag, item));
		(lance.LacksRoom, lance.Height, lance.IsIncomplete).Should().Be((false, 1, true));
	}

	[Fact]
	public void SettleIn_ModdedItemMovedToAContainerOfAnotherMap_IsReadAgainWithIt_AndLosesItsMod()
	{
		this.UseTheMaps();
		var lance = Placed(Lance, Shadow);

		this._service.SettleIn(lance, null, _ => true).Should().Be(SettleOutcome.ReadAgain);
		var unlinked = (lance.ContainerMod, lance.Mod, lance.IsIncomplete);
		this._service.SettleIn(lance, Shadow, _ => true).Should().Be(SettleOutcome.ReadAgain);

		unlinked.Should().Be(((string?)null, (string?)null, true));
		(lance.ContainerMod, lance.Mod, lance.IsIncomplete).Should().Be((Shadow, Shadow, false));
	}

	[Fact]
	public void SettleIn_BaseGameItemInALinkedContainer_HasNoMod()
	{
		// Every record of the ring is the base game's: read with the mod's data, it is still no mod's.
		this.UseTheMaps();
		var ring = Placed(RecordId.Create("records/ring.dbr"), null);

		this._service.SettleIn(ring, Shadow, _ => true).Should().Be(SettleOutcome.ReadAgain);

		(ring.ContainerMod, ring.Mod).Should().Be((Shadow, (string?)null));
	}

	[Fact]
	public void SettleIn_SameMap_IsNotReadAgain()
	{
		this.UseTheMaps();
		var ring = Placed(RecordId.Create("records/ring.dbr"), Shadow);

		this._service.SettleIn(ring, Shadow.ToUpperInvariant(), _ => true).Should().Be(SettleOutcome.Unchanged);

		this._itemProvider.Verify(p => p.GetDBData(It.IsAny<Item>()), Times.Never);
	}

	[Fact]
	public void SettleIn_NoRoomOnceReadWithTheNewMap_IsRefused_AndTheItemGoesBackToItsMap()
	{
		this.UseTheMaps();
		var lance = Placed(Lance, null);

		this._service.SettleIn(lance, Shadow, _ => false).Should().Be(SettleOutcome.NoRoom);

		(lance.ContainerMod, lance.Height, lance.LacksRoom).Should().Be(((string?)null, 1, false));
		this._itemProvider.Verify(p => p.GetDBData(lance), Times.Exactly(2));
	}

	[Theory]
	[InlineData(true, SettleOutcome.ReadAgain, false, 5)]
	[InlineData(false, SettleOutcome.Unchanged, true, 1)]
	public void SettleIn_ItemShown1x1_GetsItsRealSizeBackOnlyWhereItFits(bool fits, SettleOutcome expected, bool lacksRoom, int height)
	{
		this.UseTheMaps();
		var lance = Placed(Lance, Shadow);
		lance.ShowNoRoom();
		int? checkedHeight = null;

		this._service.SettleIn(lance, Shadow, i => { checkedHeight = i.Height; return fits; }).Should().Be(expected);

		checkedHeight.Should().Be(5, "the room is checked at its real size");
		(lance.LacksRoom, lance.Height).Should().Be((lacksRoom, height));
		this._itemProvider.Verify(p => p.GetDBData(It.IsAny<Item>()), Times.Never);
	}

	[Fact]
	public void HasRoomOnceSettled_TakingANewMap_ChecksAtTheNewSize_AndLeavesTheItemAsItWas()
	{
		// Arrange: read with the mod's data, the 1x1 tile becomes a 1x5 lance
		this.UseTheMaps();
		var lance = Placed(Lance, null, x: 2);
		int? checkedHeight = null;

		// Act
		var room = this._service.HasRoomOnceSettled(lance, Shadow, i => { checkedHeight = i.Height; return false; });

		// Assert
		room.Should().BeFalse();
		checkedHeight.Should().Be(5);
		(lance.ContainerMod, lance.Height, lance.PositionX, lance.IsIncomplete).Should().Be(((string?)null, 1, 2, true));
	}

	[Fact]
	public void HasRoomOnceSettled_SameMap_IsNotChecked()
	{
		this.UseTheMaps();
		var lance = Placed(Lance, Shadow);

		this._service.HasRoomOnceSettled(lance, Shadow, _ => false).Should().BeTrue();

		this._itemProvider.Verify(p => p.GetDBData(It.IsAny<Item>()), Times.Never);
	}

	[Fact]
	public void SettleIn_ItemWithoutAMapYet_TakesTheContainersAndIsRead()
	{
		// An imported item: read with the data in use, it now takes its vault's map.
		this.UseTheMaps();
		var item = new Item { BaseItemId = RecordId.Create("records/ring.dbr") };

		this._service.SettleIn(item, containerMod: null, _ => true).Should().Be(SettleOutcome.ReadAgain);

		(item.HasContainerMod, item.ContainerMod).Should().Be((true, (string?)null));
	}

	[Fact]
	public void SettleAll_ReturnsTheItemsReadAgain_AndMovesThoseThatNoLongerFit()
	{
		// Arrange: a 2x5 bag; the lance arrives in the bottom corner, read with the mod it takes 1x5 and moves up
		this.UseTheMaps();
		var staying = Placed(RecordId.Create("records/ring.dbr"), Shadow);
		var arriving = Placed(Lance, null, x: 1, y: 4);
		var sack = new SackCollection();
		sack.AddItem(staying);
		sack.AddItem(arriving);

		// Act
		var result = this._service.SettleAll(sack, new System.Drawing.Size(2, 5), [staying, arriving], Shadow);

		// Assert
		result.ReadAgain.Should().Equal(arriving);
		result.NoRoom.Should().BeEmpty();
		(arriving.PositionX, arriving.PositionY, arriving.Height).Should().Be((1, 0, 5));
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void SettleAll_NoRoomInTheBag_TriesAnotherBag_ElseShowsTheItem1x1(bool anotherBagHasRoom)
	{
		// Arrange: a 1x1 bag; read with the vault's map the lance takes 1x5
		this.UseTheMaps();
		var arriving = Placed(Lance, null);
		var sack = new SackCollection();
		sack.AddItem(arriving);

		// Act
		var result = this._service.SettleAll(sack, new System.Drawing.Size(1, 1), [arriving], Shadow, (_, _) => anotherBagHasRoom);

		// Assert: read with the vault's map either way
		(anotherBagHasRoom ? result.ReadAgain : result.NoRoom).Should().Equal(arriving);
		(arriving.Mod, arriving.LacksRoom).Should().Be((Shadow, !anotherBagHasRoom));
	}
}
