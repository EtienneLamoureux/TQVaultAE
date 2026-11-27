using TQ.SaveFilesExplorer.Entities;

namespace TQ.SaveFilesExplorer.Services;

public class TQFileService
{
	public TQFileService()
	{
	}

	public TQFile ReadFile(string path)
	{
		var file = TQFile.ReadFile(path);
		file.Parse();
		file.Analyse();
		return file;
	}
}
