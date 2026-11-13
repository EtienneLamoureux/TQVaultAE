using AwesomeAssertions;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// The search categories built by <see cref="SearchCategoryService"/>, checked against the builders the search dialog
/// used before (kept below as the reference of the visible lists).
/// </summary>
public class SearchCategoryServiceTests
{
	private readonly Mock<IItemProvider> _itemProvider = new();
	private readonly Mock<ITranslationService> _translation = new();
	private readonly Dictionary<Item, string[]> _skills = new(ReferenceEqualityComparer.Instance);
	private readonly SearchCategoryService _service;

	public SearchCategoryServiceTests()
	{
		this._itemProvider.Setup(p => p.GetGrantedSkillNames(It.IsAny<Item>()))
			.Returns((Item i) => this._skills.TryGetValue(i, out var s) ? s : []);
		this._translation.Setup(t => t.Translate(It.IsAny<GameDlc>())).Returns((GameDlc d) => $"Game {d}");
		this._service = new SearchCategoryService(this._itemProvider.Object, this._translation.Object);
	}

	#region Builders

	private static SearchResult Result(Item item, Action<ToFriendlyNameResult>? setup = null)
	{
		var names = new ToFriendlyNameResult(item);
		setup?.Invoke(names);
		var result = new SearchResult(item, new Lazy<ToFriendlyNameResult>(() => names));
		result.LazyLoad();
		return result;
	}

	private static Info NewInfo(string id, string recordType, params (string Name, string Value)[] values)
	{
		var record = new DBRecordCollection(RecordId.Create(id), recordType);
		foreach (var (name, value) in values)
		{
			var variable = new Variable(name, VariableDataType.StringVar, 1);
			variable[0] = value;
			record.Set(variable);
		}
		return new Info(record);
	}

	/// <summary>Texts with color tags, spaces, empty values, case and accent variants, and duplicates.</summary>
	private static readonly string?[] Texts = [
		null, "", "   ", "{^W}", "^y",
		"{^Y}Fire Damage", " Fire Damage ", "^gFire Damage", "fire damage", "Fire  Damage",
		"{^R}Of the Bear", "Of the {^R}Bear", "Of the Bear // comment", "éclair", "eclair", "Éclair",
		"+10% Strength", "+10 % Strength", "Zeal", "zeal", "{^Y}Zeal{^W}", "Àpple", "apple", "Apple",
	];

	private static readonly string[] Dlcs = [@"records\item", @"records\xpack\item", @"records\xpack2\item", @"records\xpack3\item", @"records\xpack4\item"];

	/// <summary>Items of every kind, built from a fixed seed.</summary>
	private List<SearchResult> SyntheticItems(int count, int seed = 42)
	{
		var rnd = new Random(seed);
		string? Text() => Texts[rnd.Next(Texts.Length)];
		string[] Lines() => Enumerable.Range(0, rnd.Next(4)).Select(_ => Text() ?? string.Empty).ToArray();
		var sets = Enumerable.Range(0, 4).Select(i =>
		{
			var set = new SetItemInfo($"set{i}", $"tagSet{i}", [], new DBRecordCollection(RecordId.Create($"set{i}"), "ItemSet"));
			set.Translations[$"tagSet{i}"] = i % 2 == 0 ? $"{{^G}}Set {i}" : $"Set {i}";
			return set;
		}).ToArray();

		var items = new List<SearchResult>(count);
		for (int n = 0; n < count; n++)
		{
			string dlc = Dlcs[rnd.Next(Dlcs.Length)];
			var item = new Item
			{
				// One item in ten is a relic: not equipment, no rarity
				BaseItemId = RecordId.Create(rnd.Next(10) == 0 ? $@"{dlc}\relics\r{n}.dbr" : $@"{dlc}\equipmentweapon\w{n}.dbr"),
			};
			switch (rnd.Next(4))
			{
				case 0: item.prefixInfo = NewInfo($"p{n}", "LootRandomizer", ("itemClassification", "BROKEN")); break;
				case 1: item.prefixInfo = NewInfo($"p{n}", "LootRandomizer", ("itemClassification", "RARE")); break;
				case 2: item.prefixInfo = NewInfo($"p{n}", "LootRandomizer", ("itemClassification", "MAGICAL")); break;
			}
			if (rnd.Next(3) == 0)
			{
				bool charm = rnd.Next(2) == 0;
				item.relicID = RecordId.Create(charm ? $@"records\xpack\items\charms\c{n}.dbr" : $@"records\xpack\items\relics\r{n}.dbr");
				item.RelicInfo = NewInfo(item.relicID.Raw, charm ? "ItemCharm" : "ItemRelic", ("Class", charm ? "ItemCharm" : "ItemRelic"));
			}
			if (rnd.Next(4) == 0)
			{
				bool charm = rnd.Next(2) == 0;
				item.relic2ID = RecordId.Create(charm ? $@"records\xpack\items\charms\d{n}.dbr" : $@"records\xpack\items\relics\s{n}.dbr");
				item.Relic2Info = NewInfo(item.relic2ID.Raw, charm ? "ItemCharm" : "ItemRelic", ("Class", charm ? "ItemCharm" : "ItemRelic"));
			}
			item.Place.Name = $"Container {rnd.Next(6)}";
			item.Place.SackType = (SackType)rnd.Next(4);
			if (rnd.Next(3) == 0)
				this._skills[item] = Enumerable.Range(0, rnd.Next(1, 3)).Select(_ => $"Skill {rnd.Next(5)}").ToArray();

			items.Add(Result(item, f =>
			{
				f.BaseItemInfoDescription = $"Item {n}";
				f.BaseItemInfoClass = Text() ?? string.Empty;
				f.BaseItemRarity = Text();
				f.BaseItemInfoStyle = Text();
				f.BaseItemInfoQuality = Text();
				f.PrefixInfoDescription = Text();
				f.SuffixInfoDescription = Text();
				f.RelicInfo1Description = Text();
				f.RelicInfo2Description = Text();
				f.BaseAttributes = Lines();
				f.PrefixAttributes = Lines();
				f.SuffixAttributes = Lines();
				f.ItemSet = rnd.Next(5) == 0 ? sets[rnd.Next(sets.Length)] : null;
			}));
		}

		return items;
	}

	#endregion

	#region Reference: the builders of SearchDialogAdvanced.Populate.cs before the move

	private (string Value, IEnumerable<SearchResult> Results)[] Reference(SearchCategory category, IEnumerable<SearchResult> db)
	{
		IEnumerable<IGrouping<string, SearchResult>> groups;
		switch (category)
		{
			case SearchCategory.SetItems:
				groups = from id in db
						 let set = id.LoadedFriendlyNames.ItemSet
						 where set is not null
						 let setName = set.Translations[set.setName].RemoveAllTQTags()
						 orderby setName
						 group id by setName into grp
						 select grp;
				break;
			case SearchCategory.Skills:
				groups = from id in db
						 from skill in this._itemProvider.Object.GetGrantedSkillNames(id.Item)
						 orderby skill
						 group id by skill into grp
						 select grp;
				break;
			case SearchCategory.Origin:
				var originList = new[] { GameDlc.TitanQuest, GameDlc.ImmortalThrone, GameDlc.Ragnarok, GameDlc.Atlantis, GameDlc.EternalEmbers }
					.Select(d => new { Value = d, DisplayName = this._translation.Object.Translate(d) }).ToArray();
				groups = from id in db
						 let itm = id.LoadedFriendlyNames.Item
						 from org in originList
						 where itm.GameDlc == org.Value
						 orderby org.Value
						 group id by org.DisplayName into grp
						 select grp;
				break;
			case SearchCategory.WithRelic:
				groups = from id in db
						 let itm = id.LoadedFriendlyNames.Item
						 where itm.HasRelicOrCharmSlot1 && !itm.IsRelic1Charm || itm.HasRelicOrCharmSlot2 && !itm.IsRelic2Charm
						 from reldesc in new[] { id.LoadedFriendlyNames.RelicInfo1Description, id.LoadedFriendlyNames.RelicInfo2Description }
						 where !string.IsNullOrWhiteSpace(reldesc)
						 let attClean = reldesc.RemoveAllTQTags().Trim()
						 where !string.IsNullOrEmpty(attClean)
						 orderby attClean
						 group id by attClean into grp
						 select grp;
				break;
			case SearchCategory.WithCharm:
				groups = from id in db
						 let itm = id.LoadedFriendlyNames.Item
						 where itm.HasRelicOrCharmSlot1 && itm.IsRelic1Charm || itm.HasRelicOrCharmSlot2 && itm.IsRelic2Charm
						 from reldesc in new[] { id.LoadedFriendlyNames.RelicInfo1Description, id.LoadedFriendlyNames.RelicInfo2Description }
						 where !string.IsNullOrWhiteSpace(reldesc)
						 let attClean = reldesc.RemoveAllTQTags().Trim()
						 where !string.IsNullOrEmpty(attClean)
						 orderby attClean
						 group id by attClean into grp
						 select grp;
				break;
			case SearchCategory.Vaults:
				groups = from id in db.Where(i => i.SackType == SackType.Vault)
						 let att = id.ContainerName
						 orderby att
						 group id by att into grp
						 select grp;
				break;
			case SearchCategory.Characters:
				groups = from id in db
						 where id.SackType == SackType.Player || id.SackType == SackType.Equipment
						 let att = id.ContainerName
						 orderby att
						 group id by att into grp
						 select grp;
				break;
			case SearchCategory.Rarity:
				var equipmentOnly = new[] { ItemStyle.Broken, ItemStyle.Mundane, ItemStyle.Common, ItemStyle.Rare, ItemStyle.Epic, ItemStyle.Legendary };
				groups = from id in db
						 where equipmentOnly.Contains(id.ItemStyle)
						 let att = id.LoadedFriendlyNames.BaseItemRarity
						 where !string.IsNullOrWhiteSpace(att)
						 let attClean = att.RemoveAllTQTags().Trim()
						 where !string.IsNullOrEmpty(attClean)
						 orderby attClean
						 group id by attClean into grp
						 select grp;
				break;
			case SearchCategory.PrefixName:
				groups = from id in db
						 let att = id.LoadedFriendlyNames.PrefixInfoDescription
						 where !string.IsNullOrWhiteSpace(att)
						 let attClean = att.TQCleanup().Trim()
						 where !string.IsNullOrEmpty(attClean)
						 orderby attClean
						 group id by attClean into grp
						 select grp;
				break;
			default:
				Func<ToFriendlyNameResult, IEnumerable<string?>> field = category switch
				{
					SearchCategory.ItemType => f => [f.BaseItemInfoClass ?? f.Item.ItemClass],
					SearchCategory.Style => f => [f.BaseItemInfoStyle],
					SearchCategory.Quality => f => [f.BaseItemInfoQuality],
					SearchCategory.SuffixName => f => [f.SuffixInfoDescription],
					SearchCategory.ItemAttributes => f => f.AttributesAll,
					SearchCategory.BaseAttributes => f => f.BaseAttributes,
					SearchCategory.PrefixAttributes => f => f.PrefixAttributes,
					SearchCategory.SuffixAttributes => f => f.SuffixAttributes,
					_ => throw new ArgumentOutOfRangeException(nameof(category)),
				};
				groups = from id in db
						 from att in field(id.LoadedFriendlyNames)
						 where !string.IsNullOrWhiteSpace(att)
						 let attClean = att.RemoveAllTQTags().Trim()
						 where !string.IsNullOrEmpty(attClean)
						 orderby attClean
						 group id by attClean into grp
						 select grp;
				break;
		}

		return groups.Select(g => (g.Key, (IEnumerable<SearchResult>)g)).ToArray();
	}

	#endregion

	public static TheoryData<SearchCategory> Categories => new(Enum.GetValues<SearchCategory>());

	[Theory]
	[MemberData(nameof(Categories))]
	public void BuildCategory_GivesTheListsTheDialogShowedBefore(SearchCategory category)
	{
		var items = this.SyntheticItems(3000);
		var expected = this.Reference(category, items).ToArray();

		var entries = this._service.BuildCategory(category, items);

		expected.Should().NotBeEmpty("the synthetic items cover every category");
		entries.Select(e => e.DisplayValue).Should().Equal(expected.Select(e => e.Value));
		for (int i = 0; i < expected.Length; i++)
			entries[i].Results.Should().Equal(expected[i].Results, (a, b) => ReferenceEquals(a, b), "the items of {0} keep their order", expected[i].Value);
	}

	[Fact]
	public void BuildCategory_CleansTrimsAndMergesValues_DroppingEmptyOnes()
	{
		var fire1 = Result(new Item(), f => f.BaseItemInfoQuality = "{^Y}Fire");
		var fire2 = Result(new Item(), f => f.BaseItemInfoQuality = "  Fire ");
		var air = Result(new Item(), f => f.BaseItemInfoQuality = "^bAir");
		var tagOnly = Result(new Item(), f => f.BaseItemInfoQuality = "{^W}");
		var none = Result(new Item(), f => f.BaseItemInfoQuality = null);

		var entries = this._service.BuildCategory(SearchCategory.Quality, [fire1, air, fire2, tagOnly, none]);

		entries.Select(e => e.DisplayValue).Should().Equal("Air", "Fire");
		entries[1].Results.Should().Equal(fire1, fire2);
	}

	[Fact]
	public void BuildCategory_PrefixName_RemovesOnlyTheLeadingColorTag_UnlikeSuffixName()
	{
		// The prefix name list uses TQCleanup, the others RemoveAllTQTags: a tag inside the text stays in the prefix list.
		var item = Result(new Item(), f =>
		{
			f.PrefixInfoDescription = "{^Y}Tyrant's {^R}Fury";
			f.SuffixInfoDescription = "{^Y}Of the {^R}Bear";
		});

		var prefix = this._service.BuildCategory(SearchCategory.PrefixName, [item]);
		var suffix = this._service.BuildCategory(SearchCategory.SuffixName, [item]);

		prefix.Single().DisplayValue.Should().Be("Tyrant's {^R}Fury");
		suffix.Single().DisplayValue.Should().Be("Of the Bear");
	}

	[Theory]
	[InlineData(SearchCategory.BaseAttributes)]
	[InlineData(SearchCategory.ItemAttributes)]
	public void BuildCategory_Attributes_LeaveOutTextLines_KeepingTheAttributesUnderAHeader(SearchCategory category)
	{
		// A granted skill: its header and description lines are text, its name and effects are listed.
		string[] lines = [
			string.Empty,
			"{^W}Grants Skill :",
			"{^W}Healing Light (Activated on low health)",
			"{^W}    Heals you and your allies",
			"{^W}    for a short duration.",
			"{^S}{^N}Bonus to All Pets:",
			"{^Y}    +50 Health",
		];
		var item = Result(new Item(), f =>
		{
			f.BaseAttributes = lines;
			f.MarkTextLine(lines[1]);
			f.MarkTextLine(lines[3]);
			f.MarkTextLine(lines[4]);
			f.MarkTextLine(lines[5]);
		});

		var entries = this._service.BuildCategory(category, [item]);

		entries.Select(e => e.DisplayValue).Should().BeEquivalentTo(["Healing Light (Activated on low health)", "+50 Health"]);
	}

	[Fact]
	public void BuildCategory_Attributes_ATextLineOfOneItem_IsStillListedForAnotherItemHavingItAsAnAttribute()
	{
		var described = Result(new Item(), f =>
		{
			f.BaseAttributes = ["Fire Damage"];
			f.MarkTextLine("Fire Damage");
		});
		var magical = Result(new Item(), f => f.BaseAttributes = ["{^Y}Fire Damage"]);

		var entries = this._service.BuildCategory(SearchCategory.BaseAttributes, [described, magical]);

		entries.Should().ContainSingle().Which.Results.Should().Equal(magical);
	}

	[Fact]
	public void BuildCategory_Rarity_ListsEquipmentOnly()
	{
		var sword = Result(new Item { BaseItemId = RecordId.Create("""records\xpack\equipmentweapon\sword.dbr""") }, f => f.BaseItemRarity = "Common");
		var relic = Result(new Item { BaseItemId = RecordId.Create("""records\xpack\items\relics\relic.dbr""") }, f => f.BaseItemRarity = "Relic");

		var entries = this._service.BuildCategory(SearchCategory.Rarity, [sword, relic]);

		entries.Should().ContainSingle().Which.DisplayValue.Should().Be("Common");
	}

	[Fact]
	public void BuildCategory_Origin_ListsTheGamesInReleaseOrder_MergingSameNames()
	{
		this._translation.Setup(t => t.Translate(GameDlc.Atlantis)).Returns("Game ImmortalThrone");
		var embers = Result(new Item { BaseItemId = RecordId.Create("""records\xpack4\equipmentweapon\a.dbr""") });
		var atlantis = Result(new Item { BaseItemId = RecordId.Create("""records\xpack3\equipmentweapon\b.dbr""") });
		var it = Result(new Item { BaseItemId = RecordId.Create("""records\xpack\equipmentweapon\c.dbr""") });
		var tq = Result(new Item { BaseItemId = RecordId.Create("""records\item\equipmentweapon\d.dbr""") });

		var entries = this._service.BuildCategory(SearchCategory.Origin, [embers, atlantis, it, tq]);

		entries.Select(e => e.DisplayValue).Should().Equal("Game TitanQuest", "Game ImmortalThrone", "Game EternalEmbers");
		entries[1].Results.Should().Equal(it, atlantis);
	}
}
