namespace TQVaultAE.Application.DTOs;

/// <summary>
/// The vault maintenance functions.
/// </summary>
public enum VaultMaintenance
{
	/// <summary>
	/// Nothing selected
	/// </summary>
	None = 0,

	/// <summary>
	/// Create a new vault
	/// </summary>
	New,

	/// <summary>
	/// Copy a vault
	/// </summary>
	Copy,

	/// <summary>
	/// Delete a vault
	/// </summary>
	Delete,

	/// <summary>
	/// Rename a vault
	/// </summary>
	Rename
}

/// <summary>
/// Why a vault maintenance request is refused.
/// </summary>
public enum VaultMaintenanceError
{
	/// <summary>
	/// The request is valid.
	/// </summary>
	None = 0,

	/// <summary>
	/// The target name is empty.
	/// </summary>
	EmptyName,

	/// <summary>
	/// The target name holds characters not allowed in a file name.
	/// </summary>
	InvalidCharacters,

	/// <summary>
	/// The source vault is not in the vault list.
	/// </summary>
	SourceMissing,

	/// <summary>
	/// A vault with the target name exists already (case ignored).
	/// </summary>
	DuplicateName,
}
