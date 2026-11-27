namespace TQVaultAE.Application.DTOs;

/// <summary>
/// The bag tooltip entries of the vault bag menu.
/// </summary>
public enum BagTooltipChange
{
	/// <summary>Shows the tooltip of the bag again.</summary>
	Enable,

	/// <summary>Hides the tooltip of the bag.</summary>
	Disable,

	/// <summary>Shows the tooltip of every bag.</summary>
	EnableAll,

	/// <summary>Hides the tooltip of every bag.</summary>
	DisableAll,
}
