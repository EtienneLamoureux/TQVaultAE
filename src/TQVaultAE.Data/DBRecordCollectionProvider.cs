using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Data;

public class DBRecordCollectionProvider : IDBRecordCollectionProvider
{
	private readonly IDirectoryIO DirectoryIO;
	private readonly IPathIO PathIO;
	private readonly IFileIO FileIO;

	public DBRecordCollectionProvider(IDirectoryIO directoryIO, IPathIO pathIO, IFileIO fileIO)
	{
		this.DirectoryIO = directoryIO;
		PathIO = pathIO;
		FileIO = fileIO;
	}

	/// <summary>
	/// Writes all variables into a file.
	/// </summary>
	/// <param name="drc">source</param>
	/// <param name="baseFolder">Path in the file.</param>
	/// <param name="fileName">file name to be written</param>
	public void Write(DBRecordCollection drc, string baseFolder, string? fileName = null)
	{
		// construct's full path
		string fullPath = PathIO.Combine(baseFolder, drc.Id.Normalized);
		string destinationFolder = PathIO.GetDirectoryName(fullPath);

		if (fileName != null)
		{
			fullPath = PathIO.Combine(baseFolder, fileName);
			destinationFolder = baseFolder;
		}

		// Create's folder path if necessary
		if (!DirectoryIO.Exists(destinationFolder))
			DirectoryIO.CreateDirectory(destinationFolder);

		// Write's all variables
		List<string> content = new();
		foreach (Variable variable in drc)
		{
			content.Add(variable.ToString());
		}

		// Write's all variables
		FileIO.WriteAllLines(fullPath, content);
	}
}