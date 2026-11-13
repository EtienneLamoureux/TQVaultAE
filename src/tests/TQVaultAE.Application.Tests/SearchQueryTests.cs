using AwesomeAssertions;
using TQVaultAE.Application.DTOs;

namespace TQVaultAE.Tests.Application;

public class SearchQueryTests
{
	private static SearchQuery FullQuery() => new()
	{
		QueryName = "Fire swords",
		CheckedItems = [new("scalingCheckedListBoxItemType", "scalingLabelItemType", "Sword"), new("", "scalingLabelSearchTerm", "fire")],
		Filter = "dmg",
		Visible = [new("scalingButtonMenuSkills", false)],
		MaxElement = 7,
		Reduce = true,
		Logic = SearchOperator.Or,
		MinRequirement = true,
		MaxRequirement = true,
		MinLvl = 10,
		MaxLvl = 60,
		MinStr = 1,
		MaxStr = 2,
		MinDex = 3,
		MaxDex = 4,
		MinInt = 5,
		MaxInt = 6,
		HavingPrefix = true,
		HavingSuffix = true,
		HavingRelic = true,
		HavingCharm = true,
		IsSetItem = true,
		HavingSkill = true,
	};

	[Fact]
	public void SettingsOnly_KeepsTheDialogSettings()
	{
		var query = FullQuery();

		var settings = query.SettingsOnly();

		settings.Filter.Should().Be("dmg");
		settings.Visible.Should().Equal(query.Visible);
		settings.Visible.Should().NotBeSameAs(query.Visible);
		settings.MaxElement.Should().Be(7);
		settings.Reduce.Should().BeTrue();
		settings.Logic.Should().Be(SearchOperator.Or);
	}

	[Fact]
	public void SettingsOnly_DropsEveryFilter()
	{
		var settings = FullQuery().SettingsOnly();

		settings.HasFilters().Should().BeFalse();
		settings.Should().BeEquivalentTo(new SearchQuery
		{
			Filter = "dmg",
			Visible = [new("scalingButtonMenuSkills", false)],
			MaxElement = 7,
			Reduce = true,
			Logic = SearchOperator.Or,
		});
	}
}
