using System.Globalization;
using AwesomeAssertions;
using TQVaultAE.Application.Search;
using TQVaultAE.Presentation;

namespace TQVaultAE.Tests.Presentation;

public class ItemCopySearchTextTests
{
	private static T In<T>(string culture, Func<T> text)
	{
		var current = CultureInfo.CurrentUICulture;
		try
		{
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
			return text();
		}
		finally
		{
			CultureInfo.CurrentUICulture = current;
		}
	}

	[Theory]
	[InlineData("en")]
	[InlineData("fr")]
	public void MenuLabel_EverySearchHasItsOwnEntry(string culture)
	{
		var labels = In(culture, () => ItemCopySearch.Kinds.Select(ItemCopySearchText.MenuLabel).ToList());

		labels.Should().OnlyContain(label => !string.IsNullOrWhiteSpace(label));
		labels.Should().OnlyHaveUniqueItems();
	}

	[Theory]
	[InlineData("en")]
	[InlineData("fr")]
	public void Title_EverySearchNamesTheItemAndTheCount(string culture)
	{
		var titles = In(culture, () => ItemCopySearch.Kinds.Select(kind => ItemCopySearchText.Title(kind, "Ring of Kings", 7)).ToList());

		titles.Should().OnlyContain(title => title.Contains("Ring of Kings") && title.Contains('7'));
		titles.Should().OnlyHaveUniqueItems();
	}

	[Fact]
	public void Title_AllCopies_InEnglish()
		=> In("en", () => ItemCopySearchText.Title(CopySearchKind.AllCopies, "Ring", 2)).Should().Be("All copies of Ring (2 found)");
}
