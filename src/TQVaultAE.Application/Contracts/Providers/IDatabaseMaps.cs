namespace TQVaultAE.Application.Contracts.Providers;

/// <summary>
/// One game database per custom map (mod): <see cref="IDatabase"/> answers from the active one, and switching keeps the
/// others loaded, so going back to a map is instant.
/// </summary>
public interface IDatabaseMaps
{
	/// <summary>
	/// The custom map folder of the active database; empty for the base game.
	/// </summary>
	string ActiveMapFolder { get; }

	/// <summary>
	/// The custom map folders whose database is loaded (empty string for the base game).
	/// </summary>
	IReadOnlyCollection<string> LoadedMapFolders { get; }

	/// <summary>
	/// Makes the database of a custom map the active one, loading it the first time.
	/// </summary>
	/// <param name="mapFolder">The custom map folder; null or empty for the base game.</param>
	/// <returns>True when the active database changed.</returns>
	bool Activate(string? mapFolder);

	/// <summary>
	/// Makes the database of a custom map answer for the current thread until the result is disposed, loading it the
	/// first time: an item linked to a mod is read and shown with its mod's data whatever the active map is. Scopes nest.
	/// </summary>
	/// <param name="mapFolder">The custom map folder; null or empty for the base game.</param>
	IDisposable Use(string? mapFolder);
}
