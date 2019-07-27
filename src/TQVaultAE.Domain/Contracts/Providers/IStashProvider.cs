using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Contracts.Providers
{
	public interface IStashProvider
	{
		/// <summary>
		/// Converts the live data back into the raw binary data format
		/// </summary>
		/// <returns>byte array holding the raw data</returns>
		byte[] Encode(Stash sta);
		/// <summary>
		/// Loads a stash file
		/// </summary>
		/// <returns>false if the file does not exist otherwise true.</returns>
		bool LoadFile(Stash sta);
		/// <summary>
		/// Saves the stash file
		/// </summary>
		/// <param name="fileName">file name of this stash file</param>
		void Save(Stash sta, string fileName);
	}
}