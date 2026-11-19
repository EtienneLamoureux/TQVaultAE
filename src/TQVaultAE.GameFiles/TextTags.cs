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

	/// <summary>
	/// The first grammatical variant of a text, without its markers: <c>[ms]renforcé[fs]renforcée</c> gives
	/// <c>renforcé</c>, <c>Lame [fs]crochue[ms]crochu</c> gives <c>Lame crochue</c>. A text wholly in brackets loses them;
	/// a leading bracketed word (<c>[surpris] Aaaah!</c>) is dropped. Other texts are returned unchanged.
	/// </summary>
	public static string FirstVariant(string text)
	{
		ArgumentNullException.ThrowIfNull(text);
		if (LeadingMarkerOrBrackets().Match(text) is { Success: true } leading)
			return leading.Groups["Label"].Value.Trim();

		var marker = VariantMarker().Match(text);
		if (!marker.Success)
			return text;
		int start = marker.Index + marker.Length;
		var next = VariantMarker().Match(text, start);
		return text[..marker.Index] + (next.Success ? text[start..next.Index] : text[start..]);
	}

	[GeneratedRegex(@"^(?<Tag>\[\w+\])(?<Label>[^\[]+)|^\[(?<Label>[^\]]+)\]$")]
	private static partial Regex LeadingMarkerOrBrackets();

	[GeneratedRegex(@"\[[a-zA-Z]{1,3}\]")]
	private static partial Regex VariantMarker();

	/// <summary>
	/// The lines of a game text, split at its line breaks (<c>{^n}</c>, <c>^n</c>); colour codes stay in the lines. Two
	/// breaks in a row give an empty line.
	/// </summary>
	public static string[] SplitLines(string text)
	{
		ArgumentNullException.ThrowIfNull(text);
		return LineBreak().Split(text);
	}
}
