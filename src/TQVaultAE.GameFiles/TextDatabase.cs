//-----------------------------------------------------------------------
// <copyright file="TextDatabase.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Text;

namespace TQVaultAE.GameFiles;

public static partial class TextDatabaseFactory
{
	/// <summary>Loads every .txt in the archive (UTF-16LE with BOM or cp1252), key=value, // comments, last wins.</summary>
	public static ITextDatabase Load(string textArchivePath, string language)
	{
		ArgumentNullException.ThrowIfNull(textArchivePath);
		var entries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		using (var arc = ArcArchiveFactory.Open(textArchivePath))
		{
			foreach (var entry in arc.Entries)
			{
				if (!entry.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
					continue;
				TextDatabase.Parse(TextDatabase.DecodeText(arc.ReadAll(entry)), entries);
			}
		}
		return new TextDatabase(language, entries);
	}

	/// <summary>
	/// Reads the entries of one text file (UTF-16LE/BE or UTF-8 with a BOM, cp1252 without): key=value lines split on
	/// the first '=', '//' comments skipped, the last of a duplicated key wins.
	/// </summary>
	public static Dictionary<string, string> ReadEntries(ReadOnlySpan<byte> bytes)
		=> ReadEntries(DecodeText(bytes));

	/// <summary>Decodes a game text file: UTF-16LE/BE or UTF-8 when a BOM is present, cp1252 otherwise.</summary>
	public static string DecodeText(ReadOnlySpan<byte> bytes) => TextDatabase.DecodeText(bytes);

	private static Dictionary<string, string> ReadEntries(string text)
	{
		var entries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		TextDatabase.Parse(text, entries);
		return entries;
	}
}

/// <summary>In-memory tag table.</summary>
internal sealed class TextDatabase : ITextDatabase
{
	private readonly Dictionary<string, string> _entries;

	public TextDatabase(string language, Dictionary<string, string> entries)
	{
		this.Language = language;
		this._entries = entries;
	}

	public string Language { get; }

	public IReadOnlyDictionary<string, string> Entries => this._entries;

	public string? Resolve(string? tag)
	{
		if (string.IsNullOrWhiteSpace(tag))
			return null;
		return this._entries.TryGetValue(tag.Trim(), out var value) ? value : null;
	}

	/// <summary>Decodes a text file: UTF-16LE/BE or UTF-8 when a BOM is present, cp1252 otherwise.</summary>
	internal static string DecodeText(ReadOnlySpan<byte> bytes)
	{
		if (bytes.StartsWith((ReadOnlySpan<byte>)[0xFF, 0xFE]))
			return Encoding.Unicode.GetString(bytes[2..]);
		if (bytes.StartsWith((ReadOnlySpan<byte>)[0xFE, 0xFF]))
			return Encoding.BigEndianUnicode.GetString(bytes[2..]);
		if (bytes.StartsWith((ReadOnlySpan<byte>)[0xEF, 0xBB, 0xBF]))
			return Encoding.UTF8.GetString(bytes[3..]);
		return TextEncodings.Cp1252.GetString(bytes);
	}

	/// <summary>Parses key=value lines into <paramref name="into"/> (split on the first '=', '//' comments, last wins).</summary>
	internal static void Parse(string text, Dictionary<string, string> into)
	{
		foreach (var rawLine in text.AsSpan().EnumerateLines())
		{
			var line = rawLine.Trim();
			if (line.IsEmpty || line.StartsWith("//"))
				continue;
			int eq = line.IndexOf('=');
			if (eq <= 0)
				continue;
			var key = line[..eq].Trim();
			if (key.IsEmpty)
				continue;
			into[key.ToString()] = line[(eq + 1)..].Trim().ToString();
		}
	}
}
