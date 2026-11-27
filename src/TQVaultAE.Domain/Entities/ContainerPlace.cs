namespace TQVaultAE.Domain.Entities;

/// <summary>
/// Represents the location of an item within the container hierarchy
/// </summary>
public record ContainerPlace
{
	/// <summary>
	/// Gets or sets the container file path (vault/player/stash file)
	/// </summary>
	public string Path { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the container display name
	/// </summary>
	public string Name { get; set; } = string.Empty;

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

	/// <summary>
	/// Gets or sets the loadout's name, for <see cref="SackType.Loadout"/>; null otherwise
	/// </summary>
	public string? Loadout { get; set; }

	/// <summary>
	/// Copies another place into this one, keeping this instance: the item and its search result share it.
	/// </summary>
	/// <param name="other">The place to copy.</param>
	public void CopyFrom(ContainerPlace other)
	{
		this.SackType = other.SackType;
		this.Path = other.Path;
		this.Name = other.Name;
		this.StashType = other.StashType;
		this.SackNumber = other.SackNumber;
		this.Loadout = other.Loadout;
	}
}
