//-----------------------------------------------------------------------
// <copyright file="TextTags.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Text.RegularExpressions;

namespace TQVaultAE.GameFiles;

/// <summary>
/// Format codes of the game texts: colour codes <c>{^y}</c> or <c>^y</c> (one letter or digit) and the line break
/// <c>{^n}</c> or <c>^n</c>. Shared by the app and tqdb.
/// </summary>
public static partial class TextTags
{
	/// <summary>A colour code, four-character <c>{^x}</c> or two-character <c>^x</c> form (groups ColorTag, ColorId).</summary>
	public const string ColorTagPattern = @"(?<ColorTag>\{\^(?<ColorId>\w)}|\^(?<ColorId>\w))";

	/// <summary>Matches every colour code (<see cref="ColorTagPattern"/>); line breaks match too.</summary>
	[GeneratedRegex(ColorTagPattern)]
	public static partial Regex ColorTag();

	[GeneratedRegex(@"\{\^[nN]}|\^[nN]")]
	private static partial Regex LineBreak();

	/// <summary>Removes every colour code (line breaks included) from <paramref name="text"/>.</summary>
	public static string RemoveColorTags(string text)
	{
		ArgumentNullException.ThrowIfNull(text);
		return ColorTag().Replace(text, string.Empty);
	}

	/// <summary>Plain text: line breaks replaced by <paramref name="newLine"/>, colour codes removed.</summary>
	public static string ToPlainText(string text, string newLine = "\n")
	{
		ArgumentNullException.ThrowIfNull(text);
		return RemoveColorTags(LineBreak().Replace(text, _ => newLine));
	}
}
