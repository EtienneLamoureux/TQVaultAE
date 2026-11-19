//-----------------------------------------------------------------------
// <copyright file="MappedFile.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Buffers;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;

namespace TQVaultAE.GameFiles;

/// <summary>Read-only memory-mapped view of a whole file. Reads are thread-safe.</summary>
internal sealed unsafe class MappedFile : IDisposable
{
	private readonly MemoryMappedFile? _map;
	private readonly MemoryMappedViewAccessor? _view;
	private readonly byte* _base;
	private int _disposed;

	public MappedFile(string path)
	{
		this.Path = path;
		using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
		this.Length = fs.Length;
		if (this.Length == 0)
			return;

		this._map = MemoryMappedFile.CreateFromFile(fs, null, 0, MemoryMappedFileAccess.Read, HandleInheritability.None, leaveOpen: false);
		this._view = this._map.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
		byte* p = null;
		this._view.SafeMemoryMappedViewHandle.AcquirePointer(ref p);
		this._base = p + this._view.PointerOffset;
	}

	public string Path { get; }

	public long Length { get; }

	/// <summary>Span over [offset, offset+length), bounds-checked against the file.</summary>
	public ReadOnlySpan<byte> Span(long offset, int length)
	{
		this.Check(offset, length);
		return length == 0 ? [] : new ReadOnlySpan<byte>(this._base + offset, length);
	}

	/// <summary>Stream over [offset, offset+length) with no copy.</summary>
	public UnmanagedMemoryStream Stream(long offset, long length)
	{
		this.Check(offset, length);
		return new UnmanagedMemoryStream(this._base + offset, length, length, FileAccess.Read);
	}

	/// <summary>Inflates the zlib stream at [offset, offset+compressedSize) into <paramref name="destination"/>; returns the bytes written.</summary>
	public int Inflate(long offset, int compressedSize, Span<byte> destination)
	{
		using var src = this.Stream(offset, compressedSize);
		using var z = new ZLibStream(src, CompressionMode.Decompress);
		int total = 0;
		while (total < destination.Length)
		{
			int n = z.Read(destination[total..]);
			if (n == 0)
				break;
			total += n;
		}
		return total;
	}

	/// <summary>
	/// Inflates the zlib stream at [offset, offset+compressedSize) when its size is unknown: <paramref name="pooledBuffer"/>,
	/// rented from <see cref="ArrayPool{T}.Shared"/>, is replaced by a bigger rented one as needed; returns the bytes written.
	/// </summary>
	public int Inflate(long offset, int compressedSize, ref byte[] pooledBuffer)
	{
		using var src = this.Stream(offset, compressedSize);
		using var z = new ZLibStream(src, CompressionMode.Decompress);
		int total = 0;
		while (true)
		{
			if (total == pooledBuffer.Length)
			{
				var bigger = ArrayPool<byte>.Shared.Rent(pooledBuffer.Length * 2);
				pooledBuffer.AsSpan(0, total).CopyTo(bigger);
				ArrayPool<byte>.Shared.Return(pooledBuffer);
				pooledBuffer = bigger;
			}
			int n = z.Read(pooledBuffer, total, pooledBuffer.Length - total);
			if (n == 0)
				return total;
			total += n;
		}
	}

	private void Check(long offset, long length)
	{
		if (offset < 0 || length < 0 || offset > this.Length - length)
			throw new InvalidDataException($"{this.Path}: range {offset}+{length} is outside the file ({this.Length} bytes).");
		ObjectDisposedException.ThrowIf(this._disposed != 0, this);
	}

	public void Dispose()
	{
		if (Interlocked.Exchange(ref this._disposed, 1) != 0)
			return;
		if (this._view is not null)
		{
			this._view.SafeMemoryMappedViewHandle.ReleasePointer();
			this._view.Dispose();
		}
		this._map?.Dispose();
	}
}
