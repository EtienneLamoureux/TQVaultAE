using System.IO;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Domain.Contracts.Services;

public interface IGamePathService
{
	/// <summary>
	/// Json Vault file name extension 
	/// </summary>
	string VaultFileNameExtensionJson { get; }
	/// <summary>
	/// Classic Vault file name extension 
	/// </summary>
	string VaultFileNameExtensionOld { get; }

	/// <summary>
	/// Return current path to Git Backup repo
	/// </summary>
	string LocalGitRepositoryDirectory { get; }
	/// <summary>
	/// Return current path to Git Backup repo <c>.git</c> directory
	/// </summary>
	string LocalGitRepositoryGitDir { get; }
	/// <summary>
	/// Does Git Backup repo <c>.git</c> directory exist ?
	/// </summary>
	bool LocalGitRepositoryGitDirExist { get; }

	/// <summary>
	/// Parses filename to try to determine the base character name.
	/// </summary>
	/// <param name="filename">filename of the character file</param>
	/// <returns>string containing the character name</returns>
	string GetNameFromFile(string filename);

	/// <summary>
	/// Gets or sets the Immortal Throne game path.
	/// </summary>
	string ImmortalThronePath { get; set; }
	/// <summary>
	/// Gets the Immortal Throne Character save folder
	/// </summary>
	string ImmortalThroneSaveFolder { get; }
	/// <summary>
	/// Gets a value indicating whether Atlantis DLC has been installed.
	/// </summary>
	bool IsAtlantisInstalled { get; }
	/// <summary>
	/// Gets a value indicating whether a custom map has been specified.
	/// </summary>
	bool IsCustom { get; }
	/// <summary>
	/// Gets a value indicating whether Ragnarok DLC has been installed.
	/// </summary>
	bool IsRagnarokInstalled { get; }
	/// <summary>
	/// Gets a value indicating whether Eternal Embers DLC has been installed.
	/// </summary>
	bool IsEmbersInstalled { get; }
	/// <summary>
	/// Gets or sets the name of the custom map.
	/// Added to support custom quest characters
	/// </summary>
	string MapName { get; set; }
	/// <summary>
	/// Gets the filename for the game's relic vault stash.
	/// Stash files for Mods all have their own subdirectory which is the same as the mod's custom map folder
	/// </summary>
	string RelicVaultStashFileFullPath { get; }
	/// <summary>
	/// Gets or sets the Titan Quest game path.
	/// </summary>
	string TQPath { get; set; }
	/// <summary>
	/// Gets the Titan Quest Character save folder.
	/// </summary>
	string TQSaveFolder { get; }
	/// <summary>
	/// Gets the name of the game settings file.
	/// </summary>
	string TQSettingsFile { get; }
	/// <summary>
	/// Gets the name of the game settings file.
	/// </summary>
	string ITSettingsFile { get; }
	/// <summary>
	/// Gets the vault backup folder path.
	/// </summary>
	string TQVaultBackupFolder { get; }
	/// <summary>
	/// Gets the current Vault save folder.
	/// Should be equal to the config value "VaultPath".
	/// </summary>
	string TQVaultSaveFolder { get; set; }
	/// <summary>
	/// Gets the filename for the game's transfer stash.
	/// Stash files for Mods all have their own subdirectory which is the same as the mod's custom map folder
	/// </summary>
	string TransferStashFileFullPath { get; }
	/// <summary>
	/// Gets a value indicating whether the vault save folder has been changed.
	/// Usually done via settings and triggers a reload of the vaults.
	/// </summary>
	bool VaultFolderChanged { get; }

	/// <summary>
	/// Converts a file path to a backup file.  Adding the current date and time to the name.
	/// </summary>
	/// <param name="prefix">prefix of the backup file.</param>
	/// <param name="filePath">Full path of the file to backup.</param>
	/// <returns>Returns the name of the backup file to use for this file.  The backup file will have the given prefix.</returns>
	string ConvertFilePathToBackupPath(string prefix, string filePath);
	/// <summary>
	/// Gets the base character save folder.
	/// Changed to support custom quest characters
	/// </summary>
	/// <returns>path of the save folder</returns>
	string GetBaseCharacterFolder();
	/// <summary>
	/// Gets a list of all of the character files in the save folder.
	/// </summary>
	/// <returns>List of character files in a string array</returns>
	string[] GetCharacterList();
	/// <summary>
	/// Return all known custom map directories
	/// </summary>
	/// <returns>List of custom maps in a string array</returns>
	GamePathEntry[] GetCustomMapList();
	/// <summary>
	/// Gets the full path to the player character file.
	/// </summary>
	/// <param name="characterName">name of the character</param>
	/// <returns>full path to the character file.</returns>
	string GetPlayerFile(string characterName);
	/// <summary>
	/// Gets the full path to the player's stash file.
	/// </summary>
	/// <param name="characterName">name of the character</param>
	/// <returns>full path to the player stash file</returns>
	string GetPlayerStashFile(string characterName);
	/// <summary>
	/// Gets the file name and path for a vault.
	/// </summary>
	/// <param name="vaultName">The name of the vault file.</param>
	/// <returns>The full path along with extension of the vault file.</returns>
	string GetVaultFile(string vaultName);
	/// <summary>
	/// Gets a list of all of the vault files.
	/// </summary>
	/// <returns>The list of all of the vault files in the save folder.</returns>
	string[] GetVaultList();
	/// <summary>
	/// Try to resolve the local game path
	/// </summary>
	/// <returns></returns>
	string ResolveGamePath();

	string TransferStashFileName { get; }
	string RelicVaultStashFileName { get; }
	string PlayerSaveFileName { get; }
	string PlayerStashFileNameB { get; }
	string PlayerStashFileNameG { get; }
	string PlayerSettingsFileName { get; }


	/// <summary>
	/// Return the vault name from <paramref name="vaultFilePath"/>
	/// </summary>
	/// <param name="vaultFilePath"></param>
	/// <returns>name stripted from path and extension</returns>
	string GetVaultNameFromPath(string vaultFilePath);

	/// <summary>
	/// Return ARC filename from <paramref name="resourceIdOrPrefix"/>
	/// </summary>
	/// <param name="resourceIdOrPrefix"></param>
	/// <returns></returns>
	(string ArcFileName, bool IsDLC) ResolveArcFileName(RecordId resourceIdOrPrefix);

}