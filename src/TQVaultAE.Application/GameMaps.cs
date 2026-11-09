namespace TQVaultAE.Application;

/// <summary>
/// Rules shared by everything that names a custom map (mod) folder.
/// </summary>
public static class GameMaps
{
	/// <summary>
	/// The key of a custom map: its folder without a trailing separator, or empty for the base game (null, blank or
	/// <c>main</c>, the name the game uses for the main campaign).
	/// </summary>
	public static string Normalize(string? mapFolder)
		=> string.IsNullOrWhiteSpace(mapFolder) || mapFolder.Trim().Equals("main", StringComparison.OrdinalIgnoreCase)
			? string.Empty
			: Path.TrimEndingDirectorySeparator(mapFolder.Trim());

	/// <summary>
	/// True when two custom map folders name the same map.
	/// </summary>
	public static bool Same(string? a, string? b)
		=> string.Equals(Normalize(a), Normalize(b), StringComparison.OrdinalIgnoreCase);
}
