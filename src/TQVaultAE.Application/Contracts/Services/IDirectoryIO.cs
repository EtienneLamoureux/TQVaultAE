namespace TQVaultAE.Application.Contracts.Services;

public interface IDirectoryIO
{
	bool Exists(string path);
	void CreateDirectory(string path);
	string[] GetFiles(string path);
	string[] GetFiles(string path, string searchPattern);
	string[] GetDirectories(string path);
	string[] GetDirectories(string path, string searchPattern);
	void Delete(string path);
	void Delete(string path, bool recursive);
	void Move(string sourceDirName, string destDirName);
}