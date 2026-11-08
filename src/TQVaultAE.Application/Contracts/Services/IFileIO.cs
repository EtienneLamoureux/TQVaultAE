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

	/// <summary>
	/// Whether <see cref="MoveToRecycleBin"/> can keep this file: on Windows, on a fixed local drive. Elsewhere (a network
	/// or removable drive) Windows would delete the file for good without telling.
	/// </summary>
	bool CanMoveToRecycleBin(string path);

	/// <summary>
	/// Moves a file to the Recycle Bin, where the user can restore it. Check <see cref="CanMoveToRecycleBin"/> first.
	/// </summary>
	void MoveToRecycleBin(string path);
	void Copy(string sourceFileName, string destFileName);
	void Copy(string sourceFileName, string destFileName, bool overwrite);
	void Move(string sourceFileName, string destFileName);
	string ReadAllText(string path, System.Text.Encoding encoding);
	void WriteAllText(string path, string contents, System.Text.Encoding encoding);
}