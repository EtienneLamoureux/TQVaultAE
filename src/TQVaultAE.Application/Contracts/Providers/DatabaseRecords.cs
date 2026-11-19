using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Providers;

/// <summary>The record tables of the databases an <see cref="IDatabase"/> has loaded.</summary>
public static class DatabaseRecords
{
	/// <summary>
	/// The loaded databases, the one whose records win first: the custom map (priority 0), Immortal Throne when it is a
	/// separate file (1), the base game (2).
	/// </summary>
	public static IEnumerable<(int Priority, ArzFile ArzFile)> ArzFilesByPriority(this IDatabase database)
	{
		ArgumentNullException.ThrowIfNull(database);
		if (database.ArzFileMod is not null)
			yield return (0, database.ArzFileMod);

		if (database.ArzFileIT is not null && database.ArzFileIT != database.ArzFile)
			yield return (1, database.ArzFileIT);

		if (database.ArzFile is not null)
			yield return (2, database.ArzFile);
	}

	/// <summary>
	/// The records of some classes (ignoring case) in each loaded database, by priority: a record overridden by a
	/// custom map comes once per database that has it, the winning one first.
	/// </summary>
	public static IEnumerable<(RecordId Id, string Class, int Priority)> RecordsOfClass(this IDatabase database, params string[] classes)
		=> database.ArzFilesByPriority().SelectMany(db => db.ArzFile.RecordInfo
			.Where(r => classes.Contains(r.Value.Class, StringComparer.OrdinalIgnoreCase))
			.Select(r => (r.Key, r.Value.Class, db.Priority)));
}
