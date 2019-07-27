using System.Collections.Generic;

namespace TQVaultAE.Domain.Entities
{
	/// <summary>
	/// Shared data context for all services
	/// </summary>
	/// <remarks>must be agnostic so no Winform references. Only data</remarks>
	public class SessionContext
	{

		/// <summary>
		/// Dictionary of all loaded player files
		/// </summary>
		public readonly Dictionary<string, PlayerCollection> Players = new Dictionary<string, PlayerCollection>();

		/// <summary>
		/// Dictionary of all loaded vault files
		/// </summary>
		public readonly Dictionary<string, PlayerCollection> Vaults = new Dictionary<string, PlayerCollection>();

		/// <summary>
		/// Dictionary of all loaded player stash files
		/// </summary>
		public readonly Dictionary<string, Stash> Stashes = new Dictionary<string, Stash>();

	}
}
