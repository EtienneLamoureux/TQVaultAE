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

	public virtual bool IsReadOnly(string path)
		=> File.GetAttributes(path).HasFlag(FileAttributes.ReadOnly);

	public virtual void SetReadOnly(string path, bool makeReadOnly)
	{
		var attributes = File.GetAttributes(path);
		var wanted = makeReadOnly ? attributes | FileAttributes.ReadOnly : attributes & ~FileAttributes.ReadOnly;
		if (wanted != attributes)
			File.SetAttributes(path, wanted);
	}

	public virtual bool CanMoveToRecycleBin(string path)
	{
		if (!OperatingSystem.IsWindows())
			return false;

		var root = Path.GetPathRoot(Path.GetFullPath(path));
		return !string.IsNullOrEmpty(root) && !root.StartsWith(@"\\", StringComparison.Ordinal)
			&& new DriveInfo(root).DriveType == DriveType.Fixed;
	}

	public virtual void MoveToRecycleBin(string path)
		=> Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
			path,
			Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
			Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);

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