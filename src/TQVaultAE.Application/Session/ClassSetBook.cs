using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Session;

/// <summary>The class sets of one character, as read from its class sets file.</summary>
/// <param name="filePath">The class sets file, in the character's save folder.</param>
public sealed class ClassSetBook(string filePath)
{
	/// <summary>The class sets file.</summary>
	public string FilePath { get; } = filePath;

	/// <summary>The class sets, in the order of the file.</summary>
	public IReadOnlyList<ClassSet> Sets { get; set; } = [];

	/// <summary>The name of the set applied or saved last (written on OK), or null.</summary>
	public string? LastApplied { get; set; }

	/// <summary>
	/// True when the file exists but could not be read: nothing is written over it, so that a damaged or locked file is
	/// not replaced by an empty list.
	/// </summary>
	public bool ReadFailed { get; set; }
}
