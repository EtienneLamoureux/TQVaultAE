using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Domain.Contracts.Services
{
	public interface IStashService
	{

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