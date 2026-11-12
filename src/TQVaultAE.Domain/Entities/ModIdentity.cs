namespace TQVaultAE.Domain.Entities;

/// <summary>
/// The identity of a mod as items, vaults and links name it (<see cref="Item.Mod"/>): its folder name, which the game
/// names its database and shared stash folder after, and where it is installed from, since a Steam Workshop mod and a
/// custom map can have the same folder name. In memory both are one key, <c>name|source</c>; files keep the name in
/// <c>"mod"</c> and the source in <c>"modSource"</c>, so that older versions still read the name.
/// </summary>
public static class ModIdentity
{
	/// <summary>Between the name and the source: a character a folder name cannot hold.</summary>
	public const char Separator = '|';

	/// <summary>The source of a mod in the custom maps folder (<c>My Games\...\CustomMaps</c>).</summary>
	public const string CustomMaps = "custommaps";

	/// <summary>The prefix of the source of a Steam Workshop mod, followed by its Workshop item id.</summary>
	public const string WorkshopPrefix = "workshop:";

	/// <summary>
	/// The key of a mod from its name and source; the name alone without a source (a file written before the source
	/// was kept).
	/// </summary>
	/// <returns>Null for no name (the base game).</returns>
	public static string? Compose(string? name, string? source)
		=> string.IsNullOrWhiteSpace(name)
			? null
			: string.IsNullOrWhiteSpace(source) ? name : $"{name}{Separator}{source}";

	/// <summary>The mod's folder name, as shown to the user.</summary>
	public static string? NameOf(string? key)
	{
		if (key is null)
			return null;

		int at = key.IndexOf(Separator, StringComparison.Ordinal);
		return at < 0 ? key : key[..at];
	}

	/// <summary>
	/// Whether two keys may name the same mod's data: the same name, and the same source unless one of them has none
	/// (a file written before the source was kept). Null is the base game.
	/// </summary>
	public static bool SameData(string? a, string? b)
	{
		if (a is null || b is null)
			return a is null && b is null;

		if (!string.Equals(NameOf(a), NameOf(b), StringComparison.OrdinalIgnoreCase))
			return false;

		string? sourceA = SourceOf(a), sourceB = SourceOf(b);
		return sourceA is null || sourceB is null || string.Equals(sourceA, sourceB, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>Where the mod is installed from; null when the key names the mod only.</summary>
	public static string? SourceOf(string? key)
	{
		if (key is null)
			return null;

		int at = key.IndexOf(Separator, StringComparison.Ordinal);
		return at < 0 || at == key.Length - 1 ? null : key[(at + 1)..];
	}
}
