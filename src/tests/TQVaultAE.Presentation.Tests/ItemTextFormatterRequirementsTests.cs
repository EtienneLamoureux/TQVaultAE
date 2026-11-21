using AwesomeAssertions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Tests.Data;

namespace TQVaultAE.Tests.Presentation;

/// <summary>
/// Tests the requirement and set texts of <see cref="TQVaultAE.Presentation.ItemText.ItemTextFormatter"/>.
/// </summary>
public class ItemTextFormatterRequirementsTests : ItemProviderTextTestBase
{
	public ItemTextFormatterRequirementsTests()
	{
		this.Translations["MeetsRequirement"] = "Required {%s0}: {%.0f1}";
		this.Translations["LevelRequirement"] = "Player Level";
		this.Translations["Strength"] = "Strength";
		this.Translations["Dexterity"] = "Dexterity";
		this.Translations["Intelligence"] = "Intelligence";
		this.Translations["tagSetTest"] = "Test Set";
		this.Translations["tagSetRing"] = "Test Ring";
	}

	[Fact]
	public void Requirements_KeepTheHighestValueOfEachRecord()
	{
		var item = this.CreateItem("ArmorJewelry_Ring", Int("levelRequirement", 20), Int("strengthRequirement", 100));
		this.AddPrefix(item, Int("strengthRequirement", 150), Int("dexterityRequirement", 80), Int("levelRequirement", 10));

		var (requirements, variables) = this.Formatter.GetRequirements(item);

		variables.Keys.Should().Equal("Dexterity", "LevelRequirement", "Strength");
		requirements.Should().Equal("Required Dexterity: 80", "Required Player Level: 20", "Required Strength: 150");
	}

	[Fact]
	public void Requirements_WithoutTranslations_ShowTheKeysBetweenQuestionMarks()
	{
		this.Translations.Remove("MeetsRequirement");
		this.Translations.Remove("LevelRequirement");
		var item = this.CreateItem("ArmorJewelry_Ring", Int("levelRequirement", 20));

		this.Formatter.GetRequirements(item).Requirements.Should().Equal("?Required? ?LevelRequirement?: 20");
	}

	[Fact]
	public void MultiValueRequirement_IsShownRaw()
	{
		var item = this.CreateItem("ArmorJewelry_Ring", Ints("levelRequirement", 10, 20));

		this.Formatter.GetRequirements(item).Requirements.Should().ContainSingle()
			.Which.Should().StartWith("LevelRequirement: ").And.Contain("20");
	}

	[Fact]
	public void SetTranslations_NameTheSetInRare_AndEachKnownMemberIndented()
	{
		this.CreateRecord("""records\test\ring.dbr""", "ArmorJewelry_Ring", Str("itemNameTag", "tagSetRing"));
		this.CreateRecord("""records\test\set.dbr""", "ItemSet",
			Str("setName", "tagSetTest"), Strs("setMembers", """records\test\ring.dbr""", """records\test\missing.dbr"""));
		var item = this.CreateItem("ArmorJewelry_Amulet", Str("itemSetName", """records\test\set.dbr"""));

		var set = this.Formatter.GetItemSetTranslations(item);

		set.Should().NotBeNull();
		set!.Translations.Should().BeEquivalentTo(new Dictionary<string, string>
		{
			["tagSetTest"] = $"{Rare}Test Set",
			["""records\test\ring.dbr"""] = $"{Common}    Test Ring",
		});
	}
}
