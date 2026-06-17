using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Services;

/// <summary>
/// Service for managing item highlight search functionality.
/// </summary>
public class HighlightService : IHighlightService
{
    private readonly ILogger<HighlightService> _log;
    private readonly IItemProvider _itemProvider;
    private readonly SessionContext _sessionContext;

    public Color HighlightSearchItemColor { get; set; } = TQColor.Indigo.Color();
    public Color HighlightSearchItemBorderColor { get; set; } = TQColor.Red.Color();
    public string HighlightSearch { get; set; }
    public HighlightFilterValues HighlightFilter { get; set; }
    public IReadOnlyList<Item> HighlightedItems { get; private set; } = new List<Item>();

    public HighlightService(
        ILogger<HighlightService> log,
        IItemProvider itemProvider,
        SessionContext sessionContext)
    {
        this._log = log;
        this._itemProvider = itemProvider;
        this._sessionContext = sessionContext;
    }

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
                .SelectMany(p => p.Sacks)
                .Where(s => s is not null && s.Count > 0);

            // Check for Stash
            var sacksStash = this._sessionContext.Stashes.Select(p => p.Value.Value)
                .Select(p => p.Sack)
                .Where(s => s is not null && s.Count > 0);

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
                }).AsQueryable();

            if (hasSearch)
            {
                var (isRegex, _, regex, regexIsValid) = StringHelper.IsTQVaultSearchRegEx(this.HighlightSearch);

                availableItems = availableItems.Where(i =>
                    isRegex && regexIsValid
                        ? i.FriendlyNames.FulltextIsMatchRegex(regex)
                        : i.FriendlyNames.FulltextIsMatchIndexOf(this.HighlightSearch)
                );
            }

            if (hasFilter)
            {
                if (this.HighlightFilter.MinRequierement)
                {
                    if (this.HighlightFilter.MinLvl != 0)
                    {
                        availableItems = availableItems.Where(i =>
                            !i.Info.Lvl.HasValue || i.Info.Lvl >= this.HighlightFilter.MinLvl);
                    }
                    if (this.HighlightFilter.MinDex != 0)
                    {
                        availableItems = availableItems.Where(i =>
                            !i.Info.Dex.HasValue || i.Info.Dex >= this.HighlightFilter.MinDex);
                    }
                    if (this.HighlightFilter.MinStr != 0)
                    {
                        availableItems = availableItems.Where(i =>
                            !i.Info.Str.HasValue || i.Info.Str >= this.HighlightFilter.MinStr);
                    }
                    if (this.HighlightFilter.MinInt != 0)
                    {
                        availableItems = availableItems.Where(i =>
                            !i.Info.Int.HasValue || i.Info.Int >= this.HighlightFilter.MinInt);
                    }
                }

                if (this.HighlightFilter.MaxRequierement)
                {
                    if (this.HighlightFilter.MaxLvl != 0)
                    {
                        availableItems = availableItems.Where(i =>
                            !i.Info.Lvl.HasValue || i.Info.Lvl <= this.HighlightFilter.MaxLvl);
                    }
                    if (this.HighlightFilter.MaxDex != 0)
                    {
                        availableItems = availableItems.Where(i =>
                            !i.Info.Dex.HasValue || i.Info.Dex <= this.HighlightFilter.MaxDex);
                    }
                    if (this.HighlightFilter.MaxStr != 0)
                    {
                        availableItems = availableItems.Where(i =>
                            !i.Info.Str.HasValue || i.Info.Str <= this.HighlightFilter.MaxStr);
                    }
                    if (this.HighlightFilter.MaxInt != 0)
                    {
                        availableItems = availableItems.Where(i =>
                            !i.Info.Int.HasValue || i.Info.Int <= this.HighlightFilter.MaxInt);
                    }
                }

                if (this.HighlightFilter.ClassItem.Any())
                {
                    availableItems = availableItems.Where(i =>
                        this.HighlightFilter.ClassItem
                            .Any(ci => ci.Equals(i.Item.ItemClass, StringComparison.OrdinalIgnoreCase)));
                }

                if (this.HighlightFilter.Rarity.Any())
                {
                    availableItems = availableItems.Where(i =>
                        this.HighlightFilter.Rarity.Contains(i.Item.Rarity));
                }

                if (this.HighlightFilter.Origin.Any())
                {
                    availableItems = availableItems.Where(i =>
                        this.HighlightFilter.Origin.Contains(i.Item.GameDlc));
                }

                if (this.HighlightFilter.HavingPrefix)
                    availableItems = availableItems.Where(i => i.Item.HasPrefix);

                if (this.HighlightFilter.HavingSuffix)
                    availableItems = availableItems.Where(i => i.Item.HasSuffix);

                if (this.HighlightFilter.HavingRelic)
                    availableItems = availableItems.Where(i => i.Item.HasRelic);

                if (this.HighlightFilter.HavingCharm)
                    availableItems = availableItems.Where(i => i.Item.HasCharm);

                if (this.HighlightFilter.IsSetItem)
                    availableItems = availableItems.Where(i => i.FriendlyNames.ItemSet != null);
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
}
