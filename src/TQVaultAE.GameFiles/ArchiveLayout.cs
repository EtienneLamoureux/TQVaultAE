//-----------------------------------------------------------------------
// <copyright file="ArchiveLayout.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Text.RegularExpressions;

namespace TQVaultAE.GameFiles;

/// <summary>
/// Folders of the game install that hold the resource archives, and the resource id prefix of an archive: the first
/// segments of the ids by which the database references the archive's entries. Shared by tqdb and TQVault, whose
/// <c>GameArchives</c> maps the other way, from a resource id to its archive.
/// </summary>
/// <remarks>
/// The prefix is the archive's path relative to the <c>Resources</c> or <c>Audio</c> folder, without extension and
/// without the language suffix of the localized archives, with <c>'\'</c> separators:
/// <list type="bullet">
/// <item><c>Resources\Items.arc</c>, <c>Audio\Sounds.arc</c>: <c>Items</c>, <c>Sounds</c>.</item>
/// <item><c>Resources\XPack2\Items.arc</c>: <c>XPack2\Items</c>.</item>
/// <item><c>Audio\Dialog_DE.arc</c>, <c>Resources\XPack2\Sounds_FR.arc</c>: <c>Dialog</c>, <c>XPack2\Sounds</c>, as the
/// localized archive replaces the archive of the same name in the player's language.</item>
/// <item>Any other archive (<c>Text\Text_EN.arc</c>, <c>Toolset\*.arc</c>, an archive outside a game install): its
/// file name without extension.</item>
/// </list>
/// The Immortal Throne sounds keep their <c>xpack/</c> folder inside <c>Resources\Sounds.arc</c>, so their ids are
/// <c>Sounds\xpack\...</c> with no special case.
/// </remarks>
public static partial class ArchiveLayout
{
	/// <summary>Folder of the game install that holds the resource archives and the expansion folders.</summary>
	public const string ResourcesFolder = "Resources";

	/// <summary>Folder of the game install that holds the sound, music and dialog archives of the base game.</summary>
	public const string AudioFolder = "Audio";

	/// <summary>Expansion folders are named <c>XPack</c>, <c>XPack2</c>, ... (Immortal Throne's is lowercase on disk).</summary>
	public static bool IsExpansionFolder(string name)
		=> name.StartsWith("XPACK", StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Resource id prefix of the archive at <paramref name="archivePath"/> (absolute or relative to the game folder):
	/// <c>XPack2\Items</c> for <c>...\Resources\XPack2\Items.arc</c>.
	/// </summary>
	public static string ResourcePrefix(string archivePath)
	{
		ArgumentNullException.ThrowIfNull(archivePath);
		var stem = Path.GetFileNameWithoutExtension(archivePath);
		var folder = Path.GetDirectoryName(archivePath);
		var parent = Path.GetFileName(folder) ?? string.Empty;

		if (parent.Equals(ResourcesFolder, StringComparison.OrdinalIgnoreCase)
			|| parent.Equals(AudioFolder, StringComparison.OrdinalIgnoreCase))
			return Unlocalized(stem);

		if (IsExpansionFolder(parent)
			&& string.Equals(Path.GetFileName(Path.GetDirectoryName(folder)), ResourcesFolder, StringComparison.OrdinalIgnoreCase))
			return $"{parent}\\{Unlocalized(stem)}";

		return stem;
	}

	/// <summary><c>Dialog</c> for <c>Dialog_DE</c>: localized archives end with <c>_</c> and a two-letter language code.</summary>
	private static string Unlocalized(string stem)
	{
		var match = LocalizedStem().Match(stem);
		return match.Success ? match.Groups[1].Value : stem;
	}

	[GeneratedRegex("^(.+)_[A-Za-z]{2}$", RegexOptions.CultureInvariant)]
	private static partial Regex LocalizedStem();
}
