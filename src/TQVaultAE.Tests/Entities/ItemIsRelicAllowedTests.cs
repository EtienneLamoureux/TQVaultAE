using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

/// <summary>
/// Unit tests for Item.IsRelicAllowed and Item.TryGetAllowedGearTypes methods.
/// These methods depend on the static RelicAndCharmMap and RecordId lookups.
/// </summary>
public class ItemIsRelicAllowedTests
{
	#region TryGetAllowedGearTypes(RecordId) Tests

	[Fact]
	public void TryGetAllowedGearTypes_WithEmptyRecordId_ReturnsFalse()
	{
		// Act
		var result = Item.TryGetAllowedGearTypes(RecordId.Empty, out var types);

		// Assert
		result.Should().BeFalse();
		types.Should().Be(GearType.Undefined);
	}

	[Fact]
	public void TryGetAllowedGearTypes_WithAegisRelic_ReturnsShieldGearType()
	{
		// Arrange - Aegis of Athena is a shield relic
		var relicId = RelicAndCharm.AEGISOFATHENA_ACT1_01.GetRecordId();

		// Act
		var result = Item.TryGetAllowedGearTypes(relicId, out var types);

		// Assert
		result.Should().BeTrue();
		types.Should().Be(GearType.Shield);
	}

	[Fact]
	public void TryGetAllowedGearTypes_WithAmunRaGloryRelic_ReturnsAllArmorGearType()
	{
		// Arrange - Amun-Ra's Glory is an armor relic
		var relicId = RelicAndCharm.AMUNRASGLORY_ACT2_01.GetRecordId();

		// Act
		var result = Item.TryGetAllowedGearTypes(relicId, out var types);

		// Assert
		result.Should().BeTrue();
		types.Should().Be(GearType.AllArmor);
	}

	[Fact]
	public void TryGetAllowedGearTypes_WithAnkhOfIsis_ReturnsJewelleryGearType()
	{
		// Arrange - Ankh of Isis is a jewellery relic (rings and amulets)
		var relicId = RelicAndCharm.ANKHOFISIS_ACT2_01.GetRecordId();

		// Act
		var result = Item.TryGetAllowedGearTypes(relicId, out var types);

		// Assert
		result.Should().BeTrue();
		types.Should().Be(GearType.Jewellery);
	}

	[Fact]
	public void TryGetAllowedGearTypes_WithChaosEssence_ReturnsHeadGearType()
	{
		// Arrange - Chaos Essence is a head armor relic
		var relicId = RelicAndCharm.HCDUNGEON_ESSENCEOFCHAOS_X4_03.GetRecordId();

		// Act
		var result = Item.TryGetAllowedGearTypes(relicId, out var types);

		// Assert
		result.Should().BeTrue();
		types.Should().Be(GearType.Head);
	}

	[Fact]
	public void TryGetAllowedGearTypes_AllActLevelsOfAegis_ReturnShield()
	{
		// Arrange - All 3 act levels of Aegis should be Shield
		var act1Id = RelicAndCharm.AEGISOFATHENA_ACT1_01.GetRecordId();
		var act2Id = RelicAndCharm.AEGISOFATHENA_ACT1_02.GetRecordId();
		var act3Id = RelicAndCharm.AEGISOFATHENA_ACT1_03.GetRecordId();

		// Act & Assert
		Item.TryGetAllowedGearTypes(act1Id, out var act1Type).Should().BeTrue();
		Item.TryGetAllowedGearTypes(act2Id, out var act2Type).Should().BeTrue();
		Item.TryGetAllowedGearTypes(act3Id, out var act3Type).Should().BeTrue();

		act1Type.Should().Be(GearType.Shield);
		act2Type.Should().Be(GearType.Shield);
		act3Type.Should().Be(GearType.Shield);
	}

	[Fact]
	public void TryGetAllowedGearTypes_WithUnknownRelic_ReturnsFalse()
	{
		// Arrange - NORECORDS is the Unknown relic
		var unknownId = RelicAndCharm.Unknown.GetRecordId();

		// Act
		var result = Item.TryGetAllowedGearTypes(unknownId, out var types);

		// Assert
		result.Should().BeFalse();
		types.Should().Be(GearType.Undefined);
	}

	[Fact]
	public void TryGetAllowedGearTypes_WithUnknownRecordId_ReturnsFalse()
	{
		// Arrange - record ID that doesn't exist in RelicAndCharmMap
		var unknownId = RecordId.Create("records/item/relics/unknown_relic.dbr");

		// Act
		var result = Item.TryGetAllowedGearTypes(unknownId, out var types);

		// Assert
		result.Should().BeFalse();
		types.Should().Be(GearType.Undefined);
	}

	[Fact]
	public void TryGetAllowedGearTypes_WithPartialPath_ReturnsFalse()
	{
		// Arrange - partial path that doesn't match any relic
		var partialId = RecordId.Create("records/item/relics/01_");

		// Act
		var result = Item.TryGetAllowedGearTypes(partialId, out var types);

		// Assert
		result.Should().BeFalse();
	}

	#endregion

	#region IsRelicAllowed(Item, RecordId) Tests

	[Fact]
	public void IsRelicAllowed_WithDefaultItem_ReturnsFalse()
	{
		// Arrange - default Item has no GearType (ItemClass is empty)
		var item = new Item();
		var relicId = RelicAndCharm.AEGISOFATHENA_ACT1_01.GetRecordId();

		// Act
		var result = Item.IsRelicAllowed(item, relicId);

		// Assert
		result.Should().BeFalse("default Item has no ItemClass, so GearType is Undefined");
	}

	[Fact]
	public void IsRelicAllowed_WithNonRelicRecordId_ReturnsFalse()
	{
		// Arrange - Item with default state, non-relic RecordId
		var item = new Item();
		var nonRelicId = RecordId.Create("records/item/weapons/sword.dbr");

		// Act
		var result = Item.IsRelicAllowed(item, nonRelicId);

		// Assert
		result.Should().BeFalse();
	}

	#endregion

	#region IsRelicAllowed(Item, Item) Tests

	[Fact]
	public void IsRelicAllowed_WithNonRelicItem_ThrowsArgumentException()
	{
		// Arrange - item to enchant, but relicItem is not actually a relic/charm
		var itemToEnchant = new Item();
		var fakeRelic = new Item { BaseItemId = RecordId.Create("records/item/weapons/sword.dbr") };

		// Act & Assert
		var act = () => Item.IsRelicAllowed(itemToEnchant, fakeRelic);
		act.Should().Throw<ArgumentException>("fakeRelic is not a relic or charm");
	}

	[Fact]
	public void IsRelicAllowed_WithRelicItemButDefaultTarget_ReturnsFalse()
	{
		// Arrange - Item to enchant is default (no GearType), relic is valid
		var itemToEnchant = new Item();
		var relic = new Item { BaseItemId = RelicAndCharm.AEGISOFATHENA_ACT1_01.GetRecordId() };

		// Act
		var result = Item.IsRelicAllowed(itemToEnchant, relic);

		// Assert
		result.Should().BeFalse("target item has no GearType");
	}

	#endregion

	#region Integration with Item Instance Method

	[Fact]
	public void IsRelicAllowed_InstanceMethod_WithValidRelicRecordId_ReturnsFalseForDefaultItem()
	{
		// Arrange - test the instance method overload
		var item = new Item();
		var relicId = RelicAndCharm.AMUNRASGLORY_ACT2_01.GetRecordId();

		// Act - call the instance method
		var result = item.IsRelicAllowed(relicId);

		// Assert
		result.Should().BeFalse("default Item has no GearType");
	}

	[Fact]
	public void IsRelicAllowed_InstanceMethod_WithValidRelicItem_ReturnsFalseForDefaultItem()
	{
		// Arrange - test the instance method overload
		var item = new Item();
		var relic = new Item { BaseItemId = RelicAndCharm.AMUNRASGLORY_ACT2_01.GetRecordId() };

		// Act - call the instance method
		var result = item.IsRelicAllowed(relic);

		// Assert
		result.Should().BeFalse("default Item has no GearType");
	}

	#endregion
}
