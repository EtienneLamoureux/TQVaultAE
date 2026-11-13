using AwesomeAssertions;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Domain;

/// <summary>
/// <see cref="ItemIdentity"/>: a worn piece and a loadout's copy are the same piece only when every saved field is equal.
/// </summary>
public class ItemIdentityTests
{
	/// <summary>A socketed sword with every saved field set.</summary>
	internal static Item Piece(int seed = 1234) => new()
	{
		BaseItemId = RecordId.Create("records/item/sword.dbr"),
		prefixID = RecordId.Create("records/item/lootmagicalaffixes/prefix/fire.dbr"),
		suffixID = RecordId.Create("records/item/lootmagicalaffixes/suffix/life.dbr"),
		relicID = RecordId.Create("records/item/relics/ember.dbr"),
		RelicBonusId = RecordId.Create("records/item/lootmagicalaffixes/relics/ember_bonus.dbr"),
		Var1 = 3,
		atlantis = true,
		relic2ID = RecordId.Create("records/item/charms/claw.dbr"),
		RelicBonus2Id = RecordId.Create("records/item/lootmagicalaffixes/charms/claw_bonus.dbr"),
		Var2 = 2,
		Seed = seed,
	};

	[Fact]
	public void AreIdentical_ACopyOfEverySavedField_IsTheSamePiece()
	{
		var worn = Piece();
		var copy = worn.Clone();
		copy.PositionX = 7;
		copy.Mod = "SomeMod";

		ItemIdentity.AreIdentical(worn, copy).Should().BeTrue("place and map are not saved fields of the item");
	}

	public static TheoryData<string> ChangedFields => ["base", "prefix", "suffix", "relic", "relicBonus", "var1", "relic2", "relicBonus2", "var2", "seed", "stack"];

	[Theory]
	[MemberData(nameof(ChangedFields))]
	public void AreIdentical_OneSavedFieldDiffers_IsAnotherPiece(string field)
	{
		var worn = Piece();
		var other = worn.Clone();
		switch (field)
		{
			case "base": other.BaseItemId = RecordId.Create("records/item/axe.dbr"); break;
			case "prefix": other.prefixID = RecordId.Empty; break;
			case "suffix": other.suffixID = RecordId.Create("records/item/lootmagicalaffixes/suffix/mana.dbr"); break;
			case "relic": other.relicID = RecordId.Create("records/item/relics/other.dbr"); break;
			case "relicBonus": other.RelicBonusId = RecordId.Create("records/item/lootmagicalaffixes/relics/other_bonus.dbr"); break;
			case "var1": other.Var1 = 2; break;
			case "relic2": other.relic2ID = RecordId.Empty; break;
			case "relicBonus2": other.RelicBonus2Id = RecordId.Empty; break;
			case "var2": other.Var2 = 1; break;
			case "seed": other.Seed = 4321; break;
			case "stack": other.StackSize = 2; break;
		}

		ItemIdentity.AreIdentical(worn, other).Should().BeFalse();
	}

	[Fact]
	public void AreIdentical_RecordPathsDifferingByCaseAndSlash_AreTheSameRecord()
	{
		var worn = Piece();
		var copy = worn.Clone();
		copy.BaseItemId = RecordId.Create("""Records\Item\SWORD.dbr""");

		ItemIdentity.AreIdentical(worn, copy).Should().BeTrue();
	}

	[Fact]
	public void AreIdentical_NoSecondRelicReadWithOrWithoutItsFields_IsTheSamePiece()
	{
		// The game writes empty second relic fields; a file without them gives the same empty values.
		var saved = Piece();
		saved.relic2ID = RecordId.Empty;
		saved.RelicBonus2Id = RecordId.Empty;
		saved.Var2 = Item.var2Default;
		var read = saved.Clone();
		read.atlantis = false;
		read.relic2ID = null!;

		ItemIdentity.AreIdentical(saved, read).Should().BeTrue();
	}

	[Fact]
	public void AreIdentical_EmptySlots_AreIdenticalButNotToAPiece()
	{
		var empty = new Item { BaseItemId = RecordId.Empty };

		ItemIdentity.AreIdentical(empty, null).Should().BeTrue();
		ItemIdentity.AreIdentical(empty, Piece()).Should().BeFalse();
		ItemIdentity.AreIdentical(null, Piece()).Should().BeFalse();
	}
}
