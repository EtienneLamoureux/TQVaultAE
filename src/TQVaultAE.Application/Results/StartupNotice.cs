namespace TQVaultAE.Application.Results;

/// <summary>
/// A message to show while the game paths are set up at startup.
/// </summary>
public enum StartupNotice
{
	/// <summary>Nothing to show.</summary>
	None,

	/// <summary>No vault path is configured: the default folder is used.</summary>
	DefaultVaultPath,

	/// <summary>No vault path is configured and the vault folder is still given by a legacy shortcut.</summary>
	LegacyVaultShortcut,
}
