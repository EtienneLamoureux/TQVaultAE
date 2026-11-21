using System.Collections;
using System.Text;
using Microsoft.Extensions.Logging;

namespace TQVaultAE.GUI.Infrastructure;

/// <summary>
/// Formats an exception for the log and the error boxes.
/// </summary>
public static class ILogExtension
{
	/// <summary>
	/// The exception as the runtime writes it (type, message, inner exceptions and stack traces), followed by the entries
	/// of its <see cref="Exception.Data"/> when it has some.
	/// </summary>
	public static string FormatException(this ILogger Log, Exception ex)
	{
		ArgumentNullException.ThrowIfNull(ex);

		if (ex.Data.Count == 0)
			return ex.ToString();

		var text = new StringBuilder(ex.ToString()).AppendLine().AppendLine("Data:");
		foreach (DictionaryEntry entry in ex.Data)
			text.Append('\t').Append(entry.Key).Append(" : ").AppendLine(entry.Value?.ToString() ?? "{null}");

		return text.ToString();
	}
}
