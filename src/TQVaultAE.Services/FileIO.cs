using TQVaultAE.Application.Contracts.Services;

namespace TQVaultAE.Services;

public class FileIO : IFileIO
{
	public virtual byte[] ReadAllBytes(string path)
	{
		return File.ReadAllBytes(path);
	}

	public virtual void WriteAllBytes(string path, byte[] bytes)
	{
		File.WriteAllBytes(path, bytes);
	}

	public virtual bool Exists(string path)
	{
		return File.Exists(path);
	}

	public virtual string[] ReadAllLines(string path)
	{
		return File.ReadAllLines(path);
	}

	public virtual void WriteAllLines(string path, IEnumerable<string> contents)
	{
		File.WriteAllLines(path, contents);
	}

	public virtual string ReadAllText(string path)
	{
		return File.ReadAllText(path);
	}

	public virtual void WriteAllText(string path, string contents)
	{
		File.WriteAllText(path, contents);
	}

	public virtual void Delete(string path)
	{
		File.Delete(path);
	}

	public virtual void Copy(string sourceFileName, string destFileName)
	{
		File.Copy(sourceFileName, destFileName);
	}

	public virtual void Copy(string sourceFileName, string destFileName, bool overwrite)
	{
		File.Copy(sourceFileName, destFileName, overwrite);
	}

	public virtual void Move(string sourceFileName, string destFileName)
	{
		File.Move(sourceFileName, destFileName);
	}

	public virtual string ReadAllText(string path, System.Text.Encoding encoding)
	{
		return File.ReadAllText(path, encoding);
	}

	public virtual void WriteAllText(string path, string contents, System.Text.Encoding encoding)
	{
		File.WriteAllText(path, contents, encoding);
	}
}