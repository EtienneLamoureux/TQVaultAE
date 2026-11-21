namespace TQVaultAE.Application.DTOs;

/// <summary>
/// Why a name is refused for a new or renamed tag.
/// </summary>
public enum TagNameError
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
	/// A rename keeps the current name, ignoring case.
	/// </summary>
	SameAsCurrent,

	/// <summary>
	/// Another tag already has this name, ignoring case.
	/// </summary>
	AlreadyExists,
}
