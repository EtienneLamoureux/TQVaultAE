using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

/// <summary>
/// Unit tests for LootRandomizerItem entity
/// </summary>
public class LootRandomizerItemTests
{
	[Fact]
	public void LootRandomizerItem_TranslationTagIsEmpty_WhenTagIsEmpty()
	{
		// Arrange
		var recordId = RecordId.Create("test/loot");
		var item = LootRandomizerItem.Default(recordId);

		// Act & Assert
		item.TranslationTagIsEmpty.Should().BeTrue();
	}

	[Fact]
	public void LootRandomizerItem_WithTag_TranslationTagIsEmptyIsFalse()
	{
		// Arrange
		var recordId = RecordId.Create("test/loot");
		var item = new LootRandomizerItem(
			recordId,
			"tag123",
			100,
			5,
			"class",
			"description",
			"translation"
		);

		// Act & Assert
		item.TranslationTagIsEmpty.Should().BeFalse();
	}

}
