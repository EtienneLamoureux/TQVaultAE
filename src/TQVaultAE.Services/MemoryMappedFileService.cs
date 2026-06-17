using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Services;

public class MemoryMappedFileService : IFileDataService
{
	private readonly ILogger _logger;
	private readonly LazyConcurrentDictionary<string, MemoryMappedFile> _mmfCache = new();
	private readonly LazyConcurrentDictionary<string, long> _fileSizeCache = new();

	public MemoryMappedFileService(ILogger<MemoryMappedFileService> logger)
	{
		this._logger = logger;
	}

	public ReadOnlySpan<byte> GetReadOnlySpan(string filePath, int offset, int length)
	{
		var mmf = GetOrCreateMmf(filePath);
		if (mmf == null)
			return ReadOnlySpan<byte>.Empty;

		try
		{
			using var accessor = mmf.CreateViewAccessor(offset, length, MemoryMappedFileAccess.Read);
			if (accessor.SafeMemoryMappedViewHandle.IsInvalid)
				return ReadOnlySpan<byte>.Empty;

			var span = new byte[length];
			accessor.ReadArray(0, span, 0, length);

			return span;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error reading memory-mapped file at offset {Offset}, length {Length}", offset, length);
			return ReadOnlySpan<byte>.Empty;
		}
	}

	public Memory<byte> GetMemory(string filePath, int offset, int length)
	{
		var mmf = GetOrCreateMmf(filePath);
		if (mmf == null)
			return Memory<byte>.Empty;

		try
		{
			using var accessor = mmf.CreateViewAccessor(offset, length, MemoryMappedFileAccess.Read);
			var memory = new byte[length];
			accessor.ReadArray(0, memory, 0, length);
			return memory;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error reading memory-mapped file at offset {Offset}, length {Length}", offset, length);
			return Memory<byte>.Empty;
		}
	}

	public T Read<T>(string filePath, int offset) where T : unmanaged
	{
		var mmf = GetOrCreateMmf(filePath);
		if (mmf == null)
			return default;

		try
		{
			var size = Marshal.SizeOf<T>();
			using var accessor = mmf.CreateViewAccessor(offset, size, MemoryMappedFileAccess.Read);
			T result = default;
			accessor.Read(0, out result);
			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error reading type {Type} at offset {Offset}", typeof(T).Name, offset);
			return default;
		}
	}

	public void ReleaseAll()
	{
		foreach (var kvp in _mmfCache)
		{
			try
			{
				kvp.Value.Value.Dispose();
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Error disposing MMF for {FilePath}", kvp.Key);
			}
		}
		_mmfCache.Clear();
		_fileSizeCache.Clear();
	}

	public void Release(string filePath)
	{
		if (_mmfCache.TryRemove(filePath, out var mmf))
		{
			_fileSizeCache.TryRemove(filePath, out _);
			try
			{
				mmf.Dispose();
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Error disposing MMF for {FilePath}", filePath);
			}
		}
	}

	private MemoryMappedFile GetOrCreateMmf(string filePath)
	{
		return _mmfCache.GetOrAddAtomic(filePath, path =>
		{
			try
			{
				_fileSizeCache.GetOrAddAtomic(path, s =>
				{
					var fileInfo = new FileInfo(path);
					return fileInfo.Length;
				});
				return MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating memory-mapped file for {FilePath}", path);
				return null;
			}
		});
	}
}
