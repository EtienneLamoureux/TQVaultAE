namespace TQVaultAE.Application.Contracts.Services;

public interface IPathIO
{
	string Combine(params string[] paths);
	string GetDirectoryName(string path);
	string GetFileName(string path);
	string GetFileNameWithoutExtension(string path);
	string GetExtension(string path);
	string ChangeExtension(string path, string extension);
	char[] GetInvalidPathChars();
	char[] GetInvalidFileNameChars();
}