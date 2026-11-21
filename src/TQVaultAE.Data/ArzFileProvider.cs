using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GameFiles;

namespace TQVaultAE.Data;

/// <summary>
/// Reads database.arz files through the shared <see cref="IArzDatabase"/> reader and maps records to <see cref="DBRecordCollection"/>.
/// </summary>
public class ArzFileProvider(ILogger<ArzFileProvider> log) : IArzFileProvider
{
	private readonly ILogger Log = log;

	/// <summary>
	/// Opens the ARZ file and loads its record table (records are decoded on demand).
	/// </summary>
	/// <returns>true on success</returns>
	public bool Read(ArzFile file)
	{
		try
		{
			var database = ArzDatabaseFactory.Open(file.FileName);
			file.Database?.Dispose();
			file.Database = database;

			file.RecordInfo = new Dictionary<RecordId, ArzRecordInfo>(database.Records.Count);
			foreach (var entry in database.Records)
				file.RecordInfo.TryAdd(entry.Id, entry);
		}
		catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException)
		{
			this.Log.LogError(exception, "Failed to read the database {ArzFile}", file.FileName);
			return false;
		}

		return true;
	}

	/// <summary>
	/// Gets the DBRecord for a particular ID.
	/// </summary>
	/// <param name="recordId">string ID of the record will be normalized internally</param>
	/// <returns>DBRecord corresponding to the string ID, <c>null</c> when not found.</returns>
	public DBRecordCollection? GetItem(ArzFile file, RecordId recordId)
	{
		if (recordId is null) return null;

		if (!file.RecordInfo.TryGetValue(recordId, out var info))
			return null;

		return this.Decode(file, info);
	}

	/// <summary>
	/// Gets a database record without adding it to the cache.
	/// </summary>
	/// <param name="recordId">String ID of the record.  Will be normalized internally.</param>
	/// <returns>Decompressed RecordInfo record</returns>
	public DBRecordCollection GetRecordNotCached(ArzFile file, RecordId recordId)
		=> this.Decode(file, file.RecordInfo[recordId]);

	/// <summary>
	/// Decompresses one record and converts it to the TQVault model.
	/// </summary>
	/// <remarks>
	/// Values keep the historical TQVault representation: booleans as Int32, strings trimmed.
	/// </remarks>
	/// <exception cref="ArgumentException">The record cannot be decoded.</exception>
	private DBRecordCollection Decode(ArzFile file, ArzRecordInfo info)
	{
		if (file.Database is null)
			throw new InvalidOperationException($"{file.FileName} has not been read.");

		ArzRecord record;
		try
		{
			record = file.Database.Read(info);
		}
		catch (InvalidDataException ex)
		{
			var error = new ArgumentException($"Error while parsing arz record {info.Id} of {file.FileName}", ex);
			this.Log.LogError(ex, "Failed to parse record {Record} of {ArzFile}", info.Id, file.FileName);
			throw error;
		}

		var result = new DBRecordCollection(info.Id, info.Class, record.Variables.Count) { Source = file.Source };
		foreach (var source in record.Variables)
		{
			if (source.Values.Length == 0)
				continue;

			var variable = new Variable(source.Name, (VariableDataType)source.Type, source.Values.Length);
			for (int i = 0; i < source.Values.Length; i++)
			{
				variable[i] = source.Values[i] switch
				{
					bool b => b ? 1 : 0,
					string s => s.Trim(),
					var value => value,
				};
			}

			result.Set(variable);
		}

		return result;
	}
}
