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
		relicID = socketed1 ? RecordId.Create(@"records\item\relics\01_act1_aegisofathena.dbr") : RecordId.Empty,
		relic2ID = socketed2 ? RecordId.Create(@"records\item\relics\01_act1_aegisofathena.dbr") : RecordId.Empty,
	};

	private static Item Potions(int count)
	{
		var potion = new Item { BaseItemId = RecordId.Create(@"records\item\oneshot\potionhealth01.dbr") };
		potion.Number = count;
		return potion;
	}

	/// <summary>The actions offered, without <see cref="ItemActions.ChangeMod"/> (tested on its own).</summary>
	private static ItemActions For(Item? item, int selectionCount = 0, bool itemIsTheSelection = false
		, bool readOnly = false, bool allowEdit = false, bool allowCopy = false)
		=> ItemActionRules.Available(new ItemActionContext(item, selectionCount, itemIsTheSelection, readOnly, allowEdit, allowCopy))
			& ~ItemActions.ChangeMod;

	[Theory]
	[InlineData(true, 0, false, true)]    // the item under the cursor
	[InlineData(true, 3, false, true)]    // even with a selection: the menu acts on the item under the cursor
	[InlineData(false, 3, false, false)]  // a selection alone
	[InlineData(true, 0, true, false)]    // read-only equipment
	public void Available_ChangeMod_ForTheItemUnderTheCursorOfAWritableVault(bool item, int selectionCount, bool readOnly, bool expected)
	{
		var sword = Sword();
		sword.InVault = true;

		ItemActionRules.Available(new ItemActionContext(item ? sword : null, selectionCount, false, readOnly, false, false))
			.HasFlag(ItemActions.ChangeMod).Should().Be(expected);
	}

	[Fact]
	public void Available_ChangeMod_NotForACharactersItem()
		// A character's files cannot keep the choice: the next read would lose it.
		=> ItemActionRules.Available(new ItemActionContext(Sword(), 0, false, false, false, false))
			.HasFlag(ItemActions.ChangeMod).Should().BeFalse();

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

	[Theory]
	[InlineData(SackType.Player, false)]
	[InlineData(SackType.Equipment, false)]
	[InlineData(SackType.Stash, false)]
	[InlineData(SackType.Equipment, true)]
	public void IsReadOnly_ASackOfAReadOnlyCharacterNeverChanges(SackType sackType, bool playerReadonly)
		=> ItemActionRules.IsReadOnly(sackType, playerReadonly, sackReadOnly: true).Should().BeTrue();

	[Fact]
	public void Available_ASackOfAReadOnlyCharacterOnlyOffersTheReadingActions()
	{
		var item = new Item { BaseItemId = RecordId.Create("""records\item\equipmentweapon\sword\us_n_01.dbr""") };
		var context = new ItemActionContext(item, 0, false
			, ReadOnly: ItemActionRules.IsReadOnly(SackType.Player, false, sackReadOnly: true), AllowItemEdit: true, AllowItemCopy: true);

		ItemActionRules.Available(context).Should().Be(ItemActions.Properties | ItemActions.Compare | ItemActions.Copy | ItemActions.Duplicate);
	}

	private static ItemActions Key(ItemActions action, Item? item, bool readOnly = false, bool allowCopy = true, int selectionCount = 0)
		=> ItemActionRules.KeyAction(action, new ItemActionContext(item, selectionCount, false, readOnly, AllowItemEdit: true, allowCopy));

	[Theory]
	[InlineData(ItemActions.Copy, ItemActions.Copy)]
	[InlineData(ItemActions.Duplicate, ItemActions.Duplicate)]
	[InlineData(ItemActions.Delete, ItemActions.Delete)]
	[InlineData(ItemActions.Properties, ItemActions.None)]
	[InlineData(ItemActions.Copy | ItemActions.Delete, ItemActions.None)]
	public void KeyAction_OnlyCopyDuplicateOrDeleteAlone(ItemActions action, ItemActions expected)
		=> Key(action, Sword()).Should().Be(expected);

	[Fact]
	public void KeyAction_ReadOnlyEquipment_BackspaceDoesNotDelete_CopyStillWorks()
	{
		Key(ItemActions.Delete, Sword(), readOnly: true).Should().Be(ItemActions.None);
		Key(ItemActions.Copy, Sword(), readOnly: true).Should().Be(ItemActions.Copy);
	}

	[Fact]
	public void KeyAction_CopyDisabled_NoCopyNorDuplicate()
	{
		Key(ItemActions.Copy, Sword(), allowCopy: false).Should().Be(ItemActions.None);
		Key(ItemActions.Duplicate, Sword(), allowCopy: false).Should().Be(ItemActions.None);
	}

	[Fact]
	public void KeyAction_ActsOnTheItemUnderTheCursor_NotOnTheSelection()
	{
		// Copy is not offered for a selection, but the key copies the item under the cursor.
		Key(ItemActions.Copy, Sword(), selectionCount: 3).Should().Be(ItemActions.Copy);
		Key(ItemActions.Delete, null, selectionCount: 3).Should().Be(ItemActions.None);
	}

	[Fact]
	public void Available_NothingUnderCursorNorSelected_OffersNothing()
		=> For(null).Should().Be(ItemActions.None);

	[Fact]
	public void Available_SingleItem_OffersItsActionsAndTheEnabledOptions()
	{
		const ItemActions single = GroupActions | ItemActions.Properties | ItemActions.Compare;
		For(Sword()).Should().Be(single);
		For(Sword(), allowCopy: true).Should().Be(single | ItemActions.Copy | ItemActions.Duplicate);
		For(Sword(), allowEdit: true).Should().Be(single | ItemActions.Edit);
	}

	[Fact]
	public void Available_ReadOnlySack_OnlyLooksAndCopies()
		=> For(Sword(socketed1: true), readOnly: true, allowEdit: true, allowCopy: true)
			.Should().Be(ItemActions.Properties | ItemActions.Compare | ItemActions.Copy | ItemActions.Duplicate);

	[Fact]
	public void Available_SeveralSelected_OffersOnlyTheGroupActions()
		=> For(Sword(), selectionCount: 3, allowEdit: true, allowCopy: true)
			.Should().Be(GroupActions | ItemActions.ClearSelection);

	[Fact]
	public void Available_SelectionOfTheItemAlone_ActsOnTheItem()
		=> For(Sword(), selectionCount: 1, itemIsTheSelection: true)
			.Should().Be(GroupActions | ItemActions.Properties | ItemActions.Compare | ItemActions.ClearSelection);

	[Fact]
	public void Available_SelectionWithoutItemUnderCursor_OffersTheGroupActions()
		=> For(null, selectionCount: 2).Should().Be(GroupActions | ItemActions.ClearSelection);

	[Theory]
	[InlineData(true, 0, true)]    // the item under the cursor starts the compare selection
	[InlineData(true, 1, true)]    // a selection of that item only
	[InlineData(true, 2, false)]   // a selection of several items: group actions only
	[InlineData(false, 2, false)]  // no item under the cursor
	public void Available_Compare_ForTheItemUnderTheCursor(bool item, int selectionCount, bool canCompare)
		=> For(item ? Sword() : null, selectionCount, itemIsTheSelection: item && selectionCount == 1)
			.HasFlag(ItemActions.Compare).Should().Be(canCompare);

	[Fact]
	public void Available_Compare_AlsoInAReadOnlySack()
		=> For(Sword(), readOnly: true).HasFlag(ItemActions.Compare).Should().BeTrue();

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
