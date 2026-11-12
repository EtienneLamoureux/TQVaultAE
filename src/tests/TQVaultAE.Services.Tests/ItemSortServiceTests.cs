using System.Drawing;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// <see cref="ItemSortService"/>: the order of each sort algorithm of the bag sort button, and the merge of relic and
/// charm shards before the sort.
/// </summary>
public class ItemSortServiceTests
{
	private readonly Mock<IItemProvider> _itemProvider = new();
	private readonly Mock<IItemDatabaseService> _itemDatabase = new();
	private readonly Dictionary<Item, int> _levels = new(ReferenceEqualityComparer.Instance);
	private readonly ItemSortService _service;

	public ItemSortServiceTests()
	{
		this._itemProvider.Setup(p => p.GetRequirementInfo(It.IsAny<Item>()))
			.Returns((Item item) => new RequirementInfo { Item = item, Lvl = this._levels.TryGetValue(item, out var level) ? level : null });

		this._service = new ItemSortService(
			new ItemMovementService(new Mock<ILogger<ItemMovementService>>().Object)
			, new ItemStackingService(this._itemProvider.Object)
			, this._itemProvider.Object
			, this._itemDatabase.Object);
	}

	#region Helpers

	/// <summary>An item of a record named <paramref name="name"/>, at a cell of the sack.</summary>
	private Item Item(string name, string itemClass, int x, int y, int width = 1, int height = 1, string classification = "", int level = 0, int completedRelicLevel = 0)
	{
		var id = RecordId.Create($"records/item/{name}.dbr");
		var record = new DBRecordCollection(id, itemClass);
		record.Set(StringVariable("Class", itemClass));
		record.Set(StringVariable("itemClassification", classification));
		var completion = new Variable("completedRelicLevel", VariableDataType.Integer, 1);
		completion[0] = completedRelicLevel;
		record.Set(completion);

		var item = new Item
		{
			BaseItemId = id,
			baseItemInfo = new Info(record),
			PositionX = x,
			PositionY = y,
			Width = width,
			Height = height,
		};
		if (level > 0)
			this._levels[item] = level;
		return item;
	}

	private static Variable StringVariable(string name, string value)
	{
		var variable = new Variable(name, VariableDataType.StringVar, 1);
		variable[0] = value;
		return variable;
	}

	private Item Helm(string name, int x, int level) => this.Item(name, Domain.Entities.Item.ICLASS_HEAD, x, 0, level: level);

	/// <summary>A relic of five shards to completion, holding <paramref name="shards"/>.</summary>
	private Item Relic(string name, int shards, int x, int y = 0)
	{
		var relic = this.Item(name, Domain.Entities.Item.ICLASS_RELIC, x, y, completedRelicLevel: 5);
		relic.Var1 = shards;
		return relic;
	}

	private static SackCollection Sack(params Item[] items)
	{
		var sack = new SackCollection();
		foreach (var item in items)
			sack.AddItem(item);
		sack.IsModified = false;
		return sack;
	}

	/// <summary>The items left to right, top to bottom, by record name.</summary>
	private static string Layout(SackCollection sack)
		=> string.Join(" ", sack.OrderBy(i => i.PositionY).ThenBy(i => i.PositionX).Select(i => $"{Name(i)}@{i.PositionX},{i.PositionY}"));

	private static string Name(Item item)
	{
		var file = item.BaseItemId.Raw.Split('/', '\\')[^1];
		return file[..file.LastIndexOf('.')];
	}

	private void BonusTableOf(Item item, RecordId bonusId)
		=> this._itemProvider.Setup(p => p.BonusTableRelicOrArtifact(item))
			.Returns(new LootTableCollection(RecordId.Create("table"), new Dictionary<RecordId, (float, LootRandomizerItem)>
			{
				[bonusId] = (1f, LootRandomizerItem.Default(bonusId)),
			}));

	#endregion

	#region Orders

	[Fact]
	public void Normal_LargestFirstThenByRecord()
	{
		// Arrange: same group, so the record breaks the tie between the small ones
		var b = this.Item("b", Domain.Entities.Item.ICLASS_RING, 3, 3);
		var big = this.Item("big", Domain.Entities.Item.ICLASS_RING, 2, 2, 2, 2);
		var a = this.Item("a", Domain.Entities.Item.ICLASS_RING, 0, 3);
		var sack = Sack(b, big, a);

		// Act
		var result = this._service.Sort(sack, new Size(4, 4), vertical: false, SackSortAlgorithm.Normal, mergeRelics: false);

		// Assert
		result.Sorted.Should().BeTrue();
		Layout(sack).Should().Be("big@0,0 a@2,0 b@3,0");
		sack.IsModified.Should().BeTrue();
	}

	[Fact]
	public void Level_LowestRequiredLevelFirst()
	{
		var sack = Sack(this.Helm("h30", 0, 30), this.Helm("h10", 1, 10), this.Helm("h20", 2, 20));

		this._service.Sort(sack, new Size(3, 1), false, SackSortAlgorithm.Level, false).Sorted.Should().BeTrue();

		Layout(sack).Should().Be("h10@0,0 h20@1,0 h30@2,0");
	}

	[Fact]
	public void Level_SameLevel_NormalOrderBreaksTheTie()
	{
		// The level comes first, then the larger item.
		var small = this.Item("small", Domain.Entities.Item.ICLASS_HEAD, 0, 0, level: 5);
		var tall = this.Item("tall", Domain.Entities.Item.ICLASS_HEAD, 1, 0, 1, 2, level: 5);
		var low = this.Item("low", Domain.Entities.Item.ICLASS_HEAD, 2, 0, level: 1);
		var sack = Sack(small, tall, low);

		this._service.Sort(sack, new Size(3, 2), false, SackSortAlgorithm.Level, false).Sorted.Should().BeTrue();

		Layout(sack).Should().Be("low@0,0 tall@1,0 small@2,0");
	}

	[Fact]
	public void Type_GroupedByItemTypeThenLevel()
	{
		var ring = this.Item("ring", Domain.Entities.Item.ICLASS_RING, 0, 0, level: 1);
		var helmHigh = this.Item("helmHigh", Domain.Entities.Item.ICLASS_HEAD, 1, 0, level: 40);
		var sword = this.Item("sword", Domain.Entities.Item.ICLASS_SWORD, 2, 0, level: 10);
		var helmLow = this.Item("helmLow", Domain.Entities.Item.ICLASS_HEAD, 3, 0, level: 2);
		var sack = Sack(ring, helmHigh, sword, helmLow);

		this._service.Sort(sack, new Size(4, 1), false, SackSortAlgorithm.Type, false).Sorted.Should().BeTrue();

		// Weapons, then armour, then jewellery: the order of the normal sort's groups.
		Layout(sack).Should().Be("sword@0,0 helmLow@1,0 helmHigh@2,0 ring@3,0");
	}

	[Fact]
	public void Rarity_RarestGearFirstThenTheRest()
	{
		var mundane = this.Item("mundane", Domain.Entities.Item.ICLASS_HEAD, 0, 0);
		var relic = this.Relic("relic", 1, 1);
		var epic = this.Item("epic", Domain.Entities.Item.ICLASS_HEAD, 2, 0, classification: "Epic");
		var legendary = this.Item("legendary", Domain.Entities.Item.ICLASS_HEAD, 3, 0, classification: "Legendary");
		var rare = this.Item("rare", Domain.Entities.Item.ICLASS_HEAD, 4, 0, classification: "Rare");
		var sack = Sack(mundane, relic, epic, legendary, rare);

		this._service.Sort(sack, new Size(5, 1), false, SackSortAlgorithm.Rarity, false).Sorted.Should().BeTrue();

		Layout(sack).Should().Be("legendary@0,0 epic@1,0 rare@2,0 mundane@3,0 relic@4,0");
	}

	[Theory]
	[InlineData(SackSortAlgorithm.Level)]
	[InlineData(SackSortAlgorithm.Type)]
	[InlineData(SackSortAlgorithm.Rarity)]
	public void EveryAlgorithm_ItemDoesNotFit_SackLeftAsItWas(SackSortAlgorithm algorithm)
	{
		// Arrange: in level order the small item comes first and leaves no room for the large one
		var small = this.Item("small", Domain.Entities.Item.ICLASS_RING, 1, 1, level: 1, classification: "Legendary");
		var big = this.Item("big", Domain.Entities.Item.ICLASS_SWORD, 0, 0, 1, 2, level: 50);
		var sack = Sack(small, big);

		// Act
		var result = this._service.Sort(sack, new Size(1, 2), false, algorithm, false);

		// Assert
		result.Sorted.Should().BeFalse();
		Layout(sack).Should().Be("big@0,0 small@1,1");
		sack.IsModified.Should().BeFalse();
	}

	#endregion

	#region Relic merge

	[Fact]
	public void Merge_ShardsOfTheSameRelic_StackUpToCompletionWithItsBonus()
	{
		// Arrange: 3 + 4 + 2 shards of a five-shard relic; the fullest is completed first
		var second = this.Relic("relic", 3, 0);
		var first = this.Relic("relic", 4, 1);
		var third = this.Relic("relic", 2, 2);
		var other = this.Relic("other", 1, 3);
		var bonus = RecordId.Create("records/bonus/b.dbr");
		this.BonusTableOf(first, bonus);
		var sack = Sack(first, second, third, other);

		// Act
		var result = this._service.Sort(sack, new Size(4, 1), false, SackSortAlgorithm.Normal, mergeRelics: true);

		// Assert: one complete relic with its rolled bonus, one with the 4 shards left, the emptied one gone
		result.Sorted.Should().BeTrue();
		first.IsRelicComplete.Should().BeTrue();
		first.RelicBonusId.Should().Be(bonus);
		second.Var1.Should().Be(4, "one of its shards completed the first relic, the third relic's two joined it");
		sack.Should().NotContain(third);
		sack.Should().HaveCount(3);
		other.Var1.Should().Be(1, "a shard of another relic stays alone");
		result.Removed.Should().Equal(third);
		result.Changed.Should().HaveCount(2).And.Contain(first).And.Contain(second);
		this._itemDatabase.Verify(d => d.RemoveItemFromDatabase(third), Times.Once);
		this._itemDatabase.Verify(d => d.RefreshItems(It.Is<IReadOnlyCollection<Item>>(c => c.Count == 2 && c.Contains(first) && c.Contains(second))), Times.Once);
		this._itemProvider.Verify(p => p.InvalidateFriendlyNamesCache(It.IsAny<IEnumerable<Item>>()), Times.Once);
	}

	[Fact]
	public void Merge_Off_ShardsLeftAsTheyAre()
	{
		var first = this.Relic("relic", 3, 0);
		var second = this.Relic("relic", 3, 1);
		var sack = Sack(first, second);

		var result = this._service.Sort(sack, new Size(2, 1), false, SackSortAlgorithm.Normal, mergeRelics: false);

		result.Sorted.Should().BeTrue();
		sack.Should().HaveCount(2);
		first.Var1.Should().Be(3);
		second.Var1.Should().Be(3);
		this._itemDatabase.VerifyNoOtherCalls();
	}

	[Fact]
	public void Merge_CompleteRelicsAndCharmsOfAnotherRecord_Untouched()
	{
		var complete = this.Relic("relic", 5, 0);
		var shard = this.Relic("relic", 2, 1);
		var charm = this.Item("charm", Domain.Entities.Item.ICLASS_CHARM, 2, 0, completedRelicLevel: 5);
		charm.Var1 = 2;
		var sack = Sack(complete, shard, charm);

		var result = this._service.Sort(sack, new Size(3, 1), false, SackSortAlgorithm.Normal, mergeRelics: true);

		result.Sorted.Should().BeTrue();
		result.Removed.Should().BeEmpty();
		result.Changed.Should().BeEmpty();
		sack.Should().HaveCount(3);
		shard.Var1.Should().Be(2);
		charm.Var1.Should().Be(2);
	}

	[Fact]
	public void Merge_ThenItemDoesNotFit_EverythingPutBack()
	{
		// Arrange: a 2x2 item fills the sack, so the shard left after the merge does not fit
		var big = this.Item("big", Domain.Entities.Item.ICLASS_SWORD, 0, 0, 2, 2);
		var first = this.Relic("relic", 3, 2, 0);
		var second = this.Relic("relic", 3, 3, 0);
		var bonus = RecordId.Create("records/bonus/b.dbr");
		this.BonusTableOf(first, bonus);
		var sack = Sack(big, first, second);
		second.IsModified = true;

		// Act
		var result = this._service.Sort(sack, new Size(2, 2), false, SackSortAlgorithm.Normal, mergeRelics: true);

		// Assert: the same items in the same order and place, the shards and bonus as they were
		result.Sorted.Should().BeFalse();
		sack.Should().Equal(big, first, second);
		Layout(sack).Should().Be("big@0,0 relic@2,0 relic@3,0");
		first.Var1.Should().Be(3);
		second.Var1.Should().Be(3);
		first.RelicBonusId.Should().Be(RecordId.Empty, "the rolled bonus goes with the undone completion");
		first.IsModified.Should().BeFalse();
		second.IsModified.Should().BeTrue();
		sack.IsModified.Should().BeFalse();
		this._itemProvider.Verify(p => p.GetDBData(first), Times.Exactly(2), "read once completed, once put back");
		this._itemDatabase.VerifyNoOtherCalls();
	}

	#endregion
}
