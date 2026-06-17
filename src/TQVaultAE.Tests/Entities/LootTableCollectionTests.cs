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

}
