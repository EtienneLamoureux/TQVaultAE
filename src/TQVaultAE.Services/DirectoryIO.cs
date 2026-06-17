using TQVaultAE.Application.Contracts.Services;
namespace TQVaultAE.Services;

public class DirectoryIO : IDirectoryIO
{
	public virtual bool Exists(string path)
	{
		return System.IO.Directory.Exists(path);
	}

	public virtual void CreateDirectory(string path)
	{
		System.IO.Directory.CreateDirectory(path);
	}

	public virtual string[] GetFiles(string path)
	{
		return System.IO.Directory.GetFiles(path);
	}

	public virtual string[] GetFiles(string path, string searchPattern)
	{
		return System.IO.Directory.GetFiles(path, searchPattern);
	}

	public virtual string[] GetDirectories(string path)
	{
		return System.IO.Directory.GetDirectories(path);
	}

	public virtual string[] GetDirectories(string path, string searchPattern)
	{
		return System.IO.Directory.GetDirectories(path, searchPattern);
	}

	public virtual void Delete(string path)
	{
		System.IO.Directory.Delete(path);
	}

	public virtual void Delete(string path, bool recursive)
	{
		System.IO.Directory.Delete(path, recursive);
	}

	public virtual void Move(string sourceDirName, string destDirName)
	{
		System.IO.Directory.Move(sourceDirName, destDirName);
	}
}