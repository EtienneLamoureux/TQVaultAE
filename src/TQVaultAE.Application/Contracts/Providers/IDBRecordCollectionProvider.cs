using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Providers;

public interface IDBRecordCollectionProvider
{
	/// <summary>
	/// Writes all variables into a file.
	/// </summary>
	/// <param name="drc">source</param>
	/// <param name="baseFolder">Path in the file.</param>
	/// <param name="fileName">file name to be written</param>
	void Write(DBRecordCollection drc, string baseFolder, string fileName = null);
}