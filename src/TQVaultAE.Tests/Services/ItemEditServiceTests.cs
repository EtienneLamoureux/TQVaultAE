using AwesomeAssertions;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

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
		this._service = new ItemEditService(this._database.Object, this._itemProvider.Object);
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
	public void Split_KeepsOneAndReturnsTheRest()
	{
		var potions = MakeItem("records/potion.dbr", Item.ICLASS_POTIONHEALTH);
		potions.StackSize = 5;

		var rest = this._service.Split(potions);

		rest.Should().NotBeNull();
		rest!.StackSize.Should().Be(4);
		potions.StackSize.Should().Be(1);
		this._service.Split(potions).Should().BeNull("a single item cannot be split");
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
	/// <summary>An item a container holds: in a vault or not, with a map it inherits or owns.</summary>
	private static Item Placed(string record, string? mod, ModOrigin origin, bool inVault = true)
		=> new() { BaseItemId = RecordId.Create(record), Mod = mod, ModOrigin = origin, InVault = inVault, Width = 1, Height = 1 };

	/// <summary>The mod has the lance; nothing has the unknown sword.</summary>
	private void ModHasTheLance(RecordId lance)
		=> this._itemProvider.Setup(p => p.GetDBData(It.IsAny<Item>())).Callback<Item>(i =>
			i.baseItemInfo = i.Mod == "shadowchampions" && i.BaseItemId == lance ? MakeInfo(lance.Raw, "WeaponMelee_Spear") : null);

	[Fact]
	public void ChangeMod_GivesTheModAsTheItemsOwnMap_AndReadsItAgain()
	{
		var item = Placed("records/ring.dbr", null, ModOrigin.Inherited);

		this._service.ChangeMod(this._sack, item, "shadowchampions").Should().BeTrue();

		(item.Mod, item.ModOrigin).Should().Be(("shadowchampions", ModOrigin.Own));
		item.IsModified.Should().BeTrue();
		this._sack.IsModified.Should().BeTrue();
		this._itemProvider.Verify(p => p.GetDBData(item), Times.Once);
	}

	[Fact]
	public void ChangeMod_ItemNoLongerFits_UndoesTheChoice()
	{
		// Arrange: read with the mod's data the item takes another size, and its container has no room for it
		var item = Placed("records/ring.dbr", null, ModOrigin.Inherited);
		this._sack.IsModified = false;

		// Act
		var changed = this._service.ChangeMod(this._sack, item, "shadowchampions", _ => false);

		// Assert: nothing to save, and the item is read again with the map it had
		changed.Should().BeFalse();
		(item.Mod, item.ModOrigin).Should().Be(((string?)null, ModOrigin.Inherited));
		this._sack.IsModified.Should().BeFalse();
		this._itemProvider.Verify(p => p.GetDBData(item), Times.Exactly(2));
	}

	[Fact]
	public void ChangeMod_SameOwnMod_ChangesNothing()
	{
		var item = Placed("records/ring.dbr", "shadowchampions", ModOrigin.Own);

		this._service.ChangeMod(this._sack, item, "ShadowChampions").Should().BeFalse();

		this._sack.IsModified.Should().BeFalse();
		this._itemProvider.Verify(p => p.GetDBData(It.IsAny<Item>()), Times.Never);
	}

	[Fact]
	public void ChangeMod_TheMapItInherits_BecomesItsOwn_WithoutReadingItAgain()
	{
		var item = Placed("records/ring.dbr", "shadowchampions", ModOrigin.Inherited);

		this._service.ChangeMod(this._sack, item, "shadowchampions").Should().BeTrue();

		item.ModOrigin.Should().Be(ModOrigin.Own);
		this._itemProvider.Verify(p => p.GetDBData(It.IsAny<Item>()), Times.Never);
	}

	[Fact]
	public void ChangeMod_BaseGameChosenInAModdedVault_IsKeptWhenTheVaultChangesItsLink()
	{
		// Arrange: an item inheriting the vault's Lilith link
		var vault = new TQVaultAE.Application.PlayerCollection("vault", "vault.json") { IsVault = true, Mod = "Lilith" };
		var sack = new SackCollection();
		var item = Placed("records/a.dbr", "Lilith", ModOrigin.Inherited);
		sack.AddItem(item);
		vault.Sacks = [sack];

		// Act: the base game is chosen for it, then the vault is linked to another mod
		this._service.ChangeMod(sack, item, null).Should().BeTrue();
		var change = this._service.ChangeVaultMod(vault, "shadowchampions");

		// Assert
		change.Changed.Should().BeEmpty();
		(item.Mod, item.ModOrigin).Should().Be(((string?)null, ModOrigin.Own));
	}

	[Fact]
	public void ChangeVaultMod_InheritedMapsTakeTheNewOne_OwnMapsAreKept()
	{
		var vault = new TQVaultAE.Application.PlayerCollection("vault", "vault.json") { IsVault = true, Mod = "Lilith" };
		var sack = new SackCollection();
		var inherited = Placed("records/b.dbr", "Lilith", ModOrigin.Inherited);
		var own = Placed("records/d.dbr", "Lilith", ModOrigin.Own);
		var other = Placed("records/c.dbr", "Masteries", ModOrigin.Own);
		sack.AddItem(inherited);
		sack.AddItem(own);
		sack.AddItem(other);
		vault.Sacks = [sack];

		var change = this._service.ChangeVaultMod(vault, "shadowchampions");

		vault.Mod.Should().Be("shadowchampions");
		change.Changed.Should().Equal(inherited);
		change.Kept.Should().BeEmpty();
		(inherited.Mod, inherited.ModOrigin).Should().Be(("shadowchampions", ModOrigin.Inherited));
		(own.Mod, other.Mod).Should().Be(("Lilith", "Masteries"));
		vault.IsModified.Should().BeTrue();
	}

	[Fact]
	public void ChangeVaultMod_AnItemTheModCompletes_StillOnlyInheritsIt()
	{
		// Arrange: an incomplete lance in a vault without link; the mod completes it
		var lance = RecordId.Create("records/customitems/lance.dbr");
		this.ModHasTheLance(lance);
		var vault = new TQVaultAE.Application.PlayerCollection("vault", "vault.json") { IsVault = true };
		var sack = new SackCollection();
		var item = Placed(lance.Raw, null, ModOrigin.Inherited);
		sack.AddItem(item);
		vault.Sacks = [sack];

		// Act: linked to the mod, then unlinked
		this._service.ChangeVaultMod(vault, "shadowchampions");
		var linked = (item.Mod, item.ModOrigin, item.IsIncomplete);
		this._service.ChangeVaultMod(vault, null);

		// Assert: no guess, the item follows the vault both times
		linked.Should().Be(("shadowchampions", ModOrigin.Inherited, false));
		(item.Mod, item.ModOrigin, item.IsIncomplete).Should().Be(((string?)null, ModOrigin.Inherited, true));
	}

	[Fact]
	public void ChangeVaultMod_ItemWithoutRoom_KeepsItsPreviousMapAsItsOwn()
	{
		var vault = new TQVaultAE.Application.PlayerCollection("vault", "vault.json") { IsVault = true, Mod = "Lilith" };
		var sack = new SackCollection();
		var item = Placed("records/b.dbr", "Lilith", ModOrigin.Inherited);
		sack.AddItem(item);
		vault.Sacks = [sack];

		var change = this._service.ChangeVaultMod(vault, "shadowchampions", (_, _) => false);

		change.Changed.Should().BeEmpty();
		change.Kept.Should().Equal(item);
		(item.Mod, item.ModOrigin).Should().Be(("Lilith", ModOrigin.Own));
		this._itemProvider.Verify(p => p.GetDBData(item), Times.Exactly(2));
	}

	[Fact]
	public void SettleIn_OwnMap_GoesWithTheItem()
	{
		var item = Placed("records/lilith/sword.dbr", "Lilith", ModOrigin.Own, inVault: false);

		this._service.SettleIn(item, inVault: true, "shadowchampions", _ => true).Should().Be(SettleOutcome.Unchanged);

		(item.Mod, item.ModOrigin, item.InVault).Should().Be(("Lilith", ModOrigin.Own, true));
		this._itemProvider.Verify(p => p.GetDBData(It.IsAny<Item>()), Times.Never);
	}

	[Theory]
	[InlineData("shadowchampions")]
	[InlineData(null)]
	public void SettleIn_LeavingACharacterForAVault_MakesTheMapItsOwn(string? vaultMod)
	{
		var item = Placed("records/item/sword.dbr", "shadowchampions", ModOrigin.Inherited, inVault: false);

		this._service.SettleIn(item, inVault: true, vaultMod, _ => true).Should().Be(SettleOutcome.Unchanged);

		(item.Mod, item.ModOrigin, item.InVault).Should().Be(("shadowchampions", ModOrigin.Own, true));
		item.IsModified.Should().BeTrue();
		this._itemProvider.Verify(p => p.GetDBData(It.IsAny<Item>()), Times.Never);
	}

	[Fact]
	public void SettleIn_VaultItemMovedToAVaultOfAnotherMod_InheritsItAndIsReadAgain()
	{
		var lance = RecordId.Create("records/customitems/lance.dbr");
		this.ModHasTheLance(lance);
		var item = Placed(lance.Raw, null, ModOrigin.Inherited);

		this._service.SettleIn(item, inVault: true, "shadowchampions", _ => true).Should().Be(SettleOutcome.ReadAgain);

		(item.Mod, item.ModOrigin, item.IsIncomplete).Should().Be(("shadowchampions", ModOrigin.Inherited, false));
	}

	[Fact]
	public void SettleIn_VaultItemTakenToACharacter_InheritsTheCharactersMap()
	{
		var item = Placed("records/item/ring.dbr", null, ModOrigin.Inherited);

		this._service.SettleIn(item, inVault: false, "shadowchampions", _ => true).Should().Be(SettleOutcome.ReadAgain);

		(item.Mod, item.ModOrigin, item.InVault).Should().Be(("shadowchampions", ModOrigin.Inherited, false));
		this._itemProvider.Verify(p => p.GetDBData(item), Times.Once);
	}

	[Fact]
	public void SettleIn_SameMap_IsNotReadAgain()
	{
		var item = Placed("records/ring.dbr", "shadowchampions", ModOrigin.Inherited);

		this._service.SettleIn(item, inVault: true, "SHADOWCHAMPIONS", _ => true).Should().Be(SettleOutcome.Unchanged);

		this._itemProvider.Verify(p => p.GetDBData(It.IsAny<Item>()), Times.Never);
	}

	[Fact]
	public void SettleIn_NoRoomOnceReadWithTheNewMap_KeepsThePreviousMapAsItsOwn()
	{
		var item = Placed("records/customitems/lance.dbr", null, ModOrigin.Inherited);

		this._service.SettleIn(item, inVault: true, "shadowchampions", _ => false).Should().Be(SettleOutcome.KeptOwnMap);

		(item.Mod, item.ModOrigin).Should().Be(((string?)null, ModOrigin.Own));
		this._itemProvider.Verify(p => p.GetDBData(item), Times.Exactly(2));
	}

	[Fact]
	public void SettleIn_ItemWithoutAMapYet_InheritsTheContainersAndIsRead()
	{
		// An imported item without "mod": read with the data in use, it now takes its vault's map.
		var item = new Item { BaseItemId = RecordId.Create("records/ring.dbr") };

		this._service.SettleIn(item, inVault: true, containerMod: null, _ => true).Should().Be(SettleOutcome.ReadAgain);

		(item.Mod, item.ModOrigin, item.InVault).Should().Be(((string?)null, ModOrigin.Inherited, true));
	}

	[Fact]
	public void SettleAll_ReturnsTheItemsReadAgain_AndMovesThoseThatNoLongerFit()
	{
		// Arrange: a 2x2 bag; the new item lies in the corner, read again it takes 1x2 and moves up
		var sack = new SackCollection();
		var staying = Placed("records/ring.dbr", null, ModOrigin.Inherited);
		var arriving = new Item { BaseItemId = RecordId.Create("records/customitems/lance.dbr"), PositionX = 1, PositionY = 1, Width = 1, Height = 1 };
		sack.AddItem(staying);
		sack.AddItem(arriving);
		this._itemProvider.Setup(p => p.GetDBData(arriving)).Callback<Item>(i => i.Height = 2);

		// Act
		var result = this._service.SettleAll(sack, new System.Drawing.Size(2, 2), [staying, arriving], inVault: true, containerMod: null);

		// Assert
		result.ReadAgain.Should().Equal(arriving);
		result.Kept.Should().BeEmpty();
		(arriving.PositionX, arriving.PositionY).Should().Be((1, 0));
	}

	[Theory]
	[InlineData(true, SettleOutcome.ReadAgain)]
	[InlineData(false, SettleOutcome.KeptOwnMap)]
	public void SettleAll_NoRoomInTheBag_TriesAnotherBag_ElseKeepsTheMap(bool anotherBagHasRoom, SettleOutcome expected)
	{
		// Arrange: a 1x1 bag; read with the vault's map the item takes 1x2
		var sack = new SackCollection();
		var arriving = Placed("records/customitems/lance.dbr", null, ModOrigin.Inherited);
		sack.AddItem(arriving);
		this._itemProvider.Setup(p => p.GetDBData(arriving)).Callback<Item>(i => i.Height = i.Mod is null ? 1 : 2);

		// Act
		var result = this._service.SettleAll(sack, new System.Drawing.Size(1, 1), [arriving], inVault: true, "shadowchampions"
			, (_, _) => anotherBagHasRoom);

		// Assert
		(expected == SettleOutcome.ReadAgain ? result.ReadAgain : result.Kept).Should().Equal(arriving);
		arriving.Mod.Should().Be(expected == SettleOutcome.ReadAgain ? "shadowchampions" : null);
	}
}
