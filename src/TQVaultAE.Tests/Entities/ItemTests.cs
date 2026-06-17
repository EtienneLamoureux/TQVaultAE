using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

/// <summary>
/// Unit tests for Item entity.
/// Tests simple properties that don't require complex database mocking.
/// </summary>
public class ItemTests
{
	#region Constructor Tests

	[Fact]
	public void Constructor_InitializesDefaultValues()
	{
		// Act
		var item = new Item();

		// Assert
		item.StackSize.Should().Be(1);
		item.itemScalePercent.Should().BeApproximately(1.00F, 0.001F);
	}

	#endregion

	#region HasPrefix and HasSuffix Tests

	[Fact]
	public void HasPrefix_WithEmptyPrefixID_ReturnsFalse()
	{
		// Arrange
		var item = new Item { prefixID = RecordId.Empty };

		// Act & Assert
		item.HasPrefix.Should().BeFalse();
	}

	[Fact]
	public void HasPrefix_WithValidPrefixID_ReturnsTrue()
	{
		// Arrange
		var item = new Item { prefixID = RecordId.Create("records/items/prefix_test") };

		// Act & Assert
		item.HasPrefix.Should().BeTrue();
	}

	[Fact]
	public void HasSuffix_WithEmptySuffixID_ReturnsFalse()
	{
		// Arrange
		var item = new Item { suffixID = RecordId.Empty };

		// Act & Assert
		item.HasSuffix.Should().BeFalse();
	}

	[Fact]
	public void HasSuffix_WithValidSuffixID_ReturnsTrue()
	{
		// Arrange
		var item = new Item { suffixID = RecordId.Create("records/items/suffix_test") };

		// Act & Assert
		item.HasSuffix.Should().BeTrue();
	}

	#endregion

	#region AcceptExtraRelic Tests

	[Fact]
	public void AcceptExtraRelic_WithoutSuffix_ReturnsFalse()
	{
		// Arrange
		var item = new Item { suffixID = RecordId.Empty };

		// Act & Assert
		item.AcceptExtraRelic.Should().BeFalse();
	}

	[Fact]
	public void AcceptExtraRelic_WithNormalSuffix_ReturnsFalse()
	{
		// Arrange - normal suffix does not end with RARE_EXTRARELIC_01.DBR
		var item = new Item { suffixID = RecordId.Create("records/items/normal_suffix_01.dbr") };

		// Act & Assert
		item.AcceptExtraRelic.Should().BeFalse();
	}

	[Theory]
	[InlineData("records/items/rare_extrarelic_01.dbr")]
	[InlineData("records\\XPACK\\rare_extrarelic_01.dbr")]
	[InlineData("RECORDS/ITEMS/RARE_EXTRARELIC_01.DBR")]
	public void AcceptExtraRelic_WithExtraRelicSuffix_ReturnsTrue(string suffixPath)
	{
		// Arrange - suffix ending with RARE_EXTRARELIC_01.DBR
		var item = new Item { suffixID = RecordId.Create(suffixPath) };

		// Act & Assert
		item.AcceptExtraRelic.Should().BeTrue();
	}

	[Fact]
	public void AcceptExtraRelic_CaseInsensitive_ReturnsTrue()
	{
		// Arrange - case insensitive check
		var item = new Item { suffixID = RecordId.Create("records/items/rare_extrarelic_01.DBR") };

		// Act & Assert
		item.AcceptExtraRelic.Should().BeTrue();
	}

	#endregion

	#region HasNumber Tests (Deferred - requires baseItemInfo)

	// Note: HasNumber property depends on IsPotion and IsRelicOrCharm
	// which require baseItemInfo to be set. These tests require
	// mocking the database record which is complex.

	#endregion

	#region BaseItemId Tests

	[Fact]
	public void BaseItemId_CanBeSetAndRetrieved()
	{
		// Arrange
		var item = new Item();
		var recordId = RecordId.Create("records/items/sword");

		// Act
		item.BaseItemId = recordId;

		// Assert
		item.BaseItemId.Should().Be(recordId);
	}

	#endregion

	#region IsModified Tests

	[Fact]
	public void IsModified_DefaultValue_IsFalse()
	{
		// Arrange
		var item = new Item();

		// Act & Assert
		item.IsModified.Should().BeFalse();
	}

	[Fact]
	public void IsModified_CanBeSetToTrue()
	{
		// Arrange
		var item = new Item();

		// Act
		item.IsModified = true;

		// Assert
		item.IsModified.Should().BeTrue();
	}

	[Fact]
	public void IsModified_CanBeSetToFalse()
	{
		// Arrange
		var item = new Item { IsModified = true };

		// Act
		item.IsModified = false;

		// Assert
		item.IsModified.Should().BeFalse();
	}

	#endregion

	#region StackSize Tests

	[Fact]
	public void StackSize_DefaultValue_IsOne()
	{
		// Arrange
		var item = new Item();

		// Act & Assert
		item.StackSize.Should().Be(1);
	}

	[Fact]
	public void StackSize_CanBeSetAndRetrieved()
	{
		// Arrange
		var item = new Item();

		// Act
		item.StackSize = 10;

		// Assert
		item.StackSize.Should().Be(10);
	}

	#endregion

	#region RecordId Assignment Tests

	[Fact]
	public void PrefixID_CanBeSetAndRetrieved()
	{
		// Arrange
		var item = new Item();
		var recordId = RecordId.Create("records/items/prefix");

		// Act
		item.prefixID = recordId;

		// Assert
		item.prefixID.Should().Be(recordId);
		item.HasPrefix.Should().BeTrue();
	}

	[Fact]
	public void SuffixID_CanBeSetAndRetrieved()
	{
		// Arrange
		var item = new Item();
		var recordId = RecordId.Create("records/items/suffix");

		// Act
		item.suffixID = recordId;

		// Assert
		item.suffixID.Should().Be(recordId);
		item.HasSuffix.Should().BeTrue();
	}

	[Fact]
	public void RelicID_CanBeSetAndRetrieved()
	{
		// Arrange
		var item = new Item();
		var recordId = RecordId.Create("records/items/relics/aegisofathena_01");

		// Act
		item.relicID = recordId;

		// Assert
		item.relicID.Should().Be(recordId);
	}

	[Fact]
	public void Relic2ID_CanBeSetAndRetrieved()
	{
		// Arrange
		var item = new Item();
		var recordId = RecordId.Create("records/items/relics/aegisofathena_02");

		// Act
		item.relic2ID = recordId;

		// Assert
		item.relic2ID.Should().Be(recordId);
	}

	#endregion

	#region Constants Tests

	[Fact]
	public void WeaponSlotIndicator_IsNegativeThree()
	{
		// Assert
		Item.WeaponSlotIndicator.Should().Be(-3);
	}

	[Fact]
	public void Var2Default_IsCorrectValue()
	{
		// Assert
		Item.var2Default.Should().Be(2035248);
	}

	#endregion
}
