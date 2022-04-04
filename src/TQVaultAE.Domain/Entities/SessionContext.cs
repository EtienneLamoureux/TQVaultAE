using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Helpers;

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
		/// Hightlight search items
		/// </summary>
		public readonly List<Item> HighlightedItems = new();

		/// <summary>
		/// Find items to highlight
		/// </summary>
		public void FindHighlight()
		{
			if (!string.IsNullOrWhiteSpace(this.HighlightSearch))
			{
				this.HighlightedItems.Clear();

				// Check for players
				var sacksplayers = this.Players.Select(p => p.Value.Value)
					.SelectMany(p => new[] { p.EquipmentSack }.Concat(p.Sacks));
				// Check for Vaults
				var sacksVault = this.Vaults.Select(p => p.Value.Value)
					.SelectMany(p => p.Sacks);
				// Check for Stash
				var sacksStash = this.Stashes.Select(p => p.Value.Value)
					.Select(p => p.Sack);

				var foundItems = sacksplayers.Concat(sacksVault).Concat(sacksStash)
					.SelectMany(i => i)
					.Where(i =>
						ItemProvider
						.GetFriendlyNames(i, FriendlyNamesExtraScopes.ItemFullDisplay)
						.FullText.IndexOf(
							this.HighlightSearch, StringComparison.OrdinalIgnoreCase
						) != -1
					);

				this.HighlightedItems.AddRange(foundItems);
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
		}

		#endregion

	}
}
