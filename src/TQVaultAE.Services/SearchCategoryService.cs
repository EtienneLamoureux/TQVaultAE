using System.Runtime.InteropServices;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Services;

/// <summary>
/// Builds the value lists of the advanced search categories.
/// </summary>
/// <remarks>
/// The items are grouped by value first and only the distinct values are sorted: sorting every (item, value) pair
/// with the culture comparer was most of the time spent, as an attribute line is shared by thousands of items.
/// The result is the same: the values are in culture order, values comparing equal keep their first appearance
/// order, and each value keeps its items in database order.
/// </remarks>
public class SearchCategoryService(IItemProvider itemProvider, ITranslationService translationService) : ISearchCategoryService
{
	private readonly IItemProvider _itemProvider = itemProvider;
	private readonly ITranslationService _translationService = translationService;

	/// <summary>Styles of the equipment, the only items listed by rarity.</summary>
	private static readonly ItemStyle[] EquipmentStyles = [
		ItemStyle.Broken,
		ItemStyle.Mundane,
		ItemStyle.Common,
		ItemStyle.Rare,
		ItemStyle.Epic,
		ItemStyle.Legendary,
	];

	/// <summary>The games listed by origin, in display order.</summary>
	private static readonly GameDlc[] Origins = [
		GameDlc.TitanQuest,
		GameDlc.ImmortalThrone,
		GameDlc.Ragnarok,
		GameDlc.Atlantis,
		GameDlc.EternalEmbers,
	];

	/// <inheritdoc/>
	public IReadOnlyList<SearchCategoryEntry> BuildCategory(SearchCategory category, IEnumerable<SearchResult> items)
	{
		var groups = new Groups();
		switch (category)
		{
			case SearchCategory.Characters:
				foreach (var r in items)
				{
					if (r.SackType == SackType.Player || r.SackType == SackType.Equipment)
						groups.Add(r.ContainerName, r);
				}
				break;
			case SearchCategory.Vaults:
				foreach (var r in items)
				{
					if (r.SackType == SackType.Vault)
						groups.Add(r.ContainerName, r);
				}
				break;
			case SearchCategory.ItemType:
				foreach (var r in items)
				{
					var names = r.LoadedFriendlyNames;
					groups.AddClean(names.BaseItemInfoClass ?? names.Item.ItemClass, r);
				}
				break;
			case SearchCategory.Rarity:
				foreach (var r in items)
				{
					if (EquipmentStyles.Contains(r.ItemStyle))
						groups.AddClean(r.LoadedFriendlyNames.BaseItemRarity, r);
				}
				break;
			case SearchCategory.Style:
				foreach (var r in items)
					groups.AddClean(r.LoadedFriendlyNames.BaseItemInfoStyle, r);
				break;
			case SearchCategory.Quality:
				foreach (var r in items)
					groups.AddClean(r.LoadedFriendlyNames.BaseItemInfoQuality, r);
				break;
			case SearchCategory.WithCharm:
			case SearchCategory.WithRelic:
				bool charm = category == SearchCategory.WithCharm;
				foreach (var r in items)
				{
					var itm = r.LoadedFriendlyNames.Item;
					// An item with a charm and a relic is listed under both descriptions in each category.
					if (itm.HasRelicOrCharmSlot1 && itm.IsRelic1Charm == charm || itm.HasRelicOrCharmSlot2 && itm.IsRelic2Charm == charm)
					{
						groups.AddClean(r.LoadedFriendlyNames.RelicInfo1Description, r);
						groups.AddClean(r.LoadedFriendlyNames.RelicInfo2Description, r);
					}
				}
				break;
			case SearchCategory.Origin:
				return this.BuildOrigin(items);
			case SearchCategory.SetItems:
				foreach (var r in items)
				{
					if (r.LoadedFriendlyNames.ItemSet is { } set)
						groups.Add(set.Translations[set.setName].RemoveAllTQTags(), r);
				}
				break;
			case SearchCategory.Skills:
				foreach (var r in items)
				{
					foreach (var skill in this._itemProvider.GetGrantedSkillNames(r.Item))
						groups.Add(skill, r);
				}
				break;
			case SearchCategory.ItemAttributes:
				foreach (var r in items)
					groups.AddAttributes(r.LoadedFriendlyNames.AttributesAll, r);
				break;
			case SearchCategory.BaseAttributes:
				foreach (var r in items)
					groups.AddAttributes(r.LoadedFriendlyNames.BaseAttributes, r);
				break;
			case SearchCategory.PrefixAttributes:
				foreach (var r in items)
					groups.AddAttributes(r.LoadedFriendlyNames.PrefixAttributes, r);
				break;
			case SearchCategory.SuffixAttributes:
				foreach (var r in items)
					groups.AddAttributes(r.LoadedFriendlyNames.SuffixAttributes, r);
				break;
			case SearchCategory.PrefixName:
				foreach (var r in items)
				{
					// TQCleanup, not RemoveAllTQTags: only the leading color tag is removed (see BuildCategory tests).
					var att = r.LoadedFriendlyNames.PrefixInfoDescription;
					if (!string.IsNullOrWhiteSpace(att))
						groups.AddIfNotEmpty(att.TQCleanup().Trim(), r);
				}
				break;
			case SearchCategory.SuffixName:
				foreach (var r in items)
					groups.AddClean(r.LoadedFriendlyNames.SuffixInfoDescription, r);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(category), category, null);
		}

		return groups.ToSortedEntries();
	}

	/// <summary>
	/// Groups the items by the translated name of their game, the games in their release order.
	/// </summary>
	private SearchCategoryEntry[] BuildOrigin(IEnumerable<SearchResult> items)
	{
		var byDlc = new Dictionary<GameDlc, List<SearchResult>>();
		foreach (var r in items)
		{
			var dlc = r.LoadedFriendlyNames.Item.GameDlc;
			if (!Origins.Contains(dlc))
				continue;

			if (!byDlc.TryGetValue(dlc, out var list))
				byDlc[dlc] = list = [];
			list.Add(r);
		}

		// Two games translated alike share one entry.
		var groups = new Groups();
		foreach (var dlc in Origins)
		{
			if (!byDlc.TryGetValue(dlc, out var list))
				continue;

			var name = this._translationService.Translate(dlc);
			foreach (var r in list)
				groups.Add(name, r);
		}

		return groups.ToEntries();
	}

	/// <summary>
	/// Items grouped by value, the values in their first appearance order.
	/// </summary>
	private sealed class Groups
	{
		private readonly Dictionary<string, List<SearchResult>> _byValue = new(StringComparer.Ordinal);
		private readonly List<string> _order = [];

		public void Add(string value, SearchResult result)
		{
			ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault(this._byValue, value, out bool exists);
			if (!exists)
			{
				list = [];
				this._order.Add(value);
			}

			list!.Add(result);
		}

		public void AddIfNotEmpty(string value, SearchResult result)
		{
			if (!string.IsNullOrEmpty(value))
				this.Add(value, result);
		}

		/// <summary>Adds <paramref name="raw"/> without its TQ tags and trimmed, unless it is empty.</summary>
		public void AddClean(string? raw, SearchResult result)
		{
			if (!string.IsNullOrWhiteSpace(raw))
				this.AddIfNotEmpty(raw.RemoveAllTQTags().Trim(), result);
		}

		/// <summary>
		/// Adds the attribute lines of an item, each without its TQ tags, trimmed and on one line with single spaces (a pet's
		/// "720  Health", the game's text, lists "720 Health"), leaving out empty ones and those
		/// that are text, not attributes (a granted skill's description, a header): they are no filter.
		/// </summary>
		public void AddAttributes(string[] raws, SearchResult result)
		{
			var names = result.LoadedFriendlyNames;
			foreach (var raw in raws)
			{
				if (string.IsNullOrWhiteSpace(raw))
					continue;

				var clean = raw.RemoveAllTQTags().Trim();
				// A line written on two lines (a granted skill's activation, then its chance) is one entry on one line.
				if (!names.IsTextLine(clean))
					this.AddIfNotEmpty(clean.CollapseWhiteSpace(), result);
			}
		}

		/// <summary>The values in their first appearance order.</summary>
		public SearchCategoryEntry[] ToEntries()
			=> this._order.Select(v => new SearchCategoryEntry(v, this._byValue[v])).ToArray();

		/// <summary>The values sorted with the current culture; OrderBy is stable, so equal values keep their order.</summary>
		public SearchCategoryEntry[] ToSortedEntries()
			=> this._order.OrderBy(v => v, Comparer<string>.Default).Select(v => new SearchCategoryEntry(v, this._byValue[v])).ToArray();
	}
}
