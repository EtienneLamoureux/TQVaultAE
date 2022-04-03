using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;
using System.IO;

namespace TQVaultAE.Domain.Contracts.Services
{
	public interface IStashService
	{
		/// <summary>
		/// Loads a player stash using the drop down list.
		/// </summary>
		/// <param name="selectedSave">Item from the drop down list.</param>
		/// <param name="fromFileWatcher">When <code>true</code> called from <see cref="FileSystemWatcher.Changed"/></param>
		/// <returns></returns>
		LoadPlayerStashResult LoadPlayerStash(PlayerSave selectedSave, bool fromFileWatcher = false);
		/// <summary>
		/// Loads the relic vault stash
		/// </summary>
		/// <param name="fromFileWatcher">When <code>true</code> called from <see cref="FileSystemWatcher.Changed"/></param>
		/// <returns></returns>
		LoadRelicVaultStashResult LoadRelicVaultStash(bool fromFileWatcher = false);
		/// <summary>
		/// Loads the transfer stash for immortal throne
		/// </summary>
		/// <param name="fromFileWatcher">When <code>true</code> called from <see cref="FileSystemWatcher.Changed"/></param>
		/// <returns></returns>
		LoadTransferStashResult LoadTransferStash(bool fromFileWatcher = false);
		/// <summary>
		/// Attempts to save all modified stash files.
		/// </summary>
		/// <param name="stashOnError"></param>
		/// <exception cref="IOException">can happen during file save</exception>
		/// <returns>Number of stash saved</returns>
		int SaveAllModifiedStashes(ref Stash stashOnError);
	}
}