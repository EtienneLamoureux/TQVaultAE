namespace TQVaultAE.Application.Contracts.Services;

public interface IDecompressionService
{
	byte[] DecompressZlib(ReadOnlySpan<byte> data);
	byte[] DecompressZlib(Memory<byte> data);
}
