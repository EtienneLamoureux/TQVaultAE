using System;
using Microsoft.VisualBasic.Devices;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Exceptions;
using TQVaultAE.Domain.Results;
using TQVaultAE.Presentation;

namespace TQVaultAE.Services.Win32
{
	/// <summary>
	/// Win32 IGamePathResolver implementation
	/// </summary>
	public class GamePathServiceWin : IGamePathService
	{
		private readonly ILogger Log;

		public GamePathServiceWin(ILogger<GamePathServiceWin> log)
		{
			this.Log = log;
		}

		/// <summary>
		/// Parses filename to try to determine the base character name.
		/// </summary>
		/// <param name="filename">filename of the character file</param>
		/// <returns>string containing the character name</returns>
		public string GetNameFromFile(string filename)
		{
			// Strip off the filename
			string basePath = Path.GetDirectoryName(filename);

			// Get the containing folder
			string charName = Path.GetFileName(basePath);

			if (charName.ToUpperInvariant() == "SYS")
			{
				string fileAndExtension = Path.GetFileName(filename);
				if (fileAndExtension.ToUpperInvariant().Contains("MISC"))
					// Check for the relic vault stash.
					charName = Resources.GlobalRelicVaultStash;
				else if (fileAndExtension.ToUpperInvariant().Contains("WIN"))
					// Check for the transfer stash.
					charName = Resources.GlobalTransferStash;
				else
					charName = null;
			}
			else if (charName.StartsWith("_", StringComparison.Ordinal))
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
		private string _VaultFolder;

		/// <summary>
		/// Gets the Immortal Throne Character save folder
		/// </summary>
		public string ImmortalThroneSaveFolder
			=> Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games", "Titan Quest - Immortal Throne");

		/// <summary>
		/// Gets the Titan Quest Character save folder.
		/// </summary>
		public string TQSaveFolder
			=> Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games", "Titan Quest");

		/// <summary>
		/// Gets the name of the game settings file.
		/// </summary>
		public string TQSettingsFile
			=> Path.Combine(TQSaveFolder, "Settings", "options.txt");

		/// <summary>
		/// Gets or sets the Titan Quest game path.
		/// </summary>
		public string TQPath { get; set; }

		/// <summary>
		/// Gets or sets the Immortal Throne game path.
		/// </summary>
		public string ImmortalThronePath { get; set; }

		/// <summary>
		/// Gets the filename for the game's transfer stash.
		/// Stash files for Mods all have their own subdirectory which is the same as the mod's custom map folder
		/// </summary>
		public string TransferStashFileFullPath
		{
			get
			{
				if (IsCustom)
					return Path.Combine(MapName, "SaveData", "Sys", Path.GetFileName(MapName), TRANSFERSTASHFILENAME);

				return Path.Combine(ImmortalThroneSaveFolder, "SaveData", "Sys", TRANSFERSTASHFILENAME);
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
					return Path.Combine(MapName, "SaveData", "Sys", Path.GetFileName(MapName), RELICVAULTSTASHFILENAME);

				return Path.Combine(ImmortalThroneSaveFolder, "SaveData", "Sys", RELICVAULTSTASHFILENAME);
			}
		}

		/// <summary>
		/// Gets the base character save folder.
		/// Changed to support custom quest characters
		/// </summary>
		/// <returns>path of the save folder</returns>
		public string GetBaseCharacterFolder()
		{
			string mapSaveFolder = "Main";

			if (IsCustom)
				mapSaveFolder = "User";

			return Path.Combine(Path.Combine(ImmortalThroneSaveFolder, "SaveData"), mapSaveFolder);
		}

		/// <summary>
		/// Gets the full path to the player character file.
		/// </summary>
		/// <param name="characterName">name of the character</param>
		/// <returns>full path to the character file.</returns>
		public string GetPlayerFile(string characterName)
			=> Path.Combine(GetBaseCharacterFolder(), string.Concat("_", characterName), PLAYERSAVEFILENAME);

		/// <summary>
		/// Gets the full path to the player's stash file.
		/// </summary>
		/// <param name="characterName">name of the character</param>
		/// <returns>full path to the player stash file</returns>
		public string GetPlayerStashFile(string characterName)
			=> Path.Combine(GetBaseCharacterFolder(), string.Concat("_", characterName), PLAYERSTASHFILENAMEB);

		/// <summary>
		/// Gets a list of all of the character files in the save folder.
		/// </summary>
		/// <returns>List of character files in a string array</returns>
		public string[] GetCharacterList()
		{
			try
			{
				// Get all folders that start with a '_'.
				string[] folders = Directory.GetDirectories(GetBaseCharacterFolder(), "_*");
				return !folders.Any() ? null : folders;
			}
			catch (DirectoryNotFoundException)
			{
				return null;
			}
		}

		/// <summary>
		/// Return all known custom map directories
		/// </summary>
		/// <returns>List of custom maps in a string array</returns>
		public GamePathEntry[] GetCustomMapList()
			=> new GamePathEntry[][] { GetCustomMapListLegacy(), GetCustomMapListSteamWork() }
				.Where(g => g?.Any() ?? false)
				.SelectMany(g => g)
				.ToArray();

		/// <summary>
		/// Gets a value indicating whether Ragnarok DLC has been installed.
		/// </summary>
		public bool IsRagnarokInstalled
			=> Directory.Exists(ImmortalThronePath + "\\Resources\\XPack2");

		/// <summary>
		/// Gets a value indicating whether Atlantis DLC has been installed.
		/// </summary>
		public bool IsAtlantisInstalled
			=> Directory.Exists(ImmortalThronePath + "\\Resources\\XPack3");

		/// <summary>
		/// Gets a value indicating whether Eternal Embers DLC has been installed.
		/// </summary>
		public bool IsEmbersInstalled
			=> Directory.Exists(ImmortalThronePath + "\\Resources\\XPack4");


		/// <summary>
		/// Gets or sets the name of the custom map.
		/// Added to support custom quest characters
		/// </summary>
		public string MapName { get; set; }

		/// <summary>
		/// Gets a value indicating whether a custom map has been specified.
		/// </summary>
		public bool IsCustom
			=> !string.IsNullOrEmpty(MapName) && (MapName.Trim().ToUpperInvariant() != "MAIN");

		/// <summary>
		/// TQ has an annoying habit of throwing away your char in preference
		/// for the Backup folder if it exists if it thinks your char is not valid.
		/// We need to move that folder away so TQ won't find it.
		/// </summary>
		/// <param name="playerFile">Name of the player file to backup</param>
		public void BackupStupidPlayerBackupFolder(string playerFile)
		{
			string playerFolder = Path.GetDirectoryName(playerFile);
			string backupFolder = Path.Combine(playerFolder, "Backup");
			if (Directory.Exists(backupFolder))
			{
				// we need to move it.
				string newFolder = Path.Combine(playerFolder, "Backup-moved by TQVault");
				if (Directory.Exists(newFolder))
				{
					try
					{
						// It already exists--we need to remove it
						Directory.Delete(newFolder, true);
					}
					catch (Exception)
					{
						int fn = 1;
						while (Directory.Exists(String.Format("{0}({1})", newFolder, fn)))
						{
							fn++;
						}
						newFolder = String.Format("{0}({1})", newFolder, fn);
					}
				}

				Directory.Move(backupFolder, newFolder);
			}
		}


		/// <summary>
		/// Converts a file path to a backup file.  Adding the current date and time to the name.
		/// </summary>
		/// <param name="prefix">prefix of the backup file.</param>
		/// <param name="filePath">Full path of the file to backup.</param>
		/// <returns>Returns the name of the backup file to use for this file.  The backup file will have the given prefix.</returns>
		public string ConvertFilePathToBackupPath(string prefix, string filePath)
		{
			// Get the actual filename without any path information
			string filename = Path.GetFileName(filePath);

			// Strip the extension off of it.
			string extension = Path.GetExtension(filename);
			if (!string.IsNullOrEmpty(extension))
				filename = Path.GetFileNameWithoutExtension(filename);

			// Now come up with a timestamp string
			string timestamp = DateTime.Now.ToString("-yyyyMMdd-HHmmss-fffffff-", CultureInfo.InvariantCulture);

			string pathHead = Path.Combine(TQVaultBackupFolder, prefix);
			pathHead = string.Concat(pathHead, "-", filename, timestamp);

			int uniqueID = 0;

			// Now loop and construct the filename and check to see if the name
			// is already in use.  If it is, increment uniqueID and try again.
			while (true)
			{
				string fullFilePath = string.Concat(pathHead, uniqueID.ToString("000", CultureInfo.InvariantCulture), extension);

				if (!File.Exists(fullFilePath))
					return fullFilePath;

				// try again
				++uniqueID;
			}
		}

		/// <summary>
		/// Backs up the file to the backup folder.
		/// </summary>
		/// <param name="prefix">prefix of the backup file</param>
		/// <param name="file">file name to backup</param>
		/// <returns>Returns the name of the backup file, or NULL if file does not exist</returns>
		public string BackupFile(string prefix, string file)
		{
			if (File.Exists(file))
			{
				// only backup if it exists!
				string backupFile = ConvertFilePathToBackupPath(prefix, file);

				File.Copy(file, backupFile);

				// Added by VillageIdiot
				// Backup the file pairs for the player stash files.
				if (Path.GetFileName(file).ToUpperInvariant() == PLAYERSTASHFILENAMEB.ToUpperInvariant())
				{
					string dxgfile = Path.ChangeExtension(file, ".dxg");

					if (File.Exists(dxgfile))
					{
						// only backup if it exists!
						backupFile = ConvertFilePathToBackupPath(prefix, dxgfile);

						File.Copy(dxgfile, backupFile);
					}
				}

				return backupFile;
			}
			else
				return null;
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
					string folderPath = Path.Combine(TQSaveFolder, "TQVaultData");

					// Lets see if our path exists and create it if it does not
					if (!Directory.Exists(folderPath))
					{
						try
						{
							Directory.CreateDirectory(folderPath);
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
				if (Directory.Exists(value))
					_VaultFolder = value;
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
				string folderPath = Path.Combine(baseFolder, "Backups");

				// Create the path if it does not yet exist
				if (!Directory.Exists(folderPath))
				{
					try
					{
						Directory.CreateDirectory(folderPath);
					}
					catch (Exception e)
					{
						throw new InvalidOperationException(string.Concat("Error creating directory: ", folderPath), e);
					}
				}

				return folderPath;
			}
		}

		public const string TRANSFERSTASHFILENAME = "winsys.dxb";
		public string TransferStashFileName => TRANSFERSTASHFILENAME;

		public const string RELICVAULTSTASHFILENAME = "miscsys.dxb";
		public string RelicVaultStashFileName => RELICVAULTSTASHFILENAME;

		public const string PLAYERSAVEFILENAME = "Player.chr";
		public string PlayerSaveFileName => PLAYERSAVEFILENAME;

		public const string PLAYERSTASHFILENAMEB = "winsys.dxb";
		public string PlayerStashFileNameB => PLAYERSTASHFILENAMEB;

		public const string PLAYERSTASHFILENAMEG = "winsys.dxg";
		public string PlayerStashFileNameG => PLAYERSTASHFILENAMEG;


		/// <summary>
		/// Gets the file name and path for a vault.
		/// </summary>
		/// <param name="vaultName">The name of the vault file.</param>
		/// <returns>The full path along with extension of the vault file.</returns>
		public string GetVaultFile(string vaultName)
			=> string.Concat(Path.Combine(TQVaultSaveFolder, vaultName), ".vault");

		/// <summary>
		/// Gets a list of all of the vault files.
		/// </summary>
		/// <returns>The list of all of the vault files in the save folder.</returns>
		public string[] GetVaultList()
		{
			try
			{
				// Get all files that have a .vault extension.
				string[] files = Directory.GetFiles(TQVaultSaveFolder, "*.vault");

				if (files == null || files.Length < 1)
					return null;

				List<string> vaultList = new List<string>(files.Length);

				// Strip out the path information and extension.
				foreach (string file in files)
				{
					vaultList.Add(Path.GetFileNameWithoutExtension(file));
				}

				// sort alphabetically
				vaultList.Sort();
				return vaultList.ToArray();
			}
			catch (DirectoryNotFoundException)
			{
				return null;
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
				string TQITFolder = ImmortalThronePath;
				var steamworkshopRootDir = Regex.Replace(TQITFolder, @"(?i)^(?<SteamappsRoot>.+steamapps).*", @"${SteamappsRoot}\workshop\content\475150");

				if (steamworkshopRootDir == TQITFolder)// regex failed ! This is not a steamapps path
					return null;

				var modDir = Directory.GetDirectories(steamworkshopRootDir, "*");

				if (!(modDir?.Any() ?? false))
					return null;

				var customMapList = modDir
					// Find SubModDir having readable Mod names
					.SelectMany(rd => Directory.GetDirectories(rd, "*"))
					// Make entries
					.Select(p => new GamePathEntry(p, $"SteamWorkshop : {Path.GetFileName(p)}"))
					.OrderBy(e => e.DisplayName)// sort alphabetically
					.ToArray();

				return customMapList;
			}
			catch (DirectoryNotFoundException)
			{
				return null;
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
				string saveFolder = ImmortalThroneSaveFolder;

				var mapFolders = Directory.GetDirectories(Path.Combine(saveFolder, "CustomMaps"), "*");

				if (!(mapFolders?.Any() ?? false))
					return null;

				var customMapList = mapFolders
				.Select(p => new GamePathEntry(p, $"Legacy : {Path.GetFileName(p)}"))
				.OrderBy(e => e.DisplayName)// sort alphabetically
				.ToArray();

				return customMapList;
			}
			catch (DirectoryNotFoundException)
			{
				return null;
			}
		}

		/// <summary>
		/// Try to resolve the local game path
		/// </summary>
		/// <returns></returns>
		public string ResolveGamePath()
		{
			string titanQuestGamePath = null;

			// ForceGamePath precedence for dev on PC with partial installation
			if (!string.IsNullOrEmpty(Config.Settings.Default.ForceGamePath))
				titanQuestGamePath = Config.Settings.Default.ForceGamePath;

			// We are either autodetecting or the path has not been set
			//
			// Detection logic for a GOG install of the anniversary edition ~Malgardian
			if (string.IsNullOrEmpty(titanQuestGamePath))
			{
				string[] path = { "SOFTWARE", "GOG.com", "Games", "1196955511", "PATH" };
				titanQuestGamePath = ReadRegistryKey(Microsoft.Win32.Registry.LocalMachine, path);
			}

			if (string.IsNullOrEmpty(titanQuestGamePath))
			{
				string[] path = { "SOFTWARE", "WOW6432Node", "GOG.com", "Games", "1196955511", "PATH" };
				titanQuestGamePath = ReadRegistryKey(Microsoft.Win32.Registry.LocalMachine, path);
			}

			// Detection logic for a Steam install of the anniversary edition ~Malgardian
			if (string.IsNullOrEmpty(titanQuestGamePath))
			{
				string steamTQPath = "\\SteamApps\\common\\Titan Quest Anniversary Edition";

				string[] registryPath = { "Software", "Valve", "Steam", "SteamPath" };
				string steamPath = ReadRegistryKey(Microsoft.Win32.Registry.CurrentUser, registryPath).Replace("/", "\\");

				if (Directory.Exists(steamPath + steamTQPath))
					titanQuestGamePath = steamPath + steamTQPath;
				else
				{
					//further looking for Steam library
					//read libraryfolders.vdf
					Regex vdfPathRegex = new Regex(@"""\d+""\t+""([^""]+)""");  // "2"		"D:\\games\\Steam"
					var vdfFile = Path.Combine(steamPath, @"SteamApps\libraryfolders.vdf");
					if (File.Exists(vdfFile))
					{
						string[] libFile = File.ReadAllLines(vdfFile);

						foreach (var line in libFile)
						{
							Match match = vdfPathRegex.Match(line.Trim());
							if (match.Success && Directory.Exists(match.Groups[1] + steamTQPath))
							{
								titanQuestGamePath = match.Groups[1] + steamTQPath;
								break;
							}
						}
					}
				}
			}

			// Disc version detection logic -old
			if (string.IsNullOrEmpty(titanQuestGamePath))
			{
				string[] path = { "SOFTWARE", "Iron Lore", "Titan Quest", "Install Location" };
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
				key = key.OpenSubKey(path[i]);
				if (key == null)
					return string.Empty;
			}

			return (string)key.GetValue(path[valueKey]);
		}
	}
}
