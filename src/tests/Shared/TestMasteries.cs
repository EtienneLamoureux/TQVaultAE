using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Shared;

/// <summary>
/// The base game's mastery records, as its skill panes give them, to build mastery trees without a database. Shared by
/// the test projects (linked source).
/// </summary>
internal static class TestMasteries
{
	/// <summary>The mastery record of each slot in the base game.</summary>
	public static readonly IReadOnlyDictionary<Masteries, string> BaseGameRecords = new Dictionary<Masteries, string>
	{
		[Masteries.Defensive] = """Records\Skills\Defensive\DefensiveMastery.dbr""",
		[Masteries.Storm] = """Records\Skills\Storm\StormMastery.dbr""",
		[Masteries.Earth] = """Records\Skills\Earth\EarthMastery.dbr""",
		[Masteries.Nature] = """Records\Skills\Nature\NatureMastery.dbr""",
		[Masteries.Dream] = """Records\XPack\Skills\Dream\DreamMastery.dbr""",
		[Masteries.Rune] = """records\xpack2\skills\RuneMaster\RuneMaster_Mastery.dbr""",
		[Masteries.Warfare] = """Records\Skills\Warfare\WarfareMastery.dbr""",
		[Masteries.Hunting] = """Records\Skills\Hunting\HuntingMastery.dbr""",
		[Masteries.Stealth] = """Records\Skills\Stealth\StealthMastery.dbr""",
		[Masteries.Spirit] = """Records\Skills\Spirit\SpiritMastery.dbr""",
		[Masteries.Neidan] = """Records\Xpack4\Skills\Neidan\NeidanMastery.dbr""",
	};

	/// <summary>The 11 base game masteries, without skills.</summary>
	public static IReadOnlyList<MasteryTree> BaseGame => MasteryTree.AllOf(m => Tree(m));

	/// <summary>A mastery with the base game's record and the given skills.</summary>
	public static MasteryTree Tree(Masteries mastery, params string[] skills)
		=> ModTree(mastery, BaseGameRecords[mastery], skills);

	/// <summary>A mastery with its own record (a mod's) and the given skills.</summary>
	public static MasteryTree ModTree(Masteries mastery, string record, params string[] skills)
		=> new(mastery, record.ToRecordId(), string.Empty, string.Empty, string.Empty, 40,
			[.. skills.Select(s => new MasterySkill(s.ToRecordId(), string.Empty, string.Empty, string.Empty, 10, 1, null))]);
}
