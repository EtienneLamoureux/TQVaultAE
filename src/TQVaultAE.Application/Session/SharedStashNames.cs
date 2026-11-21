using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Session;

/// <summary>
/// The names the services give the stashes shared by the characters of a map: neutral names, used in the search index,
/// the backup file names and the logs. The UI shows them translated (<c>SharedStashText</c> in Presentation).
/// </summary>
public static class SharedStashNames
{
	/// <summary>The name of the transfer stash.</summary>
	public const string TransferStash = "Transfer Stash";

	/// <summary>The name of the relic vault.</summary>
	public const string RelicVault = "Relic Vault";

	/// <summary>
	/// Gets the name of a shared stash.
	/// </summary>
	/// <param name="type">The transfer stash or the relic vault.</param>
	/// <returns>Its name, or null for another stash type.</returns>
	public static string? Of(StashType? type) => type switch
	{
		StashType.TransferStash => TransferStash,
		StashType.RelicVaultStash => RelicVault,
		_ => null,
	};
}
