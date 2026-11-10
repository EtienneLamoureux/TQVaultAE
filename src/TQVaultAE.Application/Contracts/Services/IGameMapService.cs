using TQVaultAE.Application.DTOs;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// A cache of data read from the game database, emptied when the active custom map (mod) changes, since the same record
/// or resource can differ from one map to another.
/// </summary>
public interface IGameDataCache
{
	/// <summary>
	/// Forgets everything read from the game database.
	/// </summary>
	void ClearGameDataCache();
}

/// <summary>
/// The custom map (mod) whose data the session uses now: regular and modded characters live in one session, each loaded
/// with its own map's database, archives and shared stashes.
/// </summary>
public interface IGameMapService
{
	/// <summary>
	/// The active custom map folder; empty for the base game.
	/// </summary>
	string ActiveMap { get; }

	/// <summary>
	/// Raised after the active map changed.
	/// </summary>
	event EventHandler? ActiveMapChanged;

	/// <summary>
	/// The custom map a character is played with: its linked mod when installed, else none (the base game data).
	/// </summary>
	string MapFor(PlayerSave save);

	/// <summary>
	/// The mod of a character, resolved from its link.
	/// </summary>
	CharacterMod ModOf(PlayerSave save);

	/// <summary>
	/// Makes a character's map the active one: its database, archives and shared stash files.
	/// </summary>
	/// <returns>True when the active map changed.</returns>
	bool ActivateFor(PlayerSave save);

	/// <summary>
	/// Makes a custom map the active one.
	/// </summary>
	/// <param name="mapFolder">The custom map folder; empty for the base game.</param>
	/// <returns>True when the active map changed.</returns>
	bool Activate(string? mapFolder);

	/// <summary>
	/// True when a character is played with the active map.
	/// </summary>
	bool UsesActiveMap(PlayerSave save);

	/// <summary>
	/// Updates a listed character after its link changed: its mod and its label.
	/// </summary>
	void Refresh(PlayerSave save);

	/// <summary>
	/// Notes that a character's files were just read with the active map's database.
	/// </summary>
	void MarkLoaded(PlayerSave save);

	/// <summary>
	/// True when a character's files were read with another map's database than the active one: its items were
	/// resolved with other records and must be read again.
	/// </summary>
	bool LoadedWithOtherMap(PlayerSave save);
}
