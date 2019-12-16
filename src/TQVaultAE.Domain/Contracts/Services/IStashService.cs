using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Domain.Contracts.Services
{
	public interface IStashService
	{
		/// <summary>
		/// Loads a player stash using the drop down list.
		/// </summary>
		/// <param name="selectedSave">Item from the drop down list.</param>
		/// <returns></returns>
		LoadPlayerStashResult LoadPlayerStash(PlayerSave selectedSave);
		/// <summary>
		/// Loads the relic vault stash
		/// </summary>
		LoadRelicVaultStashResult LoadRelicVaultStash();
		/// <summary>
		/// Loads the transfer stash for immortal throne
		/// </summary>
		LoadTransferStashResult LoadTransferStash();
		/// <summary>
		/// Attempts to save all modified stash files.
		/// </summary>
		/// <param name="stashOnError"></param>
		/// <exception cref="IOException">can happen during file save</exception>
		void SaveAllModifiedStashes(ref Stash stashOnError);
	}
}