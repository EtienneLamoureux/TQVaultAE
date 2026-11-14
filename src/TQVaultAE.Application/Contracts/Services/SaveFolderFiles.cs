namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// The files of a character save folder, from the file names <see cref="IGamePathService"/> gives.
/// </summary>
public static class SaveFolderFiles
{
	/// <summary>
	/// The player file of a character save folder.
	/// </summary>
	public static string PlayerFileIn(this IGamePathService paths, IPathIO pathIO, string saveFolder)
		=> pathIO.Combine(saveFolder, paths.PlayerSaveFileName);

	/// <summary>
	/// The stash file of a character save folder.
	/// </summary>
	public static string StashFileIn(this IGamePathService paths, IPathIO pathIO, string saveFolder)
		=> pathIO.Combine(saveFolder, paths.PlayerStashFileNameB);
}
