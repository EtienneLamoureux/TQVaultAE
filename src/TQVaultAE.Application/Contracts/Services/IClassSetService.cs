using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// The class sets of the characters (saved builds: masteries, skill points, attribute points): a file in each
/// character's save folder, beside the loadouts file. The character editor reads it when it opens and writes it on OK.
/// </summary>
public interface IClassSetService
{
	/// <summary>The class sets file, in the character's save folder next to its player file and its loadouts file.</summary>
	const string FileName = "TQVaultAE.ClassSets.json";

	/// <summary>Reads the class sets of a character: none without a file; <see cref="ClassSetBook.ReadFailed"/> when it could not be read.</summary>
	/// <param name="saveFolder">The character's save folder.</param>
	ClassSetBook Read(string saveFolder);

	/// <summary>Writes the class sets of a character (the file is deleted when none is left) and keeps them in the book.</summary>
	/// <param name="book">The character's class sets, as read.</param>
	/// <param name="sets">The class sets to write.</param>
	/// <param name="lastApplied">The set applied or saved last, shown at the next opening; null for none.</param>
	/// <exception cref="IOException">The file could not be written.</exception>
	/// <exception cref="UnauthorizedAccessException">The file or its folder is read-only or protected.</exception>
	void Write(ClassSetBook book, IReadOnlyList<ClassSet> sets, string? lastApplied);
}
