using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Domain.Contracts.Services;

public interface IGameFileService
{
	/// <summary>
	/// Init the local git repository.
	/// </summary>
	void GitRepositorySetup();
	/// <summary>
	/// add, commit, tag and push the git repo.
	/// </summary>
	/// <returns><c>true</c> if a change has been pushed</returns>
	bool GitAddCommitTagAndPush();
	/// <summary>
	/// Backs up the file to the backup folder.
	/// </summary>
	/// <param name="prefix">prefix of the backup file</param>
	/// <param name="file">file name to backup</param>
	/// <returns>Returns the name of the backup file, or NULL if file does not exist</returns>
	string BackupFile(string prefix, string file);
	/// <summary>
	/// TQ has an annoying habit of throwing away your char in preference
	/// for the Backup folder if it exists if it thinks your char is not valid.
	/// We need to move that folder away so TQ won't find it.
	/// </summary>
	/// <param name="playerFile">Name of the player file to backup</param>
	void BackupStupidPlayerBackupFolder(string playerFile);
	
	/// <summary>
	/// Duplicate player save files
	/// </summary>
	/// <param name="playerSaveDirectory"></param>
	/// <param name="newname"></param>
	/// <returns>new directory path</returns>
	string DuplicateCharacterFiles(string playerSaveDirectory, string newname);

	/// <summary>
	/// Archive a character
	/// </summary>
	/// <param name="ps"></param>
	bool Archive(PlayerSave ps);

	/// <summary>
	/// Unarchive a character
	/// </summary>
	/// <param name="ps"></param>
	bool Unarchive(PlayerSave ps);
}
