﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Domain.Entities
{
	/// <summary>
	/// Shared data context for all services
	/// </summary>
	/// <remarks>must be agnostic so no Winform references. Only data</remarks>
	public class SessionContext
	{
		private readonly IItemProvider ItemProvider;

		public SessionContext(IItemProvider ItemProvider)
		{
			this.ItemProvider = ItemProvider;
		}

		/// <summary>
		/// Currently selected player
		/// </summary>
		public PlayerCollection CurrentPlayer { get; set; }

		private BagButtonIconInfo iconInfoCopy;
		/// <summary>
		/// Last icon info copied
		/// </summary>
		public BagButtonIconInfo IconInfoCopy
		{
			get => iconInfoCopy;
			set
			{
				iconInfoCopy = value;
				iconInfoCopied = true;
			}
		}

		private bool iconInfoCopied;
		/// <summary>
		/// Is there any IconInfo copied
		/// </summary>
		/// <remarks>this allow <see cref="IconInfoCopy"/> to have null relevant</remarks>
		public bool IconInfoCopied => iconInfoCopied;
		/// <summary>
		/// Dictionary of all loaded player files
		/// </summary>
		public readonly LazyConcurrentDictionary<string, PlayerCollection> Players = new LazyConcurrentDictionary<string, PlayerCollection>();

		/// <summary>
		/// Dictionary of all loaded vault files
		/// </summary>
		public readonly LazyConcurrentDictionary<string, PlayerCollection> Vaults = new LazyConcurrentDictionary<string, PlayerCollection>();

		/// <summary>
		/// Dictionary of all loaded player stash files
		/// </summary>
		public readonly LazyConcurrentDictionary<string, Stash> Stashes = new LazyConcurrentDictionary<string, Stash>();

		#region HighlightSearchItem

		/// <summary>
		/// Gets or sets the background Color for item highlight.
		/// </summary>
		public Color HighlightSearchItemColor { get; set; } = TQColor.Indigo.Color();
		/// <summary>
		/// Gets or sets the border color for item highlight.
		/// </summary>
		public Color HighlightSearchItemBorderColor { get; set; } = TQColor.Red.Color();

		/// <summary>
		/// Hightlight search string
		/// </summary>
		public string HighlightSearch { get; set; }

		/// <summary>
		/// Hightlight search filters
		/// </summary>
		public HighlightFilterValues HighlightFilter { get; set; }

		/// <summary>
		/// Hightlight search items
		/// </summary>
		public readonly List<Item> HighlightedItems = new();

		/// <summary>
		/// Find items to highlight
		/// </summary>
		public void FindHighlight()
		{
			var hasSearch = !string.IsNullOrWhiteSpace(this.HighlightSearch);
			var hasFilter = this.HighlightFilter is not null;

			if (hasSearch || hasFilter)
			{
				this.HighlightedItems.Clear();
				
				// Check for players
				var sacksplayers = this.Players.Select(p => p.Value.Value)
					.SelectMany(p => {
						var retval = new List<SackCollection>();

						if (p.EquipmentSack is not null)
							retval.Add(p.EquipmentSack);

						if (p.Sacks is not null)
							retval.AddRange(p.Sacks);

						return retval;
					})
					.Where(s => s.Count > 0);

				// Check for Vaults
				var sacksVault = this.Vaults.Select(p => p.Value.Value)
					.SelectMany(p => p.Sacks)
					.Where(s => s is not null && s.Count > 0);

				// Check for Stash
				var sacksStash = this.Stashes.Select(p => p.Value.Value)
					.Select(p => p.Sack)
					.Where(s => s is not null && s.Count > 0);

				var availableItems = sacksplayers.Concat(sacksVault).Concat(sacksStash).SelectMany(i => i)
					.Select(i =>
					{
						var fnr = ItemProvider.GetFriendlyNames(i, FriendlyNamesExtraScopes.ItemFullDisplay);

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
						// Min Lvl
						if (this.HighlightFilter.MinLvl != 0)
						{
							availableItems = availableItems.Where(i =>
								!i.Info.Lvl.HasValue // Item doesn't have requirement
								|| i.Info.Lvl >= this.HighlightFilter.MinLvl
							);
						}
						// Min Dex
						if (this.HighlightFilter.MinDex != 0)
						{
							availableItems = availableItems.Where(i =>
								!i.Info.Dex.HasValue
								|| i.Info.Dex >= this.HighlightFilter.MinDex
							);
						}
						// Min Str
						if (this.HighlightFilter.MinStr != 0)
						{
							availableItems = availableItems.Where(i =>
								!i.Info.Str.HasValue
								|| i.Info.Str >= this.HighlightFilter.MinStr
							);
						}
						// Min Int
						if (this.HighlightFilter.MinInt != 0)
						{
							availableItems = availableItems.Where(i =>
								!i.Info.Int.HasValue
								|| i.Info.Int >= this.HighlightFilter.MinInt
							);
						}
					}

					if (this.HighlightFilter.MaxRequierement)
					{
						// Max Lvl
						if (this.HighlightFilter.MaxLvl != 0)
						{
							availableItems = availableItems.Where(i =>
								!i.Info.Lvl.HasValue // Item doesn't have requirement
								|| i.Info.Lvl <= this.HighlightFilter.MaxLvl
							);
						}
						// Max Dex
						if (this.HighlightFilter.MaxDex != 0)
						{
							availableItems = availableItems.Where(i =>
								!i.Info.Dex.HasValue
								|| i.Info.Dex <= this.HighlightFilter.MaxDex
							);
						}
						// Max Str
						if (this.HighlightFilter.MaxStr != 0)
						{
							availableItems = availableItems.Where(i =>
								!i.Info.Str.HasValue
								|| i.Info.Str <= this.HighlightFilter.MaxStr
							);
						}
						// Max Int
						if (this.HighlightFilter.MaxInt != 0)
						{
							availableItems = availableItems.Where(i =>
								!i.Info.Int.HasValue
								|| i.Info.Int <= this.HighlightFilter.MaxInt
							);
						}
					}

					if (this.HighlightFilter.ClassItem.Any())
					{
						availableItems = availableItems.Where(i =>
							this.HighlightFilter.ClassItem
							.Any(ci => ci.Equals(i.Item.ItemClass, StringComparison.OrdinalIgnoreCase))
						);
					}

					if (this.HighlightFilter.Rarity.Any())
					{
						availableItems = availableItems.Where(i =>
							this.HighlightFilter.Rarity.Contains(i.Item.Rarity)
						);
					}

					if (this.HighlightFilter.Origin.Any())
					{
						availableItems = availableItems.Where(i =>
							this.HighlightFilter.Origin.Contains(i.Item.GameDlc)
						);
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

				this.HighlightedItems.AddRange(availableItems.Select(i => i.Item).ToList());
				return;
			}
			ResetHighlight();
		}

		/// <summary>
		/// Reset Hightlight search
		/// </summary>
		public void ResetHighlight()
		{
			this.HighlightedItems.Clear();
			this.HighlightSearch = null;
			this.HighlightFilter = null;
		}

		#endregion

	}
}
