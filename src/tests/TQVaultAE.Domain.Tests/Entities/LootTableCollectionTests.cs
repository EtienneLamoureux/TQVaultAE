using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

/// <summary>
/// Unit tests for LootTableCollection entity
/// </summary>
public class LootTableCollectionTests
{
	[Fact]
	public void LootTableCollection_TotalWeight_DefaultsToOne_WhenZero()
	{
		// Arrange
		var tableId = RecordId.Create("test/table");
		var data = new Dictionary<RecordId, (float Weight, LootRandomizerItem LootRandomizer)>();

		// Act
		var collection = new LootTableCollection(tableId, data);

		// Assert - TotalWeight should default to 1 when no data
		collection.TotalWeight.Should().Be(1.0f);
		collection.Length.Should().Be(0);
	}

	private static LootTableCollection CreateTable(params (string Id, float Weight)[] entries)
	{
		var data = new Dictionary<RecordId, (float Weight, LootRandomizerItem LootRandomizer)>();
		foreach (var (id, weight) in entries)
		{
			var recordId = RecordId.Create(id);
			data.Add(recordId, (weight, LootRandomizerItem.Default(recordId)));
		}

		return new LootTableCollection(RecordId.Create("test/table"), data);
	}

	[Fact]
	public void PickWeighted_EmptyTable_ReturnsNull()
	{
		var table = CreateTable();

		table.PickWeighted(0.5f).Should().BeNull();
	}

	[Theory]
	[InlineData(0f, "a")]
	[InlineData(0.25f, "a")]
	[InlineData(0.26f, "b")]
	[InlineData(0.75f, "b")]
	[InlineData(0.76f, "c")]
	[InlineData(1f, "c")]
	public void PickWeighted_RollFallsInCumulativeWeights_ReturnsThatEntry(float roll, string expected)
	{
		// weights 1, 2, 1 → 25%, 50%, 25%
		var table = CreateTable(("a", 1f), ("b", 2f), ("c", 1f));

		table.PickWeighted(roll).Should().Be(RecordId.Create(expected));
	}

	[Fact]
	public void PickWeighted_RollAboveTotal_ReturnsLastEntry()
	{
		var table = CreateTable(("a", 1f), ("b", 1f));

		table.PickWeighted(5f).Should().Be(RecordId.Create("b"));
	}

	[Fact]
	public void PickWeighted_AllWeightsZero_ReturnsLastEntryForPositiveRoll()
	{
		// TotalWeight falls back to 1, every WeightPercent is 0
		var table = CreateTable(("a", 0f), ("b", 0f), ("c", 0f));

		table.PickWeighted(0.1f).Should().Be(RecordId.Create("c"));
		table.PickWeighted(0f).Should().Be(RecordId.Create("a"));
	}
}
