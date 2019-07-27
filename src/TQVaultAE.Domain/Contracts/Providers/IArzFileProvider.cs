using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Contracts.Providers
{
	public interface IArzFileProvider
	{
		/// <summary>
		/// Gets the DBRecord for a particular ID.
		/// </summary>
		/// <param name="recordId">string ID of the record will be normalized internally</param>
		/// <returns>DBRecord corresponding to the string ID.</returns>
		DBRecordCollection GetItem(ArzFile file, string recordId);
		/// <summary>
		/// Gets the list of keys from the recordInfo dictionary.
		/// </summary>
		/// <returns>string array holding the sorted list</returns>
		string[] GetKeyTable(ArzFile file);
		/// <summary>
		/// Gets a database record without adding it to the cache.
		/// </summary>
		/// <remarks>
		/// The Item property caches the DBRecords, which is great when you are only using a few 100 (1000?) records and are requesting
		/// them many times.  Not great if you are looping through all the records as it eats alot of memory.  This method will create
		/// the record on the fly if it is not in the cache so when you are done with it, it can be reclaimed by the garbage collector.
		/// Great for when you want to loop through all the records for some reason.  It will take longer, but use less memory.
		/// </remarks>
		/// <param name="recordId">String ID of the record.  Will be normalized internally.</param>
		/// <returns>Decompressed RecordInfo record</returns>
		DBRecordCollection GetRecordNotCached(ArzFile file, string recordId);
		/// <summary>
		/// Reads the ARZ file.
		/// </summary>
		/// <returns>true on success</returns>
		bool Read(ArzFile file);
	}
}