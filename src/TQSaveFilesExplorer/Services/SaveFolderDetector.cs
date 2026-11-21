using System.Collections.Generic;
using System.IO;
using System.Linq;
using TQVaultAE.Application.Contracts.Services;

namespace TQ.SaveFilesExplorer.Services;

/// <summary>
/// A save folder found on this computer: <paramref name="Label"/> is its menu text, <paramref name="Path"/> the folder
/// whose save files it opens.
/// </summary>
public sealed record DetectedSaveFolder(string Label, string Path);

/// <summary>
/// Finds the game's save folders through TQVault's <see cref="IGamePathService"/>, so the explorer looks where TQVault does.
/// Only reads folder names: no game installation is needed.
/// </summary>
public sealed class SaveFolderDetector(IGamePathService gamePath, IDirectoryIO directoryIO)
{
	private string MainFolderTQIT => gamePath.GetBaseCharacterFolder(IsTQIT: true, isCustomCharacter: false, isArchive: false);

	private string MainFolderTQ => gamePath.GetBaseCharacterFolder(IsTQIT: false, isCustomCharacter: false, isArchive: false);

	private string CustomMapFolderTQIT => gamePath.GetBaseCharacterFolder(IsTQIT: true, isCustomCharacter: true, isArchive: false);

	/// <summary>The <c>SaveData\Sys</c> folder of the base game's transfer stash; each custom map has a sub folder there.</summary>
	private string SharedStashFolder => Path.GetDirectoryName(gamePath.TransferStashFileFullPathFor(string.Empty))!;

	/// <summary>
	/// The folder the open dialogs start in: the Immortal Throne characters, else the Titan Quest ones, else
	/// <see langword="null"/> when neither exists.
	/// </summary>
	public string? DefaultSaveDirectory
		=> new[] { this.MainFolderTQIT, this.MainFolderTQ }.FirstOrDefault(directoryIO.Exists);

	/// <summary>
	/// The save folders found, in menu groups: main campaign characters, transfer stash, custom map transfer stashes,
	/// custom map characters. A group is empty when its folder does not exist.
	/// </summary>
	public IReadOnlyList<IReadOnlyList<DetectedSaveFolder>> Detect()
	{
		var players = this.Characters(this.MainFolderTQIT, "TQIT")
			.Concat(this.Characters(this.MainFolderTQ, "TQ"))
			.ToArray();

		var sys = this.SharedStashFolder;
		DetectedSaveFolder[] transferStash = directoryIO.Exists(sys) ? [new("TQIT : Transfer Stash", sys)] : [];

		DetectedSaveFolder[] customMapStashes = directoryIO.Exists(sys)
			? [.. directoryIO.GetDirectories(sys).Select(d => new DetectedSaveFolder($"{Path.GetFileName(d)} : Transfer Stash", d))]
			: [];

		var customMapPlayers = this.Characters(this.CustomMapFolderTQIT, "Mod").ToArray();

		return [players, transferStash, customMapStashes, customMapPlayers];
	}

	/// <summary>The character folders (<c>_name</c>) of <paramref name="folder"/>, labelled "<paramref name="prefix"/> : _name".</summary>
	private IEnumerable<DetectedSaveFolder> Characters(string folder, string prefix)
		=> directoryIO.Exists(folder)
			? directoryIO.GetDirectories(folder, "_*").Select(d => new DetectedSaveFolder($"{prefix} : {Path.GetFileName(d)}", d))
			: [];
}
