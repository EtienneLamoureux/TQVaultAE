//-----------------------------------------------------------------------
// <copyright file="TQData.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultData
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;

	/// <summary>
	/// TQData is used to store information about reading and writing the data files in TQ.
	/// </summary>
	public static class TQData
	{
		/// <summary>
		/// Name of the vault folder
		/// </summary>
		private static string vaultFolder;

		/// <summary>
		/// Path to the Immortal Throne game directory.
		/// </summary>
		private static string immortalThroneGamePath;

		/// <summary>
		/// Path to the Titan Quest Game directory.
		/// </summary>
		private static string titanQuestGamePath;

		/// <summary>
		/// Gets the Immortal Throne Character save folder
		/// </summary>
		public static string ImmortalThroneSaveFolder
		{
			get
			{
				return Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games"), "Titan Quest - Immortal Throne");
			}
		}

		/// <summary>
		/// Gets the Titan Quest Character save folder.
		/// </summary>
		public static string TQSaveFolder
		{
			get
			{
				return Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games"), "Titan Quest");
			}
		}

		/// <summary>
		/// Gets the name of the game settings file.
		/// </summary>
		public static string TQSettingsFile
		{
			get
			{
				return Path.Combine(Path.Combine(TQSaveFolder, "Settings"), "options.txt");
			}
		}

		/// <summary>
		/// Gets or sets the Titan Quest game path.
		/// </summary>
		public static string TQPath
		{
			get
			{
				// We are either autodetecting or the path has not been set
				//
				// Detection logic for a GOG install of the anniversary edition ~Malgardian
				if (string.IsNullOrEmpty(titanQuestGamePath))
				{
					string[] path = { "SOFTWARE", "GOG.com", "Games", "1196955511", "PATH" };
					titanQuestGamePath = ReadRegistryKey(Microsoft.Win32.Registry.LocalMachine, path);
				}

				// Detection logic for a Steam install of the anniversary edition ~Malgardian
				if (string.IsNullOrEmpty(titanQuestGamePath))
				{
					string steamTQPath = "\\SteamApps\\common\\Titan Quest Anniversary Edition";

					string[] registryPath = { "Software", "Valve", "Steam", "SteamPath" };
					string steamPath = ReadRegistryKey(Microsoft.Win32.Registry.CurrentUser, registryPath).Replace("/", "\\");

					if (Directory.Exists(steamPath + steamTQPath))
					{
						titanQuestGamePath = steamPath + steamTQPath;
					}
					else
					{
						//further looking for Steam library
						//read libraryfolders.vdf
						Regex vdfPathRegex = new Regex("\"\\d+\"\t+\"([^\"]+)\"");  // "2"		"D:\\games\\Steam"
						string[] libFile = File.ReadAllLines(steamPath + "\\SteamApps\\libraryfolders.vdf");

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

				//Disc version detection logic -old
				if (string.IsNullOrEmpty(titanQuestGamePath))
				{
					string[] path = { "SOFTWARE", "Iron Lore", "Titan Quest", "Install Location" };
					titanQuestGamePath = ReadRegistryKey(Microsoft.Win32.Registry.LocalMachine, path);
				}

				if (string.IsNullOrEmpty(titanQuestGamePath) && !string.IsNullOrEmpty(IniProperties.GamePath))
				{
					titanQuestGamePath = IniProperties.GamePath;
				}

				if (string.IsNullOrEmpty(titanQuestGamePath))
				{
					throw new InvalidOperationException("Unable to locate Titan Quest installation directory. Please edit TQVaultAE.ini to contain a valid path in the option 'ForceGamePath'.");
				}

				return titanQuestGamePath;
			}

			set
			{
				titanQuestGamePath = value;
			}
		}

        /// <summary>
		/// Gets a value indicating whether Ragnarok DLC has been installed.
		/// </summary>
		public static bool IsRagnarokInstalled {
            get {
                return Directory.Exists(ImmortalThronePath + "\\Resources\\XPack2");
            }
        }

		/// <summary>
		/// Gets a value indicating whether Atlantis DLC has been installed.
		/// </summary>
		public static bool IsAtlantisInstalled
		{
			get
			{
				return Directory.Exists(ImmortalThronePath + "\\Resources\\XPack3");
			}
		}


		/// <summary>
		/// Gets or sets the Immortal Throne game path.
		/// </summary>
		public static string ImmortalThronePath
		{
			get
			{
				// We are either autodetecting or the path has not been set in configuration UI
				// so we attempt to read the value from the registry

				//detection logic for a GOG install of the anniversary edition ~Malgardian
				if (string.IsNullOrEmpty(immortalThroneGamePath))
				{
					string[] path = new string[5];
					path[0] = "SOFTWARE";
					path[1] = "GOG.com";
					path[2] = "Games";
					path[3] = "1196955511";
					path[4] = "PATH";
					immortalThroneGamePath = ReadRegistryKey(Microsoft.Win32.Registry.LocalMachine, path);
					//System.Windows.Forms.MessageBox.Show(immortalThroneGamePath);
				}

				// Detection logic for a Steam install of the anniversary edition ~Malgardian
				if (string.IsNullOrEmpty(immortalThroneGamePath))
				{
					string steamTQPath = "\\SteamApps\\common\\Titan Quest Anniversary Edition";

					string[] registryPath = new string[] { "Software", "Valve", "Steam", "SteamPath" };
					string steamPath = ReadRegistryKey(Microsoft.Win32.Registry.CurrentUser, registryPath).Replace("/", "\\");

					if (Directory.Exists(steamPath + steamTQPath))
					{
						immortalThroneGamePath = steamPath + steamTQPath;
					}
					else
					{
						//further looking for Steam library
						//read libraryfolders.vdf
						Regex vdfPathRegex = new Regex("\"\\d+\"\t+\"([^\"]+)\"");  // "2"		"D:\\games\\Steam"
						string[] libFile = File.ReadAllLines(steamPath + "\\SteamApps\\libraryfolders.vdf");

						foreach (var line in libFile)
						{
							Match match = vdfPathRegex.Match(line.Trim());
							if (match.Success && Directory.Exists(match.Groups[1] + steamTQPath))
							{
								immortalThroneGamePath = match.Groups[1] + steamTQPath;
								break;
							}
						}
					}
				}

				//Disc version detection logic -old
				if (string.IsNullOrEmpty(immortalThroneGamePath))
				{
					string[] path = new string[4];
					path[0] = "SOFTWARE";
					path[1] = "Iron Lore";
					path[2] = "Titan Quest Immortal Throne";
					path[3] = "Install Location";
					immortalThroneGamePath = ReadRegistryKey(Microsoft.Win32.Registry.LocalMachine, path);
				}

				if (string.IsNullOrEmpty(immortalThroneGamePath) && !string.IsNullOrEmpty(IniProperties.GamePath))
				{
					immortalThroneGamePath = IniProperties.GamePath;
				}

				if (string.IsNullOrEmpty(immortalThroneGamePath))
				{
					throw new InvalidOperationException("Unable to locate Titan Quest installation directory. Please edit TQVaultAE.ini to contain a valid path in the option 'ForceGamePath'.");
				}

                return immortalThroneGamePath;
			}

			set
			{
				immortalThroneGamePath = value;
			}
		}

		/// <summary>
		/// Gets or sets the name of the custom map.
		/// Added to support custom quest characters
		/// </summary>
		public static string MapName { get; set; }

		/// <summary>
		/// Gets a value indicating whether a custom map has been specified.
		/// </summary>
		public static bool IsCustom
		{
			get
			{
				return !string.IsNullOrEmpty(MapName) && (MapName.Trim().ToUpperInvariant() != "MAIN");
			}
		}

		/// <summary>
		/// Gets a value indicating whether the vault save folder has been changed.
		/// Usually done via settings and triggers a reload of the vaults.
		/// </summary>
		public static bool VaultFolderChanged { get; private set; }

		/// <summary>
		/// Gets or sets the vault save folder path.
		/// </summary>
		public static string TQVaultSaveFolder
		{
			get
			{
				if (string.IsNullOrEmpty(vaultFolder))
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

					vaultFolder = folderPath;
					VaultFolderChanged = true;
				}

				return vaultFolder;
			}

			// Added by VillageIdiot
			// Used to set vault path by configuration UI.
			set
			{
				if (Directory.Exists(value))
				{
					vaultFolder = value;
				}
			}
		}

		/// <summary>
		/// Gets the vault backup folder path.
		/// </summary>
		public static string TQVaultBackupFolder
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

		/// <summary>
		/// Gets the filename for the game's transfer stash.
		/// Stash files for Mods all have their own subdirectory which is the same as the mod's custom map folder
		/// </summary>
		public static string TransferStashFile
		{
			get
			{
				if (IsCustom)
				{
					return Path.Combine(Path.Combine(Path.Combine(Path.Combine(ImmortalThroneSaveFolder, "SaveData"), "Sys"), MapName), "winsys.dxb");
				}

				return Path.Combine(Path.Combine(Path.Combine(ImmortalThroneSaveFolder, "SaveData"), "Sys"), "winsys.dxb");
			}
		}

		/// <summary>
		/// Gets the filename for the game's relic vault stash.
		/// Stash files for Mods all have their own subdirectory which is the same as the mod's custom map folder
		/// </summary>
		public static string RelicVaultStashFile
		{
			get
			{
				if (IsCustom)
				{
					return Path.Combine(Path.Combine(Path.Combine(Path.Combine(ImmortalThroneSaveFolder, "SaveData"), "Sys"), MapName), "miscsys.dxb");
				}

				return Path.Combine(Path.Combine(Path.Combine(ImmortalThroneSaveFolder, "SaveData"), "Sys"), "miscsys.dxb");
			}
		}

		/// <summary>
		/// Validates that the next string is a certain value and throws an exception if it is not.
		/// </summary>
		/// <param name="value">value to be validated</param>
		/// <param name="reader">BinaryReader instance</param>
		public static void ValidateNextString(string value, BinaryReader reader)
		{
			string label = ReadCString(reader);
			if (!label.ToUpperInvariant().Equals(value.ToUpperInvariant()))
			{
				// Turn on debugging so we can log the exception.
				if (!TQDebug.DebugEnabled)
				{
					TQDebug.DebugEnabled = true;
				}

				TQDebug.DebugWriteLine(string.Format(
					CultureInfo.InvariantCulture,
					"Error reading file at position {2}.  Expecting '{0}'.  Got '{1}'",
					value,
					label,
					reader.BaseStream.Position - label.Length - 4));

				throw new ArgumentException(string.Format(
					CultureInfo.InvariantCulture,
					"Error reading file at position {2}.  Expecting '{0}'.  Got '{1}'",
					value,
					label,
					reader.BaseStream.Position - label.Length - 4));
			}
		}

		public static bool MatchNextString(string value, BinaryReader reader)
		{
			long readerPosition = reader.BaseStream.Position;

			string label = ReadCString(reader);
			reader.BaseStream.Position = readerPosition;

			if (!label.ToUpperInvariant().Equals(value.ToUpperInvariant()))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Writes a string along with its length to the file.
		/// </summary>
		/// <param name="writer">BinaryWriter instance</param>
		/// <param name="value">string value to be written.</param>
		public static void WriteCString(BinaryWriter writer, string value)
		{
			// Convert the string to ascii
			// Vorbis' fix for extended characters in the database.
			Encoding ascii = Encoding.GetEncoding(1252);

			byte[] rawstring = ascii.GetBytes(value);

			// Write the 4-byte length of the string
			writer.Write(rawstring.Length);

			// now write the string
			writer.Write(rawstring);
		}

		/// <summary>
		/// Normalizes the record path to Upper Case Invariant Culture and replace backslashes with slashes.
		/// </summary>
		/// <param name="recordId">record path to be normalized</param>
		/// <returns>normalized record path</returns>
		public static string NormalizeRecordPath(string recordId)
		{
			// uppercase it
			string normalizedRecordId = recordId.ToUpperInvariant();

			// replace any '/' with '\\'
			normalizedRecordId = normalizedRecordId.Replace('/', '\\');
			return normalizedRecordId;
		}

		/// <summary>
		/// Reads a string from the binary stream.
		/// Expects an integer length value followed by the actual string of the stated length.
		/// </summary>
		/// <param name="reader">BinaryReader instance</param>
		/// <returns>string of data that was read</returns>
		public static string ReadCString(BinaryReader reader)
		{
			// first 4 bytes is the string length, followed by the string.
			int len = reader.ReadInt32();

			// Convert the next len bytes into a string
			// Vorbis' fix for extended characters in the database.
			Encoding ascii = Encoding.GetEncoding(1252);

			byte[] rawData = reader.ReadBytes(len);

			char[] chars = new char[ascii.GetCharCount(rawData, 0, len)];
			ascii.GetChars(rawData, 0, len, chars, 0);

			string ans = new string(chars);

			return ans;
		}

		/// <summary>
		/// Reads a value from the registry
		/// </summary>
		/// <param name="key">registry hive to be read</param>
		/// <param name="path">path of the value to be read</param>
		/// <returns>string value of the key and value that was read or empty string if there was a problem.</returns>
		public static string ReadRegistryKey(Microsoft.Win32.RegistryKey key, string[] path)
		{
			// The last item in the path array is the actual value name.
			int valueKey = path.Length - 1;

			// All other values in the path are keys which will need to be navigated.
			int lastSubKey = path.Length - 2;

			for (int i = 0; i <= lastSubKey; ++i)
			{
				key = key.OpenSubKey(path[i]);
				if (key == null)
				{
					return string.Empty;
				}
			}

			return (string)key.GetValue(path[valueKey]);
		}

		/// <summary>
		/// TQ has an annoying habit of throwing away your char in preference
		/// for the Backup folder if it exists if it thinks your char is not valid.
		/// We need to move that folder away so TQ won't find it.
		/// </summary>
		/// <param name="playerFile">Name of the player file to backup</param>
		public static void BackupStupidPlayerBackupFolder(string playerFile)
		{
			string playerFolder = Path.GetDirectoryName(playerFile);
			string backupFolder = Path.Combine(playerFolder, "Backup");
			if (Directory.Exists(backupFolder))
			{
				// we need to move it.
				string newFolder = Path.Combine(playerFolder, "Backup-moved by TQVault");
				if (Directory.Exists(newFolder))
				{
					try {
						// It already exists--we need to remove it
						Directory.Delete(newFolder, true);
					} catch (Exception e)
					{
						int fn = 1;
						while(Directory.Exists(String.Format("{0}({1})",newFolder,fn)))
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
		/// Gets the base character save folder.
		/// Changed to support custom quest characters
		/// </summary>
		/// <returns>path of the save folder</returns>
		public static string GetBaseCharacterFolder()
		{
			string mapSaveFolder = "Main";

			if (IsCustom)
			{
				mapSaveFolder = "User";
			}

			return Path.Combine(Path.Combine(ImmortalThroneSaveFolder, "SaveData"), mapSaveFolder);
		}

		/// <summary>
		/// Gets the full path to the player character file.
		/// </summary>
		/// <param name="characterName">name of the character</param>
		/// <returns>full path to the character file.</returns>
		public static string GetPlayerFile(string characterName)
		{
			return Path.Combine(Path.Combine(GetBaseCharacterFolder(), string.Concat("_", characterName)), "Player.chr");
		}

		/// <summary>
		/// Gets the full path to the player's stash file.
		/// </summary>
		/// <param name="characterName">name of the character</param>
		/// <returns>full path to the player stash file</returns>
		public static string GetPlayerStashFile(string characterName)
		{
			return Path.Combine(Path.Combine(GetBaseCharacterFolder(), string.Concat("_", characterName)), "winsys.dxb");
		}

		/// <summary>
		/// Gets a list of all of the character files in the save folder.
		/// Added support for loading custom quest characters
		/// </summary>
		/// <returns>List of character files in a string array</returns>
		public static string[] GetCharacterList()
		{
			try
			{
				// Get all folders that start with a '_'.
				string[] folders = Directory.GetDirectories(GetBaseCharacterFolder(), "_*");

				if (folders == null || folders.Length < 1)
				{
					return null;
				}

				List<string> characterList = new List<string>(folders.Length);

				// Copy the names over without the '_' and strip out the path information.
				foreach (string folder in folders)
				{
					characterList.Add(Path.GetFileName(folder).Substring(1));
				}

				// sort alphabetically
				characterList.Sort();
				return characterList.ToArray();
			}
			catch (DirectoryNotFoundException)
			{
				return null;
			}
		}

		/// <summary>
		/// Gets a list of all of the custom maps.
		/// </summary>
		/// <returns>List of custom maps in a string array</returns>
		public static string[] GetCustomMapList()
		{
			try
			{
				// Get all folders in the CustomMaps directory.
				string saveFolder;

				saveFolder = ImmortalThroneSaveFolder;

				string[] mapFolders = Directory.GetDirectories(Path.Combine(saveFolder, "CustomMaps"), "*");

				if (mapFolders == null || mapFolders.Length < 1)
				{
					return null;
				}

				List<string> customMapList = new List<string>(mapFolders.Length);

				// Strip out the path information
				foreach (string mapFolder in mapFolders)
				{
					customMapList.Add(Path.GetFileName(mapFolder));
				}

				// sort alphabetically
				customMapList.Sort();
				return customMapList.ToArray();
			}
			catch (DirectoryNotFoundException)
			{
				return null;
			}
		}

		/// <summary>
		/// Gets the file name and path for a vault.
		/// </summary>
		/// <param name="vaultName">The name of the vault file.</param>
		/// <returns>The full path along with extension of the vault file.</returns>
		public static string GetVaultFile(string vaultName)
		{
			return string.Concat(Path.Combine(TQVaultSaveFolder, vaultName), ".vault");
		}

		/// <summary>
		/// Gets a list of all of the vault files.
		/// </summary>
		/// <returns>The list of all of the vault files in the save folder.</returns>
		public static string[] GetVaultList()
		{
			try
			{
				// Get all files that have a .vault extension.
				string[] files = Directory.GetFiles(TQVaultSaveFolder, "*.vault");

				if (files == null || files.Length < 1)
				{
					return null;
				}

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
		/// Converts a file path to a backup file.  Adding the current date and time to the name.
		/// </summary>
		/// <param name="prefix">prefix of the backup file.</param>
		/// <param name="filePath">Full path of the file to backup.</param>
		/// <returns>Returns the name of the backup file to use for this file.  The backup file will have the given prefix.</returns>
		public static string ConvertFilePathToBackupPath(string prefix, string filePath)
		{
			// Get the actual filename without any path information
			string filename = Path.GetFileName(filePath);

			// Strip the extension off of it.
			string extension = Path.GetExtension(filename);
			if (!string.IsNullOrEmpty(extension))
			{
				filename = Path.GetFileNameWithoutExtension(filename);
			}

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
				{
					return fullFilePath;
				}

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
		public static string BackupFile(string prefix, string file)
		{
			if (File.Exists(file))
			{
				// only backup if it exists!
				string backupFile = ConvertFilePathToBackupPath(prefix, file);

				File.Copy(file, backupFile);

				// Added by VillageIdiot
				// Backup the file pairs for the player stash files.
				if (Path.GetFileName(file).ToUpperInvariant() == "WINSYS.DXB")
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
			{
				return null;
			}
		}
	}
}