using AwesomeAssertions;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Domain;

/// <summary>
/// <see cref="ItemActionRules"/>: the actions of the sack context menu.
/// </summary>
public class ItemActionRulesTests
{
	private const ItemActions GroupActions = ItemActions.Delete | ItemActions.MoveTo;

	private static Item Sword(bool socketed1 = false, bool socketed2 = false) => new()
	{
		BaseItemId = RecordId.Create(@"records\xpack\items\weapons\swords\u_e_01.dbr"),
		relicID = socketed1 ? RelicAndCharm.AEGISOFATHENA_ACT1_01.GetRecordId() : RecordId.Empty,
		relic2ID = socketed2 ? RelicAndCharm.AEGISOFATHENA_ACT1_01.GetRecordId() : RecordId.Empty,
	};

	private static Item Potions(int count)
	{
		var potion = new Item { BaseItemId = RecordId.Create(@"records\item\oneshot\potionhealth01.dbr") };
		potion.Number = count;
		return potion;
	}

	private static ItemActions For(Item? item, int selectionCount = 0, bool itemIsTheSelection = false
		, bool readOnly = false, bool allowEdit = false, bool allowCopy = false)
		=> ItemActionRules.Available(new ItemActionContext(item, selectionCount, itemIsTheSelection, readOnly, allowEdit, allowCopy));

	[Theory]
	[InlineData("""records\item\equipmentweapon\sword\us_n_01.dbr""", false, true)]
	[InlineData("""records\xpack\items\weapons\swords\u_e_01.dbr""", false, false)]
	[InlineData("""records\xpack\items\weapons\swords\u_e_01.dbr""", true, true)]
	[InlineData("""records\item\equipmentweapon\sword\us_n_01.dbr""", true, true)]
	public void IsSuitableFor_OriginalPlayerOnlyTakesBaseGameItems(string baseItemId, bool immortalThronePlayer, bool expected)
		=> ItemActionRules.IsSuitableFor(new Item { BaseItemId = RecordId.Create(baseItemId) }, immortalThronePlayer).Should().Be(expected);

	[Theory]
	[InlineData(SackType.Equipment, true, true)]
	[InlineData(SackType.Equipment, false, false)]
	[InlineData(SackType.Player, true, false)]
	[InlineData(SackType.Trash, true, false)]
	[InlineData(SackType.Stash, true, false)]
	[InlineData(SackType.Vault, true, false)]
	public void IsReadOnly_PlayerReadonlyLocksTheEquipmentOnly(SackType sackType, bool playerReadonly, bool expected)
		=> ItemActionRules.IsReadOnly(sackType, playerReadonly).Should().Be(expected);

	private static ItemActions Key(char key, Item? item, bool readOnly = false, bool allowCopy = true, int selectionCount = 0)
		=> ItemActionRules.KeyAction(key, new ItemActionContext(item, selectionCount, false, readOnly, AllowItemEdit: true, allowCopy));

	[Theory]
	[InlineData('c', ItemActions.Copy)]
	[InlineData('d', ItemActions.Duplicate)]
	[InlineData('\b', ItemActions.Delete)]
	[InlineData('x', ItemActions.None)]
	public void KeyAction_MapsTheKeysToTheirAction(char key, ItemActions expected)
		=> Key(key, Sword()).Should().Be(expected);

	[Fact]
	public void KeyAction_ReadOnlyEquipment_BackspaceDoesNotDelete_CopyStillWorks()
	{
		Key('\b', Sword(), readOnly: true).Should().Be(ItemActions.None);
		Key('c', Sword(), readOnly: true).Should().Be(ItemActions.Copy);
	}

	[Fact]
	public void KeyAction_CopyDisabled_NoCopyNorDuplicate()
	{
		Key('c', Sword(), allowCopy: false).Should().Be(ItemActions.None);
		Key('d', Sword(), allowCopy: false).Should().Be(ItemActions.None);
	}

	[Fact]
	public void KeyAction_ActsOnTheItemUnderTheCursor_NotOnTheSelection()
	{
		// Copy is not offered for a selection, but the key copies the item under the cursor.
		Key('c', Sword(), selectionCount: 3).Should().Be(ItemActions.Copy);
		Key('\b', null, selectionCount: 3).Should().Be(ItemActions.None);
	}

	[Fact]
	public void Available_NothingUnderCursorNorSelected_OffersNothing()
		=> For(null).Should().Be(ItemActions.None);

	[Fact]
	public void Available_SingleItem_OffersItsActionsAndTheEnabledOptions()
	{
		For(Sword()).Should().Be(GroupActions | ItemActions.Properties);
		For(Sword(), allowCopy: true).Should().Be(GroupActions | ItemActions.Properties | ItemActions.Copy | ItemActions.Duplicate);
		For(Sword(), allowEdit: true).Should().Be(GroupActions | ItemActions.Properties | ItemActions.Edit);
	}

	[Fact]
	public void Available_ReadOnlySack_OnlyLooksAndCopies()
		=> For(Sword(socketed1: true), readOnly: true, allowEdit: true, allowCopy: true)
			.Should().Be(ItemActions.Properties | ItemActions.Copy | ItemActions.Duplicate);

	[Fact]
	public void Available_SeveralSelected_OffersOnlyTheGroupActions()
		=> For(Sword(), selectionCount: 3, allowEdit: true, allowCopy: true)
			.Should().Be(GroupActions | ItemActions.ClearSelection);

	[Fact]
	public void Available_SelectionOfTheItemAlone_ActsOnTheItem()
		=> For(Sword(), selectionCount: 1, itemIsTheSelection: true)
			.Should().Be(GroupActions | ItemActions.Properties | ItemActions.ClearSelection);

	[Fact]
	public void Available_SelectionWithoutItemUnderCursor_OffersTheGroupActions()
		=> For(null, selectionCount: 2).Should().Be(GroupActions | ItemActions.ClearSelection);

	[Theory]
	[InlineData(true, false, ItemActions.RemoveRelic)]
	[InlineData(false, true, ItemActions.RemoveRelic2)]
	[InlineData(true, true, ItemActions.RemoveRelic | ItemActions.RemoveRelic2)]
	public void Available_SocketedItem_RemovesEachFilledSlotWhenEditing(bool slot1, bool slot2, ItemActions expected)
	{
		const ItemActions relics = ItemActions.RemoveRelic | ItemActions.RemoveRelic2;

		(For(Sword(slot1, slot2), allowEdit: true) & relics).Should().Be(expected);
		(For(Sword(slot1, slot2)) & relics).Should().Be(ItemActions.None, "removing a relic edits the item");
	}

	[Theory]
	[InlineData(2, true)]
	[InlineData(1, false)]
	public void Available_Stack_SplitsOnlyAStackOfSeveral(int count, bool canSplit)
		=> For(Potions(count)).HasFlag(ItemActions.Split).Should().Be(canSplit);

	private static Item WithClass(string itemClass, int shards = 0, int completedLevel = 0)
	{
		var id = RecordId.Create("records/item.dbr");
		var record = new DBRecordCollection(id, itemClass);
		var cls = new Variable("Class", VariableDataType.StringVar, 1);
		cls[0] = itemClass;
		record.Set(cls);
		var level = new Variable("completedRelicLevel", VariableDataType.Integer, 1);
		level[0] = completedLevel;
		record.Set(level);
		return new Item { BaseItemId = id, baseItemInfo = new Info(record), Var1 = shards };
	}

	private const ItemActions SeedActions = ItemActions.Seed | ItemActions.ForceSeed;

	[Fact]
	public void EditActions_AnyItem_OffersTheSeedOnly()
		=> ItemActionRules.EditActions(Sword()).Should().Be(SeedActions);

	[Theory]
	[InlineData(Item.ICLASS_RELIC, 2, true)]
	[InlineData(Item.ICLASS_CHARM, 2, true)]
	[InlineData(Item.ICLASS_RELIC, 5, false)]
	public void EditActions_RelicOrCharm_CompletesOnlyAnIncompleteOne(string itemClass, int shards, bool canComplete)
		=> ItemActionRules.EditActions(WithClass(itemClass, shards, completedLevel: 5))
			.Should().Be(canComplete ? SeedActions | ItemActions.CompleteRelic : SeedActions);

	[Fact]
	public void EditActions_Formula_CraftsTheArtifact()
		=> ItemActionRules.EditActions(WithClass(Item.ICLASS_FORMULA))
			.Should().Be(SeedActions | ItemActions.CraftArtifact);
}
