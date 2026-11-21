namespace TQVaultAE.Domain.Entities;

/// <summary>
/// Sack panel types
/// </summary>
public enum SackType
{
	/// <summary>
	/// Player panel
	/// </summary>
	Player = 0,

	/// <summary>
	/// Stash panel
	/// </summary>
	Stash,

	/// <summary>
	/// Equipment panel
	/// </summary>
	Equipment,

	/// <summary>
	/// Vault panel
	/// </summary>
	Vault,

	/// <summary>
	/// Trash panel
	/// </summary>
	Trash,

	/// <summary>
	/// A loadout of a character (saved equipment set): in the search only, no panel shows it.
	/// </summary>
	Loadout
}
