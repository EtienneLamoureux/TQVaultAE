using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Helpers;

public static class StringHelper
{
	const StringComparison noCase = StringComparison.OrdinalIgnoreCase;

	public const string TQNewLineTag = @"{^N}";

	public static bool Contains(this string input, string search, StringComparison comparison)
		=> input.IndexOf(search, comparison) > -1;
	
	public static bool ContainsIgnoreCase(this string input, string search)
		=> input.Contains(search, noCase);


	#region Eval

	static DataTable CheapestDotNetEval = new DataTable();
	public static T Eval<T>(this string expression)
		=> (T)Convert.ChangeType(CheapestDotNetEval.Compute(expression, null), typeof(T));

	#endregion

	/// <summary>
	/// Tells if <paramref name="search"/> is a regex input search
	/// </summary>
	/// <param name="search"></param>
	/// <returns></returns>
	public static (bool IsRegex, string Pattern, Regex Regex, bool RegexIsValid) IsTQVaultSearchRegEx(string search)
	{
		if (string.IsNullOrWhiteSpace(search)) return (false, string.Empty, null, false);

		var isregex = search.First() == '/';

		if (isregex)
		{
			Regex rex = null;
			var pattern = search.Substring(1);
			try
			{
				rex = new Regex(pattern, RegexOptions.IgnoreCase);
				return (isregex, pattern, rex, true);
			}
			catch (ArgumentException)
			{
				return (true, pattern, null, false);
			}
		}

		return (false, search, null, false);
	}

	public static string MakeMD5(this string input)
	{
		string hash;

		using var md5Hash = MD5.Create();

		// Convert the input string to a byte array and compute the hash.
		byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

		StringBuilder sBuilder = new StringBuilder();

		for (int i = 0; i < data.Length; i++)
			sBuilder.Append(data[i].ToString("x2"));

		hash = sBuilder.ToString();

		return hash;
	}

	public static string ToFirstCharUpperCase(this string text)
	{
		if (string.IsNullOrEmpty(text)) return text;
		return string.Concat(char.ToUpperInvariant(text[0]), text.Substring(1));
	}

	/// <summary>
	/// Indicate if <paramref name="TQText"/> contain a ColorTag only
	/// </summary>
	/// <param name="TQText"></param>
	/// <returns></returns>
	public static bool IsColorTagOnly(this string TQText) => IsColorTagOnlyExtended(TQText).Value;

	static readonly Regex IsColorTagOnlyExtendedRegEx = new Regex(@"^" + TQColorHelper.RegExTQTag + @"$", RegexOptions.Compiled);
	/// <summary>
	/// Indicate if <paramref name="TQText"/> contain a ColorTag only
	/// </summary>
	/// <param name="TQText"></param>
	/// <returns></returns>
	public static (bool Value, int Length) IsColorTagOnlyExtended(this string TQText)
	{
		if (string.IsNullOrWhiteSpace(TQText)) return (false, 0);
		return (IsColorTagOnlyExtendedRegEx.IsMatch(TQText), TQText.Length);
	}


	public static string RemoveAllTQTags(this string TQText)
	{
		if (string.IsNullOrWhiteSpace(TQText)) return TQText;
		return TQColorHelper.RegExTQTagInstance.Replace(TQText, string.Empty);
	}

	static readonly Regex TQCleanupRegEx = new Regex(@"(?<Legit>[^/]*)(?<Comment>//.*)", RegexOptions.Compiled);
	const string TQCleanupReplaceResultPattern = @"${Legit}";
	/// <summary>
	/// Remove Leading ColorTag + Trailing comment
	/// </summary>
	/// <param name="TQText"></param>
	/// <returns></returns>
	public static string TQCleanup(this string TQText, bool keepLeadingColorTag = false)
	{
		if (TQText is null) return string.Empty;
		var txt = TQColorHelper.RemoveLeadingColorTag(TQText);
		string replaceTxt = TQCleanupReplaceResultPattern;
		if (keepLeadingColorTag)
		{
			var col = TQColorHelper.GetColorFromTaggedString(TQText);
			if (col.HasValue)
				replaceTxt = col.Value.ColorTag() + TQCleanupReplaceResultPattern;
		}
		return TQCleanupRegEx.Replace(txt, replaceTxt).Trim();
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
	static readonly Regex PrettyFileNameRegExNumber = new Regex(@"(?<number>\d+)", RegexOptions.Compiled);
	static readonly Regex PrettyFileNameRegExTitleCaseStart = new Regex(@"(?<TitleCaseStart>BOW|DA|OA|XP|Mastery[A-Ha-h]|[A-Z][a-z]*)", RegexOptions.Compiled);
	// Orderered by word length
	// language=regex, IgnorePatternWhitespace
	static string PrettyFileNameRegExLowerCaseStartPattern = @"
(?<Start>
	intelligence|protection|impairment|offensive|defensive|dexterity|elemental|
	mobility|cooldown|mastery|current|protect|defense|offense|reflect|
	damage|energy|pierce|guards|neidan|resist|health|poison|weapon|plants|hermes|sandal|
	armor|chance|[rR]unes|[dD]ream|bleed|total|bonus|woods|multi|relic|light|attac|speed|reduc|block|equip|
	clubs|sleep|metal|leech|regen|dodge|retal|
	cold|burn|life|fire|mana|stun|(?<!ext)rare|slow|wood|
	req|int|(?<!h)all(?!owed)|dmg|(?<!con)str(?!uction)|atk|att|spd|run|dex|(?<=pierce)ret|
	xp|(?<=chance)of|(?<!insec)to(?!rm)|(?<=(%|att))da|(?<=att)oa|
	[\-\+]?%|(?<=(offense|resists|%da))x(?!(tra|alted))|&|[\-\+]
)";
	static readonly Regex PrettyFileNameRegExLowerCaseStart = new Regex(PrettyFileNameRegExLowerCaseStartPattern, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
	/// <summary>
	/// Preprare <paramref name="TQPath"/> for display
	/// </summary>
	/// <param name="TQPath"></param>
	/// <returns></returns>
	public static string PrettyFileName(this string TQPath)
	{
		if (TQPath is null) return null;
		var filename = Path.GetFileNameWithoutExtension(TQPath).Replace('_', ' ');
		filename = PrettyFileNameRegExNumber.Replace(filename, "(${number})");// Enclose Numbers

		var filenameSplit1 = PrettyFileNameRegExTitleCaseStart
			.Replace(filename, ' ' + "${TitleCaseStart}")// Add space on Title Case
				.Split(_Delim, StringSplitOptions.RemoveEmptyEntries);// Split on spaces

		var filenameSplit2 = filenameSplit1.SelectMany(w => PrettyFileNameRegExLowerCaseStart
			.Replace(w, ' ' + "${Start}") // Add space on word begining for non TitleCase words
				.Split(_Delim, StringSplitOptions.RemoveEmptyEntries)// Split on spaces
			);

		filename = filenameSplit2.Select(w =>
			{
				var title = w.ToFirstCharUpperCase();// TitleCase words
				return title switch // Substitute acronym
				{
					"Dmg" => "Damage",
					"Int" => "Intelligence",
					"Str" => "Strength",
					"Att" => "Attribute",
					"Neg" => "Negative",
					"Req" => "Requirement",
					"Protect" => "Protection",
					"Equip" => "Equipement",
					"Atk" => "Attack",
					"Xp" => "XP",
					"Spd" => "Speed",
					"BOW" => "Bow",
					"Dex" => "Dexterity",
					var x when x == "Retal" || x == "Ret" => "Retaliation",
					var x when x == "DA" || x == "Da" => "Defensive Ability",
					var x when x == "OA" || x == "Oa" => "Offensive Ability",
					var x when x.Equals("MasteryA", noCase) => "Mastery Warfare",
					var x when x.Equals("MasteryB", noCase) => "Mastery Defense",
					var x when x.Equals("MasteryC", noCase) => "Mastery Hunting",
					var x when x.Equals("MasteryD", noCase) => "Mastery Rogue",
					var x when x.Equals("MasteryE", noCase) => "Mastery Earth",
					var x when x.Equals("MasteryF", noCase) => "Mastery Nature",
					var x when x.Equals("MasteryG", noCase) => "Mastery Spirit",
					var x when x.Equals("MasteryH", noCase) => "Mastery Storm",
					_ => title
				};
			})
			.JoinString(" ");// Put it back together

		return filename;
	}

	/// <summary>
	/// Normalizes the record path to Upper Case Invariant Culture and replace slashes with backslashes.
	/// </summary>
	/// <param name="recordId">record path to be normalized</param>
	/// <returns>normalized record path</returns>
	public static string NormalizeRecordPath(this string recordId)
	{
		// uppercase it
		string normalizedRecordId = recordId.ToUpperInvariant();

		// replace any '/' with '\\'
		return normalizedRecordId.Replace('/', '\\');
	}

	/// <summary>
	/// Make a <see cref="RecordId"/> from <paramref name="recordId"/>.
	/// </summary>
	/// <param name="recordId"></param>
	/// <returns></returns>
	public static RecordId ToRecordId(this string recordId)
		=> RecordId.Create(recordId);

	/// <summary>
	/// Explode <see cref="PrettyFileName"/> result 
	/// </summary>
	/// <param name="TQPath"></param>
	/// <returns></returns>
	public static (string PrettyFileName, string Effect, string Number, bool IsMatch) PrettyFileNameExploded(this string TQPath)
	{
		var pretty = TQPath.PrettyFileName();
		return pretty.ExplodePrettyFileName();
	}

	// language=regex, IgnorePatternWhitespace
	static readonly Regex _ExplodePrettyFileName = new Regex(@"
[^\(]+\((?<Num1>\d+)\)(?<Effect>[^\(]+)\((?<Num>\d+)\) # match 'trash (0) effect (0)'
|
\((?<Num>\d+)\)(?<Effect>.+)  # match '(0) effect'
|
(?<Effect>[^\(]+)\((?<Num>\d+)\) # match 'effect (0)'
", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

	/// <summary>
	/// Explode an already prettyfied file name
	/// </summary>
	/// <param name="prettyFileName"></param>
	/// <returns></returns>
	public static (string PrettyFileName, string Effect, string Number, bool IsMatch) ExplodePrettyFileName(this string prettyFileName)
	{
		var m = _ExplodePrettyFileName.Match(prettyFileName);

		if (!m.Success)
			return (prettyFileName, prettyFileName, string.Empty, false);

		return (prettyFileName, m.Groups["Effect"].Value.Trim(), m.Groups["Num"].Value, true);
	}

	static readonly Regex AllContiguousSpaceRegEx = new Regex(@"\s+", RegexOptions.Compiled);

	static readonly Regex InsertAfterColorPrefixRegEx = new Regex(TQColorHelper.RegExStartingColorTagOrEmpty + @"(?<Content>.+)", RegexOptions.Compiled);
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
		return InsertAfterColorPrefixRegEx.Replace(TQText
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
			var IsColorTagOnly = tmp[i].IsColorTagOnly();
			if (IsColorTagOnly && (i + 1) < tmp.Length - 1)
			{
				// Merge it with next element
				res.Add(string.Concat(tmp[i], tmp[i + 1]));
				i++;// Move forward
			}
			else if (IsColorTagOnly && (i + 1) == tmp.Length)// ColorTag is last element
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

	static readonly Regex SplitOnTQNewLineRegEx = new Regex(@"(?i)\{\^N}", RegexOptions.Compiled);
	public static IEnumerable<string> SplitOnTQNewLine(this string TQText) => SplitOnTQNewLineRegEx.Split(TQText);

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
				var batch = AllContiguousSpaceRegEx.Split(t);
				string line = string.Empty;
				string currentColor = string.Empty;
				List<string> res = new List<string>();
				foreach (var word in batch)
				{
					var foundColor = TQColorHelper.GetColorFromTaggedString(word);
					if (line != string.Empty
						// Not a ColorTag alone
						&& !line.IsColorTagOnly()
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
