using System.Collections.Concurrent;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Data;

/// <summary>
/// Reads the masteries and their skills from the panes of the game's skill window (<c>SkillPaneCtrl</c> records):
/// each pane lists its skill buttons, whose row gives the tier of the skill.
/// </summary>
public class SkillTreeProvider(IDatabase database) : ISkillTreeProvider
{
	private const StringComparison NoCase = StringComparison.OrdinalIgnoreCase;

	private readonly IDatabase _database = database;

	private readonly ConcurrentDictionary<Masteries, MasteryTree?> _trees = new();

	/// <summary>
	/// The pane records of each mastery, the latest expansion first: Atlantis added two skills to each pane of the
	/// first ten masteries.
	/// </summary>
	internal static readonly IReadOnlyDictionary<Masteries, string[]> PaneRecords = new Dictionary<Masteries, string[]>
	{
		[Masteries.Warfare] = Panes(1),
		[Masteries.Defensive] = Panes(2),
		[Masteries.Earth] = Panes(3),
		[Masteries.Storm] = Panes(4),
		[Masteries.Stealth] = Panes(5),
		[Masteries.Hunting] = Panes(6),
		[Masteries.Spirit] = Panes(7),
		[Masteries.Nature] = Panes(8),
		[Masteries.Dream] = ["""records\xpack3\ui\skills\mastery 9\panectrl.dbr""", """records\xpack\ui\skills\mastery 9\panectrl.dbr"""],
		[Masteries.Rune] = ["""records\xpack3\ui\skills\mastery 10\panectrl.dbr""", """records\xpack2\ui\skills\mastery 10\panectrl.dbr"""],
		[Masteries.Neidan] = ["""records\xpack4\ui\skills\mastery 11\panectrl.dbr"""],
	};

	private static string[] Panes(int index) =>
	[
		$"""records\xpack3\ui\skills\mastery {index}\panectrl.dbr""",
		$"""records\xpack\ui\skills\mastery {index}\panectrl.dbr""",
		$"""records\ingameui\player skills\mastery {index}\panectrl.dbr""",
	];

	/// <inheritdoc/>
	public MasteryTree? GetMasteryTree(Masteries mastery)
		=> this._trees.GetOrAdd(mastery, this.ReadMasteryTree);

	private MasteryTree? ReadMasteryTree(Masteries mastery)
	{
		if (!PaneRecords.TryGetValue(mastery, out var panes))
			return null;

		var pane = panes
			.Select(p => this._database.GetRecordFromFile(p.ToRecordId()))
			.FirstOrDefault(r => r is not null);
		var buttonIds = pane?.GetAllStrings("tabSkillButtons");
		if (buttonIds is null)
			return null;

		var buttons = buttonIds
			.Where(b => !string.IsNullOrWhiteSpace(b))
			.Select(b => this._database.GetRecordFromFile(b.ToRecordId()))
			.OfType<DBRecordCollection>()
			.Select(b => (Skill: b.GetString("skillName", 0).ToRecordId(), X: b.GetInt32("bitmapPositionX", 0), Y: b.GetInt32("bitmapPositionY", 0)))
			.Where(b => !RecordId.IsNullOrEmpty(b.Skill))
			.ToList();

		DBRecordCollection? masteryRecord = null;
		RecordId? masteryId = null;
		var skills = new List<(SkillButton Button, DBRecordCollection Record)>();
		foreach (var button in buttons)
		{
			if (this._database.GetRecordFromFile(button.Skill) is not { } record)
				continue;

			if (record.GetString("Class", 0).Equals("Skill_Mastery", NoCase))
			{
				// The id as the pane spells it, the game's casing, is what a new skill entry of the save gets.
				masteryId ??= button.Skill;
				masteryRecord ??= record;
			}
			else
				skills.Add((new SkillButton(button.Skill, button.X, button.Y, IsModifier(record)), record));
		}

		if (masteryRecord is null || masteryId is null)
			return null;

		var buttonsOnly = skills.Select(s => s.Button).ToList();
		var masterySkills = skills
			.Select(s => this.ToMasterySkill(s.Record, s.Button, FindParent(s.Button, buttonsOnly)))
			.OrderBy(s => s.Tier)
			.ThenBy(s => buttonsOnly.First(b => b.Id == s.Id).X)
			.ToList();

		return new MasteryTree(
			mastery
			, masteryId
			, masteryRecord.GetString("skillDisplayName", 0)
			, masteryRecord.GetString("skillBaseDescription", 0)
			, masteryRecord.GetString("skillUpBitmapName", 0)
			, masteryRecord.GetInt32("skillMaxLevel", 0)
			, masterySkills);
	}

	/// <summary>A skill button of a pane.</summary>
	internal sealed record SkillButton(RecordId Id, int X, int Y, bool IsModifier);

	/// <summary>
	/// A modifier skill improves the skill it is connected to: <c>Skill_Modifier</c>, <c>Skill_ProjectileModifier</c>
	/// and the secondary skills (chain lightning, pet modifiers...).
	/// </summary>
	internal static bool IsModifier(DBRecordCollection record)
	{
		var cls = record.GetString("Class", 0);
		return cls.Equals("Skill_Modifier", NoCase)
			|| cls.Equals("Skill_ProjectileModifier", NoCase)
			|| cls.StartsWith("SkillSecondary_", NoCase);
	}

	/// <summary>
	/// The skill a modifier is connected to: the closest skill under it in its column of the pane that is not a modifier.
	/// </summary>
	internal static RecordId? FindParent(SkillButton button, IEnumerable<SkillButton> pane)
		=> !button.IsModifier
			? null
			: pane
				.Where(b => !b.IsModifier && b.X == button.X && b.Y > button.Y)
				.OrderBy(b => b.Y)
				.FirstOrDefault()?.Id;

	private MasterySkill ToMasterySkill(DBRecordCollection record, SkillButton button, RecordId? parentId)
	{
		// Skills that only cast a buff or a pet skill take their name, description and icon from it.
		var shown = record;
		foreach (var link in new[] { "buffSkillName", "petSkillName" })
		{
			var linkedId = record.GetString(link, 0).ToRecordId();
			if (!RecordId.IsNullOrEmpty(linkedId) && this._database.GetRecordFromFile(linkedId) is { } linked)
			{
				shown = linked;
				break;
			}
		}

		string First(string variable)
		{
			var value = shown.GetString(variable, 0);
			return value.Length != 0 ? value : record.GetString(variable, 0);
		}

		var maxLevel = record.GetInt32("skillMaxLevel", 0);
		if (maxLevel == 0)
			maxLevel = shown.GetInt32("skillMaxLevel", 0);

		return new MasterySkill(
			button.Id
			, First("skillDisplayName")
			, First("skillBaseDescription")
			, First("skillUpBitmapName")
			, maxLevel
			, SkillTiers.FromButtonY(button.Y)
			, parentId);
	}
}
