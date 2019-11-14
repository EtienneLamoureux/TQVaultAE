using System.Collections.Generic;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Domain.Entities
{
	/// <summary>
	/// Shared data context for all services
	/// </summary>
	/// <remarks>must be agnostic so no Winform references. Only data</remarks>
	public class SessionContext
	{
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

	}
}
