//-----------------------------------------------------------------------
// <copyright file="PathSanitizer.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Text;

namespace TQVaultAE.GameFiles;

/// <summary>
/// Turns archive entry names and record ids into safe relative paths under an output root (tqdb extract and
/// exports, ARZExplorer Extract All).
/// </summary>
public static class PathSanitizer
{
	private static readonly HashSet<string> Reserved = new(StringComparer.OrdinalIgnoreCase)
	{
		"CON", "PRN", "AUX", "NUL",
		"COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
		"LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
	};

	/// <summary>
	/// Sanitized relative path ('/' or '\' accepted as separators): drive/root prefixes rejected, '.' and empty segments dropped,
	/// control and invalid characters replaced by '_', trailing dots/spaces trimmed, reserved device names prefixed with '_'.
	/// cp1252 letters are kept. Throws <see cref="InvalidDataException"/> on '..' segments or rooted names.
	/// </summary>
	public static string Relative(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new InvalidDataException("Empty entry name.");
		if (name[0] is '/' or '\\' || (name.Length >= 2 && name[1] == ':'))
			throw new InvalidDataException($"Rooted entry name '{name}' rejected.");

		var sb = new StringBuilder(name.Length);
		foreach (var raw in name.Split('/', '\\'))
		{
			if (raw.Length == 0 || raw == ".")
				continue;
			if (raw == "..")
				throw new InvalidDataException($"Entry name '{name}' escapes the output folder.");

			var segment = CleanSegment(raw);
			if (segment.Length == 0)
				continue;
			if (sb.Length > 0)
				sb.Append(Path.DirectorySeparatorChar);
			sb.Append(segment);
		}

		if (sb.Length == 0)
			throw new InvalidDataException($"Entry name '{name}' has no usable path segment.");
		return sb.ToString();
	}

	/// <summary>Combines <paramref name="root"/> with a sanitized <paramref name="name"/> and checks the result stays under root.</summary>
	public static string Combine(string root, string name)
	{
		var fullRoot = Path.GetFullPath(root);
		var full = Path.GetFullPath(Path.Combine(fullRoot, Relative(name)));
		var prefix = fullRoot.EndsWith(Path.DirectorySeparatorChar) ? fullRoot : fullRoot + Path.DirectorySeparatorChar;
		if (!full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			throw new InvalidDataException($"Entry name '{name}' escapes the output folder.");
		return full;
	}

	private static string CleanSegment(string segment)
	{
		var chars = segment.ToCharArray();
		for (int i = 0; i < chars.Length; i++)
		{
			char c = chars[i];
			if (c < 32 || c is '<' or '>' or ':' or '"' or '|' or '?' or '*')
				chars[i] = '_';
		}
		var s = new string(chars).TrimEnd('.', ' ').Trim();
		if (s.Length == 0)
			return string.Empty;
		var stem = s.Split('.', 2)[0];
		return Reserved.Contains(stem) ? "_" + s : s;
	}
}
