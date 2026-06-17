namespace TQVaultAE.Application.Contracts.Services;

public interface IFileDataService
{
	ReadOnlySpan<byte> GetReadOnlySpan(string filePath, int offset, int length);
	Memory<byte> GetMemory(string filePath, int offset, int length);
	T Read<T>(string filePath, int offset) where T : unmanaged;
	void ReleaseAll();
}
