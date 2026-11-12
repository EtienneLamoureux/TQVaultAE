//-----------------------------------------------------------------------
// <copyright file="Contracts.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// Public contract of the game file readers, shared by tqdb (TQVaultAE.DbCli) and TQVaultAE.Data.
// Change it only in agreement with both sides.

namespace TQVaultAE.GameFiles;

/// <summary>Database variable value type, as stored in database.arz.</summary>
public enum ArzValueType : short
{
	Int = 0,
	Float = 1,
	String = 2,
	Bool = 3,
}

/// <summary>One variable of a decoded record. Values are int, float, string or bool according to <see cref="Type"/>.</summary>
/// <param name="Name">Variable name, original casing.</param>
/// <param name="Type">Value type.</param>
/// <param name="Values">Decoded values (boxed only at this boundary; readers decode without boxing).</param>
public sealed record ArzVariable(string Name, ArzValueType Type, object[] Values);

/// <summary>Record table entry: available without decompressing the record.</summary>
/// <param name="Index">Position in the record table.</param>
/// <param name="Id">Record path as stored, e.g. <c>records\item\equipmentweapon\sword\c10_sword01.dbr</c>.</param>
/// <param name="Class">Record class (equals the record's <c>Class</c> variable, may be empty).</param>
/// <param name="Offset">Absolute file offset of the zlib stream.</param>
/// <param name="CompressedSize">Compressed size in bytes.</param>
/// <param name="LastWrite">Record FILETIME, UTC.</param>
public sealed record ArzRecordInfo(int Index, string Id, string Class, long Offset, int CompressedSize, DateTime LastWrite);

/// <summary>A decoded record.</summary>
/// <param name="Info">Table entry.</param>
/// <param name="Variables">Variables in file order. Lookups by name are case-insensitive via <see cref="Get"/>.</param>
public sealed record ArzRecord(ArzRecordInfo Info, IReadOnlyList<ArzVariable> Variables)
{
	/// <summary>Case-insensitive variable lookup, <see langword="null"/> when absent.</summary>
	public ArzVariable? Get(string name)
	{
		foreach (var v in this.Variables)
			if (string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase))
				return v;
		return null;
	}
}

/// <summary>Read access to a database.arz. Thread-safe; backed by a memory-mapped file.</summary>
public interface IArzDatabase : IDisposable
{
	/// <summary>Path of the .arz file.</summary>
	string Path { get; }

	/// <summary>All record table entries, in table order (no decompression).</summary>
	IReadOnlyList<ArzRecordInfo> Records { get; }

	/// <summary>Case-insensitive lookup by record id; accepts '/' or '\' separators.</summary>
	ArzRecordInfo? Find(string id);

	/// <summary>Decompresses and decodes one record.</summary>
	ArzRecord Read(ArzRecordInfo info);

	/// <summary>
	/// Decodes many records in parallel, preserving input order in the output sequence only when
	/// <paramref name="ordered"/> is true. <paramref name="predicate"/> runs on the worker thread.
	/// </summary>
	IEnumerable<ArzRecord> ReadMany(IEnumerable<ArzRecordInfo> infos, Func<ArzRecord, bool>? predicate = null, bool ordered = false, int maxDegreeOfParallelism = -1, CancellationToken cancellationToken = default);
}

/// <summary>ARC file table entry.</summary>
/// <param name="Archive">Owning archive.</param>
/// <param name="Name">Entry path as stored (lowercase, '/' separated, cp1252 decoded).</param>
/// <param name="StorageType">1 = stored, 3 = zlib parts.</param>
/// <param name="Size">Uncompressed size.</param>
/// <param name="CompressedSize">Compressed size.</param>
/// <param name="Adler32">Adler-32 of the uncompressed bytes as stored in the table.</param>
/// <param name="LastWrite">FILETIME, UTC.</param>
public sealed record ArcEntry(IArcArchive Archive, string Name, int StorageType, long Size, long CompressedSize, uint Adler32, DateTime LastWrite)
{
	/// <summary>
	/// Resource id as the database references it: <c>&lt;archive prefix&gt;\&lt;name with '\'&gt;</c>, e.g. <c>Items\weapons\...\x.tex</c>
	/// in <c>Resources\Items.arc</c>, <c>XPack2\Items\...\x.tex</c> in <c>Resources\XPack2\Items.arc</c>
	/// (<see cref="IArcArchive.ResourcePrefix"/>).
	/// </summary>
	public string ResourceId => $"{this.Archive.ResourcePrefix}\\{this.Name.Replace('/', '\\')}";
}

/// <summary>Read access to one .arc archive. Thread-safe.</summary>
public interface IArcArchive : IDisposable
{
	/// <summary>Path of the .arc file.</summary>
	string Path { get; }

	/// <summary>First segments of its entries' resource ids, from its place in the game install (<see cref="ArchiveLayout.ResourcePrefix"/>).</summary>
	string ResourcePrefix { get; }

	/// <summary>Active entries (zero-part inactive entries excluded).</summary>
	IReadOnlyList<ArcEntry> Entries { get; }

	/// <summary>Case-insensitive lookup; accepts '/' or '\'.</summary>
	ArcEntry? Find(string name);

	/// <summary>Streams the uncompressed bytes part by part (never allocates the whole file); verifies Adler-32 when <paramref name="verify"/>.</summary>
	void CopyTo(ArcEntry entry, Stream destination, bool verify = false);

	/// <summary>Reads a whole entry into memory (for small files: text, tex conversion).</summary>
	byte[] ReadAll(ArcEntry entry);
}

/// <summary>Localized text (Text_XX.arc), case-insensitive keys, last definition wins.</summary>
public interface ITextDatabase
{
	/// <summary>Language code, e.g. EN.</summary>
	string Language { get; }

	/// <summary>All tags.</summary>
	IReadOnlyDictionary<string, string> Entries { get; }

	/// <summary>Resolves a tag, <see langword="null"/> when unknown.</summary>
	string? Resolve(string? tag);
}
