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
	public void FlagFilter_ItemWithoutNames_HasNoSetOrAffix()
	{
		new ItemFlagFilter(false, false, false, false, true, false).Matches(new Item(), null, _ => true).Should().BeFalse();
		new ItemFlagFilter(true, false, false, false, false, false).Matches(new Item(), null, _ => true).Should().BeFalse();
	}
}
