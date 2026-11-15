using AwesomeAssertions;
using TQVaultAE.Application.Search;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Tests.Application;

/// <summary>
/// <see cref="RequirementBounds"/> and <see cref="ItemFlagFilter"/>: the rules the advanced search and the highlight
/// filter share.
/// </summary>
public class ItemFiltersTests
{
	private static RequirementBounds Level(int min, int max) => new(min, max, 0, 0, 0, 0, 0, 0);

	[Theory]
	[InlineData(10, 0, 9, false)]
	[InlineData(10, 0, 10, true)]
	[InlineData(0, 20, 21, false)]
	[InlineData(0, 20, 20, true)]
	[InlineData(10, 20, 15, true)]
	public void Matches_ChecksBothEndsInclusively(int min, int max, int level, bool expected)
		=> Level(min, max).Matches(new RequirementInfo { Lvl = level }).Should().Be(expected);

	[Fact]
	public void Matches_ItemWithoutThatRequirementOrWithoutInformation_Passes()
	{
		var bounds = new RequirementBounds(10, 20, 100, 200, 0, 0, 0, 0);

		bounds.Matches(null).Should().BeTrue("an item whose requirements are unknown is kept");
		bounds.Matches(new RequirementInfo { Lvl = 15 }).Should().BeTrue("no strength requirement passes a strength bound");
		bounds.Matches(new RequirementInfo { Lvl = 15, Str = 50 }).Should().BeFalse();
	}

	[Fact]
	public void IsEmpty_OnlyWhenNoBoundIsSet()
	{
		new RequirementBounds().IsEmpty.Should().BeTrue();
		new RequirementBounds(0, 0, 0, 0, 0, 0, 0, 1).IsEmpty.Should().BeFalse();
	}

	[Fact]
	public void FlagFilter_RequiresEveryFlag_AndReadsTheSkillOnlyWhenAsked()
	{
		var item = new Item();
		var names = new ToFriendlyNameResult(item);
		int skillReads = 0;
		bool HasSkill(Item _) { skillReads++; return true; }

		new ItemFlagFilter(false, false, false, false, false, false).Matches(item, names, HasSkill).Should().BeTrue();
		skillReads.Should().Be(0);

		new ItemFlagFilter(true, false, false, false, false, true).Matches(item, names, HasSkill)
			.Should().BeFalse("the item has no prefix");
		skillReads.Should().Be(0, "a failed flag stops before the slow skill check");

		new ItemFlagFilter(false, false, false, false, false, true).Matches(item, names, HasSkill).Should().BeTrue();
		skillReads.Should().Be(1);
	}

	[Fact]
	public void FlagFilter_Modded_KeepsTheItemsFromAMod_InstalledOrNot()
	{
		var modded = new ItemFlagFilter(false, false, false, false, false, false, Modded: true);

		modded.IsEmpty.Should().BeFalse();
		modded.Matches(new Item { Mod = "Mod|custommaps" }, null, _ => false).Should().BeTrue();
		modded.Matches(new Item { Mod = "Gone" }, null, _ => false).Should().BeTrue("a mod not installed is still a mod");
		modded.Matches(new Item(), null, _ => false).Should().BeFalse("a base game item has no mod");
		modded.Matches(new Item { Mod = "  " }, null, _ => false).Should().BeFalse();
	}

	[Fact]
	public void ItemMods_IsFrom_ComparesTheModIdentityIgnoringCase_AndNoModMatchesNothing()
	{
		var item = new Item { Mod = "ShadowChampions|workshop:1" };

		ItemMods.IsFrom(item, "shadowchampions|WORKSHOP:1").Should().BeTrue();
		ItemMods.IsFrom(item, "shadowchampions|custommaps").Should().BeFalse("the same name from another source is another mod");
		ItemMods.IsFrom(item, null).Should().BeFalse();
		ItemMods.IsFrom(new Item(), null).Should().BeFalse("a base game item is from no mod");
	}

	[Fact]
	public void ItemMods_IsFromAny_IsAnOrInsideTheSection_AndAnEmptySectionKeepsEverything()
	{
		var a = new Item { Mod = "A|custommaps" };
		var b = new Item { Mod = "B|custommaps" };
		var none = new Item();
		string[] onlyA = ["A|custommaps"];
		string[] both = ["A|custommaps", "B|custommaps"];

		ItemMods.IsFromAny(a, onlyA).Should().BeTrue();
		ItemMods.IsFromAny(b, onlyA).Should().BeFalse();
		ItemMods.IsFromAny(b, both).Should().BeTrue();
		ItemMods.IsFromAny(none, both).Should().BeFalse();
		ItemMods.IsFromAny(none, []).Should().BeTrue();
	}

	[Fact]
	public void FlagFilter_ItemWithoutNames_HasNoSetOrAffix()
	{
		new ItemFlagFilter(false, false, false, false, true, false).Matches(new Item(), null, _ => true).Should().BeFalse();
		new ItemFlagFilter(true, false, false, false, false, false).Matches(new Item(), null, _ => true).Should().BeFalse();
	}
}
