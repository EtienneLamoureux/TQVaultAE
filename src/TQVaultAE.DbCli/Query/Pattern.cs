//-----------------------------------------------------------------------
// <copyright file="Pattern.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;
using TQVaultAE.DbCli.Commands;

namespace TQVaultAE.DbCli.Query;

/// <summary>
/// Text matcher built from a glob (anchored, <c>*</c> <c>?</c> <c>[abc]</c> <c>[!abc]</c>) or a <c>/regex/</c> (unanchored search).
/// Always case-insensitive. Path-like patterns treat '/' and '\' as the same separator.
/// </summary>
internal sealed class Pattern
{
	private static readonly ConcurrentDictionary<(string Text, bool PathLike), Pattern> Cache = new();

	private readonly Regex _regex;

	private Pattern(string source, Regex regex, bool pathLike, string? literal) {
		this.Source = source;
		this._regex = regex;
		this.PathLike = pathLike;
		this.Literal = literal;
	}

	/// <summary>Text as given by the user.</summary>
	public string Source { get; }

	/// <summary>Separators are normalized to '/' before matching.</summary>
	public bool PathLike { get; }

	/// <summary>The glob had no wildcard: exact (case-insensitive) literal, usable for indexed lookups.</summary>
	public string? Literal { get; }

	/// <summary>Pattern is a <c>/regex/</c>.</summary>
	public bool IsRegex => this.Literal is null && this.Source.Length >= 2 && this.Source[0] == '/' && this.Source[^1] == '/';

	/// <summary>Parses (and caches) a pattern.</summary>
	public static Pattern Parse(string text, bool pathLike = false) => Cache.GetOrAdd((text, pathLike), static k => Create(k.Text, k.PathLike));

	/// <summary>True when <paramref name="text"/> looks like a glob with wildcards or a regex.</summary>
	public static bool HasWildcards(string text) => text.AsSpan().IndexOfAny("*?[") >= 0 || (text.Length >= 2 && text[0] == '/' && text[^1] == '/');

	private static Pattern Create(string text, bool pathLike) {
		const RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant;
		if (text.Length >= 2 && text[0] == '/' && text[^1] == '/') {
			var body = text[1..^1];
			try {
				return new Pattern(text, new Regex(body, options), pathLike, null);
			}
			catch (ArgumentException ex) {
				throw new UsageException($"Invalid regular expression '{body}': {ex.Message}");
			}
		}

		var glob = pathLike ? text.Replace('\\', '/') : text;
		string? literal = glob.AsSpan().IndexOfAny("*?[") < 0 ? glob : null;
		return new Pattern(text, new Regex(GlobToRegex(glob), options), pathLike, literal);
	}

	/// <summary>Translates a glob into an anchored regular expression.</summary>
	internal static string GlobToRegex(string glob) {
		var sb = new StringBuilder(glob.Length + 8);
		sb.Append('^');
		for (int i = 0; i < glob.Length; i++) {
			char c = glob[i];
			switch (c) {
				case '*':
					sb.Append(".*");
					break;
				case '?':
					sb.Append('.');
					break;
				case '[':
					int close = glob.IndexOf(']', i + 2);
					if (close < 0) {
						sb.Append(@"\[");
						break;
					}
					var set = glob.AsSpan(i + 1, close - i - 1);
					sb.Append('[');
					if (set[0] == '!' || set[0] == '^') {
						sb.Append('^');
						set = set[1..];
					}
					foreach (var sc in set) {
						if (sc is '\\' or '[' or ']' or '^')
							sb.Append('\\');
						sb.Append(sc);
					}
					sb.Append(']');
					i = close;
					break;
				default:
					sb.Append(Regex.Escape(c.ToString()));
					break;
			}
		}
		sb.Append('$');
		return sb.ToString();
	}

	/// <summary>Matches a string; <see langword="null"/> never matches.</summary>
	public bool IsMatch(string? value) {
		if (value is null)
			return false;
		if (this.PathLike && value.Contains('\\'))
			value = value.Replace('\\', '/');
		return this._regex.IsMatch(value);
	}

	/// <summary>Matches a value already normalized to '/' separators (avoids re-normalizing for several patterns).</summary>
	public bool IsMatchNormalized(string value) => this._regex.IsMatch(value);

	/// <inheritdoc/>
	public override string ToString() => this.Source;
}
