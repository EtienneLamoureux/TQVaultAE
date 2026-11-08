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
}
