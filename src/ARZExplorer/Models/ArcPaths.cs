using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace ArzExplorer.Models;

/// <summary>
/// Paths of the ARC entries as shown in the tree, and their conversion to the ids <see cref="TQVaultAE.Application.Contracts.Providers.IArcFileProvider"/> expects.
/// </summary>
/// <remarks>
/// The tree shows an entry of <c>Resources\Items.arc</c> as <c>Items\entry</c>,
/// and an entry of <c>Resources\XPack2\Items.arc</c> as <c>XPack2\Items\entry</c>, the way the database references them.
/// </remarks>
internal static class ArcPaths
{
	/// <summary>
	/// Prefix of the tree keys of the entries of an ARC file: its name, after its expansion folder if it is in one.
	/// </summary>
	/// <param name="arcFile">Full path of the ARC file.</param>
	internal static string TreePrefix(RecordId arcFile)
	{
		var tokens = arcFile.TokensRaw;
		var name = Path.GetFileNameWithoutExtension(tokens[^1]);
		var folder = tokens.Count > 1 ? tokens[^2] : string.Empty;
		return IsExpansionToken(folder) ? folder + '\\' + name : name;
	}

	/// <summary>
	/// Id of a tree key for <c>IArcFileProvider.GetData</c>: the ARC file name followed by the entry.
	/// </summary>
	internal static RecordId DataId(RecordId treeKey)
		=> IsExpansionToken(treeKey.TokensRaw[0])
			? treeKey.TokensRaw.Skip(1).JoinString("\\").ToRecordId()
			: treeKey;

	/// <summary>
	/// Entry of a tree key inside its ARC file, for <c>IArcFileProvider.Write</c>.
	/// </summary>
	internal static RecordId EntryId(RecordId treeKey)
		=> DataId(treeKey).TokensRaw.Skip(1).JoinString("\\").ToRecordId();

	/// <summary>
	/// ARC file, relative to the game folder, that holds a resource referenced by the database.
	/// </summary>
	/// <param name="resourceId">Resource path as the database references it, like <c>XPack\Items\x.tex</c>.</param>
	internal static RecordId ArcFileOf(RecordId resourceId)
	{
		var tokens = resourceId.TokensNormalized;
		if (IsExpansionToken(tokens[0]) && tokens.Count > 1)
			return $@"RESOURCES\{tokens[0]}\{tokens[1]}.ARC";

		// Sounds of the base game are in the Audio folder, those of Immortal Throne in Resources\Sounds.arc under "xpack/"
		var normalized = resourceId.Normalized;
		var isSound = normalized.EndsWith(".MP3", StringComparison.Ordinal) || normalized.EndsWith(".WAV", StringComparison.Ordinal);
		if (isSound && !(tokens.Count > 1 && IsExpansionToken(tokens[1])))
			return $@"AUDIO\{tokens[0]}.ARC";

		return $@"RESOURCES\{tokens[0]}.ARC";
	}

	/// <summary>
	/// Expansion folders are named <c>XPack</c>, <c>XPack2</c>, ...
	/// </summary>
	private static bool IsExpansionToken(string token)
		=> token.StartsWith("XPACK", StringComparison.OrdinalIgnoreCase);
}
