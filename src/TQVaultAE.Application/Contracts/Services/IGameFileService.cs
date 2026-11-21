using TQVaultAE.Application.Session;

namespace TQVaultAE.Application.Contracts.Services;

public interface IGameFileService
{
	/// <summary>
	/// Init the local git repository.
	/// </summary>
	/// <returns>The step that failed, for the UI to tell the user, or null.</returns>
	Results.GitBackupError? GitRepositorySetup();
	/// <summary>
	/// add, commit, tag and push the git repo.
	/// </summary>
	/// <returns>Whether a change has been pushed, and the step that failed, for the UI to tell the user.</returns>
	Results.GitPushResult GitAddCommitTagAndPush();
	/// <summary>
	/// Backs up the file to the backup folder.
	/// </summary>
	/// <param name="prefix">prefix of the backup file</param>
	/// <param name="file">file name to backup</param>
	/// <returns>Returns the name of the backup file, or NULL if file does not exist</returns>
	string? BackupFile(string prefix, string file);
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
	/// Gets the save folder <see cref="DuplicateCharacterFiles"/> creates for <paramref name="newname"/>.
	/// </summary>
	/// <param name="playerSaveDirectory">Save folder of the character to duplicate.</param>
	/// <param name="newname">Name of the new character.</param>
	/// <returns>The new save folder path, beside the source one.</returns>
	string GetDuplicateCharacterFolder(string playerSaveDirectory, string newname);

	/// <summary>
	/// Archives or unarchives a character: moves its save folder, then the save and its file watchers follow it.
	/// Nothing happens when the save is in that state already.
	/// </summary>
	/// <param name="ps">The character.</param>
	/// <param name="archive"><see langword="true"/> to archive, <see langword="false"/> to unarchive.</param>
	/// <exception cref="IOException">The folder could not be moved; the save is left where it was.</exception>
	void ToggleArchive(PlayerSave ps, bool archive);
}
