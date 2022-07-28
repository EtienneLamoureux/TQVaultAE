using System;
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
					.SelectMany(p => new[] { p.EquipmentSack }.Concat(p.Sacks))
					.Where(s => s is not null && s.Count > 0);
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

						int? Lvl = null, Str = null, Int = null, Dex = null;

						if (fnr.RequirementVariables.TryGetValue("LevelRequirement", out var varLvl))
							Lvl = varLvl.GetInt32(0);

						if (fnr.RequirementVariables.TryGetValue("Strength", out var varStr))
							Str = varStr.GetInt32(0);

						if (fnr.RequirementVariables.TryGetValue("Dexterity", out var varDex))
							Dex = varDex.GetInt32(0);

						if (fnr.RequirementVariables.TryGetValue("Intelligence", out var varIntel))
							Int = varIntel.GetInt32(0);

						return new
						{
							Item = i,
							FriendlyNames = fnr,
							Lvl,
							Str,
							Dex,
							Int
						};
					}).AsQueryable();

				if (hasSearch)
				{
					var (isRegex, _, regex, regexIsValid) = ToFriendlyNameResult.FulltextIsRegEx(this.HighlightSearch);

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
								!i.Lvl.HasValue // Item doesn't have requirement
								|| i.Lvl >= this.HighlightFilter.MinLvl
							);
						}
						// Min Dex
						if (this.HighlightFilter.MinDex != 0)
						{
							availableItems = availableItems.Where(i =>
								!i.Dex.HasValue
								|| i.Dex >= this.HighlightFilter.MinDex
							);
						}
						// Min Str
						if (this.HighlightFilter.MinStr != 0)
						{
							availableItems = availableItems.Where(i =>
								!i.Str.HasValue
								|| i.Str >= this.HighlightFilter.MinStr
							);
						}
						// Min Int
						if (this.HighlightFilter.MinInt != 0)
						{
							availableItems = availableItems.Where(i =>
								!i.Int.HasValue
								|| i.Int >= this.HighlightFilter.MinInt
							);
						}
					}

					if (this.HighlightFilter.MaxRequierement)
					{
						// Max Lvl
						if (this.HighlightFilter.MaxLvl != 0)
						{
							availableItems = availableItems.Where(i =>
								!i.Lvl.HasValue // Item doesn't have requirement
								|| i.Lvl <= this.HighlightFilter.MaxLvl
							);
						}
						// Max Dex
						if (this.HighlightFilter.MaxDex != 0)
						{
							availableItems = availableItems.Where(i =>
								!i.Dex.HasValue
								|| i.Dex <= this.HighlightFilter.MaxDex
							);
						}
						// Max Str
						if (this.HighlightFilter.MaxStr != 0)
						{
							availableItems = availableItems.Where(i =>
								!i.Str.HasValue
								|| i.Str <= this.HighlightFilter.MaxStr
							);
						}
						// Max Int
						if (this.HighlightFilter.MaxInt != 0)
						{
							availableItems = availableItems.Where(i =>
								!i.Int.HasValue
								|| i.Int <= this.HighlightFilter.MaxInt
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
							this.HighlightFilter.Rarity.Contains(i.Item.GearLevel)
						);
					}

					if (this.HighlightFilter.Origin.Any())
					{
						availableItems = availableItems.Where(i =>
							this.HighlightFilter.Origin.Contains(i.Item.GameExtension)
						);
					}

					if (this.HighlightFilter.HavingPrefix)
						availableItems = availableItems.Where(i => i.Item.HasPrefix);

					if (this.HighlightFilter.HavingSuffix)
						availableItems = availableItems.Where(i => i.Item.HasSuffix);
				}

				this.HighlightedItems.AddRange(availableItems.Select(i => i.Item));
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
