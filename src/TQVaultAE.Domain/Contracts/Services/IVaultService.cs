using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Domain.Contracts.Services
{
	public interface IVaultService
	{
		/// <summary>
		/// Creates a new empty vault file
		/// </summary>
		/// <param name="name">Name of the vault.</param>
		/// <param name="file">file name of the vault.</param>
		/// <returns>Player instance of the new vault.</returns>
		PlayerCollection CreateVault(string name, string file);
		/// <summary>
		/// Loads a vault file
		/// </summary>
		/// <param name="vaultName">Name of the vault</param>
		LoadVaultResult LoadVault(string vaultName);
		/// <summary>
		/// Attempts to save all modified vault files
		/// </summary>
		/// <param name="vaultOnError"></param>
		/// <exception cref="IOException">can happen during file save</exception>
		void SaveAllModifiedVaults(ref PlayerCollection vaultOnError);
		/// <summary>
		/// Updates VaultPath key from the configuration UI
		/// Needed since all vaults will need to be reloaded if this key changes.
		/// </summary>
		/// <param name="vaultPath">Path to the vault files</param>
		void UpdateVaultPath(string vaultPath);
	}
}