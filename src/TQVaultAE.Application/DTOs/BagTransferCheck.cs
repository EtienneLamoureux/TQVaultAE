namespace TQVaultAE.Application.DTOs;

/// <summary>
/// Whether a bag can be copied or merged onto another bag of the same vault.
/// </summary>
public enum BagTransferCheck
{
	/// <summary>
	/// The source or the destination is not a bag of the vault, or they are the same bag.
	/// </summary>
	Invalid = 0,

	/// <summary>
	/// The destination bag is empty.
	/// </summary>
	Allowed,

	/// <summary>
	/// The destination bag holds items: copying over it loses them.
	/// </summary>
	DestinationNotEmpty,
}
