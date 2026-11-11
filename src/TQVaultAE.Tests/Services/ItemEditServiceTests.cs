using AwesomeAssertions;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
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
	[Fact]
	public void ChangeMod_LinksTheItemAndReadsItAgain()
	{
		var item = MakeItem("records/ring.dbr", "ArmorJewelry_Ring");

		this._service.ChangeMod(this._sack, item, "shadowchampions").Should().BeTrue();

		item.Mod.Should().Be("shadowchampions");
		item.IsModified.Should().BeTrue();
		this._sack.IsModified.Should().BeTrue();
		this._itemProvider.Verify(p => p.GetDBData(item), Times.Once);
	}

	[Fact]
	public void ChangeMod_ItemNoLongerFits_PutsTheLinkBack()
	{
		// Arrange: read with the mod's data the item takes another size, and its sack has no room for it
		var item = MakeItem("records/ring.dbr", "ArmorJewelry_Ring");
		this._sack.IsModified = false;

		// Act
		var changed = this._service.ChangeMod(this._sack, item, "shadowchampions", _ => false);

		// Assert: nothing to save, and the item is read again with the data it had
		changed.Should().BeFalse();
		item.Mod.Should().BeNull();
		this._sack.IsModified.Should().BeFalse();
		this._itemProvider.Verify(p => p.GetDBData(item), Times.Exactly(2));
	}

	[Fact]
	public void ChangeMod_SameMod_ChangesNothing()
	{
		var item = MakeItem("records/ring.dbr", "ArmorJewelry_Ring");
		item.Mod = "shadowchampions";

		this._service.ChangeMod(this._sack, item, "ShadowChampions").Should().BeFalse();

		this._sack.IsModified.Should().BeFalse();
		this._itemProvider.Verify(p => p.GetDBData(It.IsAny<Item>()), Times.Never);
	}

	[Fact]
	public void ChangeVaultMod_ItemsOfTheVaultFollow_ItemsWithTheirOwnLinkKeepIt()
	{
		var vault = new TQVaultAE.Application.PlayerCollection("vault", "vault.json") { IsVault = true, Mod = "Lilith" };
		var sack = new SackCollection();
		var plain = MakeItem("records/a.dbr", "ArmorJewelry_Ring");
		var ofVault = MakeItem("records/b.dbr", "ArmorJewelry_Ring");
		ofVault.Mod = "Lilith";
		ofVault.ModFromVault = true;
		var own = MakeItem("records/d.dbr", "ArmorJewelry_Ring");
		own.Mod = "Lilith";
		var other = MakeItem("records/c.dbr", "ArmorJewelry_Ring");
		other.Mod = "Masteries";
		sack.AddItem(plain);
		sack.AddItem(ofVault);
		sack.AddItem(own);
		sack.AddItem(other);
		vault.Sacks = [sack];

		var changed = this._service.ChangeVaultMod(vault, "shadowchampions");

		vault.Mod.Should().Be("shadowchampions");
		changed.Should().BeEquivalentTo([plain, ofVault]);
		(plain.Mod, plain.ModFromVault).Should().Be(("shadowchampions", true));
		(ofVault.Mod, ofVault.ModFromVault).Should().Be(("shadowchampions", true));
		own.Mod.Should().Be("Lilith");
		other.Mod.Should().Be("Masteries");
		vault.IsModified.Should().BeTrue();
	}

	[Fact]
	public void ChangeVaultMod_ItemsTheModCompletes_GetItAsTheirOwnLink()
	{
		// The mod has the lance; nothing has the unknown sword.
		var lance = RecordId.Create("records/customitems/lance.dbr");
		var sword = RecordId.Create("records/unknown/sword.dbr");
		this._itemProvider.Setup(p => p.GetDBData(It.IsAny<Item>())).Callback<Item>(i =>
			i.baseItemInfo = i.Mod == "shadowchampions" && i.BaseItemId == lance ? MakeInfo(lance.Raw, "WeaponMelee_Spear") : null);
		var vault = new TQVaultAE.Application.PlayerCollection("vault", "vault.json") { IsVault = true };
		var sack = new SackCollection();
		var resolved = new Item { BaseItemId = lance };
		var stillMissing = new Item { BaseItemId = sword };
		var baseGame = MakeItem("records/a.dbr", "ArmorJewelry_Ring");
		this._itemProvider.Setup(p => p.GetDBData(baseGame)).Callback<Item>(i => i.baseItemInfo = MakeInfo("records/a.dbr", "ArmorJewelry_Ring"));
		sack.AddItem(resolved);
		sack.AddItem(stillMissing);
		sack.AddItem(baseGame);
		vault.Sacks = [sack];

		this._service.ChangeVaultMod(vault, "shadowchampions");

		(resolved.Mod, resolved.ModFromVault).Should().Be(("shadowchampions", false));
		(stillMissing.Mod, stillMissing.ModFromVault).Should().Be(("shadowchampions", true));
		(baseGame.Mod, baseGame.ModFromVault).Should().Be(("shadowchampions", true));

		// Unlinking the vault: the resolved item keeps its mod, the others follow.
		this._service.ChangeVaultMod(vault, null);

		resolved.Mod.Should().Be("shadowchampions");
		stillMissing.Mod.Should().BeNull();
		baseGame.Mod.Should().BeNull();
	}

	/// <summary>The mod has the lance; nothing has the unknown sword.</summary>
	private void ModHasTheLance(RecordId lance)
		=> this._itemProvider.Setup(p => p.GetDBData(It.IsAny<Item>())).Callback<Item>(i =>
			i.baseItemInfo = i.Mod == "shadowchampions" && i.BaseItemId == lance ? MakeInfo(lance.Raw, "WeaponMelee_Spear") : null);

	[Fact]
	public void AdoptContainerMod_UnresolvedItemTheVaultsModResolves_GetsItAsItsOwnLink()
	{
		var lance = RecordId.Create("records/customitems/lance.dbr");
		this.ModHasTheLance(lance);
		var item = new Item { BaseItemId = lance };

		this._service.AdoptContainerMod(item, "shadowchampions", _ => true).Should().BeTrue();

		(item.Mod, item.ModFromVault).Should().Be(("shadowchampions", false));
	}

	[Fact]
	public void AdoptContainerMod_ItemStillUnresolved_FollowsTheVault()
	{
		this.ModHasTheLance(RecordId.Create("records/customitems/lance.dbr"));
		var item = new Item { BaseItemId = RecordId.Create("records/unknown/sword.dbr") };

		this._service.AdoptContainerMod(item, "shadowchampions", _ => true).Should().BeTrue();

		(item.Mod, item.ModFromVault).Should().Be(("shadowchampions", true));
	}

	[Fact]
	public void AdoptContainerMod_ResolvedItemNoLongerFits_IsLeftAsItWas()
	{
		var lance = RecordId.Create("records/customitems/lance.dbr");
		this.ModHasTheLance(lance);
		var item = new Item { BaseItemId = lance };

		this._service.AdoptContainerMod(item, "shadowchampions", _ => false).Should().BeFalse();

		(item.Mod, item.ModFromVault).Should().Be(((string?)null, false));
		item.IsIncomplete.Should().BeTrue();
	}

	[Fact]
	public void AdoptContainerMod_ItemWithItsOwnLink_KeepsIt()
	{
		var item = MakeItem("records/ring.dbr", "ArmorJewelry_Ring");
		item.Mod = "Lilith";

		this._service.AdoptContainerMod(item, "shadowchampions", _ => true).Should().BeFalse();

		item.Mod.Should().Be("Lilith");
		this._itemProvider.Verify(p => p.GetDBData(It.IsAny<Item>()), Times.Never);
	}

	[Fact]
	public void SettleIn_ItemPutInAVaultWithoutLink_IsReadAgainAsAVaultItem()
	{
		var item = new Item { BaseItemId = RecordId.Create("records/customitems/lance.dbr") };
		var readInVault = new List<bool>();
		this._itemProvider.Setup(p => p.GetDBData(item)).Callback<Item>(i => readInVault.Add(i.InVault));

		this._service.SettleIn(item, inVault: true, vaultMod: null, _ => true).Should().BeTrue();

		item.InVault.Should().BeTrue();
		item.Mod.Should().BeNull();
		item.IsModified.Should().BeFalse();
		readInVault.Should().Equal(true);
	}

	[Fact]
	public void SettleAll_ReturnsTheItemsReadAgain_AndMovesThoseThatNoLongerFit()
	{
		// Arrange: a 2x2 bag; the new item lies in the corner, read again it takes 1x2 and moves up
		var sack = new SackCollection();
		var staying = new Item { BaseItemId = RecordId.Create("records/ring.dbr"), InVault = true, Width = 1, Height = 1 };
		var arriving = new Item { BaseItemId = RecordId.Create("records/customitems/lance.dbr"), PositionX = 1, PositionY = 1, Width = 1, Height = 1 };
		sack.AddItem(staying);
		sack.AddItem(arriving);
		this._itemProvider.Setup(p => p.GetDBData(arriving)).Callback<Item>(i => i.Height = 2);

		// Act
		var changed = this._service.SettleAll(sack, new System.Drawing.Size(2, 2), [staying, arriving], inVault: true, vaultMod: null);

		// Assert
		changed.Should().Equal(arriving);
		(arriving.PositionX, arriving.PositionY).Should().Be((1, 0));
	}

	[Fact]
	public void SettleIn_ItemAlreadyThere_IsNotReadAgain()
	{
		var item = new Item { BaseItemId = RecordId.Create("records/ring.dbr"), InVault = true };

		this._service.SettleIn(item, inVault: true, vaultMod: null, _ => true).Should().BeFalse();

		this._itemProvider.Verify(p => p.GetDBData(It.IsAny<Item>()), Times.Never);
	}

	[Fact]
	public void SettleIn_ItemPutInAVaultLinkedToAMod_FollowsTheVault()
	{
		var lance = RecordId.Create("records/customitems/lance.dbr");
		this.ModHasTheLance(lance);
		var item = new Item { BaseItemId = lance };

		this._service.SettleIn(item, inVault: true, vaultMod: "shadowchampions", _ => true).Should().BeTrue();

		(item.InVault, item.Mod, item.ModFromVault).Should().Be((true, "shadowchampions", false));
	}

	[Fact]
	public void SettleIn_ItemTakenToACharacter_IsReadAgainOutOfTheVault()
	{
		var item = new Item { BaseItemId = RecordId.Create("records/customitems/lance.dbr"), InVault = true };
		var readInVault = new List<bool>();
		this._itemProvider.Setup(p => p.GetDBData(item)).Callback<Item>(i => readInVault.Add(i.InVault));

		this._service.SettleIn(item, inVault: false, vaultMod: null, _ => true).Should().BeTrue();

		item.InVault.Should().BeFalse();
		readInVault.Should().Equal(false);
	}

	[Fact]
	public void SettleIn_ItemTakenToACharacterNoLongerFits_IsLeftAsItWas()
	{
		var item = new Item { BaseItemId = RecordId.Create("records/customitems/lance.dbr"), InVault = true };
		var readInVault = new List<bool>();
		this._itemProvider.Setup(p => p.GetDBData(item)).Callback<Item>(i => readInVault.Add(i.InVault));

		this._service.SettleIn(item, inVault: false, vaultMod: null, _ => false).Should().BeFalse();

		item.InVault.Should().BeTrue();
		readInVault.Should().Equal(false, true);
	}

	[Fact]
	public void ChangeMod_ItemFollowingTheVault_GetsItsOwnLink()
	{
		var item = MakeItem("records/ring.dbr", "ArmorJewelry_Ring");
		item.Mod = "shadowchampions";
		item.ModFromVault = true;

		this._service.ChangeMod(this._sack, item, "shadowchampions").Should().BeTrue();

		item.ModFromVault.Should().BeFalse();
	}
}
