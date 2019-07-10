using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TQVaultAE.Entities
{
	public static class StringHelper
	{
		public static string ToFirstCharUpperCase(this string text)
		{
			if (string.IsNullOrEmpty(text)) return text;
			return string.Concat(char.ToUpperInvariant(text[0]), text.Substring(1));
		}

		public static string RemoveAllTQTags(this string TQText)
		{
			return Regex.Replace(TQText
				, TQColorHelper.RegExColorTag
				, string.Empty
			);
		}

		/// <summary>
		/// Remove Leading ColorTag + Trailing comment
		/// </summary>
		/// <param name="TQText"></param>
		/// <returns></returns>
		public static string TQCleanup(this string TQText)
		{
			if (TQText is null) return string.Empty;
			var t = TQColorHelper.RemoveLeadingColorTag(TQText);
			return Regex.Replace(t, @"(?<Legit>[^/]*)(?<Comment>//.*)", @"${Legit}").Trim();
		}

		/// <summary>
		/// Indicate if <paramref name="TQText"/> has a color tag prefix.
		/// </summary>
		/// <param name="TQText"></param>
		/// <returns></returns>
		public static bool HasColorPrefix(this string TQText)
		{
			if (TQText is null) return false;
			var t = TQColorHelper.GetColorFromTaggedString(TQText);
			return t.HasValue;
		}


		static char[] _Delim = new char[] { ' ' };
		/// <summary>
		/// Preprare <paramref name="TQPath"/> for display
		/// </summary>
		/// <param name="TQPath"></param>
		/// <returns></returns>
		public static string PrettyFileName(this string TQPath)
		{
			if (TQPath is null) return null;
			var filename = Path.GetFileNameWithoutExtension(TQPath).Replace('_', ' ');
			filename = Regex.Replace(filename, @"(?<number>\d+)", "(${number})");// Enclose Numbers

			filename = Regex
				.Replace(filename, @"(?<TitleCaseStart>[A-Z][a-z]*)", " ${TitleCaseStart}")// Add space on Title Case
				.Split(_Delim, StringSplitOptions.RemoveEmptyEntries)// Split on spaces
				.SelectMany(w => Regex
					.Replace(w, @"(?<Start>resist|light|attac|speed|reduc|life|poison)", " ${Start}") // Add space on word begining for non TitleCase words
					.Split(_Delim, StringSplitOptions.RemoveEmptyEntries)// Split on spaces
				).Select(w => w.ToFirstCharUpperCase()) // Capitalize words
				.JoinString(" ");// Put it back together

			return filename;
		}

		/// <summary>
		/// Insert <paramref name="insertedText"/> between the color tag prefix and the text.
		/// </summary>
		/// <param name="TQText"></param>
		/// <param name="insertedText"></param>
		/// <returns></returns>
		public static string InsertAfterColorPrefix(this string TQText, string insertedText)
		{
			if (TQText is null) return insertedText;
			if (string.IsNullOrEmpty(insertedText)) return TQText;
			return Regex.Replace(TQText
				, $@"{TQColorHelper.RegExStartingColorTagOrEmpty}(?<Content>.+)"
				, string.Concat(@"${ColorTag}", insertedText, @"${Content}")
			).Trim();
		}

		public static IEnumerable<string> RemoveEmptyAndSanitize(this IEnumerable<string> TQText)
			=> TQText.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t =>
			{
				// Split ColorTag & Content
				var tag = t.GetColorFromTaggedString();
				var text = t.RemoveLeadingColorTag()
					.Replace("//", string.Empty).Trim()// Cleanup "//"
					.ToFirstCharUpperCase();
				return $"{(tag?.ColorTag())}{text}";
			});
		public static string JoinString(this IEnumerable<string> Text, string delim) => string.Join(delim, Text);

		public static string JoinWithoutStartingSpaces(this IEnumerable<string> TQText, string delim)
		{
			var tmp = TQText.ToArray();
		repass:
			List<string> res = new List<string>();
			for (int i = 0; i < tmp.Length; i++)
			{
				// Color Prefix alone
				var HasColorPrefix = tmp[i].HasColorPrefix();
				if (HasColorPrefix && tmp[i].Length == 4 && (i + 1) < tmp.Length - 1)
				{
					// Merge it with next element
					res.Add(string.Concat(tmp[i], tmp[i + 1]));
					i++;// Move forward
				}
				else if (HasColorPrefix && tmp[i].Length == 4 && (i + 1) == tmp.Length)// ColorTag is last element
					continue;
				else res.Add(tmp[i]);
			}

			if (tmp.Length != res.Count)
			{
				tmp = res.ToArray();
				goto repass;// Recheck for multiple occurences
			}

			return string.Join(delim, res.ToArray());
		}

		public static IEnumerable<string> SplitOnTQNewLine(this string TQText) => Regex.Split(TQText, @"(?i)\{\^N}");

		/// <summary>
		/// Wraps the words in a text description.
		/// </summary>
		/// <param name="TQText">Text to be word wrapped</param>
		/// <param name="Columns">maximum number of columns before wrapping</param>
		/// <returns>List of wrapped text</returns>
		public static Collection<string> WrapWords(string TQText, int Columns)
		{
			List<string> choppedLines = new List<string>();
			// First split on NL tag
			choppedLines.AddRange(SplitOnTQNewLine(TQText));
			// split on columns args length
			choppedLines = choppedLines.SelectMany(t => SplitOnColumns(t)).ToList();

			IEnumerable<string> SplitOnColumns(string t)
			{
				if (t.Length > Columns)
				{
					// split on spaces
					var batch = Regex.Split(t, @"\s+");
					string line = string.Empty;
					string currentColor = string.Empty;
					List<string> res = new List<string>();
					foreach (var word in batch)
					{
						var foundColor = TQColorHelper.GetColorFromTaggedString(word);
						if (line != string.Empty
							// Not a ColorTag alone
							&& !(line.HasColorPrefix() && line.Length == 4) // TODO Should create line.IsColorTagOnly()
						) line += ' ';
						if (foundColor.HasValue) currentColor = foundColor?.ColorTag();
						if (line.Length + word.Length > Columns)
						{
							res.Add(line);
							line = currentColor + string.Empty;
						}
						line += word;
					}
					res.Add(line);// remnant
					return res;
				}
				else return new string[] { t };
			}

			return new Collection<string>(choppedLines);
		}
	}
}
