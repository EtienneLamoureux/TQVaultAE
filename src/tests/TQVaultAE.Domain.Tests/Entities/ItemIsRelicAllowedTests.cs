using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

/// <summary>
/// Unit tests for Item.IsRelicAllowed and Item.TryGetAllowedGearTypes methods.
/// The gear a relic allows comes from the slot flags of its record.
/// </summary>
public class ItemIsRelicAllowedTests
{
	private static readonly RecordId Aegis1 = RecordId.Create(@"records\item\relics\01_act1_aegisofathena.dbr");
	private static readonly RecordId Aegis2 = RecordId.Create(@"records\item\relics\02_act1_aegisofathena.dbr");
	private static readonly RecordId Aegis3 = RecordId.Create(@"records\item\relics\03_act1_aegisofathena.dbr");
	private static readonly RecordId AmunRasGlory = RecordId.Create(@"records\item\relics\01_act2_amunrasglory.dbr");
	private static readonly RecordId AnkhOfIsis = RecordId.Create(@"records\item\relics\01_act2_ankhofisis.dbr");
	private static readonly RecordId ChaosEssence = RecordId.Create(@"records\item\hcdungeon\03_x4_essenceofchaos.dbr");

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
		// Act
		var result = Item.TryGetAllowedGearTypes(Aegis1, out var types, ModRelic(Aegis1.Raw, "shield"));

		// Assert
		result.Should().BeTrue();
		types.Should().Be(GearType.Shield);
	}

	[Fact]
	public void TryGetAllowedGearTypes_WithAmunRaGloryRelic_ReturnsAllArmorGearType()
	{
		// Arrange - Amun-Ra's Glory is an armor relic
		var info = ModRelic(AmunRasGlory.Raw, "helmet", "bodyArmor", "armband", "greaves");

		// Act
		var result = Item.TryGetAllowedGearTypes(AmunRasGlory, out var types, info);

		// Assert
		result.Should().BeTrue();
		types.Should().Be(GearType.AllArmor);
	}

	[Fact]
	public void TryGetAllowedGearTypes_WithAnkhOfIsis_ReturnsJewelleryGearType()
	{
		// Arrange - Ankh of Isis is a jewellery relic (rings and amulets)
		// Act
		var result = Item.TryGetAllowedGearTypes(AnkhOfIsis, out var types, ModRelic(AnkhOfIsis.Raw, "ring", "amulet"));

		// Assert
		result.Should().BeTrue();
		types.Should().Be(GearType.Jewellery);
	}

	[Fact]
	public void TryGetAllowedGearTypes_WithChaosEssence_ReturnsHeadGearType()
	{
		// Arrange - Chaos Essence is a head armor relic
		// Act
		var result = Item.TryGetAllowedGearTypes(ChaosEssence, out var types, ModRelic(ChaosEssence.Raw, "helmet"));

		// Assert
		result.Should().BeTrue();
		types.Should().Be(GearType.Head);
	}

	[Fact]
	public void TryGetAllowedGearTypes_AllActLevelsOfAegis_ReturnShield()
	{
		// Arrange - All 3 act levels of Aegis should be Shield
		// Act & Assert
		Item.TryGetAllowedGearTypes(Aegis1, out var act1Type, ModRelic(Aegis1.Raw, "shield")).Should().BeTrue();
		Item.TryGetAllowedGearTypes(Aegis2, out var act2Type, ModRelic(Aegis2.Raw, "shield")).Should().BeTrue();
		Item.TryGetAllowedGearTypes(Aegis3, out var act3Type, ModRelic(Aegis3.Raw, "shield")).Should().BeTrue();

		act1Type.Should().Be(GearType.Shield);
		act2Type.Should().Be(GearType.Shield);
		act3Type.Should().Be(GearType.Shield);
	}

	[Fact]
	public void TryGetAllowedGearTypes_WithUnknownRecordId_ReturnsFalse()
	{
		// Arrange - a relic given without its record
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

		// Act
		var result = Item.IsRelicAllowed(item, Aegis1, ModRelic(Aegis1.Raw, "shield"));

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
		var relic = new Item { BaseItemId = Aegis1 };

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
		// Act - call the instance method
		var result = item.IsRelicAllowed(AmunRasGlory, ModRelic(AmunRasGlory.Raw, "helmet"));

		// Assert
		result.Should().BeFalse("default Item has no GearType");
	}

	[Fact]
	public void IsRelicAllowed_InstanceMethod_WithValidRelicItem_ReturnsFalseForDefaultItem()
	{
		// Arrange - test the instance method overload
		var item = new Item();
		var relic = new Item { BaseItemId = AmunRasGlory };

		// Act - call the instance method
		var result = item.IsRelicAllowed(relic);

		// Assert
		result.Should().BeFalse("default Item has no GearType");
	}

	#endregion

	#region Relic records (base game and mods)

	private static Info ModRelic(string id, params string[] slotFlags)
	{
		var record = new DBRecordCollection(RecordId.Create(id), "ItemRelic");
		foreach (var flag in slotFlags)
		{
			var variable = new Variable(flag, VariableDataType.Boolean, 1);
			variable[0] = 1;
			record.Set(variable);
		}
		return new Info(record);
	}

	[Fact]
	public void TryGetAllowedGearTypes_ModRelic_ReadsTheSlotFlagsOfItsRecord()
	{
		var id = RecordId.Create("""records\xmod\relics\blade.dbr""");
		var info = ModRelic(id.Raw, "sword", "bow");

		var result = Item.TryGetAllowedGearTypes(id, out var types, info);

		result.Should().BeTrue();
		types.Should().Be(GearType.Sword | GearType.Bow | GearType.Thrown, "relics made for thrown weapons only set bow");
	}

	[Fact]
	public void TryGetAllowedGearTypes_ModRelicWithoutItsRecord_ReturnsFalse()
	{
		var id = RecordId.Create("""records\xmod\relics\blade.dbr""");

		Item.TryGetAllowedGearTypes(id, out var types).Should().BeFalse();
		types.Should().Be(GearType.Undefined);
	}

	[Fact]
	public void TryGetAllowedGearTypes_RecordOfAnotherRelic_IsIgnored()
	{
		var id = RecordId.Create("""records\xmod\relics\blade.dbr""");
		var other = ModRelic("""records\xmod\relics\other.dbr""", "sword");

		Item.TryGetAllowedGearTypes(id, out _, other).Should().BeFalse();
	}

	[Fact]
	public void TryGetAllowedGearTypes_ModRelicWithoutSlotFlags_ReturnsFalse()
	{
		var id = RecordId.Create("""records\xmod\relics\blade.dbr""");

		Item.TryGetAllowedGearTypes(id, out _, ModRelic(id.Raw)).Should().BeFalse();
	}

	[Fact]
	public void TryGetAllowedGearTypes_BaseGameRelic_ReadsItsRecord()
	{
		// A mod may change the slots of a base-game relic: its record, read through the item's map, wins.
		Item.TryGetAllowedGearTypes(Aegis1, out var types, ModRelic(Aegis1.Raw, "sword")).Should().BeTrue();
		types.Should().Be(GearType.Sword);
	}

	[Fact]
	public void TryGetAllowedGearTypes_BaseGameRelicWithoutItsRecord_ReturnsFalse()
	{
		Item.TryGetAllowedGearTypes(Aegis1, out var types).Should().BeFalse();
		types.Should().Be(GearType.Undefined);
	}

	#endregion
}
