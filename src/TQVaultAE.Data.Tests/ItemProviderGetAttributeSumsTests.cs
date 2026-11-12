using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// Tests <c>ItemProvider.GetAttributeSums</c>, the item sums behind the character window: the values the game rolls from
/// the item seed, and the pet bonus records.
/// </summary>
public class ItemProviderGetAttributeSumsTests : ItemProviderTextTestBase
{
	[Fact]
	public void Equipment_CountsTheValuesRolledFromItsSeed()
	{
		// A character's Shimmer of Hybris (seed 23198): 150 Defensive Ability in the database, 146 in the game.
		var amulet = this.CreateItem("ArmorJewelry_Amulet", Float("characterDefensiveAbility", 150f));
		amulet.Seed = 23198;

		this.Provider.GetAttributeSums(amulet)["characterDefensiveAbility"].Should().Be(146);
	}

	[Fact]
	public void WithoutSeed_TheDatabaseValuesCount()
	{
		var amulet = this.CreateItem("ArmorJewelry_Amulet", Float("characterDefensiveAbility", 150f));

		this.Provider.GetAttributeSums(amulet)["characterDefensiveAbility"].Should().Be(150);
	}

	[Fact]
	public void PetBonusRecord_IsSummedUnderThePetPrefix()
	{
		this.CreateRecord("""records\test\petbonus.dbr""", "PetBonus", Float("offensivePhysicalModifier", 15f));
		var ring = this.CreateItem("ArmorJewelry_Ring", Str("petBonusName", """records\test\petbonus.dbr"""));

		var sum = this.Provider.GetAttributeSums(ring);

		sum[AttributeSum.PetBonusPrefix + "offensivePhysicalModifier"].Should().Be(15);
		sum["offensivePhysicalModifier"].Should().Be(0);
	}
}
