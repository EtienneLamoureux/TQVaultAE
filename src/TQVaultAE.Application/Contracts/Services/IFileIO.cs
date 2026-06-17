namespace TQVaultAE.Application.Contracts.Services;

public interface IFileIO
{
	byte[] ReadAllBytes(string path);
	void WriteAllBytes(string path, byte[] bytes);
	bool Exists(string path);
	string[] ReadAllLines(string path);
	void WriteAllLines(string path, IEnumerable<string> contents);
	string ReadAllText(string path);
	void WriteAllText(string path, string contents);
	void Delete(string path);
	void Copy(string sourceFileName, string destFileName);
	void Copy(string sourceFileName, string destFileName, bool overwrite);
	void Move(string sourceFileName, string destFileName);
	string ReadAllText(string path, System.Text.Encoding encoding);
	void WriteAllText(string path, string contents, System.Text.Encoding encoding);
}