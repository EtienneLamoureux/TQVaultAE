//-----------------------------------------------------------------------
// <copyright file="ArzDatabase.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Buffers;
using System.Buffers.Binary;


namespace TQVaultAE.GameFiles;

public static partial class ArzDatabaseFactory
{
	/// <summary>Opens and indexes an .arz (header, string table, record table; no record decompression).</summary>
	public static IArzDatabase Open(string path) => new ArzDatabase(path);
}

/// <summary>database.arz reader backed by a memory-mapped file.</summary>
internal sealed class ArzDatabase : IArzDatabase
{
	/// <summary>Header word 0 of TQ / TQAE database.arz.</summary>
	internal const int Magic = 0x00030004;

	/// <summary>Size of the header; record offsets are relative to its end.</summary>
	internal const int HeaderSize = 24;

	private const int SmallIntMin = -256;
	private const int SmallIntMax = 4096;

	private static readonly object BoxedTrue = true;
	private static readonly object BoxedFalse = false;
	private static readonly object[] SmallInts = BuildSmallInts();
	private static readonly object BoxedZeroFloat = 0f;
	private static readonly object BoxedOneFloat = 1f;

	private readonly MappedFile _file;
	private readonly string[] _strings;
	private readonly ArzRecordInfo[] _records;
	private readonly Dictionary<string, ArzRecordInfo> _byId;

	public ArzDatabase(string path)
	{
		this.Path = path;
		this._file = new MappedFile(path);
		try {
			if (this._file.Length < HeaderSize)
				throw new InvalidDataException($"{path}: file too small to be a database.arz.");
			var h = this._file.Span(0, HeaderSize);
			int magic = BinaryPrimitives.ReadInt32LittleEndian(h);
			if (magic != Magic)
				throw new InvalidDataException($"{path}: unexpected .arz header 0x{magic:X8} (expected 0x{Magic:X8}).");
			int recStart = BinaryPrimitives.ReadInt32LittleEndian(h[4..]);
			int recSize = BinaryPrimitives.ReadInt32LittleEndian(h[8..]);
			int recCount = BinaryPrimitives.ReadInt32LittleEndian(h[12..]);
			int strStart = BinaryPrimitives.ReadInt32LittleEndian(h[16..]);
			int strSize = BinaryPrimitives.ReadInt32LittleEndian(h[20..]);

			this._strings = ReadStringTable(this._file.Span(strStart, strSize));
			this._records = this.ReadRecordTable(this._file.Span(recStart, recSize), recCount);
			this._byId = new Dictionary<string, ArzRecordInfo>(this._records.Length, StringComparer.OrdinalIgnoreCase);
			foreach (var r in this._records)
				this._byId.TryAdd(NormalizeId(r.Id), r);
		} catch {
			this._file.Dispose();
			throw;
		}
	}

	public string Path { get; }

	public IReadOnlyList<ArzRecordInfo> Records => this._records;

	/// <summary>Decoded string table (shared instances: one string per id).</summary>
	internal IReadOnlyList<string> Strings => this._strings;

	public ArzRecordInfo? Find(string id)
	{
		ArgumentNullException.ThrowIfNull(id);
		return this._byId.TryGetValue(NormalizeId(id.Trim()), out var r) ? r : null;
	}

	public ArzRecord Read(ArzRecordInfo info)
	{
		ArgumentNullException.ThrowIfNull(info);
		byte[] buffer = ArrayPool<byte>.Shared.Rent(Math.Max(4096, info.CompressedSize * 6));
		try {
			int length = this.InflateRecord(info, ref buffer);
			return new ArzRecord(info, this.Decode(info, buffer.AsSpan(0, length)));
		} finally {
			ArrayPool<byte>.Shared.Return(buffer);
		}
	}

	public IEnumerable<ArzRecord> ReadMany(IEnumerable<ArzRecordInfo> infos, Func<ArzRecord, bool>? predicate = null, bool ordered = false, int maxDegreeOfParallelism = -1, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(infos);
		int dop = maxDegreeOfParallelism <= 0 ? Environment.ProcessorCount : maxDegreeOfParallelism;
		dop = Math.Clamp(dop, 1, 512);

		var query = infos.AsParallel();
		if (ordered)
			query = query.AsOrdered();
		var records = query
			.WithDegreeOfParallelism(dop)
			.WithCancellation(cancellationToken)
			.WithMergeOptions(ordered ? ParallelMergeOptions.Default : ParallelMergeOptions.NotBuffered)
			.Select(this.Read);
		return predicate is null ? records : records.Where(predicate);
	}

	public void Dispose() => this._file.Dispose();

	internal static string NormalizeId(string id) => id.Replace('/', '\\');

	private int InflateRecord(ArzRecordInfo info, ref byte[] buffer)
		=> this._file.Inflate(info.Offset, info.CompressedSize, ref buffer);

	private ArzVariable[] Decode(ArzRecordInfo info, ReadOnlySpan<byte> data)
	{
		// Count variables first so the result array is allocated exactly once.
		int count = 0;
		for (int p = 0; p + 8 <= data.Length;) {
			int n = BinaryPrimitives.ReadUInt16LittleEndian(data[(p + 2)..]);
			p += 8 + (n * 4);
			count++;
		}

		var vars = new ArzVariable[count];
		int pos = 0;
		for (int v = 0; v < count; v++) {
			var type = (ArzValueType)BinaryPrimitives.ReadInt16LittleEndian(data[pos..]);
			int n = BinaryPrimitives.ReadUInt16LittleEndian(data[(pos + 2)..]);
			int nameId = BinaryPrimitives.ReadInt32LittleEndian(data[(pos + 4)..]);
			pos += 8;
			if (pos + (n * 4) > data.Length)
				throw new InvalidDataException($"{info.Id}: variable {v} overruns the record ({data.Length} bytes).");

			var values = n == 0 ? [] : new object[n];
			var raw = data.Slice(pos, n * 4);
			for (int i = 0; i < n; i++) {
				int x = BinaryPrimitives.ReadInt32LittleEndian(raw[(i * 4)..]);
				values[i] = type switch {
					ArzValueType.Int => BoxInt(x),
					ArzValueType.Float => BoxFloat(BitConverter.Int32BitsToSingle(x)),
					ArzValueType.String => this.GetString(info, x),
					ArzValueType.Bool => x != 0 ? BoxedTrue : BoxedFalse,
					_ => throw new InvalidDataException($"{info.Id}: unknown value type {(short)type}."),
				};
			}
			pos += n * 4;
			vars[v] = new ArzVariable(this.GetString(info, nameId), type, values);
		}
		return vars;
	}

	private string GetString(ArzRecordInfo info, int id)
	{
		if ((uint)id >= (uint)this._strings.Length)
			throw new InvalidDataException($"{info.Id}: string id {id} out of range ({this._strings.Length}).");
		return this._strings[id];
	}

	private static object BoxInt(int x) => x is >= SmallIntMin and <= SmallIntMax ? SmallInts[x - SmallIntMin] : x;

	private static object BoxFloat(float f) => f == 0f && !float.IsNegative(f) ? BoxedZeroFloat : f == 1f ? BoxedOneFloat : f;

	private static object[] BuildSmallInts()
	{
		var a = new object[SmallIntMax - SmallIntMin + 1];
		for (int i = 0; i < a.Length; i++)
			a[i] = i + SmallIntMin;
		return a;
	}

	internal static string[] ReadStringTable(ReadOnlySpan<byte> table)
	{
		int count = BinaryPrimitives.ReadInt32LittleEndian(table);
		var strings = new string[count];
		int pos = 4;
		for (int i = 0; i < count; i++) {
			int len = BinaryPrimitives.ReadInt32LittleEndian(table[pos..]);
			pos += 4;
			strings[i] = TextEncodings.Cp1252.GetString(table.Slice(pos, len));
			pos += len;
		}
		return strings;
	}

	private ArzRecordInfo[] ReadRecordTable(ReadOnlySpan<byte> table, int count)
	{
		var records = new ArzRecordInfo[count];
		var classes = new Dictionary<string, string>(StringComparer.Ordinal);
		int pos = 0;
		for (int i = 0; i < count; i++) {
			int nameId = BinaryPrimitives.ReadInt32LittleEndian(table[pos..]);
			int classLen = BinaryPrimitives.ReadInt32LittleEndian(table[(pos + 4)..]);
			pos += 8;
			string cls = TextEncodings.Cp1252.GetString(table.Slice(pos, classLen));
			if (classes.TryGetValue(cls, out var shared))
				cls = shared;
			else
				classes.Add(cls, cls);
			pos += classLen;
			int offset = BinaryPrimitives.ReadInt32LittleEndian(table[pos..]);
			int csize = BinaryPrimitives.ReadInt32LittleEndian(table[(pos + 4)..]);
			long ft = BinaryPrimitives.ReadInt64LittleEndian(table[(pos + 8)..]);
			pos += 16;
			if ((uint)nameId >= (uint)this._strings.Length)
				throw new InvalidDataException($"{this.Path}: record {i} name id {nameId} out of range.");
			records[i] = new ArzRecordInfo(i, this._strings[nameId], cls, (long)offset + HeaderSize, csize, FileTime.ToUtc(ft));
		}
		return records;
	}
}
