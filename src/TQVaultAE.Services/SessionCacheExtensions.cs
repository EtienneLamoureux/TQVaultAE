using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Services;

/// <summary>
/// Reading a save file into the session caches (<see cref="Application.SessionContext"/>).
/// </summary>
internal static class SessionCacheExtensions
{
	/// <summary>
	/// The container of <paramref name="file"/>: read once, then taken from the cache, unless <paramref name="reload"/>
	/// (the file changed on disk), where the file is read again and its new items replace the old ones in the search.
	/// </summary>
	/// <param name="load">Reads the file and puts its items in the search.</param>
	public static T LoadOrReload<T>(this LazyConcurrentDictionary<string, T> cache, string file, Func<string, T> load, bool reload, IItemDatabaseService search)
		=> reload
			? cache.AddOrUpdateAtomic(file, load, (key, _) =>
			{
				search.RemoveContainerFromDatabase(key);
				return load(key);
			})
			: cache.GetOrAddAtomic(file, load);
}
