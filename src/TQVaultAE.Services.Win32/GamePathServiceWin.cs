using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TQVaultAE.Domain.Exceptions;
using TQVaultAE.Presentation;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.GameFiles;

namespace TQVaultAE.Services.Win32;

/// <summary>
/// Win32 IGamePathResolver implementation
/// </summary>
public partial class GamePathServiceWin(ILogger<GamePathServiceWin> log, ITQDataService tQDataService, IFileIO fileIO, IPathIO pathIO, IDirectoryIO directoryIO, UserSettings uSettings) : IGamePathService
{
	[GeneratedRegex(@"^(?<SteamappsRoot>.+steamapps).*", RegexOptions.IgnoreCase)]
	private static partial Regex SteamappsRootRegex();

	[GeneratedRegex(@"\\Main\\_", RegexOptions.IgnoreCase)]
	private static partial Regex MainCharacterFolderRegex();

	[GeneratedRegex(@"\\User\\_", RegexOptions.IgnoreCase)]
	private static partial Regex UserCharacterFolderRegex();

	private const StringComparison noCase = StringComparison.OrdinalIgnoreCase;

	internal const string LOCAL_GIT_REPOSITORY_DIRNAME = "LocalGitRepository";
	internal const string VAULTFILES_DEFAULT_DIRNAME = @"TQVaultData";
	internal const string SAVEDATA_DIRNAME = @"SaveData";
	internal const string SAVE_DIRNAME_TQ = @"Titan Quest";
	internal const string SAVE_DIRNAME_TQIT = @"Titan Quest - Immortal Throne";
	internal const string ARCHIVE_DIRNAME = @"ArchivedCharacters";

	public string ArchiveDirName => ARCHIVE_DIRNAME;
	public string VaultFilesDefaultDirName => VAULTFILES_DEFAULT_DIRNAME;
	public string SaveDataDirName => SAVEDATA_DIRNAME;
	public string SaveDirNameTQIT => SAVE_DIRNAME_TQIT;
	public string SaveDirNameTQ => SAVE_DIRNAME_TQ;

	internal const string TRANSFERSTASHFILENAME = "winsys.dxb";
	internal const string RELICVAULTSTASHFILENAME = "miscsys.dxb";
	internal const string PLAYERSAVEFILENAME = "Player.chr";
	internal const string PLAYERSTASHFILENAMEB = "winsys.dxb";
	internal const string PLAYERSTASHFILENAMEG = "winsys.dxg";
	internal const string PLAYERSETTINGSFILENAME = "settings.txt";
	internal const string VAULTFILENAME_EXTENSION_OLD = ".vault";
	internal const string VAULTFILENAME_EXTENSION_JSON = ".vault.json";
	public string VaultFileNameExtensionJson => VAULTFILENAME_EXTENSION_JSON;
	public string VaultFileNameExtensionOld => VAULTFILENAME_EXTENSION_OLD;

	private readonly ILogger Log = log;
	private readonly ITQDataService TQData = tQDataService;
	private readonly IFileIO FileIO = fileIO;
	private readonly IPathIO PathIO = pathIO;
	private readonly IDirectoryIO DirectoryIO = directoryIO;
	private readonly UserSettings USettings = uSettings;

	string? _ResolveTQVaultPath;
	private string ResolveTQVaultPath
	{
		get
		{
			if (string.IsNullOrWhiteSpace(_ResolveTQVaultPath))
				_ResolveTQVaultPath = Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory);
			return _ResolveTQVaultPath;
		}
	}

	public string LocalGitRepositoryDirectory => this.PathIO.Combine(ResolveTQVaultPath, LOCAL_GIT_REPOSITORY_DIRNAME);
	public string LocalGitRepositoryGitDir => this.PathIO.Combine(ResolveTQVaultPath, LOCAL_GIT_REPOSITORY_DIRNAME, @".git");
	public bool LocalGitRepositoryGitDirExist => DirectoryIO.Exists(LocalGitRepositoryGitDir);

	/// <summary>
	/// Parses filename to try to determine the base character name.
	/// </summary>
	/// <param name="filename">filename of the character file</param>
	/// <returns>string containing the character name</returns>
	public string? GetNameFromFile(string filename)
	{
		// Strip off the filename
		string basePath = this.PathIO.GetDirectoryName(filename);

		// Get the containing folder
		string? charName = this.PathIO.GetFileName(basePath);

		if (charName.Equals("SYS", StringComparison.OrdinalIgnoreCase))
		{
			string fileAndExtension = this.PathIO.GetFileName(filename);
			if (fileAndExtension.Contains("MISC", StringComparison.OrdinalIgnoreCase))
				// Check for the relic vault stash.
				charName = Resources.GlobalRelicVaultStash;
			else if (fileAndExtension.Contains("WIN", StringComparison.OrdinalIgnoreCase))
				// Check for the transfer stash.
				charName = Resources.GlobalTransferStash;
			else
				charName = null;
		}
		else if (charName.StartsWith('_'))
			// See if it is a character folder.
			charName = charName.Substring(1);
		else
			// The name is bogus so return a null.
			charName = null;

		return charName;
	}

	/// <summary>
	/// Name of the vault folder
	/// </summary>
	private string? _VaultFolder;

	public bool GamePathAreDifferent => GamePathTQIT != GamePathTQ;

	/// <summary>
	/// Gets the Immortal Throne Character save folder.
	/// Resolve as "%USERPROFILE%\Documents\My Games\Titan Quest - Immortal Throne".
	/// </summary>
	public string SaveFolderTQIT
		=> this.PathIO.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games", SAVE_DIRNAME_TQIT);

	/// <summary>
	/// Gets the Titan Quest Character save folder.
	/// Resolve as "%USERPROFILE%\Documents\My Games\Titan Quest".
	/// </summary>
	public string SaveFolderTQ
		=> this.PathIO.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games", SAVE_DIRNAME_TQ);

	/// <summary>
	/// Gets the name of the game settings file.
	/// </summary>
	public string SettingsFileTQ
		=> this.PathIO.Combine(SaveFolderTQ, "Settings", "options.txt");

	/// <summary>
	/// Gets the name of the game settings file.
	/// </summary>
	public string SettingsFileTQIT
		=> this.PathIO.Combine(SaveFolderTQIT, "Settings", "options.txt");

	/// <summary>
	/// Gets or sets the Titan Quest game path.
	/// </summary>
	public string GamePathTQ { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the Immortal Throne game path.
	/// </summary>
	public string GamePathTQIT { get; set; } = string.Empty;

	/// <summary>
	/// Gets the filename for the game's transfer stash.
	/// Stash files for Mods all have their own subdirectory which is the same as the mod's custom map folder
	/// </summary>
	public string TransferStashFileFullPath
	{
		get
		{
			if (IsCustom)
				return this.PathIO.Combine(SaveFolderTQIT, SAVEDATA_DIRNAME, "Sys", this.PathIO.GetFileName(MapName), TRANSFERSTASHFILENAME);

			return this.PathIO.Combine(SaveFolderTQIT, SAVEDATA_DIRNAME, "Sys", TRANSFERSTASHFILENAME);
		}
	}

	/// <summary>
	/// Gets the filename for the game's relic vault stash.
	/// Stash files for Mods all have their own subdirectory which is the same as the mod's custom map folder
	/// </summary>
	public string RelicVaultStashFileFullPath
	{
		get
		{
			if (IsCustom)
				return this.PathIO.Combine(SaveFolderTQIT, SAVEDATA_DIRNAME, "Sys", this.PathIO.GetFileName(MapName), RELICVAULTSTASHFILENAME);

			return this.PathIO.Combine(SaveFolderTQIT, SAVEDATA_DIRNAME, "Sys", RELICVAULTSTASHFILENAME);
		}
	}

	public string GetBaseCharacterFolder(bool IsTQIT, bool isArchive)
	{
		return GetBaseCharacterFolder(IsTQIT, this.IsCustom, isArchive);
	}

	public string GetBaseCharacterFolder(bool IsTQIT, bool isCustomCharacter, bool isArchive)
	{
		string mapSaveFolder = "Main";

		if (isCustomCharacter)
			mapSaveFolder = "User";

		if (IsTQIT)
		{
			if (isArchive)
				return this.PathIO.Combine(SaveFolderTQIT, SAVEDATA_DIRNAME, mapSaveFolder, this.ArchiveDirName);
			else
				return this.PathIO.Combine(SaveFolderTQIT, SAVEDATA_DIRNAME, mapSaveFolder);
		}
		else
		{
			if (isArchive)
				return this.PathIO.Combine(SaveFolderTQ, SAVEDATA_DIRNAME, mapSaveFolder, this.ArchiveDirName);
			else
				return this.PathIO.Combine(SaveFolderTQ, SAVEDATA_DIRNAME, mapSaveFolder);
		}
	}

	public string ArchiveTogglePath(string oldFolder)
	{
		// Case insensitive replace
		string newFolder;

		if (oldFolder.ContainsIgnoreCase(this.ArchiveDirName))
		{
			newFolder = Regex.Replace(oldFolder, @$"\\Main\\{this.ArchiveDirName}\\_", @"\Main\_", RegexOptions.IgnoreCase);
			newFolder = Regex.Replace(newFolder, @$"\\User\\{this.ArchiveDirName}\\_", @"\User\_", RegexOptions.IgnoreCase);// Cannot be both
			return newFolder;
		}

		newFolder = MainCharacterFolderRegex().Replace(oldFolder, @$"\Main\{this.ArchiveDirName}\_");
		newFolder = UserCharacterFolderRegex().Replace(newFolder, @$"\User\{this.ArchiveDirName}\_");// Cannot be both

		return newFolder;
	}

	public string GetPlayerFile(string characterName, bool IsTQIT, bool isArchive)
		=> this.PathIO.Combine(GetBaseCharacterFolder(IsTQIT, isArchive), string.Concat("_", characterName), PLAYERSAVEFILENAME);

	/// <summary>
	/// Gets the full path to the player's stash file.
	/// </summary>
	/// <param name="characterName">name of the character</param>
	/// <returns>full path to the player stash file</returns>
	public string GetPlayerStashFile(string characterName, bool isArchive)
		=> this.PathIO.Combine(GetBaseCharacterFolder(true, isArchive), string.Concat("_", characterName), PLAYERSTASHFILENAMEB);

	/// <summary>
	/// Gets a list of all of the character files in the save folder.
	/// </summary>
	/// <returns>List of character files in a string array</returns>
	public string[] GetCharacterList()
	{
		List<string> dirs = [];

		// Main campaign and custom map characters together: each one is loaded with its own map's data.
		foreach (bool isArchive in new[] { false, true })
		{
			foreach (bool isCustomCharacter in new[] { false, true })
			{
				// Get all folders that start with a '_'.
				var TQDir = GetBaseCharacterFolder(false, isCustomCharacter, isArchive);// From TQ
				var TQITDir = GetBaseCharacterFolder(true, isCustomCharacter, isArchive);// From TQIT

				if (this.USettings.EnableOriginalTQSupport && DirectoryIO.Exists(TQDir))
					dirs.AddRange(DirectoryIO.GetDirectories(TQDir, "_*"));

				if (DirectoryIO.Exists(TQITDir))
					dirs.AddRange(DirectoryIO.GetDirectories(TQITDir, "_*"));
			}
		}

		return dirs.ToArray();
	}

	/// <inheritdoc/>
	public bool IsCustomCharacterFolder(string characterFolder)
	{
		string parent = this.PathIO.GetDirectoryName(Path.TrimEndingDirectorySeparator(characterFolder));
		if (this.PathIO.GetFileName(parent).Equals(this.ArchiveDirName, noCase))
			parent = this.PathIO.GetDirectoryName(parent);

		return this.PathIO.GetFileName(parent).Equals("User", noCase);
	}

	/// <summary>
	/// Return all known custom map directories
	/// </summary>
	/// <returns>List of custom maps in a string array</returns>
	public GamePathEntry[] GetCustomMapList()
		=> new GamePathEntry[][] { GetCustomMapListLegacy(), GetCustomMapListSteamWork() }
			.Where(g => g is { Length: > 0 })
			.SelectMany(g => g)
			.ToArray();

	/// <summary>
	/// Gets a value indicating whether Ragnarok DLC has been installed.
	/// </summary>
	public bool IsRagnarokInstalled
		=> DirectoryIO.Exists(this.PathIO.Combine(GamePathTQIT, "Resources", "XPack2"));

	/// <summary>
	/// Gets a value indicating whether Atlantis DLC has been installed.
	/// </summary>
	public bool IsAtlantisInstalled
		=> DirectoryIO.Exists(this.PathIO.Combine(GamePathTQIT, "Resources", "XPack3"));

	/// <summary>
	/// Gets a value indicating whether Eternal Embers DLC has been installed.
	/// </summary>
	public bool IsEmbersInstalled
		=> DirectoryIO.Exists(this.PathIO.Combine(GamePathTQIT, "Resources", "XPack4"));

	/// <summary>
	/// Gets or sets the name of the custom map.
	/// Added to support custom quest characters
	/// </summary>
	public string MapName { get; set; } = string.Empty;

	/// <summary>
	/// Gets a value indicating whether a custom map has been specified.
	/// </summary>
	public bool IsCustom
		=> !string.IsNullOrEmpty(MapName) && (!MapName.Trim().Equals("MAIN", StringComparison.OrdinalIgnoreCase));



	/// <summary>
	/// Converts a file path to a backup file.  Adding the current date and time to the name.
	/// </summary>
	/// <param name="prefix">prefix of the backup file.</param>
	/// <param name="filePath">Full path of the file to backup.</param>
	/// <returns>Returns the name of the backup file to use for this file.  The backup file will have the given prefix.</returns>
	public string ConvertFilePathToBackupPath(string prefix, string filePath)
	{
		// Get the actual filename without any path information
		string filename = this.PathIO.GetFileName(filePath);

		// Strip the extension off of it.
		string extension = this.PathIO.GetExtension(filename);
		if (!string.IsNullOrEmpty(extension))
			filename = this.PathIO.GetFileNameWithoutExtension(filename);

		// Now come up with a timestamp string
		string timestamp = DateTime.Now.ToString("-yyyyMMdd-HHmmss-fffffff-", CultureInfo.InvariantCulture);

		string pathHead = this.PathIO.Combine(TQVaultBackupFolder, prefix);
		pathHead = string.Concat(pathHead, "-", filename, timestamp);

		int uniqueID = 0;

		// Now loop and construct the filename and check to see if the name
		// is already in use.  If it is, increment uniqueID and try again.
		while (true)
		{
			string fullFilePath = string.Concat(pathHead, uniqueID.ToString("000", CultureInfo.InvariantCulture), extension);

			if (!this.FileIO.Exists(fullFilePath))
				return fullFilePath;

			// try again
			++uniqueID;
		}
	}

	/// <summary>
	/// Gets a value indicating whether the vault save folder has been changed.
	/// Usually done via settings and triggers a reload of the vaults.
	/// </summary>
	public bool VaultFolderChanged { get; private set; }

	/// <summary>
	/// Gets or sets the vault save folder path.
	/// </summary>
	public string TQVaultSaveFolder
	{
		get
		{
			if (string.IsNullOrEmpty(_VaultFolder))
			{
				string folderPath = this.PathIO.Combine(SaveFolderTQ, VAULTFILES_DEFAULT_DIRNAME);

				// Lets see if our path exists and create it if it does not
				if (!DirectoryIO.Exists(folderPath))
				{
					try
					{
						DirectoryIO.CreateDirectory(folderPath);
					}
					catch (Exception e)
					{
						throw new InvalidOperationException(string.Concat("Error creating directory: ", folderPath), e);
					}
				}

				_VaultFolder = folderPath;
				VaultFolderChanged = true;
			}

			return _VaultFolder;
		}

		// Added by VillageIdiot
		// Used to set vault path by configuration UI.
		set
		{
			if (DirectoryIO.Exists(value))
				_VaultFolder = value;
		}
	}

	public string TQVaultConfigFolder
	{
		get
		{
			var path = this.PathIO.Combine(TQVaultSaveFolder, @"Config");

			if (!DirectoryIO.Exists(path))
				DirectoryIO.CreateDirectory(path);

			return path;
		}
	}

	/// <summary>
	/// Gets the vault backup folder path.
	/// </summary>
	public string TQVaultBackupFolder
	{
		get
		{
			string baseFolder = TQVaultSaveFolder;
			string folderPath = this.PathIO.Combine(baseFolder, "Backups");

			// Create the path if it does not yet exist
			if (!DirectoryIO.Exists(folderPath))
			{
				try
				{
					DirectoryIO.CreateDirectory(folderPath);
				}
				catch (Exception e)
				{
					throw new InvalidOperationException(string.Concat("Error creating directory: ", folderPath), e);
				}
			}

			return folderPath;
		}
	}

	public string TransferStashFileName => TRANSFERSTASHFILENAME;

	public string RelicVaultStashFileName => RELICVAULTSTASHFILENAME;

	public string PlayerSaveFileName => PLAYERSAVEFILENAME;

	public string PlayerStashFileNameB => PLAYERSTASHFILENAMEB;

	public string PlayerStashFileNameG => PLAYERSTASHFILENAMEG;

	public string PlayerSettingsFileName => PLAYERSETTINGSFILENAME;

	public GameType GameType { get; set; }

	/// <summary>
	/// Gets the file name and path for a vault.
	/// </summary>
	/// <param name="vaultName">The name of the vault file.</param>
	/// <returns>The full path along with extension of the vault file.</returns>
	public string GetVaultFile(string vaultName)
		=> string.Concat(this.PathIO.Combine(TQVaultSaveFolder, vaultName), VAULTFILENAME_EXTENSION_JSON);

	/// <summary>
	/// Gets a list of all of the vault files.
	/// </summary>
	/// <returns>The list of all of the vault files in the save folder.</returns>
	public string[] GetVaultList()
	{
		string[] empty = [];
		try
		{
			// Get all files that have a .vault extension.
			string[] filesOld = DirectoryIO.GetFiles(TQVaultSaveFolder, $"*{VAULTFILENAME_EXTENSION_OLD}");
			string[] filesJson = DirectoryIO.GetFiles(TQVaultSaveFolder, $"*{VAULTFILENAME_EXTENSION_JSON}");

			if (filesOld.Length == 0 && filesJson.Length == 0)
				return empty;

			List<string> vaultList = [];

			// Strip out the path information and extension.
			foreach (string file in filesOld)
			{
				vaultList.Add(this.PathIO.GetFileNameWithoutExtension(file));
			}

			// Pure Json vaults
			foreach (string newfile in filesJson)
			{
				if (!filesOld.Any(oldfile => newfile.StartsWith(oldfile, StringComparison.Ordinal)))
				{
					var remain = this.PathIO.GetFileNameWithoutExtension(this.PathIO.GetFileNameWithoutExtension(newfile));
					vaultList.Add(remain);
				}
			}

			// sort alphabetically
			vaultList.Sort();
			return vaultList.ToArray();
		}
		catch (DirectoryNotFoundException)
		{
			return empty;
		}
	}

	/// <summary>
	/// Gets a list of all of the custom maps "old path".
	/// </summary>
	/// <returns>List of custom maps in a string array</returns>
	GamePathEntry[] GetCustomMapListSteamWork()
	{
		try
		{
			// Get all folders in the Steam\steamapps\workshop\content directory.
			string TQITFolder = GamePathTQIT;
			var steamworkshopRootDir = SteamappsRootRegex().Replace(TQITFolder, @"${SteamappsRoot}\workshop\content\475150");

			if (steamworkshopRootDir == TQITFolder)// regex failed ! This is not a steamapps path
				return [];

			var modDir = this.DirectoryIO.GetDirectories(steamworkshopRootDir, "*");

			if (modDir is not { Length: > 0 })
				return [];

			var customMapList = modDir
				// Find SubModDir having readable Mod names
				.SelectMany(rd => DirectoryIO.GetDirectories(rd, "*"))
				// Make entries
				.Select(p => new GamePathEntry(p, $"SteamWorkshop : {this.PathIO.GetFileName(p)}"))
				.OrderBy(e => e.DisplayName)// sort alphabetically
				.ToArray();

			return customMapList;
		}
		catch (DirectoryNotFoundException)
		{
			return [];
		}
	}

	/// <summary>
	/// Gets a list of all of the custom maps "old path".
	/// </summary>
	/// <returns>List of custom maps in a string array</returns>
	GamePathEntry[] GetCustomMapListLegacy()
	{
		try
		{
			// Get all folders in the CustomMaps directory.
			string saveFolder = SaveFolderTQIT;

			var mapFolders = DirectoryIO.GetDirectories(this.PathIO.Combine(saveFolder, "CustomMaps"), "*");

			if (mapFolders is not { Length: > 0 })
				return [];

			var customMapList = mapFolders
			.Select(p => new GamePathEntry(p, $"Legacy : {this.PathIO.GetFileName(p)}"))
			.OrderBy(e => e.DisplayName)// sort alphabetically
			.ToArray();

			return customMapList;
		}
		catch (DirectoryNotFoundException)
		{
			return [];
		}
	}

	/// <summary>
	/// Try to resolve the local game path
	/// </summary>
	/// <returns></returns>
	public string ResolveGamePath()
	{
		string? titanQuestGamePath = null;

		// ForceGamePath precedence for dev on PC with partial installation
		if (!string.IsNullOrEmpty(this.USettings.ForceGamePath))
			titanQuestGamePath = this.USettings.ForceGamePath;

		// We are either autodetecting or the path has not been set
		//
		// GOG install of the anniversary edition
		if (string.IsNullOrEmpty(titanQuestGamePath))
			titanQuestGamePath = InstallLocations.GogCandidates().FirstOrDefault();

		// Steam install of the anniversary edition, in the Steam folder or one of its libraries
		if (string.IsNullOrEmpty(titanQuestGamePath))
			titanQuestGamePath = InstallLocations.SteamCandidates()
				.FirstOrDefault(p => this.FileIO.Exists(this.PathIO.Combine(p, "TQ.exe")));

		// Disc version detection logic -old
		if (string.IsNullOrEmpty(titanQuestGamePath))
		{
			string[] path = ["SOFTWARE", "Iron Lore", "Titan Quest", "Install Location"];
			titanQuestGamePath = ReadRegistryKey(Microsoft.Win32.Registry.LocalMachine, path);
		}

		if (string.IsNullOrEmpty(titanQuestGamePath))
			throw new ExGamePathNotFound(@"Unable to locate Titan Quest installation directory.
Please select the game installation directory.");

		return titanQuestGamePath;
	}

	/// <summary>
	/// Reads a value from the registry
	/// </summary>
	/// <param name="key">registry hive to be read</param>
	/// <param name="path">path of the value to be read</param>
	/// <returns>string value of the key and value that was read or empty string if there was a problem.</returns>
	string ReadRegistryKey(Microsoft.Win32.RegistryKey key, string[] path)
	{
		// The last item in the path array is the actual value name.
		int valueKey = path.Length - 1;

		// All other values in the path are keys which will need to be navigated.
		int lastSubKey = path.Length - 2;

		for (int i = 0; i <= lastSubKey; ++i)
		{
			var subKey = key.OpenSubKey(path[i]);
			if (subKey is null)
				return string.Empty;
			key = subKey;
		}

		return key.GetValue(path[valueKey]) as string ?? string.Empty;
	}

	/// <summary>
	/// Return the vault name from <paramref name="vaultFilePath"/>
	/// </summary>
	/// <param name="vaultFilePath"></param>
	/// <returns>name stripted from path and extension</returns>
	public string? GetVaultNameFromPath(string vaultFilePath)
	{
		string? vaultname = null;

		if (vaultFilePath.EndsWith(VAULTFILENAME_EXTENSION_JSON, StringComparison.OrdinalIgnoreCase))
			vaultname = this.PathIO.GetFileNameWithoutExtension(this.PathIO.GetFileNameWithoutExtension(vaultFilePath));
		else if (vaultFilePath.EndsWith(VAULTFILENAME_EXTENSION_OLD, StringComparison.OrdinalIgnoreCase))
			vaultname = this.PathIO.GetFileNameWithoutExtension(vaultFilePath);

		return vaultname;
	}

	/// <summary>
	/// Return ARC filename from <paramref name="resourceIdOrPrefix"/>
	/// </summary>
	/// <param name="resourceIdOrPrefix"></param>
	/// <returns></returns>
	public (string ArcFileName, bool IsDLC) ResolveArcFileName(RecordId resourceIdOrPrefix)
		=> (this.PathIO.Combine([GamePathTQIT, .. GameArchives.ArcFileSegments(resourceIdOrPrefix)])
			, GameArchives.IsExpansionResource(resourceIdOrPrefix));
}