namespace TQVaultAE.Application.DTOs;

/// <summary>
/// Why a name is refused for a duplicated character.
/// </summary>
public enum CharacterNameError
{
	/// <summary>
	/// The name is valid.
	/// </summary>
	None = 0,

	/// <summary>
	/// The name is empty or white space.
	/// </summary>
	Empty,

	/// <summary>
	/// The name holds characters not allowed in a path or a file name.
	/// </summary>
	InvalidCharacters,

	/// <summary>
	/// A character, or a save folder, has this name already (case ignored).
	/// </summary>
	AlreadyUsed,
}
