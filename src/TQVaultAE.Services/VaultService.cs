using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Config;
using TQVaultAE.Application.Results;

namespace TQVaultAE.Services;

public class VaultService : IVaultService
{
	private readonly ILogger Log;
	private readonly SessionContext userContext;
	private readonly IItemDatabaseService ItemDatabaseService;
	private readonly IGamePathService GamePathResolver;
	private readonly IGameFileService GameFileService;
	private readonly IPlayerCollectionProvider PlayerCollectionProvider;
	private readonly IFileIO FileIO;
	private readonly IPathIO PathIO;
	private readonly UserSettings UserSettings;
	public const string MAINVAULT = "Main Vault";

	public VaultService(
		ILogger<VaultService> log,
		SessionContext userContext,
		IItemDatabaseService itemDatabaseService,
		IPlayerCollectionProvider playerCollectionProvider,
		IGamePathService gamePathResolver,
		IGameFileService iGameFileService,
		IFileIO fileIO,
		IPathIO pathIO,
		UserSettings userSettings)
	{
		this.Log = log;
		this.userContext = userContext;
		this.ItemDatabaseService = itemDatabaseService;
		this.GamePathResolver = gamePathResolver;
		this.GameFileService = iGameFileService;
		this.PlayerCollectionProvider = playerCollectionProvider;
		this.FileIO = fileIO;
		this.PathIO = pathIO;
		this.UserSettings = userSettings;
	}

	/// <summary>
	/// Creates a new empty vault file.
	/// </summary>
	/// <param name="name">Name of the vault.</param>
	/// <param name="file">File name of the vault.</param>
	/// <returns>Player instance of the new vault.</returns>
	public PlayerCollection CreateVault(string name, string file)
	{
		// From .json to .vault
		var oldFormatfileName = this.PathIO.Combine(this.PathIO.GetDirectoryName(file), this.PathIO.GetFileNameWithoutExtension(file));

		PlayerCollection vault = new PlayerCollection(name, file);
		vault.IsVault = true;

		// Convert by reading old format
		if (this.FileIO.Exists(oldFormatfileName))
		{
			LoadVault(vault, oldFormatfileName);

			if (vault.Sacks.Any())
				vault.Sacks.First().IsModified = true;// Force Save into json on closing

			return vault;
		}

		// CreatNew
		vault.CreateEmptySacks(12); // number of bags
		return vault;
	}

	/// <summary>
	/// Creates a new empty vault with auto-generated filename.
	/// </summary>
	/// <param name="name">Name of the vault.</param>
	/// <returns>Player instance of the new vault.</returns>
	public PlayerCollection CreateVault(string name)
	{
		var fileName = $"{name}.json";
		return CreateVault(name, fileName);
	}

	/// <summary>
	/// Attempts to save all modified vault files.
	/// </summary>
	/// <param name="vaultOnError">If save fails, this will contain the vault that failed.</param>
	/// <returns>Number of vaults saved.</returns>
	/// <exception cref="IOException">can happen during file save</exception>
	public int SaveAllModifiedVaults(ref PlayerCollection vaultOnError)
	{
		int saved = 0;

		foreach (KeyValuePair<string, Lazy<PlayerCollection>> kvp in this.userContext.Vaults)
		{
			string vaultFile = kvp.Key;
			PlayerCollection vault = kvp.Value.Value;

			if (vault == null) continue;

			if (vault.IsModified)
			{
				// backup the file
				vaultOnError = vault;

				if (!this.UserSettings.DisableLegacyBackup)
					GameFileService.BackupFile(vault.PlayerName, vaultFile);

				PlayerCollectionProvider.Save(vault, vaultFile);
				vault.Saved();
				saved++;
			}
		}
		return saved;
	}

	/// <summary>
	/// Loads a vault file.
	/// </summary>
	/// <param name="vaultName">Name of the vault</param>
	/// <returns>Vault load result with vault and error information.</returns>
	public VaultLoadResult LoadVault(string vaultName)
	{
		var result = new VaultLoadResult();

		try
		{
			// Get the filename
			result.Filename = GamePathResolver.GetVaultFile(vaultName);

			// Check the cache
			var resultVault = this.userContext.Vaults.GetOrAddAtomic(result.Filename, k =>
			{
				PlayerCollection pc;
				// We need to load vault.
				if (!this.FileIO.Exists(k))
				{
					// the file does not exist so create a new vault or convert old format to Json.
					pc = this.CreateVault(vaultName, k);
					pc.VaultLoaded = true;
				}
				else
				{
					pc = new PlayerCollection(vaultName, k);
					pc.IsVault = true;
					LoadVault(pc, k);
					pc.VaultLoaded = true;
				}

				// Add vault items to the search database
				int sackNumber = -1;
				foreach (var sack in pc)
				{
					sackNumber++;
					if (sack == null)
						continue;

					foreach (var item in sack)
						this.ItemDatabaseService.AddItemToDatabase(item, k, vaultName, sackNumber, SackType.Vault);
				}

				return pc;
			});
			result.Vault = resultVault;
			result.VaultLoaded = resultVault.VaultLoaded;
			result.ArgumentException = resultVault.ArgumentException;
		}
		catch (Exception ex)
		{
			Log.LogError(ex, "Failed to load vault {VaultName}", vaultName);
			result.Error = ex;
		}

		return result;
	}

	/// <summary>
	/// Gets the list of available vault names.
	/// </summary>
	/// <returns>List of vault names.</returns>
	public IReadOnlyList<string> GetVaultList()
	{
		try
		{
			var fileNames = Directory.EnumerateFiles(this.GamePathResolver.TQVaultSaveFolder, "*.json").ToList();

			return fileNames
				.Select(f => this.PathIO.GetFileNameWithoutExtension(f))
				.ToList()
				.AsReadOnly();
		}
		catch (Exception)
		{
			return Array.Empty<string>();
		}
	}

	/// <summary>
	/// Deletes a vault file.
	/// </summary>
	/// <param name="vaultName">Name of the vault to delete.</param>
	/// <returns>True if deletion succeeded, false otherwise.</returns>
	public bool DeleteVault(string vaultName)
	{
		var fileName = this.GamePathResolver.GetVaultFile(vaultName);
		this.FileIO.Delete(fileName);
		return true;
	}

	/// <summary>
	/// Renames a vault file.
	/// </summary>
	/// <param name="oldName">Current name of the vault.</param>
	/// <param name="newName">New name for the vault.</param>
	/// <returns>True if rename succeeded, false otherwise.</returns>
	public bool RenameVault(string oldName, string newName)
	{
		var oldFileName = this.GamePathResolver.GetVaultFile(oldName);
		var newFileName = this.GamePathResolver.GetVaultFile(newName);

		// Invalidate the cache
		this.userContext.Vaults.TryRemove(oldFileName, out _);

		// Rename the file
		if (this.FileIO.Exists(newFileName))
			return false;

		this.FileIO.Move(oldFileName, newFileName);

		return true;
	}

	/// <summary>
	/// Updates VaultPath key from the configuration UI.
	/// Needed since all vaults will need to be reloaded if this key changes.
	/// </summary>
	/// <param name="vaultPath">Path to the vault files</param>
	public void UpdateVaultPath(string vaultPath)
		=> this.UserSettings.VaultPath = vaultPath;

	/// <summary>
	/// Shows the vault maintenance dialog.
	/// </summary>
	public void ShowVaultMaintenance()
	{
		// Placeholder for vault maintenance dialog
		// Implementation depends on GUI framework
	}

	/// <summary>
	/// Loads a vault from a file path (internal helper).
	/// </summary>
	private void LoadVault(PlayerCollection pc, string fileName)
	{
		try
		{
			this.PlayerCollectionProvider.LoadFile(pc, fileName);
		}
		catch (ArgumentException ae)
		{
			pc.ArgumentException = ae;
		}
	}
}
