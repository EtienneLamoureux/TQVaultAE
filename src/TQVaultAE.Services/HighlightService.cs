using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Services;

/// <summary>
/// Service for managing item highlight search functionality.
/// </summary>
public class HighlightService(
    ILogger<HighlightService> log,
    IItemProvider itemProvider,
    SessionContext sessionContext,
    ITranslationService translationService) : IHighlightService
{
    private readonly ILogger<HighlightService> _log = log;
    private readonly IItemProvider _itemProvider = itemProvider;
    private readonly SessionContext _sessionContext = sessionContext;
    private readonly ITranslationService _translationService = translationService;

    public Color HighlightSearchItemColor { get; set; } = TQColor.Indigo.Color();
    public Color HighlightSearchItemBorderColor { get; set; } = TQColor.Red.Color();
    public string? HighlightSearch { get; set; }
    public HighlightFilterValues? HighlightFilter { get; set; }
    public IReadOnlyList<Item> HighlightedItems { get; private set; } = new List<Item>();

    /// <inheritdoc/>
    public void FindHighlight()
    {
        var hasSearch = !string.IsNullOrWhiteSpace(this.HighlightSearch);
        var hasFilter = this.HighlightFilter is not null;

        if (hasSearch || hasFilter)
        {
            var items = new List<Item>();

            // Check for players
            var sacksplayers = this._sessionContext.Players.Select(p => p.Value.Value)
                .SelectMany(p =>
                {
                    var retval = new List<SackCollection>();

                    if (p.EquipmentSack is not null)
                        retval.Add(p.EquipmentSack);

                    if (p.Sacks is not null)
                        retval.AddRange(p.Sacks);

                    return retval;
                })
                .Where(s => s.Count > 0);

            // Check for Vaults
            var sacksVault = this._sessionContext.Vaults.Select(p => p.Value.Value)
                .SelectMany(p => p.Sacks ?? [])
                .Where(s => s.Count > 0);

            // Check for Stash
            var sacksStash = this._sessionContext.Stashes.Select(p => p.Value.Value)
                .Select(p => p.Sack)
                .OfType<SackCollection>()
                .Where(s => s.Count > 0);

            var availableItems = sacksplayers.Concat(sacksVault).Concat(sacksStash).SelectMany(i => i)
                .Select(i =>
                {
                    var fnr = this._itemProvider.GetFriendlyNames(i, FriendlyNamesExtraScopes.ItemFullDisplay);

                    return new
                    {
                        Item = i,
                        FriendlyNames = fnr,
                        Info = fnr.RequirementInfo,
                    };
                });

            if (hasSearch)
            {
                var (isRegex, _, regex, regexIsValid) = StringHelper.IsTQVaultSearchRegEx(this.HighlightSearch);

                availableItems = availableItems.Where(i =>
                    isRegex && regexIsValid
                        ? i.FriendlyNames.FulltextIsMatchRegex(regex!)
                        : i.FriendlyNames.FulltextIsMatchIndexOf(this.HighlightSearch!)
                );
            }

            if (this.HighlightFilter is { } filter)
            {
                // Bounds not ticked are no bounds.
                var bounds = new RequirementBounds(
                    filter.MinRequierement ? filter.MinLvl : 0, filter.MaxRequierement ? filter.MaxLvl : 0,
                    filter.MinRequierement ? filter.MinStr : 0, filter.MaxRequierement ? filter.MaxStr : 0,
                    filter.MinRequierement ? filter.MinDex : 0, filter.MaxRequierement ? filter.MaxDex : 0,
                    filter.MinRequierement ? filter.MinInt : 0, filter.MaxRequierement ? filter.MaxInt : 0);
                if (!bounds.IsEmpty)
                    availableItems = availableItems.Where(i => bounds.Matches(i.Info));

                if (filter.ClassItem.Count != 0)
                {
                    availableItems = availableItems.Where(i =>
                        filter.ClassItem
                            .Any(ci => ci.Equals(i.Item.ItemClass, StringComparison.OrdinalIgnoreCase)));
                }

                if (filter.Rarity.Count != 0)
                {
                    availableItems = availableItems.Where(i =>
                        filter.Rarity.Contains(i.Item.Rarity));
                }

                if (filter.Origin.Count != 0)
                {
                    availableItems = availableItems.Where(i =>
                        filter.Origin.Contains(i.Item.GameDlc));
                }

                var flags = new ItemFlagFilter(filter.HavingPrefix, filter.HavingSuffix, filter.HavingRelic, filter.HavingCharm, filter.IsSetItem, filter.HavingSkill);
                if (!flags.IsEmpty)
                    availableItems = availableItems.Where(i => flags.Matches(i.Item, i.FriendlyNames, this._itemProvider.HasGrantedSkill));
            }

            this.HighlightedItems = availableItems.Select(i => i.Item).ToList();
            this._log.LogInformation("Highlight search found {Count} items", this.HighlightedItems.Count);
            return;
        }

        ResetHighlight();
    }

    /// <inheritdoc/>
    public void ResetHighlight()
    {
        this.HighlightedItems = new List<Item>();
        this.HighlightSearch = null;
        this.HighlightFilter = null;
    }

    /// <inheritdoc/>
    public bool ApplyFilter(HighlightFilterValues? filter)
    {
        this.HighlightFilter = filter is { IsEmpty: false } ? filter : null;
        this.FindHighlight();
        return this.HighlightFilter is not null;
    }

    /// <inheritdoc/>
    public IReadOnlyList<HighlightTypeOption> GetTypeOptions()
    {
        // The names of some classes end with " :" or "~" in the game texts.
        HighlightTypeOption Of(string itemClass, params char[] trim)
        {
            var name = this._translationService.TranslateXTag(Item.GetClassTagName(itemClass));
            return new(trim.Length == 0 ? name : name.Trim(trim), [itemClass]);
        }

        return
        [
            Of(Item.ICLASS_HEAD, ' ', ':'),
            Of(Item.ICLASS_FOREARM, ' ', ':'),
            Of(Item.ICLASS_UPPERBODY, ' ', ':'),
            Of(Item.ICLASS_LOWERBODY, ' ', ':'),
            Of(Item.ICLASS_AXE),
            Of(Item.ICLASS_MACE),
            Of(Item.ICLASS_SWORD),
            Of(Item.ICLASS_SHIELD),
            new(this._translationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_RANGEDONEHAND), true), [Item.ICLASS_RANGEDONEHAND]),
            Of(Item.ICLASS_BOW),
            Of(Item.ICLASS_SPEAR),
            Of(Item.ICLASS_STAFF, ' ', ':'),
            Of(Item.ICLASS_AMULET),
            Of(Item.ICLASS_RING),
            Of(Item.ICLASS_ARTIFACT),
            Of(Item.ICLASS_FORMULA),
            Of(Item.ICLASS_RELIC),
            Of(Item.ICLASS_CHARM),
            Of(Item.ICLASS_SCROLL),
            // Potions and eternal scrolls are one type.
            new(this._translationService.TranslateXTag("tagTutorialTip14Title"), [Item.ICLASS_POTIONMANA, Item.ICLASS_POTIONHEALTH, Item.ICLASS_SCROLL_ETERNAL]),
            Of(Item.ICLASS_QUESTITEM),
            Of(Item.ICLASS_EQUIPMENT, ' ', '~'),
            Of(Item.ICLASS_DYE),
        ];
    }
}
