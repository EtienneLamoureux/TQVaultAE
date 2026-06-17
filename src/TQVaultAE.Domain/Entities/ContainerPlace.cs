namespace TQVaultAE.Domain.Entities;

/// <summary>
/// Represents the location of an item within the container hierarchy
/// </summary>
public record ContainerPlace
{
	/// <summary>
	/// Gets or sets the container file path (vault/player/stash file)
	/// </summary>
	public string Path { get; set; }

	/// <summary>
	/// Gets or sets the container display name
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the sack number within the container
	/// </summary>
	public int SackNumber { get; set; }

	/// <summary>
	/// Gets or sets the sack type (Sack, Player, Vault, Equipment, Trash)
	/// </summary>
	public SackType SackType { get; set; }

	/// <summary>
	/// Gets or sets the stash type (Stash, TransferStash, RelicVaultStash)
	/// Null if not a stash
	/// </summary>
	public StashType? StashType { get; set; }
}
