using System.Globalization;
using TQVaultAE.Application.Results;

namespace TQVaultAE.Presentation;

/// <summary>Texts of the item, tab and vault imports.</summary>
public static class ImportText
{
	/// <summary>The message of a failed import, in the UI language.</summary>
	public static string Message(ImportResult result)
	{
		ArgumentNullException.ThrowIfNull(result);
		var format = result.Error switch
		{
			ImportError.UnsupportedFormat => Resources.ImportErrorFormat,
			ImportError.MissingScope => Resources.ImportErrorMissingScope,
			ImportError.UnsupportedScope => Resources.ImportErrorScope,
			ImportError.MissingData => Resources.ImportErrorMissingData,
			ImportError.InvalidJson => Resources.ImportErrorJson,
			ImportError.UnreadableData => Resources.ImportErrorData,
			ImportError.NothingToImport => Resources.ImportErrorNothing,
			ImportError.PasteBinFailed => Resources.ImportErrorPasteBin,
			_ => string.Empty,
		};
		return string.Format(CultureInfo.CurrentCulture, format, result.ErrorDetail);
	}
}
