using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;

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
	/// Orders the vault names for a vault drop down list: the leading entry, then the Main Vault, then the others in
	/// their given order.
	/// </summary>
	/// <param name="vaultNames">The vault names, for example from <see cref="IGamePathService.GetVaultList"/>.</param>
	/// <param name="leadingEntry">An entry put first, such as the maintenance or "select a vault" entry; none when null.</param>
	/// <param name="excludedVault">A vault left out, such as the one open in the other vault panel; none when null.</param>
	/// <param name="alwaysIncludeMainVault">True to list the Main Vault even when <paramref name="vaultNames"/> lacks it
	/// (its file is created on first load).</param>
	/// <returns>The ordered names.</returns>
	IReadOnlyList<string> BuildVaultChoices(IEnumerable<string> vaultNames, string? leadingEntry = null, string? excludedVault = null, bool alwaysIncludeMainVault = true);

	/// <summary>
	/// Checks a bag copy or merge from <paramref name="source"/> onto <paramref name="destination"/> within a vault.
	/// </summary>
	/// <param name="vault">The vault.</param>
	/// <param name="source">Index of the source bag.</param>
	/// <param name="destination">Index of the destination bag.</param>
	/// <returns>Whether the transfer can run and whether it would overwrite items.</returns>
	BagTransferCheck CheckBagTransfer(PlayerCollection vault, int source, int destination);

	/// <summary>
	/// Empties a bag ("Empty Bag", or a bag about to be overwritten by "Copy Bag"): its items are removed and
	/// dropped from the search database, so searches no longer find them.
	/// </summary>
	/// <param name="sack">The bag to empty.</param>
	void EmptyBag(SackCollection sack);

	/// <summary>
	/// Shows or hides the tooltip of a vault bag, or of every bag. The list is saved in the vault file, so the vault is
	/// marked modified when it changes.
	/// </summary>
	/// <param name="vault">The vault.</param>
	/// <param name="change">What the menu entry does.</param>
	/// <param name="bag">The bag of <see cref="BagTooltipChange.Enable"/> and <see cref="BagTooltipChange.Disable"/>.</param>
	/// <param name="bagCount">The number of bag buttons, for <see cref="BagTooltipChange.DisableAll"/>.</param>
	void ChangeBagTooltip(PlayerCollection vault, BagTooltipChange change, int bag, int bagCount);

	/// <summary>
	/// Builds the CSV export of a vault, or of one of its bags: a header line, then one line per item.
	/// </summary>
	/// <param name="vault">The vault.</param>
	/// <param name="bagIndex">Index of the bag to export; the whole vault when null.</param>
	/// <param name="delimiter">The CSV delimiter.</param>
	/// <returns>The CSV text, lines separated by <see cref="Environment.NewLine"/>.</returns>
	string BuildCsvExport(PlayerCollection vault, int? bagIndex, char delimiter);

	/// <summary>
	/// Checks a vault maintenance request before it runs.
	/// </summary>
	/// <param name="action">The maintenance function.</param>
	/// <param name="source">The vault to copy, delete or rename; ignored for <see cref="VaultMaintenance.New"/>.</param>
	/// <param name="target">The new vault name, already trimmed; ignored for <see cref="VaultMaintenance.Delete"/>.</param>
	/// <param name="existingVaults">The names of the existing vaults.</param>
	/// <returns><see cref="VaultMaintenanceError.None"/> when the request is valid, else the first problem found.</returns>
	VaultMaintenanceError ValidateMaintenance(VaultMaintenance action, string? source, string? target, IEnumerable<string> existingVaults);

	/// <summary>
	/// Copies a vault file when the source exists and the target does not.
	/// </summary>
	/// <param name="sourceName">Name of the vault to copy.</param>
	/// <param name="targetName">Name of the new vault.</param>
	/// <returns>True if the file was copied.</returns>
	bool CopyVault(string sourceName, string targetName);

	/// <summary>
	/// Deletes a vault file when it exists and drops it from the session cache.
	/// </summary>
	/// <param name="vaultName">Name of the vault to delete.</param>
	/// <returns>True if a file was deleted.</returns>
	bool DeleteVault(string vaultName);

	/// <summary>
	/// Renames a vault file when the old one exists and the new one does not,
	/// and drops both names from the session cache.
	/// </summary>
	/// <param name="oldName">Current name of the vault.</param>
	/// <param name="newName">New name for the vault.</param>
	/// <returns>True if the file was renamed.</returns>
	bool RenameVault(string oldName, string newName);

	/// <summary>
	/// Gets the loaded vaults that have unsaved changes, with their file.
	/// </summary>
	IReadOnlyList<(string File, PlayerCollection Vault)> GetModifiedVaults();

	/// <summary>
	/// Backs up (unless legacy backups are disabled) and saves one vault file, then marks the vault saved.
	/// </summary>
	/// <exception cref="IOException">The file could not be written.</exception>
	/// <exception cref="UnauthorizedAccessException">The file or folder cannot be written to.</exception>
	void SaveModifiedVault(string file, PlayerCollection vault);

	/// <summary>
	/// Updates VaultPath key from the configuration UI.
	/// Needed since all vaults will need to be reloaded if this key changes.
	/// </summary>
	/// <param name="vaultPath">Path to the vault files</param>
	void UpdateVaultPath(string vaultPath);

	/// <summary>
	/// Saves a vault to disk.
	/// </summary>
	/// <param name="vault">The vault to save.</param>
	/// <param name="vaultName">Name of the vault.</param>
	void SaveVault(PlayerCollection vault, string vaultName);
}
