namespace TQVaultAE.Domain.Entities;

/// <summary>
/// Stash panel types
/// </summary>
public enum StashType
{
	/// <summary>
	/// Stash panel
	/// </summary>
	PlayerStash = BagIdConstants.BAGID_PLAYERSTASH,

	/// <summary>
	/// Transfer stash
	/// </summary>
	TransferStash = BagIdConstants.BAGID_TRANSFERSTASH,

	/// <summary>
	/// Relic Vault stash
	/// </summary>
	RelicVaultStash = BagIdConstants.BAGID_RELICVAULTSTASH
}
