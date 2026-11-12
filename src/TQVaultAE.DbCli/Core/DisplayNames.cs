//-----------------------------------------------------------------------
// <copyright file="DisplayNames.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Text.RegularExpressions;

namespace TQVaultAE.DbCli.Core;

/// <summary>Display names of records.</summary>
public static partial class DisplayNames
{
	internal const string ItemNameTag = "itemNameTag";
	internal const string Description = "description";
	internal const string SkillDisplayName = "skillDisplayName";
	internal const string LootRandomizerName = "lootRandomizerName";
	internal const string SetName = "setName";

	/// <summary>Generic order used when the class is unknown or its own field is empty.</summary>
	internal static readonly string[] FallbackFields = [ItemNameTag, Description, SkillDisplayName, LootRandomizerName, SetName];

	private static readonly string[] DescriptionPrefixes = ["Monster", "Npc", "FixedItem", "Pet", "OneShot_", "QuestItem", "Dye"];
	private static readonly string[] DescriptionClasses = ["ItemRelic", "ItemCharm", "ItemArtifact", "ItemArtifactFormula"];

	/// <summary>Name tag field for a record (itemNameTag, description, skillDisplayName, lootRandomizerName, setName...), <see langword="null"/> if none.</summary>
	public static string? NameTag(ArzRecord record)
	{
		ArgumentNullException.ThrowIfNull(record);
		var field = PrimaryField(ClassOf(record));
		if (field is not null && FirstString(record, field) is { } own)
			return own;
		foreach (var f in FallbackFields)
			if (FirstString(record, f) is { } s)
				return s;
		return null;
	}

	/// <summary>Resolved, cleaned display name (game format codes and [ms] gender markers stripped), <see langword="null"/> if none.</summary>
	public static string? Resolve(ArzRecord record, ITextDatabase text)
	{
		ArgumentNullException.ThrowIfNull(text);
		var tag = NameTag(record);
		if (tag is null)
			return null;
		if (text.Resolve(tag) is { } resolved)
			return NullIfEmpty(Clean(resolved));

		// Some descriptions (notably Npc) are literal text rather than tags.
		return LooksLikeLiteral(tag) ? NullIfEmpty(Clean(tag)) : null;
	}

	/// <summary>Strips game format codes (<see cref="TextTags"/>, line breaks become spaces) and [xx] gender/plural markers, keeping the first variant when several are given.</summary>
	public static string Clean(string value)
	{
		ArgumentNullException.ThrowIfNull(value);
		var s = TextTags.ToPlainText(value, " ");

		// "[ms]Name[fs]Other" lists grammatical variants: keep the first one.
		var m = GenderMarker().Match(s);
		if (m.Success) {
			int start = m.Index + m.Length;
			var next = GenderMarker().Match(s, start);
			s = s[..m.Index] + (next.Success ? s[start..next.Index] : s[start..]);
		}
		return WhiteSpace().Replace(s, " ").Trim();
	}

	/// <summary>The record's own name field for its class, <see langword="null"/> when the class has no fixed field.</summary>
	internal static string? PrimaryField(string cls)
	{
		if (cls.Length == 0)
			return SetName;
		if (cls.StartsWith("Armor", StringComparison.OrdinalIgnoreCase)
			|| cls.StartsWith("Weapon", StringComparison.OrdinalIgnoreCase)
			|| cls.StartsWith("Shield", StringComparison.OrdinalIgnoreCase))
			return ItemNameTag;
		if (cls.StartsWith("Skill", StringComparison.OrdinalIgnoreCase))
			return SkillDisplayName;
		if (cls.Equals("LootRandomizer", StringComparison.OrdinalIgnoreCase))
			return LootRandomizerName;
		if (cls.Equals("ItemSet", StringComparison.OrdinalIgnoreCase))
			return SetName;
		foreach (var c in DescriptionClasses)
			if (cls.Equals(c, StringComparison.OrdinalIgnoreCase))
				return Description;
		foreach (var p in DescriptionPrefixes)
			if (cls.StartsWith(p, StringComparison.OrdinalIgnoreCase))
				return Description;
		return null;
	}

	private static string ClassOf(ArzRecord record)
	{
		if (record.Info.Class.Length > 0)
			return record.Info.Class;
		return FirstString(record, "Class") ?? string.Empty;
	}

	private static string? FirstString(ArzRecord record, string name)
	{
		var v = record.Get(name);
		if (v is null)
			return null;
		foreach (var value in v.Values)
			if (value is string s && !string.IsNullOrWhiteSpace(s))
				return s.Trim();
		return null;
	}

	/// <summary>True for literal text (e.g. an Npc description "Old Man") as opposed to an unresolved tag or a path.</summary>
	internal static bool LooksLikeLiteral(string tag)
	{
		if (TagPrefix().IsMatch(tag))
			return false;
		if (tag.Contains(' '))
			return true;
		return tag.IndexOfAny(['_', '\\', '/', '.']) < 0;
	}

	[GeneratedRegex(@"^(x\d*)?tag", RegexOptions.IgnoreCase)]
	private static partial Regex TagPrefix();

	private static string? NullIfEmpty(string s) => s.Length == 0 ? null : s;

	[GeneratedRegex(@"\[[a-zA-Z]{1,3}\]")]
	private static partial Regex GenderMarker();

	[GeneratedRegex(@"\s+")]
	private static partial Regex WhiteSpace();
}
