using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Helpers;

/// <summary>
/// Where the game keeps the resource a database record references: the ARC file, relative to the game folder (or
/// to a custom map's folder), for a resource id such as <c>XPack\Items\x.tex</c>.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><c>XPack\Items\x.tex</c>, <c>XPack2\Dialog\x.mp3</c>: <c>Resources\XPackN\&lt;name&gt;.arc</c>, the entry after
/// the <c>XPackN\&lt;name&gt;\</c> prefix.</item>
/// <item>Sounds of the base game, <c>Sounds\x.wav</c>, <c>Dialog\x.mp3</c>, <c>PlayerSounds\x.wav</c>, <c>Music\x.wav</c>:
/// <c>Audio\&lt;name&gt;.arc</c>.</item>
/// <item>Sounds of Immortal Throne, <c>Sounds\XPack\x.wav</c>: <c>Resources\Sounds.arc</c>, under <c>xpack/</c>.</item>
/// <item>Anything else, <c>Items\x.tex</c>: <c>Resources\&lt;name&gt;.arc</c>.</item>
/// </list>
/// </remarks>
public static class GameArchives
{
	public const string ResourcesFolder = "Resources";
	public const string AudioFolder = "Audio";

	/// <summary>
	/// Folders and file name of the ARC file that holds a resource, relative to the game folder, in the case of the id:
	/// <c>["Resources", "XPack", "Items.arc"]</c> for <c>XPack\Items\x.tex</c>.
	/// </summary>
	public static string[] ArcFileSegments(RecordId resourceId)
	{
		var tokens = resourceId.TokensRaw;
		if (IsExpansionResource(resourceId))
			return [ResourcesFolder, tokens[0], tokens[1] + ".arc"];

		if (IsSound(resourceId) && !(tokens.Count > 1 && IsExpansionToken(tokens[1])))
			return [AudioFolder, tokens[0] + ".arc"];

		return [ResourcesFolder, tokens[0] + ".arc"];
	}

	/// <summary>
	/// ARC file that holds a resource, relative to the game folder: <c>Resources\XPack\Items.arc</c> for <c>XPack\Items\x.tex</c>.
	/// </summary>
	public static string ArcFileOf(RecordId resourceId)
		=> string.Join('\\', ArcFileSegments(resourceId));

	/// <summary>
	/// The resource is in an expansion folder (<c>XPack\Items\x.tex</c>): its entry in the ARC file starts after the
	/// expansion and archive names, not after the archive name only.
	/// </summary>
	public static bool IsExpansionResource(RecordId resourceId)
		=> resourceId.TokensRaw.Count > 1 && IsExpansionToken(resourceId.TokensRaw[0]);

	/// <summary>
	/// Expansion folders are named <c>XPack</c>, <c>XPack2</c>, ...
	/// </summary>
	public static bool IsExpansionToken(string token)
		=> token.StartsWith("XPACK", StringComparison.OrdinalIgnoreCase);

	private static bool IsSound(RecordId resourceId)
		=> resourceId.Normalized.EndsWith(".MP3", StringComparison.Ordinal)
			|| resourceId.Normalized.EndsWith(".WAV", StringComparison.Ordinal);
}
