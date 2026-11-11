using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Config;
using TQVaultAE.Application.Results;

namespace TQVaultAE.Services;

public class VaultService(
	ILogger<VaultService> log,
	SessionContext userContext,
	IItemDatabaseService itemDatabaseService,
	IPlayerCollectionProvider playerCollectionProvider,
	IGamePathService gamePathResolver,
	IGameFileService iGameFileService,
	IFileIO fileIO,
	IPathIO pathIO,
	UserSettings userSettings,
	IItemProvider itemProvider) : IVaultService
{
	private readonly ILogger Log = log;
	private readonly SessionContext userContext = userContext;
	private readonly IItemDatabaseService ItemDatabaseService = itemDatabaseService;
	private readonly IGamePathService GamePathResolver = gamePathResolver;
	private readonly IGameFileService GameFileService = iGameFileService;
	private readonly IPlayerCollectionProvider PlayerCollectionProvider = playerCollectionProvider;
	private readonly IFileIO FileIO = fileIO;
	private readonly IPathIO PathIO = pathIO;
	private readonly UserSettings UserSettings = userSettings;
	private readonly IItemProvider ItemProvider = itemProvider;
	public const string MAINVAULT = "Main Vault";

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

			// The old format is read like a character's file: its items are vault items, read with the base game data
			// when they have no link.
			foreach (var item in vault.Where(s => s is not null).SelectMany(s => s).Where(i => !i.InVault))
			{
				item.InVault = true;
				this.ItemProvider.GetDBData(item);
			}

			if (vault.Sacks is { Length: > 0 } sacks)
				sacks[0].IsModified = true;// Force Save into json on closing

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

	/// <inheritdoc/>
	public IReadOnlyList<(string File, PlayerCollection Vault)> GetModifiedVaults()
		=> [.. this.userContext.Vaults
			.Where(kvp => kvp.Value.IsValueCreated)
			.Select(kvp => (File: kvp.Key, Vault: kvp.Value.Value))
			.Where(v => v.Vault is not null && v.Vault.IsModified)];

	/// <inheritdoc/>
	public void SaveModifiedVault(string file, PlayerCollection vault)
	{
		if (!this.UserSettings.DisableLegacyBackup)
			this.GameFileService.BackupFile(vault.PlayerName, file);

		this.PlayerCollectionProvider.Save(vault, file);
		vault.Saved();
	}

	/// <inheritdoc/>
	public void ReindexVault(PlayerCollection vault)
	{
		this.ItemDatabaseService.RemoveContainerFromDatabase(vault.PlayerFile);
		this.IndexVaultItems(vault, vault.PlayerFile, vault.PlayerName);
	}

	/// <summary>Adds the items of a vault to the search database.</summary>
	private void IndexVaultItems(PlayerCollection vault, string file, string vaultName)
	{
		int sackNumber = -1;
		foreach (var sack in vault)
		{
			sackNumber++;
			if (sack is null)
				continue;

			foreach (var item in sack)
				this.ItemDatabaseService.AddItemToDatabase(item, file, vaultName, sackNumber, SackType.Vault);
		}
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

				this.IndexVaultItems(pc, k, vaultName);

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

	/// <inheritdoc />
	public IReadOnlyList<string> BuildVaultChoices(IEnumerable<string> vaultNames, string? leadingEntry = null, string? excludedVault = null, bool alwaysIncludeMainVault = true)
	{
		var names = vaultNames as ICollection<string> ?? vaultNames.ToList();
		var choices = new List<string>(names.Count + 2);

		if (leadingEntry is not null)
			choices.Add(leadingEntry);

		bool isMainVaultExcluded = MAINVAULT.Equals(excludedVault, StringComparison.Ordinal);
		if (!isMainVaultExcluded && (alwaysIncludeMainVault || names.Contains(MAINVAULT)))
			choices.Add(MAINVAULT);

		foreach (string name in names)
		{
			if (name.Equals(MAINVAULT, StringComparison.Ordinal) || name.Equals(excludedVault, StringComparison.Ordinal))
				continue;

			choices.Add(name);
		}

		return choices;
	}

	/// <inheritdoc />
	public BagTransferCheck CheckBagTransfer(PlayerCollection vault, int source, int destination)
	{
		int count = vault.NumberOfSacks;
		if (source < 0 || source >= count || destination < 0 || destination >= count || source == destination)
			return BagTransferCheck.Invalid;

		return vault.GetSack(destination)?.IsEmpty ?? true
			? BagTransferCheck.Allowed
			: BagTransferCheck.DestinationNotEmpty;
	}

	/// <inheritdoc />
	public string BuildCsvExport(PlayerCollection vault, int? bagIndex)
	{
		var delimiter = this.UserSettings.CsvDelimiterChar;

		IEnumerable<(Item Item, int BagId)> items = bagIndex is int index
			? (vault.GetSack(index) ?? []).Select(item => (item, index + 1))
			: (vault.Sacks ?? []).SelectMany((sack, idx) => sack.Select(item => (item, idx + 1)));

		var lines = items.Select((entry, row) =>
		{
			var friendlyNames = this.ItemProvider.GetFriendlyNames(entry.Item, FriendlyNamesExtraScopes.Requirements);
			return new CsvRow(vault.PlayerName, entry.BagId, friendlyNames, row, delimiter).ToString();
		}).Prepend(CsvRow.GetCSVHeader(delimiter));

		return string.Join(Environment.NewLine, lines);
	}

	/// <inheritdoc />
	public VaultMaintenanceError ValidateMaintenance(VaultMaintenance action, string? source, string? target, IEnumerable<string> existingVaults)
	{
		var existing = existingVaults as ICollection<string> ?? existingVaults.ToList();

		bool Exists(string? name)
			=> name is not null && existing.Any(vault => name.Equals(vault, StringComparison.OrdinalIgnoreCase));

		bool needsSource = action is VaultMaintenance.Copy or VaultMaintenance.Delete or VaultMaintenance.Rename;
		bool needsTarget = action is VaultMaintenance.New or VaultMaintenance.Copy or VaultMaintenance.Rename;

		if (needsTarget && string.IsNullOrWhiteSpace(target))
			return VaultMaintenanceError.EmptyName;

		if (needsSource && !Exists(source))
			return VaultMaintenanceError.SourceMissing;

		if (needsTarget)
		{
			if (target!.IndexOfAny(this.PathIO.GetInvalidFileNameChars()) != -1)
				return VaultMaintenanceError.InvalidCharacters;

			// Renaming a vault only by case ("gear" → "Gear") is allowed: the name in use is its own.
			if (Exists(target) && !(action == VaultMaintenance.Rename && IsCaseOnlyChange(source, target)))
				return VaultMaintenanceError.DuplicateName;
		}

		return VaultMaintenanceError.None;
	}

	/// <inheritdoc />
	public void EmptyBag(SackCollection sack)
	{
		foreach (var item in sack.ToList())
			this.ItemDatabaseService.RemoveItemFromDatabase(item);

		sack.EmptySack();
	}

	/// <inheritdoc />
	public bool MoveBag(PlayerCollection vault, int source, int destination)
	{
		if (this.CheckBagTransfer(vault, source, destination) == BagTransferCheck.Invalid || !vault.MoveSack(source, destination))
			return false;

		// Every bag between the two changed its number.
		int sackNumber = -1;
		foreach (var sack in vault)
		{
			sackNumber++;
			if (sack is null)
				continue;

			foreach (var item in sack)
				item.Place.SackNumber = sackNumber;
		}

		return true;
	}

	/// <inheritdoc />
	public bool CopyBag(PlayerCollection vault, int source, int destination)
	{
		if (this.CheckBagTransfer(vault, source, destination) == BagTransferCheck.Invalid)
			return false;

		if (vault.GetSack(destination) is { } overwritten)
		{
			foreach (var item in overwritten)
				this.ItemDatabaseService.RemoveItemFromDatabase(item);
		}

		if (!vault.CopySack(source, destination))
			return false;

		var copy = vault.GetSack(destination)!;
		copy.IsModified = true;
		foreach (var item in copy)
		{
			item.Place.SackNumber = destination;
			this.ItemDatabaseService.TryAddItemToDatabase(item);
		}

		return true;
	}

	/// <inheritdoc />
	public void ChangeBagTooltip(PlayerCollection vault, BagTooltipChange change, int bag, int bagCount)
	{
		var disabled = vault.DisabledTooltipBagId;
		var before = disabled.Order().ToList();

		switch (change)
		{
			case BagTooltipChange.Enable:
				disabled.RemoveAll(id => id == bag);
				break;
			case BagTooltipChange.Disable:
				if (!disabled.Contains(bag))
					disabled.Add(bag);
				break;
			case BagTooltipChange.EnableAll:
				disabled.Clear();
				break;
			case BagTooltipChange.DisableAll:
				disabled.Clear();
				disabled.AddRange(Enumerable.Range(0, bagCount));
				break;
		}

		// Saved with the vault: without this the change was lost unless something else was saved.
		if (!disabled.Order().SequenceEqual(before) && vault.Sacks is { Length: > 0 } sacks)
			sacks[0].IsModified = true;
	}

	/// <inheritdoc />
	public bool RememberSelectedBag(PlayerCollection vault, bool secondary, int bag)
	{
		if ((secondary ? vault.currentlyFocusedSackNumber : vault.currentlySelectedSackNumber) == bag)
			return false;

		if (secondary)
			vault.currentlyFocusedSackNumber = bag;
		else
			vault.currentlySelectedSackNumber = bag;

		// Saved with the vault.
		if (vault.Sacks is { Length: > 0 } sacks)
			sacks[0].IsModified = true;

		return true;
	}

	/// <inheritdoc />
	public int ResolveInitialBag(PlayerCollection? vault, bool secondary, int currentBag, int count)
	{
		int remembered = vault is null
			? -1
			: secondary ? vault.currentlyFocusedSackNumber : vault.currentlySelectedSackNumber;

		// -1: nothing remembered yet.
		int bag = remembered > -1 ? remembered : currentBag;

		return count > 0 ? Math.Clamp(bag, 0, count - 1) : Math.Max(bag, 0);
	}

	/// <inheritdoc />
	public bool CopyVault(string sourceName, string targetName)
	{
		var sourceFileName = this.GamePathResolver.GetVaultFile(sourceName);
		var targetFileName = this.GamePathResolver.GetVaultFile(targetName);

		if (!this.FileIO.Exists(sourceFileName) || this.FileIO.Exists(targetFileName))
			return false;

		this.FileIO.Copy(sourceFileName, targetFileName);

		// A vault created but never saved under the target name may sit in the cache.
		this.userContext.Vaults.TryRemove(targetFileName, out _);
		return true;
	}

	/// <inheritdoc />
	public VaultDeleteInfo GetDeleteInfo(string vaultName)
	{
		var sacks = this.LoadVault(vaultName).Vault?.Sacks ?? [];
		var filled = sacks.Where(sack => sack is not null && sack.Count > 0).ToList();
		var fileName = this.GamePathResolver.GetVaultFile(vaultName);
		return new VaultDeleteInfo(filled.Sum(sack => sack.Count), filled.Count, this.FileIO.CanMoveToRecycleBin(fileName));
	}

	/// <inheritdoc />
	public bool DeleteVault(string vaultName)
	{
		var fileName = this.GamePathResolver.GetVaultFile(vaultName);
		bool deleted = false;

		if (this.FileIO.Exists(fileName))
		{
			// The user can restore a vault from the Recycle Bin; where there is none, the file is deleted for good,
			// which the confirmation said.
			if (this.FileIO.CanMoveToRecycleBin(fileName))
				this.FileIO.MoveToRecycleBin(fileName);
			else
				this.FileIO.Delete(fileName);
			deleted = true;
		}

		this.userContext.Vaults.TryRemove(fileName, out _);
		this.ItemDatabaseService.RemoveContainerFromDatabase(fileName);
		return deleted;
	}

	/// <inheritdoc />
	public bool RenameVault(string oldName, string newName)
	{
		var oldFileName = this.GamePathResolver.GetVaultFile(oldName);
		var newFileName = this.GamePathResolver.GetVaultFile(newName);

		// File names ignore case on Windows: for a rename only by case, the "existing" target is the source itself.
		var caseOnly = IsCaseOnlyChange(oldFileName, newFileName);
		if (!this.FileIO.Exists(oldFileName) || (!caseOnly && this.FileIO.Exists(newFileName)))
			return false;

		this.FileIO.Move(oldFileName, newFileName);

		this.userContext.Vaults.TryRemove(oldFileName, out _);
		// The Main Vault, or a vault created but never saved, may sit in the cache under the new name.
		this.userContext.Vaults.TryRemove(newFileName, out _);
		// The search finds the items again under the new name once the renamed vault is loaded.
		this.ItemDatabaseService.RemoveContainerFromDatabase(oldFileName);
		this.ItemDatabaseService.RemoveContainerFromDatabase(newFileName);
		return true;
	}

	/// <summary>
	/// Whether two names differ only by the case of their letters.
	/// </summary>
	private static bool IsCaseOnlyChange(string? before, string? after)
		=> before is not null && after is not null
			&& before.Equals(after, StringComparison.OrdinalIgnoreCase)
			&& !before.Equals(after, StringComparison.Ordinal);

	/// <summary>
	/// Updates VaultPath key from the configuration UI.
	/// Needed since all vaults will need to be reloaded if this key changes.
	/// </summary>
	/// <param name="vaultPath">Path to the vault files</param>
	public void UpdateVaultPath(string vaultPath)
		=> this.UserSettings.VaultPath = vaultPath;

	/// <inheritdoc/>
	public string[] GetVaultList()
	{
		var vaults = this.GamePathResolver.GetVaultList();

		// Reading the list may have set the vault folder to its default: keep the setting in step.
		if (this.GamePathResolver.VaultFolderChanged)
			this.UpdateVaultPath(this.GamePathResolver.TQVaultSaveFolder);

		return vaults;
	}

	/// <inheritdoc/>
	public bool VaultExists(string vaultName)
		=> this.FileIO.Exists(this.GamePathResolver.GetVaultFile(vaultName));

	/// <inheritdoc/>
	public string ResolveVaultToLoad(string currentVault)
		=> StartupSelection.ResolveVault(this.UserSettings, currentVault, this.VaultExists);

	/// <summary>
	/// Saves a vault to disk and registers it in the session cache.
	/// </summary>
	/// <param name="vault">The vault to save.</param>
	/// <param name="vaultName">Name of the vault.</param>
	public void SaveVault(PlayerCollection vault, string vaultName)
	{
		var fileName = GamePathResolver.GetVaultFile(vaultName);
		PlayerCollectionProvider.Save(vault, fileName);
		vault.Saved();

		userContext.Vaults.GetOrAddAtomic(fileName, _ => vault);
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
