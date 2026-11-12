using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// Combination of the advanced search filters (AND / OR), the category reducer and the requirement range rule.
/// </summary>
public class ItemDatabaseServiceFilterTests
{
	private readonly ItemDatabaseService _service;
	private readonly SearchResult _a;
	private readonly SearchResult _b;
	private readonly SearchResult _c;
	private readonly SearchResult _d;

	public ItemDatabaseServiceFilterTests()
	{
		this._service = new ItemDatabaseService(
			new Mock<ILogger<ItemDatabaseService>>().Object,
			new Mock<IItemProvider>().Object,
			new SessionContext());
		this._a = NewResult();
		this._b = NewResult();
		this._c = NewResult();
		this._d = NewResult();
	}

	private static SearchResult NewResult()
	{
		var item = new Item();
		return new SearchResult(item, new Lazy<ToFriendlyNameResult>(() => new ToFriendlyNameResult(item)));
	}

	[Fact]
	public void CombineFilterResults_And_KeepsOnlyItemsInEveryFilter()
	{
		var filters = new[]
		{
			new[] { this._a, this._b, this._c },
			[this._b, this._c, this._d],
			[this._c, this._b],
		};

		var result = this._service.CombineFilterResults(filters, SearchOperator.And);

		result.Should().BeEquivalentTo(new[] { this._b, this._c });
	}

	[Fact]
	public void CombineFilterResults_Or_KeepsItemsOfAnyFilterOnce()
	{
		var filters = new[]
		{
			new[] { this._a, this._b },
			[this._b, this._c],
		};

		var result = this._service.CombineFilterResults(filters, SearchOperator.Or);

		result.Should().Equal(this._a, this._b, this._c);
	}

	[Theory]
	[InlineData(SearchOperator.And)]
	[InlineData(SearchOperator.Or)]
	public void CombineFilterResults_NoFilter_GivesNoResult(SearchOperator logic)
	{
		var result = this._service.CombineFilterResults([], logic);

		result.Should().BeEmpty();
	}

	[Fact]
	public void CombineFilterResults_And_SingleFilterWithDuplicates_ReturnsEachItemOnce()
	{
		var result = this._service.CombineFilterResults([new[] { this._a, this._a, this._b }], SearchOperator.And);

		result.Should().BeEquivalentTo(new[] { this._a, this._b });
	}

	[Fact]
	public void CombineFilterResults_And_DisjointFilters_GivesNoResult()
	{
		var result = this._service.CombineFilterResults([new[] { this._a }, new[] { this._b }], SearchOperator.And);

		result.Should().BeEmpty();
	}

	[Fact]
	public void KeepEntriesMatching_KeepsEntriesWithAResultInTheSet_InTheirOrder()
	{
		var entries = new (string Name, SearchResult[] Results)[]
		{
			("first", new[] { this._a, this._d }),
			("none", new[] { this._c }),
			("empty", Array.Empty<SearchResult>()),
			("second", new[] { this._b }),
		};

		var result = this._service.KeepEntriesMatching(entries, e => e.Results, new[] { this._d, this._b, this._d });

		result.Select(e => e.Name).Should().Equal("first", "second");
	}

	[Fact]
	public void KeepEntriesMatching_NoResult_KeepsNothing()
	{
		var entries = new[] { new[] { this._a } };

		var result = this._service.KeepEntriesMatching(entries, e => e, Array.Empty<SearchResult>());

		result.Should().BeEmpty();
	}

	[Fact]
	public void PrepareForSearch_LoadsItems_ReportsEachOne_AndDropsNamelessAndBrokenItems()
	{
		// Arrange: two named items, one without a name, one whose display data cannot be built
		var itemProvider = new Mock<IItemProvider>();
		var service = new ItemDatabaseService(new Mock<ILogger<ItemDatabaseService>>().Object, itemProvider.Object, new SessionContext());
		var named = new[] { new Item(), new Item() };
		var nameless = new Item();
		var broken = new Item();
		foreach (var item in named)
			itemProvider.Setup(x => x.GetFriendlyNames(item, It.IsAny<FriendlyNamesExtraScopes?>(), It.IsAny<bool>()))
				.Returns(new ToFriendlyNameResult(item) { BaseItemInfoDescription = "Sword" });
		itemProvider.Setup(x => x.GetFriendlyNames(nameless, It.IsAny<FriendlyNamesExtraScopes?>(), It.IsAny<bool>()))
			.Returns(new ToFriendlyNameResult(nameless));
		itemProvider.Setup(x => x.GetFriendlyNames(broken, It.IsAny<FriendlyNamesExtraScopes?>(), It.IsAny<bool>()))
			.Throws(new InvalidOperationException("broken record"));
		foreach (var item in named.Append(nameless).Append(broken))
			service.TryAddItemToDatabase(item);
		var reported = 0;
		var progress = new SyncProgress(n => Interlocked.Add(ref reported, n));

		// Act
		service.PrepareForSearch(progress);

		// Assert
		reported.Should().Be(4, "every item is reported, loaded or not");
		service.ItemDatabase.Select(r => r.Item).Should().BeEquivalentTo(named);
		service.ItemDatabase.Should().OnlyContain(r => r.ItemName == "Sword");
	}

	/// <summary>Reports on the calling thread (<see cref="Progress{T}"/> posts to the thread pool).</summary>
	private sealed class SyncProgress(Action<int> report) : IProgress<int>
	{
		public void Report(int value) => report(value);
	}

	[Theory]
	// Ordered: nothing changes
	[InlineData(3, 5, true, 3, 5)]
	[InlineData(3, 5, false, 3, 5)]
	[InlineData(4, 4, true, 4, 4)]
	// Minimum raised above the maximum: the maximum follows
	[InlineData(7, 5, true, 7, 7)]
	// Maximum lowered below the minimum: the minimum follows
	[InlineData(7, 5, false, 5, 5)]
	public void OrderRange_TheOtherBoundFollowsTheChangedOne(int min, int max, bool minChanged, int expectedMin, int expectedMax)
	{
		var (low, high) = AdvancedSearchRequest.OrderRange(min, max, minChanged);

		low.Should().Be(expectedMin);
		high.Should().Be(expectedMax);
	}
}
