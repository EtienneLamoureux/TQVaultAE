//-----------------------------------------------------------------------
// <copyright file="ArcArchive.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace TQVaultAE.GameFiles;

public static partial class ArcArchiveFactory
{
	/// <summary>Opens an .arc and reads its table of contents.</summary>
	public static IArcArchive Open(string path) => new ArcArchive(path);
}

/// <summary>.arc reader backed by a memory-mapped file.</summary>
internal sealed class ArcArchive : IArcArchive
{
	/// <summary>'ARC\0'.</summary>
	internal const uint Magic = 0x00435241;

	internal const int HeaderSize = 28;
	internal const int FileEntrySize = 44;
	internal const int PartEntrySize = 12;

	/// <summary>Storage type of raw entries.</summary>
	internal const int Stored = 1;

	/// <summary>Storage type of zlib-part entries.</summary>
	internal const int Compressed = 3;

	private readonly MappedFile _file;
	private readonly Part[] _parts;
	private readonly ArcEntry[] _entries;
	private readonly Dictionary<string, ArcEntry> _byName;
	private readonly ConditionalWeakTable<ArcEntry, Layout> _layouts = [];

	public ArcArchive(string path)
	{
		this.Path = path;
		this.ResourcePrefix = ArchiveLayout.ResourcePrefix(path);
		this._file = new MappedFile(path);
		try {
			if (this._file.Length < HeaderSize)
				throw new InvalidDataException($"{path}: file too small to be an .arc.");
			var h = this._file.Span(0, HeaderSize);
			if (BinaryPrimitives.ReadUInt32LittleEndian(h) != Magic)
				throw new InvalidDataException($"{path}: not an ARC archive.");
			int numFiles = BinaryPrimitives.ReadInt32LittleEndian(h[8..]);
			int numParts = BinaryPrimitives.ReadInt32LittleEndian(h[12..]);
			int namesBytes = BinaryPrimitives.ReadInt32LittleEndian(h[20..]);
			long tocOffset = BinaryPrimitives.ReadUInt32LittleEndian(h[24..]);

			this._parts = new Part[numParts];
			var pt = this._file.Span(tocOffset, checked(numParts * PartEntrySize));
			for (int i = 0; i < numParts; i++) {
				var p = pt[(i * PartEntrySize)..];
				this._parts[i] = new Part(
					BinaryPrimitives.ReadUInt32LittleEndian(p),
					BinaryPrimitives.ReadInt32LittleEndian(p[4..]),
					BinaryPrimitives.ReadInt32LittleEndian(p[8..]));
			}

			var names = this._file.Span(tocOffset + ((long)numParts * PartEntrySize), namesBytes);
			long fileTable = this._file.Length - ((long)numFiles * FileEntrySize);
			var ft = this._file.Span(fileTable, checked(numFiles * FileEntrySize));

			var entries = new List<ArcEntry>(numFiles);
			this._byName = new Dictionary<string, ArcEntry>(numFiles, StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < numFiles; i++) {
				var e = ft[(i * FileEntrySize)..];
				int type = BinaryPrimitives.ReadInt32LittleEndian(e);
				uint offset = BinaryPrimitives.ReadUInt32LittleEndian(e[4..]);
				uint csize = BinaryPrimitives.ReadUInt32LittleEndian(e[8..]);
				uint usize = BinaryPrimitives.ReadUInt32LittleEndian(e[12..]);
				uint adler = BinaryPrimitives.ReadUInt32LittleEndian(e[16..]);
				long fileTime = BinaryPrimitives.ReadInt64LittleEndian(e[20..]);
				int nParts = BinaryPrimitives.ReadInt32LittleEndian(e[28..]);
				int firstPart = BinaryPrimitives.ReadInt32LittleEndian(e[32..]);
				int nameLen = BinaryPrimitives.ReadInt32LittleEndian(e[36..]);
				int nameOffset = BinaryPrimitives.ReadInt32LittleEndian(e[40..]);

				// Inactive (deleted) entries keep a slot in the table but own no parts.
				if (type == Compressed && nParts == 0)
					continue;
				if (type != Stored && type != Compressed)
					continue;
				if (type == Compressed && (firstPart < 0 || nParts < 0 || firstPart > numParts - nParts))
					throw new InvalidDataException($"{path}: entry {i} references parts {firstPart}+{nParts} of {numParts}.");
				if (nameOffset < 0 || nameLen < 0 || nameOffset > names.Length - nameLen)
					throw new InvalidDataException($"{path}: entry {i} name range {nameOffset}+{nameLen} is outside the names blob.");

				var nameBytes = names.Slice(nameOffset, nameLen);
				int nul = nameBytes.IndexOf((byte)0);
				if (nul >= 0)
					nameBytes = nameBytes[..nul];
				string name = TextEncodings.Cp1252.GetString(nameBytes);

				var entry = new ArcEntry(this, name, type, usize, csize, adler, FileTime.ToUtc(fileTime));
				this._layouts.Add(entry, new Layout(offset, firstPart, nParts));
				entries.Add(entry);
				this._byName.TryAdd(NormalizeName(name), entry);
			}
			this._entries = [.. entries];
		} catch {
			this._file.Dispose();
			throw;
		}
	}

	public string Path { get; }

	public string ResourcePrefix { get; }

	public IReadOnlyList<ArcEntry> Entries => this._entries;

	public ArcEntry? Find(string name)
	{
		ArgumentNullException.ThrowIfNull(name);
		return this._byName.TryGetValue(NormalizeName(name.Trim()), out var e) ? e : null;
	}

	public void CopyTo(ArcEntry entry, Stream destination, bool verify = false)
	{
		ArgumentNullException.ThrowIfNull(destination);
		var adler = new Adler32();
		long written = 0;
		this.ForEachChunk(entry, chunk => {
			if (verify)
				adler.Append(chunk);
			destination.Write(chunk);
			written += chunk.Length;
		});
		this.CheckResult(entry, written, verify ? adler.Value : null);
	}

	public byte[] ReadAll(ArcEntry entry)
	{
		var layout = this.LayoutOf(entry);
		if (entry.Size > Array.MaxLength)
			throw new InvalidOperationException($"{entry.Name} is too large ({entry.Size} bytes) to read into memory; use CopyTo.");
		var result = new byte[entry.Size];
		if (entry.StorageType == Stored) {
			this._file.Span(layout.Offset, (int)entry.Size).CopyTo(result);
			return result;
		}

		int pos = 0;
		for (int i = 0; i < layout.PartCount; i++) {
			var part = this._parts[layout.FirstPart + i];
			if (part.Size > result.Length - pos)
				throw new InvalidDataException($"{entry.Name}: parts exceed the declared size {entry.Size}.");
			int n = this._file.Inflate(part.Offset, part.CompressedSize, result.AsSpan(pos, part.Size));
			if (n != part.Size)
				throw new InvalidDataException($"{entry.Name}: part {i} inflated to {n} bytes, expected {part.Size}.");
			pos += n;
		}
		this.CheckResult(entry, pos, null);
		return result;
	}

	/// <summary>Verifies the Adler-32 of an entry without keeping its content.</summary>
	internal bool Verify(ArcEntry entry)
	{
		var adler = new Adler32();
		this.ForEachChunk(entry, chunk => adler.Append(chunk));
		return adler.Value == entry.Adler32;
	}

	public void Dispose() => this._file.Dispose();

	internal static string NormalizeName(string name) => name.Replace('\\', '/');

	private delegate void ChunkAction(ReadOnlySpan<byte> chunk);

	private void ForEachChunk(ArcEntry entry, ChunkAction action)
	{
		var layout = this.LayoutOf(entry);
		if (entry.StorageType == Stored) {
			const int Chunk = 1 << 20;
			for (long done = 0; done < entry.Size;) {
				int n = (int)Math.Min(Chunk, entry.Size - done);
				action(this._file.Span(layout.Offset + done, n));
				done += n;
			}
			return;
		}

		int max = 0;
		for (int i = 0; i < layout.PartCount; i++)
			max = Math.Max(max, this._parts[layout.FirstPart + i].Size);
		byte[] buffer = ArrayPool<byte>.Shared.Rent(Math.Max(max, 1));
		try {
			for (int i = 0; i < layout.PartCount; i++) {
				// Type 3 parts are always zlib, even when csize == usize.
				var part = this._parts[layout.FirstPart + i];
				int n = this._file.Inflate(part.Offset, part.CompressedSize, buffer.AsSpan(0, part.Size));
				if (n != part.Size)
					throw new InvalidDataException($"{entry.Name}: part {i} inflated to {n} bytes, expected {part.Size}.");
				action(buffer.AsSpan(0, n));
			}
		} finally {
			ArrayPool<byte>.Shared.Return(buffer);
		}
	}

	private void CheckResult(ArcEntry entry, long size, uint? adler)
	{
		if (size != entry.Size)
			throw new InvalidDataException($"{entry.Name}: produced {size} bytes, expected {entry.Size}.");
		if (adler is { } a && a != entry.Adler32)
			throw new InvalidDataException($"{entry.Name}: Adler-32 mismatch (computed 0x{a:X8}, stored 0x{entry.Adler32:X8}).");
	}

	private Layout LayoutOf(ArcEntry entry)
	{
		ArgumentNullException.ThrowIfNull(entry);
		if (ReferenceEquals(entry.Archive, this)) {
			if (this._layouts.TryGetValue(entry, out var layout))
				return layout;

			// A copy of one of our entries (e.g. made with 'with'): match it by name.
			if (this._byName.TryGetValue(NormalizeName(entry.Name), out var own) && own == entry && this._layouts.TryGetValue(own, out layout))
				return layout;
		}
		throw new ArgumentException($"{entry.Name} does not belong to {this.Path}.", nameof(entry));
	}

	private readonly record struct Part(uint Offset, int CompressedSize, int Size);

	private sealed record Layout(long Offset, int FirstPart, int PartCount);
}
