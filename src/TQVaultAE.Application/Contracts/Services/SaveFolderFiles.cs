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

	/// <summary>
	/// The files of a character in its save folder, the one list of them: a duplicate copies each one that exists, and
	/// the read-only toggle covers those TQVault writes (<see cref="CharacterFile.WrittenByTQVault"/>).
	/// </summary>
	public static IReadOnlyList<CharacterFile> CharacterFiles(this IGamePathService paths)
		=> [
			new(paths.PlayerSaveFileName, WrittenByTQVault: true),
			new(paths.PlayerStashFileNameB, WrittenByTQVault: true),
			new(paths.PlayerStashFileNameG, WrittenByTQVault: true),
			// The game's own settings: TQVault never writes them.
			new(paths.PlayerSettingsFileName, WrittenByTQVault: false),
			// The mod link: a copy plays with the same mod.
			new(IModLinkService.LinkFileName, WrittenByTQVault: true),
			new(ILoadoutService.FileName, WrittenByTQVault: true),
		];
}

/// <summary>
/// A file of a character save folder.
/// </summary>
/// <param name="Name">Its file name.</param>
/// <param name="WrittenByTQVault">Whether TQVault writes it, so the character's read-only state covers it.</param>
public sealed record CharacterFile(string Name, bool WrittenByTQVault)
{
	/// <summary>
	/// The file in <paramref name="saveFolder"/>.
	/// </summary>
	public string In(IPathIO pathIO, string saveFolder) => pathIO.Combine(saveFolder, this.Name);
}
