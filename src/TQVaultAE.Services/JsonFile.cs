using System.Text.Json;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Services;

namespace TQVaultAE.Services;

/// <summary>What <see cref="JsonFile.Read"/> found.</summary>
/// <param name="Value">The file's content; null when the file is missing or could not be read.</param>
/// <param name="Failed">True when the file exists but could not be read or is not valid JSON (logged).</param>
public readonly record struct JsonFileRead<T>(T? Value, bool Failed) where T : class;

/// <summary>Reads and writes the small JSON files of the settings: class sets, loadouts, mod links, saved searches, tags.</summary>
public static class JsonFile
{
	/// <summary>
	/// Reads <paramref name="path"/>. A missing file gives no value; an unreadable file, invalid JSON or a file holding
	/// <c>null</c> give <see cref="JsonFileRead{T}.Failed"/>, logged with <paramref name="what"/>, never an exception.
	/// </summary>
	public static JsonFileRead<T> Read<T>(IFileIO fileIO, string path, JsonSerializerOptions options, ILogger log, string what) where T : class
	{
		ArgumentNullException.ThrowIfNull(fileIO);
		ArgumentNullException.ThrowIfNull(log);
		if (!fileIO.Exists(path))
			return default;

		try
		{
			return JsonSerializer.Deserialize<T>(fileIO.ReadAllText(path), options) is { } value
				? new(value, false)
				: throw new JsonException("The file holds no value");
		}
		catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
		{
			log.LogError(ex, "Failed to read the {What} {File}", what, path);
			return new(null, true);
		}
	}

	/// <summary>Writes <paramref name="value"/> to <paramref name="path"/>; file errors are thrown.</summary>
	public static void Write<T>(IFileIO fileIO, string path, T value, JsonSerializerOptions options)
	{
		ArgumentNullException.ThrowIfNull(fileIO);
		fileIO.WriteAllText(path, JsonSerializer.Serialize(value, options));
	}
}
