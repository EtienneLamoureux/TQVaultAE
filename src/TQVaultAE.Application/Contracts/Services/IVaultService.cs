using TQVaultAE.Application.Results;

namespace TQVaultAE.Application.Contracts.Services;

public interface IVaultService
{
	/// <summary>
	/// Creates a new empty vault file.
	/// </summary>
	/// <param name="name">Name of the vault.</param>
	/// <param name="file">File name of the vault.</param>
	/// <returns>Player instance of the new vault.</returns>
	PlayerCollection CreateVault(string name, string file);

	/// <summary>
	/// Creates a new empty vault with auto-generated filename.
	/// </summary>
	/// <param name="name">Name of the vault.</param>
	/// <returns>Player instance of the new vault.</returns>
	PlayerCollection CreateVault(string name);

	/// <summary>
	/// Loads a vault file.
	/// </summary>
	/// <param name="vaultName">Name of the vault</param>
	/// <returns>Vault load result with vault and error information.</returns>
	VaultLoadResult LoadVault(string vaultName);

	/// <summary>
	/// Gets the list of available vault names.
	/// </summary>
	/// <returns>List of vault names.</returns>
	IReadOnlyList<string> GetVaultList();

	/// <summary>
	/// Deletes a vault file.
	/// </summary>
	/// <param name="vaultName">Name of the vault to delete.</param>
	/// <returns>True if deletion succeeded, false otherwise.</returns>
	bool DeleteVault(string vaultName);

	/// <summary>
	/// Renames a vault file.
	/// </summary>
	/// <param name="oldName">Current name of the vault.</param>
	/// <param name="newName">New name for the vault.</param>
	/// <returns>True if rename succeeded, false otherwise.</returns>
	bool RenameVault(string oldName, string newName);

	/// <summary>
	/// Attempts to save all modified vault files.
	/// </summary>
	/// <param name="vaultOnError">If save fails, this will contain the vault that failed.</param>
	/// <returns>Number of vaults saved.</returns>
	/// <exception cref="IOException">can happen during file save</exception>
	int SaveAllModifiedVaults(ref PlayerCollection vaultOnError);

	/// <summary>
	/// Updates VaultPath key from the configuration UI.
	/// Needed since all vaults will need to be reloaded if this key changes.
	/// </summary>
	/// <param name="vaultPath">Path to the vault files</param>
	void UpdateVaultPath(string vaultPath);

	/// <summary>
	/// Shows the vault maintenance dialog.
	/// </summary>
	void ShowVaultMaintenance();
}
