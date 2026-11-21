using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Services;

/// <summary>
/// Builds the catalogs offered by the item context menu: set members, completion bonuses and affixes.
/// </summary>
public class AffixCatalogService(IItemProvider itemProvider, ITranslationService translationService) : IAffixCatalogService
{
	private const int BrokenTypeId = 0;
	private const int PrefixTypeId = 1;
	private const int SuffixTypeId = 2;

	private readonly IItemProvider _itemProvider = itemProvider;
	private readonly ITranslationService _translationService = translationService;

	/// <inheritdoc/>
	public IReadOnlyList<SetMemberChoice> GetSetMemberChoices(Item item)
	{
		var setItems = this._itemProvider.GetSetItems(item);
		if (setItems?.setMembers is not { Count: > 0 })
			return [];

		var choices = new List<SetMemberChoice>();
		foreach (var setPiece in setItems.setMembers)
		{
			// do not offer the current item
			if (item.BaseItemId == setPiece.Key.ToRecordId())
				continue;

			Info info = setPiece.Value;
			if (info is null)
				continue;

			choices.Add(new SetMemberChoice(setPiece.Key, this._translationService.TranslateXTag(info.DescriptionTag)));
		}

		return choices.OrderBy(c => c.Label).ToList();
	}

	/// <inheritdoc/>
	public IReadOnlyList<BonusChoice> GetCompletionBonusChoices(Item item)
	{
		if (!((item.IsRelicOrCharm && item.IsRelicComplete) || item.IsArtifact))
			return [];

		return BuildBonusChoices(this._itemProvider.BonusTableRelicOrArtifact(item), item.RelicBonusId);
	}

	/// <inheritdoc/>
	public (IReadOnlyList<BonusChoice> Relic1, IReadOnlyList<BonusChoice> Relic2) GetSocketedBonusChoices(Item item)
	{
		if (!this._itemProvider.BonusTableSocketedRelic(item, out var table1, out var table2))
			return ([], []);

		return (BuildBonusChoices(table1, item.RelicBonusId), BuildBonusChoices(table2, item.RelicBonus2Id));
	}

	/// <inheritdoc/>
	public AffixCatalog? GetAffixCatalog(Item item, bool enableEpicLegendaryAffixes)
	{
		if (!(item.IsArmor || item.IsWeaponShield || item.IsJewellery))
			return null;

		bool allAffixes = enableEpicLegendaryAffixes && (item.Rarity == Rarity.Epic || item.Rarity == Rarity.Legendary);
		ItemAffixes? affixes = allAffixes
			// Get all available affixes for an item type
			? this._itemProvider.GetAllAvailableAffixes(item.GearType)
			// Get affixes related to a specific item; with the setting on, also those the game never rolls on it
			: this._itemProvider.GetItemAffixes(item.BaseItemId, includeUnreachable: enableEpicLegendaryAffixes);

		if (affixes is null)
			return null;

		var groups = CurateAffixes(affixes);
		List<AffixGroup> OfType(int typeId) => groups.Where(g => g.Key.TypeId == typeId).ToList();
		List<AffixGroup> broken = OfType(BrokenTypeId), prefix = OfType(PrefixTypeId), suffix = OfType(SuffixTypeId);

		return new AffixCatalog(
			affixes.Broken.Count > 0 || broken.Count > 0
			, broken
			, affixes.Prefix.Count > 0 || prefix.Count > 0
			, prefix
			, affixes.Suffix.Count > 0 || suffix.Count > 0
			, suffix
			, allAffixes
		);
	}

	/// <inheritdoc/>
	public IReadOnlyList<AffixGroup> GroupByEffect(IEnumerable<AffixGroup> groups)
	{
		var regrouped =
			from grp in groups
			from av in grp
			let effect = av.AffixId.PrettyFileNameExploded.Effect
			let flattenedAffix = new AffixEntry(
				av.TypeId,
				av.AffixId,
				av.AffixIdDlc,
				effect,
				av.WeightPercent,
				string.Format("{0} : ({1}) {2} {3} {4}"
					, effect
					, av.AffixId.PrettyFileNameExploded.Number
					, av.Translation
					, ChanceText(av)
					, av.AffixIdDlc.GetSuffix()
				),
				av.LootRandomizer
			)
			{ Unreachable = av.Unreachable }
			orderby flattenedAffix.Unreachable, flattenedAffix.Translation, flattenedAffix.AffixId.PrettyFileNameExploded.Number
			group flattenedAffix by new AffixGroupKey(flattenedAffix.TypeId, flattenedAffix.Translation, flattenedAffix.Unreachable) into grp2
			select new AffixGroup(grp2.Key, grp2);

		return regrouped.ToList();
	}

	/// <inheritdoc/>
	public string FormatVariantLabel(AffixEntry entry, bool byEffect)
		=> string.Format("({0}) {1} {2} {3}"
			, entry.AffixId.PrettyFileNameExploded.Number
			, byEffect
				? entry.LootRandomizer.Translation // groups are effects: show the affix name
				: entry.AffixId.PrettyFileNameExploded.Effect // groups are affix names: show the effect
			, ChanceText(entry)
			, entry.AffixIdDlc.GetSuffix()
		);

	/// <summary>The loot chance shown after an affix, "(—)" for an affix the game never rolls on the item.</summary>
	private static string ChanceText(AffixEntry entry)
		=> entry.Unreachable ? "(—)" : string.Format("({0:p2})", entry.WeightPercent);

	/// <summary>
	/// Flattens the broken, prefix and suffix tables of every DLC, keeps the highest weight of each affix,
	/// and groups the affixes by type and name.
	/// </summary>
	/// <remarks>
	/// For an item's own affixes each table is the distribution of one loot item table (see
	/// <see cref="LootTableCollection.Mix"/>): the highest is the best chance among the item's drop sources, given the item
	/// gets an affix of that kind. Absolute odds also depend on the kind weights, the chance and the monster, not shown.
	/// The affixes of <see cref="ItemAffixes.Unreachable"/> the game cannot roll otherwise come after, in their own groups,
	/// without a chance.
	/// </remarks>
	private static List<AffixGroup> CurateAffixes(ItemAffixes affixes)
	{
		var affixTypes = new[] { // UNPIVOT
			(AffixTypeId: BrokenTypeId, AffixType: affixes.Broken),
			(AffixTypeId: PrefixTypeId, AffixType: affixes.Prefix),
			(AffixTypeId: SuffixTypeId, AffixType: affixes.Suffix)
		};

		var reachable =
			from types in affixTypes
			from dlc in types.AffixType
			from ltvalues in dlc.Value
			from value in ltvalues
			select (TypeId: types.AffixTypeId, AffixId: value.Key, value.Value.WeightPercent, value.Value.LootRandomizer, Unreachable: false);
		var reachableList = reachable.ToList();
		var rolled = reachableList.Select(v => (v.TypeId, v.AffixId)).ToHashSet();

		var unreachable =
			from kindTable in affixes.Unreachable ?? []
			from value in kindTable.Table
			where !rolled.Contains((kindTable.Kind, value.Key))
			select (TypeId: kindTable.Kind, AffixId: value.Key, WeightPercent: 0F, value.Value.LootRandomizer, Unreachable: true);

		var curatedAffixes =
			from flat in reachableList.Concat(unreachable)
			group flat by new { flat.TypeId, flat.AffixId } into grp
			let f = grp.First()
			let _AffixIdDlc = grp.Key.AffixId.Dlc
			let _translation = f.LootRandomizer.Translation
			let _WeightPercent = grp.Max(v => v.WeightPercent)
			let affixEntry = new AffixEntry(
				grp.Key.TypeId,
				grp.Key.AffixId,
				_AffixIdDlc,
				_translation,
				_WeightPercent,
				string.Empty,
				f.LootRandomizer
			)
			{ Unreachable = f.Unreachable }
			let labelled = affixEntry with
			{
				FormattedText = string.Format("{0} : {1} {2} {3}" // Default format for Order by affix name
					, _translation
					, grp.Key.AffixId.PrettyFileName
					, ChanceText(affixEntry)
					, _AffixIdDlc.GetSuffix()
				),
			}
			group labelled by new AffixGroupKey(labelled.TypeId, labelled.Translation, labelled.Unreachable) into grp2
			orderby grp2.Key.TypeId, grp2.Key.Unreachable, grp2.Key.Translation // Order by affix name, the unreachable last
			select new AffixGroup(grp2.Key, grp2);

		return curatedAffixes.ToList();
	}

	private static List<BonusChoice> BuildBonusChoices(LootTableCollection? table, RecordId? currentBonusId)
	{
		if (table is not { Length: > 0 })
			return [];

		return table
			.Select(entry => new BonusChoice(
				entry.Key
				, string.Format("{0} : {1} ({2:p2}) {3}"
					, entry.Value.LootRandomizer.Translation
					, entry.Key.PrettyFileName
					, entry.Value.WeightPercent
					, entry.Key.Dlc.GetSuffix()
				)
				, entry.Value.LootRandomizer.Unknown
				, entry.Value.LootRandomizer.TranslationTagIsEmpty
				, entry.Key == currentBonusId
			))
			.OrderBy(c => c.Label)
			.ToList();
	}
}
