namespace TQVaultAE.Domain.Entities;

/// <summary>
/// The classes of the game: the text tag of the class each mastery and each pair of masteries makes (Warfare and Neidan
/// make x4tagNeidanWarfare, "Daoist"), and the masteries' short names the class line writes ("Warfare-Neidan").
/// </summary>
public static class CharacterClasses
{
	/// <summary>Each class tag with its masteries, in the order the game names them.</summary>
	private static readonly (string Tag, Masteries First, Masteries? Second)[] Table =
	[
		("tagCClass01", Masteries.Spirit, null),
		("tagCClass02", Masteries.Nature, null),
		("tagCClass03", Masteries.Stealth, null),
		("tagCClass04", Masteries.Hunting, null),
		("tagCClass05", Masteries.Storm, null),
		("tagCClass06", Masteries.Earth, null),
		("tagCClass07", Masteries.Defensive, null),
		("tagCClass08", Masteries.Warfare, null),
		("tagCClass09", Masteries.Spirit, Masteries.Warfare),
		("tagCClass10", Masteries.Nature, Masteries.Warfare),
		("tagCClass11", Masteries.Stealth, Masteries.Warfare),
		("tagCClass12", Masteries.Hunting, Masteries.Warfare),
		("tagCClass13", Masteries.Storm, Masteries.Warfare),
		("tagCClass14", Masteries.Earth, Masteries.Warfare),
		("tagCClass15", Masteries.Defensive, Masteries.Warfare),
		("tagCClass16", Masteries.Spirit, Masteries.Defensive),
		("tagCClass17", Masteries.Nature, Masteries.Defensive),
		("tagCClass18", Masteries.Stealth, Masteries.Defensive),
		("tagCClass19", Masteries.Hunting, Masteries.Defensive),
		("tagCClass20", Masteries.Storm, Masteries.Defensive),
		("tagCClass21", Masteries.Earth, Masteries.Defensive),
		("tagCClass22", Masteries.Spirit, Masteries.Earth),
		("tagCClass23", Masteries.Nature, Masteries.Earth),
		("tagCClass24", Masteries.Stealth, Masteries.Earth),
		("tagCClass25", Masteries.Hunting, Masteries.Earth),
		("tagCClass26", Masteries.Storm, Masteries.Earth),
		("tagCClass27", Masteries.Spirit, Masteries.Storm),
		("tagCClass28", Masteries.Nature, Masteries.Storm),
		("tagCClass29", Masteries.Stealth, Masteries.Storm),
		("tagCClass30", Masteries.Hunting, Masteries.Storm),
		("tagCClass31", Masteries.Spirit, Masteries.Hunting),
		("tagCClass32", Masteries.Nature, Masteries.Hunting),
		("tagCClass33", Masteries.Stealth, Masteries.Hunting),
		("tagCClass34", Masteries.Spirit, Masteries.Stealth),
		("tagCClass35", Masteries.Nature, Masteries.Stealth),
		("tagCClass36", Masteries.Spirit, Masteries.Nature),
		("xtagCharacterClass01", Masteries.Dream, null),
		("xtagCharacterClass02", Masteries.Warfare, Masteries.Dream),
		("xtagCharacterClass03", Masteries.Defensive, Masteries.Dream),
		("xtagCharacterClass04", Masteries.Earth, Masteries.Dream),
		("xtagCharacterClass05", Masteries.Storm, Masteries.Dream),
		("xtagCharacterClass06", Masteries.Hunting, Masteries.Dream),
		("xtagCharacterClass07", Masteries.Stealth, Masteries.Dream),
		("xtagCharacterClass08", Masteries.Nature, Masteries.Dream),
		("xtagCharacterClass09", Masteries.Spirit, Masteries.Dream),
		("x2tag_class_rm_rm", Masteries.Rune, null),
		("x2tag_class_warfare_rm", Masteries.Warfare, Masteries.Rune),
		("x2tag_class_defense_rm", Masteries.Defensive, Masteries.Rune),
		("x2tag_class_earth_rm", Masteries.Earth, Masteries.Rune),
		("x2tag_class_storm_rm", Masteries.Storm, Masteries.Rune),
		("x2tag_class_hunting_rm", Masteries.Hunting, Masteries.Rune),
		("x2tag_class_stealth_rm", Masteries.Stealth, Masteries.Rune),
		("x2tag_class_nature_rm", Masteries.Nature, Masteries.Rune),
		("x2tag_class_spirit_rm", Masteries.Spirit, Masteries.Rune),
		("x2tag_class_dream_rm", Masteries.Dream, Masteries.Rune),
		("x4tagNeidan", Masteries.Neidan, null),
		("x4tagNeidanDefense", Masteries.Defensive, Masteries.Neidan),
		("x4tagNeidanDream", Masteries.Dream, Masteries.Neidan),
		("x4tagNeidanEarth", Masteries.Earth, Masteries.Neidan),
		("x4tagNeidanHunting", Masteries.Hunting, Masteries.Neidan),
		("x4tagNeidanNature", Masteries.Nature, Masteries.Neidan),
		("x4tagNeidanRogue", Masteries.Stealth, Masteries.Neidan),
		("x4tagNeidanRune", Masteries.Rune, Masteries.Neidan),
		("x4tagNeidanSpirit", Masteries.Spirit, Masteries.Neidan),
		("x4tagNeidanStorm", Masteries.Storm, Masteries.Neidan),
		("x4tagNeidanWarfare", Masteries.Warfare, Masteries.Neidan),
	];

	/// <summary>The tag of the class of one mastery or two, null when <paramref name="masteries"/> is none or more than two.</summary>
	public static string? TagOf(Masteries masteries)
		=> Table.FirstOrDefault(c => (c.First | (c.Second ?? 0)) == masteries).Tag;

	/// <summary>The tag of the class the masteries make, null when they make none.</summary>
	public static string? TagOf(IEnumerable<Masteries> masteries)
		=> TagOf(masteries.Aggregate((Masteries)0, (all, m) => all | m));

	/// <summary>The masteries of a class, in the order the game names them; empty for a tag that is no class.</summary>
	public static IReadOnlyList<Masteries> MasteriesOf(string? tag)
		=> Table.FirstOrDefault(c => string.Equals(c.Tag, tag, StringComparison.OrdinalIgnoreCase)) is { Tag: not null } c
			? c.Second is { } second ? [c.First, second] : [c.First]
			: [];

	/// <summary>The text tag of a mastery's short name ("Warfare").</summary>
	public static string BriefTag(Masteries mastery) => mastery switch
	{
		Masteries.Warfare => "tagMasteryBrief01",
		Masteries.Defensive => "tagMasteryBrief02",
		Masteries.Earth => "tagMasteryBrief03",
		Masteries.Storm => "tagMasteryBrief04",
		Masteries.Stealth => "tagMasteryBrief05",
		Masteries.Hunting => "tagMasteryBrief06",
		Masteries.Spirit => "tagMasteryBrief07",
		Masteries.Nature => "tagMasteryBrief08",
		Masteries.Dream => "xtagMasteryBrief09",
		Masteries.Rune => "x2tagRuneMasteryBrief",
		Masteries.Neidan => "x4tagNeidan",
		_ => throw new ArgumentOutOfRangeException(nameof(mastery), mastery, "One mastery."),
	};
}
