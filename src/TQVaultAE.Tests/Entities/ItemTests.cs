using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

/// <summary>
/// Unit tests for Item entity.
/// Tests simple properties that don't require complex database mocking.
/// </summary>
public class ItemTests
{
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

	#endregion

	#region Copy Tests

	public static TheoryData<string> CopyMethods => ["Duplicate", "Clone"];

	private static Item Copy(Item item, string method) => method switch
	{
		"Duplicate" => item.Duplicate(true),
		_ => item.Clone(),
	};

	[Theory]
	[MemberData(nameof(CopyMethods))]
	public void Copy_MovingTheCopy_LeavesTheOriginalPlace(string method)
	{
		// A shared Place made the original report the copy's bag, so search results focused the wrong item.
		var original = new Item();
		original.Place.Name = "Main Vault";
		original.Place.SackNumber = 2;
		original.Place.SackType = SackType.Vault;

		var copy = Copy(original, method);
		copy.Place.Name = "Second Vault";
		copy.Place.SackNumber = 5;

		original.Place.Name.Should().Be("Main Vault");
		original.Place.SackNumber.Should().Be(2);
		copy.Place.SackType.Should().Be(SackType.Vault, "the copy starts at the original's place");
	}

	[Theory]
	[MemberData(nameof(CopyMethods))]
	public void Copy_DoesNotKeepTheOriginalsFriendlyNames(string method)
	{
		var original = new Item();
		original.CurrentFriendlyNameResult = new TQVaultAE.Domain.Results.ToFriendlyNameResult(original);

		Copy(original, method).CurrentFriendlyNameResult.Should().BeNull();
	}

	#endregion

	#region Socketed slot Tests

	[Fact]
	public void HasSocketedSlot_AnyRecord_CountsEvenWhenNotARelic()
	{
		// God mode in the Forge can socket a weapon: "Remove Relic" must still be offered for it.
		var item = new Item
		{
			relicID = RecordId.Create(@"records\item\equipmentweapon\spear\u_e_spear01.dbr"),
			relic2ID = RecordId.Create(@"records\item\relics\01_act1_aegisofathena.dbr"),
		};

		item.HasSocketedSlot1.Should().BeTrue();
		item.HasRelicOrCharmSlot1.Should().BeFalse("a weapon record is not a relic");
		item.HasSocketedSlot2.Should().BeTrue();
		item.HasRelicOrCharmSlot2.Should().BeTrue();
	}

	[Fact]
	public void HasSocketedSlot_EmptySlots_AreFalse()
	{
		var item = new Item { relicID = RecordId.Empty, relic2ID = RecordId.Empty };

		item.HasSocketedSlot1.Should().BeFalse();
		item.HasSocketedSlot2.Should().BeFalse();
	}

	#endregion

	#region MissingRecords

	private static Info InfoOf(string id)
		=> new(new DBRecordCollection(RecordId.Create(id), "ItemRelic"));

	[Fact]
	public void MissingRecords_EveryRecordFound_IsEmpty()
	{
		var item = new Item
		{
			BaseItemId = RecordId.Create("records/item/sword.dbr"),
			baseItemInfo = InfoOf("records/item/sword.dbr"),
			prefixID = RecordId.Create("records/prefix.dbr"),
			prefixInfo = InfoOf("records/prefix.dbr"),
		};

		item.MissingRecords.Should().BeEmpty();
		item.IsIncomplete.Should().BeFalse();
	}

	[Fact]
	public void MissingRecords_ListsEachNamedRecordWithoutInfo_InOrder()
	{
		var item = new Item
		{
			BaseItemId = RecordId.Create("records/mod/sword.dbr"),
			prefixID = RecordId.Create("records/prefix.dbr"),
			prefixInfo = InfoOf("records/prefix.dbr"),
			suffixID = RecordId.Create("records/mod/suffix.dbr"),
			relic2ID = RecordId.Create("records/mod/charm.dbr"),
		};

		item.MissingRecords.Should().Equal(
			(ItemRecordRole.BaseItem, item.BaseItemId),
			(ItemRecordRole.Suffix, item.suffixID),
			(ItemRecordRole.Relic, item.relic2ID));
		item.IsIncomplete.Should().BeTrue();
	}

	#endregion
}
