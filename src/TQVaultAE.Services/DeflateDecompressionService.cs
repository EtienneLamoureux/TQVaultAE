using System.Buffers;
using System.IO.Compression;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Services;

namespace TQVaultAE.Services;

public class DeflateDecompressionService : IDecompressionService
{
	private readonly ILogger _logger;

	/// <summary>
	/// Default buffer size for decompression output.
	/// Uses an initial size that accommodates most compressed data.
	/// </summary>
	private const int DefaultOutputSize = 8192;

	public DeflateDecompressionService(ILogger<DeflateDecompressionService> logger)
	{
		this._logger = logger;
	}

	public byte[] DecompressZlib(ReadOnlySpan<byte> data)
	{
		if (data.Length < 2)
			return Array.Empty<byte>();

		// Check for zlib header (CMF + FLG bytes)
		// 0x78 0x01, 0x78 0x9C, or 0x78 0xDA are valid zlib headers
		var skipZlibHeader = data.Length > 6 && data[0] == 0x78 && (data[1] == 0x01 || data[1] == 0x9C || data[1] == 0xDA);
		var compressedData = skipZlibHeader ? data.Slice(2) : data;

		try
		{
			// Use pooled buffer for output to reduce GC pressure
			var outputBuffer = ArrayPool<byte>.Shared.Rent(DefaultOutputSize);
			var totalWritten = 0;
			var bufferLength = outputBuffer.Length;

			try
			{
				// Read compressed data into a stream for DeflateStream
				using var compressedStream = new MemoryStream(compressedData.ToArray());

				// Use DeflateStream for decompression
				// DeflateStream expects raw deflate format (zlib header should be stripped)
				using (var deflate = new DeflateStream(compressedStream, CompressionMode.Decompress))
				{
					int bytesRead;
					while ((bytesRead = deflate.Read(outputBuffer, totalWritten, bufferLength - totalWritten)) > 0)
					{
						totalWritten += bytesRead;

						// Expand buffer if needed
						if (totalWritten >= bufferLength)
						{
							var newBuffer = ArrayPool<byte>.Shared.Rent(bufferLength * 2);
							Array.Copy(outputBuffer, newBuffer, bufferLength);
							ArrayPool<byte>.Shared.Return(outputBuffer, clearArray: true);
							outputBuffer = newBuffer;
							bufferLength = newBuffer.Length;
						}
					}
				}

				// Create exact-sized result array
				var result = new byte[totalWritten];
				Array.Copy(outputBuffer, result, totalWritten);
				return result;
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(outputBuffer, clearArray: true);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error decompressing zlib data");
			return Array.Empty<byte>();
		}
	}

	public byte[] DecompressZlib(Memory<byte> data)
	{
		return DecompressZlib(data.Span);
	}
}
