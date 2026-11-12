using AwesomeAssertions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Tests.Domain;

/// <summary>
/// <see cref="ToFriendlyNameResult"/>: the completion line of a relic or charm socketed in an item.
/// </summary>
public class ToFriendlyNameResultTests
{
	private static Info Relic(string itemClass, int completedLevel)
	{
		var id = RecordId.Create("records/relic.dbr");
		var record = new DBRecordCollection(id, itemClass);
		var cls = new Variable("Class", VariableDataType.StringVar, 1);
		cls[0] = itemClass;
		record.Set(cls);
		var level = new Variable("completedRelicLevel", VariableDataType.Integer, 1);
		level[0] = completedLevel;
		record.Set(level);
		return new Info(record);
	}

	private static ToFriendlyNameResult For(Item item) => new(item)
	{
		AnimalPart = "Monster Charm",
		AnimalPartRatio = "{0} - {1} / {2}",
		RelicShard = "Relic",
		RelicRatio = "{0} - {1} / {2}",
	};

	[Theory]
	[InlineData("ItemCharm", "Monster Charm - 3 / 5")]
	[InlineData("ItemRelic", "Relic - 3 / 5")]
	public void Slot1_IncompleteSocketed_ShowsTheShardsNeeded(string itemClass, string expected)
	{
		// An incomplete relic has no completion bonus yet: the count needed comes from the relic itself.
		var item = new Item { RelicInfo = Relic(itemClass, 5), Var1 = 3 };

		For(item).RelicInfo1CompletionResolved.Should().Be(expected);
	}

	[Theory]
	[InlineData("ItemCharm", "Monster Charm - 2 / 5")]
	[InlineData("ItemRelic", "Relic - 2 / 5")]
	public void Slot2_IncompleteSocketed_ShowsTheShardsNeeded(string itemClass, string expected)
	{
		var item = new Item { Relic2Info = Relic(itemClass, 5), Var2 = 2 };

		For(item).RelicInfo2CompletionResolved.Should().Be(expected);
	}
}
