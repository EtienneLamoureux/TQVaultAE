using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Data;

public class DBRecordCollectionProvider(IDirectoryIO directoryIO, IPathIO pathIO, IFileIO fileIO) : IDBRecordCollectionProvider
{
	private readonly IDirectoryIO DirectoryIO = directoryIO;
	private readonly IPathIO PathIO = pathIO;
	private readonly IFileIO FileIO = fileIO;

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

		if (fileName is not null)
		{
			fullPath = PathIO.Combine(baseFolder, fileName);
			destinationFolder = baseFolder;
		}

		// Create's folder path if necessary
		if (!DirectoryIO.Exists(destinationFolder))
			DirectoryIO.CreateDirectory(destinationFolder);

		// Write's all variables
		List<string> content = [];
		foreach (Variable variable in drc)
		{
			content.Add(variable.ToString());
		}

		// Write's all variables
		FileIO.WriteAllLines(fullPath, content);
	}
}