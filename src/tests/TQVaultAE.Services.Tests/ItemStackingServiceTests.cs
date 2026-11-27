using System.Drawing;
using AwesomeAssertions;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// <see cref="ItemStackingService"/>: an item dropped on the same potion, scroll, relic or charm.
/// </summary>
public class ItemStackingServiceTests
{
	private const string RelicId = "records/relic.dbr";
	private static readonly Point DropLocation = new(3, 4);

	private readonly Mock<IItemProvider> _itemProvider = new();
	private readonly SackCollection _sack = new();
	private readonly ItemStackingService _service;

	public ItemStackingServiceTests()
	{
		this._service = new ItemStackingService(this._itemProvider.Object);
	}

	#region Helpers

	private static Item MakeItem(string id, string itemClass, int completedRelicLevel = 0)
	{
		var record = new DBRecordCollection(RecordId.Create(id), itemClass);
		var cls = new Variable("Class", VariableDataType.StringVar, 1);
		cls[0] = itemClass;
		record.Set(cls);
		var level = new Variable("completedRelicLevel", VariableDataType.Integer, 1);
		level[0] = completedRelicLevel;
		record.Set(level);
		return new Item { BaseItemId = RecordId.Create(id), baseItemInfo = new Info(record) };
	}

	private static Item Potions(int count, string id = "records/potion.dbr")
	{
		var potions = MakeItem(id, Item.ICLASS_POTIONHEALTH);
		potions.StackSize = count;
		return potions;
	}

	private static Item Relic(int shards, string id = RelicId, string itemClass = Item.ICLASS_RELIC)
	{
		var relic = MakeItem(id, itemClass, completedRelicLevel: 5);
		relic.Var1 = shards;
		return relic;
	}

	/// <summary>An item of the sack, at its own place.</summary>
	private Item InSack(Item item)
	{
		item.Location = new Point(3, 4);
		this._sack.AddItem(item);
		this._sack.IsModified = false;
		return item;
	}

	private void BonusTableOf(Item item, RecordId bonusId)
		=> this._itemProvider.Setup(p => p.BonusTableRelicOrArtifact(item))
			.Returns(new LootTableCollection(RecordId.Create("table"), new Dictionary<RecordId, (float, LootRandomizerItem)>
			{
				[bonusId] = (1f, LootRandomizerItem.Default(bonusId)),
			}));

	#endregion

	[Fact]
	public void TryStack_Potions_JoinTheStackInTheSack()
	{
		// Arrange
		var dragged = Potions(3);
		var under = this.InSack(Potions(2));

		// Act
		var result = this._service.TryStack(dragged, under, this._sack, DropLocation, disableAutoStacking: false);

		// Assert
		result.Stacked.Should().BeTrue();
		result.PickedUpItem.Should().BeNull("nothing is picked up after a stack");
		under.StackSize.Should().Be(5);
		under.IsModified.Should().BeTrue();
		this._sack.IsModified.Should().BeTrue();
		this._sack.Count.Should().Be(1);
	}

	[Fact]
	public void TryStack_IncompleteRelics_MergeWithoutBonus()
	{
		var dragged = Relic(1);
		var under = this.InSack(Relic(2));

		var result = this._service.TryStack(dragged, under, this._sack, DropLocation, false);

		result.Stacked.Should().BeTrue();
		result.DroppedItem.Should().BeSameAs(dragged);
		result.PickedUpItem.Should().BeNull();
		under.Var1.Should().Be(3);
		under.IsRelicComplete.Should().BeFalse();
		this._sack.IsModified.Should().BeTrue();
		this._itemProvider.Verify(p => p.BonusTableRelicOrArtifact(It.IsAny<Item>()), Times.Never);
		this._itemProvider.Verify(p => p.GetDBData(It.IsAny<Item>()), Times.Never);
	}

	[Fact]
	public void TryStack_ShardsCompleteTheRelic_RollTheBonus()
	{
		// Arrange
		var dragged = Relic(2);
		var under = this.InSack(Relic(3));
		var bonus = RecordId.Create("records/bonus/b.dbr");
		this.BonusTableOf(under, bonus);

		// Act
		var result = this._service.TryStack(dragged, under, this._sack, DropLocation, false);

		// Assert
		result.Stacked.Should().BeTrue();
		result.PickedUpItem.Should().BeNull("every shard was used");
		under.IsRelicComplete.Should().BeTrue();
		under.RelicBonusId.Should().Be(bonus);
		this._itemProvider.Verify(p => p.GetDBData(under), Times.Once);
	}

	[Fact]
	public void TryStack_ShardsToSpare_CompletedRelicStaysAndTheRestIsDragged()
	{
		// Arrange
		var dragged = Relic(4);
		var under = this.InSack(Relic(3));
		this.BonusTableOf(under, RecordId.Create("records/bonus/b.dbr"));

		// Act
		var result = this._service.TryStack(dragged, under, this._sack, DropLocation, false);

		// Assert: the references are swapped, as the drop code expects.
		result.Stacked.Should().BeTrue();
		result.DroppedItem.Should().BeSameAs(under);
		result.PickedUpItem.Should().BeSameAs(dragged);
		under.Var1.Should().Be(5);
		dragged.Var1.Should().Be(2, "2 of the 4 shards completed the relic");
		dragged.IsModified.Should().BeTrue();
		under.Location.Should().Be(DropLocation);
		this._sack.Count(i => ReferenceEquals(i, under)).Should().Be(1, "the completed relic must not be listed (and saved) twice");
		this._sack.IsModified.Should().BeTrue();
	}

	[Fact]
	public void TryStack_RelicAndCharm_DifferentKinds_AreNotStacked()
	{
		var dragged = Relic(1, "records/charm.dbr", Item.ICLASS_CHARM);
		var under = this.InSack(Relic(1));

		var result = this._service.TryStack(dragged, under, this._sack, DropLocation, false);

		result.Should().Be(ItemStackResult.NotStacked(dragged, under));
		under.Var1.Should().Be(1);
		this._sack.IsModified.Should().BeFalse();
	}

	[Fact]
	public void TryStack_DifferentPotions_AreNotStacked()
	{
		var dragged = Potions(3, "records/potion_mana.dbr");
		var under = this.InSack(Potions(2));

		var result = this._service.TryStack(dragged, under, this._sack, DropLocation, false);

		result.Stacked.Should().BeFalse();
		result.PickedUpItem.Should().BeSameAs(under, "a regular drop picks up the item under it");
		under.StackSize.Should().Be(2);
	}

	[Fact]
	public void TryStack_CompletedRelic_IsNotStacked()
	{
		var dragged = Relic(1);
		var under = this.InSack(Relic(5));

		this._service.TryStack(dragged, under, this._sack, DropLocation, false).Stacked.Should().BeFalse();
		dragged.Var1.Should().Be(1);
	}

	[Fact]
	public void TryStack_AutoStackingDisabled_IsNotStacked()
	{
		var dragged = Potions(3);
		var under = this.InSack(Potions(2));

		this._service.TryStack(dragged, under, this._sack, DropLocation, disableAutoStacking: true).Stacked.Should().BeFalse();
		under.StackSize.Should().Be(2);
	}

	[Fact]
	public void TryStack_NothingUnder_IsNotStacked()
	{
		var dragged = Potions(3);

		var result = this._service.TryStack(dragged, null, this._sack, DropLocation, false);

		result.Stacked.Should().BeFalse();
		result.DroppedItem.Should().BeSameAs(dragged);
	}
}
