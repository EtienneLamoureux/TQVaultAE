using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// Saved search queries: validation, ordering, storage in <c>SearchQueries.json</c> and re-attachment of the filters.
/// </summary>
public class SearchQueryServiceTests
{
	private const string ConfigFolder = "config";
	private static readonly string FilePath = Path.Combine(ConfigFolder, SearchQueryService.FileName);

	/// <summary>A file written by the search dialog before the saved queries moved to <see cref="SearchQueryService"/>.</summary>
	private const string FileOfEarlierVersions = """
		[
		  {
		    "CheckedItems": [
		      {
		        "CheckedListName": "scalingCheckedListBoxItemType",
		        "CategoryName": "scalingLabelItemType",
		        "DisplayValue": "Sword"
		      },
		      {
		        "CheckedListName": "",
		        "CategoryName": "scalingLabelSearchTerm",
		        "DisplayValue": "fire"
		      }
		    ],
		    "QueryName": "Fire swords",
		    "Filter": "dmg",
		    "Visible": [
		      {
		        "Name": "scalingButtonMenuAttribute",
		        "Visible": true
		      },
		      {
		        "Name": "scalingButtonMenuSkills",
		        "Visible": false
		      }
		    ],
		    "MaxElement": 7,
		    "Reduce": true,
		    "Logic": "Or",
		    "MaxStr": 300,
		    "MaxDex": 0,
		    "MaxInt": 0,
		    "MinStr": 100,
		    "MinDex": 0,
		    "MinInt": 0,
		    "MinRequirement": true,
		    "MaxRequirement": true,
		    "MaxLvl": 60,
		    "MinLvl": 10,
		    "HavingPrefix": true,
		    "HavingSuffix": false,
		    "HavingRelic": true,
		    "HavingCharm": false,
		    "IsSetItem": true,
		    "HavingSkill": false
		  },
		  {
		    "CheckedItems": [],
		    "QueryName": "charms",
		    "Filter": "",
		    "Visible": [],
		    "MaxElement": 5,
		    "Reduce": false,
		    "Logic": "And",
		    "MaxStr": 0,
		    "MaxDex": 0,
		    "MaxInt": 0,
		    "MinStr": 0,
		    "MinDex": 0,
		    "MinInt": 0,
		    "MinRequirement": false,
		    "MaxRequirement": false,
		    "MaxLvl": 0,
		    "MinLvl": 0,
		    "HavingPrefix": false,
		    "HavingSuffix": false,
		    "HavingRelic": false,
		    "HavingCharm": true,
		    "IsSetItem": false,
		    "HavingSkill": false
		  }
		]
		""";

	private readonly Dictionary<string, string> _files = [];
	private readonly Mock<IFileIO> _fileIO = new();
	private readonly Mock<ILogger<SearchQueryService>> _log = new();
	private readonly SearchQueryService _service;

	public SearchQueryServiceTests()
	{
		this._fileIO.Setup(f => f.Exists(It.IsAny<string>())).Returns((string p) => this._files.ContainsKey(p));
		this._fileIO.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns((string p) => this._files[p]);
		this._fileIO.Setup(f => f.WriteAllText(It.IsAny<string>(), It.IsAny<string>())).Callback((string p, string c) => this._files[p] = c);
		var pathIO = new Mock<IPathIO>();
		pathIO.Setup(p => p.Combine(It.IsAny<string[]>())).Returns((string[] paths) => Path.Combine(paths));
		var gamePath = new Mock<IGamePathService>();
		gamePath.Setup(g => g.TQVaultConfigFolder).Returns(ConfigFolder);
		this._service = new SearchQueryService(this._log.Object, gamePath.Object, this._fileIO.Object, pathIO.Object);
	}

	private static string Normalized(string json) => json.ReplaceLineEndings("\n");

	private static SearchQuery WithFilter(string value = "Sword")
		=> new() { CheckedItems = [new SearchFilter("list", "label", value)] };

	[Fact]
	public void Queries_FileOfEarlierVersions_ReadsEveryField()
	{
		this._files[FilePath] = FileOfEarlierVersions;

		var queries = this._service.Queries;

		queries.Select(q => q.QueryName).Should().Equal("Fire swords", "charms");
		var q = queries[0];
		q.CheckedItems.Should().Equal(
			new SearchFilter("scalingCheckedListBoxItemType", "scalingLabelItemType", "Sword"),
			new SearchFilter("", "scalingLabelSearchTerm", "fire"));
		q.Visible.Should().Equal(new SearchCategoryVisibility("scalingButtonMenuAttribute", true), new SearchCategoryVisibility("scalingButtonMenuSkills", false));
		q.Filter.Should().Be("dmg");
		q.MaxElement.Should().Be(7);
		q.Reduce.Should().BeTrue();
		q.Logic.Should().Be(SearchOperator.Or);
		(q.MinStr, q.MaxStr, q.MinLvl, q.MaxLvl).Should().Be((100, 300, 10, 60));
		(q.MinRequirement, q.MaxRequirement).Should().Be((true, true));
		(q.HavingPrefix, q.HavingSuffix, q.HavingRelic, q.HavingCharm, q.IsSetItem, q.HavingSkill).Should().Be((true, false, true, false, true, false));
		queries[1].HavingCharm.Should().BeTrue();
	}

	[Fact]
	public void Save_WritesTheFormatOfEarlierVersions()
	{
		this._files[FilePath] = FileOfEarlierVersions;
		var charms = this._service.Queries[1];

		// Saving a query again unchanged rewrites the same file
		this._service.Save("charms", charms);

		Normalized(this._files[FilePath]).Should().Be(Normalized(FileOfEarlierVersions));
	}

	[Fact]
	public void Queries_UnreadableFile_GivesNoQuery_AndLeavesTheFile()
	{
		this._files[FilePath] = "{ not json";

		this._service.Queries.Should().BeEmpty();

		this._files[FilePath].Should().Be("{ not json");
	}

	[Theory]
	[InlineData("", SearchQuerySaveCheck.NameMissing)]
	[InlineData("   ", SearchQuerySaveCheck.NameMissing)]
	[InlineData("CHARMS", SearchQuerySaveCheck.NameExists)]
	[InlineData(" charms ", SearchQuerySaveCheck.NameExists)]
	[InlineData("charms 2", SearchQuerySaveCheck.Valid)]
	public void CheckSave_ChecksTheName(string name, SearchQuerySaveCheck expected)
	{
		this._files[FilePath] = FileOfEarlierVersions;

		this._service.CheckSave(name, WithFilter()).Should().Be(expected);
	}

	[Fact]
	public void CheckSave_QueryWithoutFilter_IsRefused_BeforeTheNameClash()
	{
		this._files[FilePath] = FileOfEarlierVersions;

		this._service.CheckSave("charms", new SearchQuery()).Should().Be(SearchQuerySaveCheck.NoFilter);
	}

	public static TheoryData<SearchQuery, bool> FilterCases => new()
	{
		{ new SearchQuery(), false },
		{ WithFilter(), true },
		// A requirement bound counts only when its requirement is enabled, and not at 0
		{ new SearchQuery { MinLvl = 10 }, false },
		{ new SearchQuery { MinRequirement = true }, false },
		{ new SearchQuery { MinRequirement = true, MinInt = 5 }, true },
		{ new SearchQuery { MaxRequirement = true, MinDex = 5 }, false },
		{ new SearchQuery { MaxRequirement = true, MaxDex = 5 }, true },
		{ new SearchQuery { HavingSkill = true }, true },
		{ new SearchQuery { IsSetItem = true }, true },
		// Display settings filter nothing
		{ new SearchQuery { Reduce = true, Filter = "fire", MaxElement = 3, Logic = SearchOperator.Or }, false },
	};

	[Theory]
	[MemberData(nameof(FilterCases))]
	public void HasFilters_CountsFiltersQuickFiltersAndEnabledRequirements(SearchQuery query, bool expected)
	{
		query.HasFilters().Should().Be(expected);
	}

	[Fact]
	public void Save_NewQuery_IsAddedInNameOrder_AndWritten()
	{
		this._files[FilePath] = FileOfEarlierVersions;

		var saved = this._service.Save("  Belts ", WithFilter());

		saved.QueryName.Should().Be("Belts");
		this._service.Queries.Select(q => q.QueryName).Should().Equal("Belts", "charms", "Fire swords");
		this._files[FilePath].Should().Contain("\"QueryName\": \"Belts\"");
	}

	[Fact]
	public void Save_ExistingName_ReplacesTheQueryAtItsPlace_WithTheNewSpelling()
	{
		this._files[FilePath] = FileOfEarlierVersions;
		var draft = WithFilter("Axe");

		this._service.Save("FIRE SWORDS", draft);

		this._service.Queries.Select(q => q.QueryName).Should().Equal("FIRE SWORDS", "charms");
		this._service.Queries[0].Should().BeSameAs(draft);
		this._files[FilePath].Should().Contain("Axe").And.NotContain("Fire swords");
	}

	[Fact]
	public void Save_WithoutFile_CreatesIt()
	{
		this._service.Save("first", WithFilter());

		this._files.Should().ContainKey(FilePath);
		this._service.Queries.Should().ContainSingle();
	}

	[Fact]
	public void Delete_RemovesTheQuery_AndWritesTheFile()
	{
		this._files[FilePath] = FileOfEarlierVersions;

		this._service.Delete(this._service.Queries[0]);

		this._service.Queries.Select(q => q.QueryName).Should().Equal("charms");
		this._files[FilePath].Should().NotContain("Fire swords");
	}

	[Fact]
	public void AttachFilters_MatchesSavedFiltersToLiveOnes_RebuildsTheSearchTerm_AndDropsMissingOnes()
	{
		var query = new SearchQuery
		{
			CheckedItems = [
				new SearchFilter("listType", "labelType", "Sword"),
				new SearchFilter("", "labelTerm", "fire"),
				new SearchFilter("listType", "labelType", "Lost value"),
				new SearchFilter("listStyle", "labelStyle", "Sword"),
			],
		};
		var live = new Dictionary<SearchFilter, string>
		{
			[new SearchFilter("listStyle", "labelStyle", "Sword")] = "style:Sword",
			[new SearchFilter("listType", "labelType", "Sword")] = "type:Sword",
			[new SearchFilter("listType", "labelType", "Axe")] = "type:Axe",
		};

		var filters = this._service.AttachFilters(query, live, "labelTerm", term => $"term:{term}");

		filters.Should().Equal("type:Sword", "term:fire", "style:Sword");
	}
}
