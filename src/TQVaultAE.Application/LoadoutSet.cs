using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application;

/// <summary>
/// The loadouts of one character, as read from its loadouts file.
/// </summary>
/// <param name="filePath">The loadouts file, in the character's save folder.</param>
/// <param name="characterName">The character's name (its save folder without the underscore).</param>
/// <param name="mod">The character's map: the pieces are read with it, as the character's items.</param>
public sealed class LoadoutSet(string filePath, string characterName, string? mod)
{
	/// <summary>The loadouts file, in the character's save folder.</summary>
	public string FilePath { get; } = filePath;

	/// <summary>The character's name.</summary>
	public string CharacterName { get; } = characterName;

	/// <summary>The character's map (null for the base game).</summary>
	public string? Mod { get; } = mod;

	/// <summary>The loadouts, in the order of the file.</summary>
	public IReadOnlyList<Loadout> Loadouts { get; set; } = [];

	/// <summary>
	/// True when the file exists but could not be read: nothing is written over it, so that a damaged or locked file is
	/// not replaced by an empty list.
	/// </summary>
	public bool ReadFailed { get; set; }

	/// <summary>The loadout of that name (case ignored), or null.</summary>
	/// <param name="name">The name.</param>
	public Loadout? Find(string? name)
		=> this.Loadouts.FirstOrDefault(l => string.Equals(l.Name, name?.Trim(), StringComparison.OrdinalIgnoreCase));
}
