using AwesomeAssertions;
using TQVaultAE.Application.Session;
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
		requirements.Should().Equal("Required Player Level: 20", "Required Strength: 150", "Required Dexterity: 80");
	}

	[Fact]
	public void FormatRequirements_ForACharacter_WriteTheGameFormats()
	{
		this.Translations["MeetsRequirementWithReduction"] = "Required {%s0}: {%.0f1} [ {^r%.0f2 ^y- %.0f3}% Reduction]";
		this.Translations["FailsRequirement"] = "{^r}Required {%s0: %.0f1}";
		this.Translations["FailsRequirementWithReduction"] = "{^r}Required {%s0: %.0f1} [ {%.0f2 ^y- %.0f3}% Reduction]";
		var item = this.CreateItem("WeaponHunting_Bow", Int("levelRequirement", 7), Int("strengthRequirement", 57),
			Int("dexterityRequirement", 96), Int("intelligenceRequirement", 50));
		var variables = this.Formatter.GetRequirements(item).RequirementVariables;
		var reduced = new Dictionary<string, ReducedRequirement>
		{
			[Variable.KEY_LEVELREQ] = new(7, 7, 8, 8),
			[Variable.KEY_STRENGTH] = new(57, 52, 8, 60), // the base met: shown alone
			[Variable.KEY_DEXTERITY] = new(96, 88, 8, 80),
			[Variable.KEY_INTELLIGENCE] = new(50, 46, 8, 47),
		};

		this.Formatter.FormatRequirements(variables, reduced).Should().Equal(
			"Required Player Level: 7",
			"Required Strength: 57",
			"{^r}Required Dexterity: 88 [ 96 {^y}- 8% Reduction]",
			"Required Intelligence: 46 [ {^r}50 {^y}- 8% Reduction]");

		reduced[Variable.KEY_INTELLIGENCE] = new(50, 50, 0, 40);
		this.Formatter.FormatRequirements(variables, reduced)[3].Should().Be("{^r}Required Intelligence: 50");
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
