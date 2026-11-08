using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Application;

/// <summary>
/// Edits the masteries and the skill points of a character for the character edit dialog, with the rules of the
/// game's skill window: each point costs one skill point, a skill needs the mastery level of its tier and a modifier
/// needs a point in its skill. Lowering a mastery or a skill resets the skills it locks and gives their points back.
/// </summary>
public sealed class CharacterSkillEditor
{
	private readonly IReadOnlyList<SkillRecord> _original;
	private readonly Func<Masteries, MasteryTree?> _getTree;
	private readonly MasteryTree?[] _slots;
	private readonly HashSet<RecordId> _originalMasteries;

	/// <summary>The level of every skill of the masteries chosen now or at the start; 0 when absent.</summary>
	private readonly Dictionary<RecordId, int> _levels = [];

	/// <summary>The skill levels at the start, to tell what changed.</summary>
	private readonly Dictionary<RecordId, int> _originalLevels = [];

	/// <summary>The trees whose skills are tracked: those chosen now or at the start.</summary>
	private readonly List<MasteryTree> _tracked = [];

	/// <summary>
	/// Initializes a new instance of the <see cref="CharacterSkillEditor"/> class.
	/// </summary>
	/// <param name="skillRecords">The skills of the save, left untouched.</param>
	/// <param name="masteriesAllowed">The number of masteries the character may have.</param>
	/// <param name="availablePoints">The skill points left to spend.</param>
	/// <param name="getTree">Gives the skills of a mastery, or <see langword="null"/> when the game has none.</param>
	public CharacterSkillEditor(IEnumerable<SkillRecord> skillRecords, int masteriesAllowed, int availablePoints, Func<Masteries, MasteryTree?> getTree)
	{
		this._original = skillRecords.ToList();
		this._getTree = getTree;
		this.AvailablePoints = availablePoints;

		var savedLevels = new Dictionary<RecordId, int>();
		var savedOrder = new Dictionary<RecordId, int>();
		foreach (var record in this._original)
		{
			var id = record.skillName.ToRecordId();
			if (savedLevels.TryAdd(id, record.skillLevel))
				savedOrder[id] = savedOrder.Count;
		}

		// The masteries in the order of the save, which is the order they were chosen.
		var chosen = Enum.GetValues<Masteries>()
			.Select(m => getTree(m))
			.OfType<MasteryTree>()
			.Where(t => savedLevels.ContainsKey(t.Id))
			.OrderBy(t => savedOrder[t.Id])
			.ToList();

		this._slots = new MasteryTree?[Math.Max(masteriesAllowed, chosen.Count)];
		for (int i = 0; i < chosen.Count; i++)
		{
			this._slots[i] = chosen[i];
			this.Track(chosen[i]);
		}

		foreach (var tree in this._tracked)
			foreach (var id in Ids(tree))
				if (savedLevels.TryGetValue(id, out var level))
					this._levels[id] = this._originalLevels[id] = level;

		this._originalMasteries = chosen.Select(t => t.Id).ToHashSet();
	}

	/// <summary>The skill points left to spend.</summary>
	public int AvailablePoints { get; set; }

	/// <summary>The mastery of each slot, <see langword="null"/> for an empty slot.</summary>
	public IReadOnlyList<MasteryTree?> Slots => this._slots;

	/// <summary>The masteries the game database describes, to choose from.</summary>
	public IReadOnlyList<MasteryTree> KnownMasteries
		=> Enum.GetValues<Masteries>().Select(this._getTree).OfType<MasteryTree>().ToList();

	/// <summary>True when the set of masteries changed, which changes the class of the character.</summary>
	public bool MasteriesChanged
		=> !this._originalMasteries.SetEquals(this._slots.OfType<MasteryTree>().Select(t => t.Id));

	/// <summary>True when a mastery or a skill level changed.</summary>
	public bool Changed
		=> this.MasteriesChanged
			|| this._levels.Keys.Union(this._originalLevels.Keys).Any(id => this.GetLevel(id) != this._originalLevels.GetValueOrDefault(id));

	/// <summary>The level of a mastery or a skill, 0 when it has no point.</summary>
	public int GetLevel(RecordId skillId) => this._levels.GetValueOrDefault(skillId);

	/// <summary>The level of the mastery of a slot, 0 for an empty slot.</summary>
	public int GetMasteryLevel(int slot) => this._slots[slot] is { } tree ? this.GetLevel(tree.Id) : 0;

	/// <summary>The points spent in the mastery of a slot and its skills.</summary>
	public int GetPointsSpent(int slot) => this._slots[slot] is { } tree ? Ids(tree).Sum(this.GetLevel) : 0;

	/// <summary>True when a skill can take points: its tier is open and its parent skill has a point.</summary>
	public bool IsUnlocked(int slot, MasterySkill skill)
		=> this.GetMasteryLevel(slot) >= skill.RequiredMasteryLevel
			&& (skill.ParentId is null || this.GetLevel(skill.ParentId) > 0);

	/// <summary>
	/// Chooses the mastery of a slot, or empties it: the points of the mastery it replaces are given back, the new one
	/// starts at level 1.
	/// </summary>
	/// <returns>false, with nothing changed, when the mastery is already in another slot, unknown, or there is no
	/// point to spend on it.</returns>
	public bool SetMastery(int slot, Masteries? mastery)
	{
		var current = this._slots[slot];
		if (current?.Mastery == mastery)
			return true;

		MasteryTree? tree = null;
		if (mastery is { } m)
		{
			tree = this._getTree(m);
			if (tree is null || this._slots.Any(s => s?.Mastery == m))
				return false;

			if (this.AvailablePoints + this.GetPointsSpent(slot) < 1)
				return false;
		}

		if (current is not null)
			foreach (var id in Ids(current))
				this.ChangeLevel(id, 0);

		this._slots[slot] = tree;
		if (tree is not null)
		{
			this.Track(tree);
			this.ChangeLevel(tree.Id, 1);
		}

		return true;
	}

	/// <summary>
	/// Sets the level of the mastery of a slot, between 1 and its highest level and within the points left. Lowering it
	/// resets the skills of the tiers it closes.
	/// </summary>
	/// <returns>The level set.</returns>
	public int SetMasteryLevel(int slot, int level)
	{
		if (this._slots[slot] is not { } tree)
			return 0;

		var current = this.GetLevel(tree.Id);
		level = this.Affordable(current, Math.Clamp(level, 1, Math.Max(tree.MaxLevel, current)));
		this.ChangeLevel(tree.Id, level);

		if (level < current)
			this.ResetLockedSkills(slot);

		return level;
	}

	/// <summary>
	/// Sets the level of a skill of a slot, between 0 and its highest level and within the points left. A locked skill
	/// can only be lowered. Taking its last point resets its modifiers.
	/// </summary>
	/// <returns>The level set.</returns>
	public int SetSkillLevel(int slot, RecordId skillId, int level)
	{
		if (this._slots[slot]?.Skills.FirstOrDefault(s => s.Id == skillId) is not { } skill)
			return 0;

		var current = this.GetLevel(skillId);
		level = Math.Clamp(level, 0, Math.Max(skill.MaxLevel, current));
		if (level > current && !this.IsUnlocked(slot, skill))
			level = current;

		level = this.Affordable(current, level);
		this.ChangeLevel(skillId, level);

		if (level == 0 && current > 0)
			this.ResetLockedSkills(slot);

		return level;
	}

	/// <summary>
	/// Builds the skill list to save: the other skills (default attacks, quest rewards) and the unchanged ones stay as
	/// they are, at their place; a changed level is updated, a skill with no point left is removed and a new one is
	/// added at the end, after its mastery.
	/// </summary>
	public List<SkillRecord> BuildSkillRecords()
	{
		var tracked = this._tracked.SelectMany(Ids).ToHashSet();
		var result = new List<SkillRecord>();
		var kept = new HashSet<RecordId>();

		foreach (var record in this._original)
		{
			var id = record.skillName.ToRecordId();
			if (!tracked.Contains(id))
			{
				result.Add(record);
				continue;
			}

			var level = this.GetLevel(id);
			if (level == 0 || !kept.Add(id))
				continue;

			result.Add(level == record.skillLevel ? record : Copy(record, level));
		}

		foreach (var tree in this._slots.OfType<MasteryTree>())
			foreach (var id in Ids(tree))
				if (this.GetLevel(id) is var level and > 0 && kept.Add(id))
					result.Add(new SkillRecord { skillName = id.Raw, skillLevel = level, skillEnabled = 1 });

		return result;
	}

	private static SkillRecord Copy(SkillRecord record, int level) => new()
	{
		skillName = record.skillName,
		skillLevel = level,
		skillEnabled = record.skillEnabled,
		skillSubLevel = record.skillSubLevel,
		skillActive = record.skillActive,
		skillTransition = record.skillTransition,
	};

	/// <summary>The mastery record then its skills.</summary>
	private static IEnumerable<RecordId> Ids(MasteryTree tree)
		=> tree.Skills.Select(s => s.Id).Prepend(tree.Id);

	private void Track(MasteryTree tree)
	{
		if (!this._tracked.Any(t => t.Mastery == tree.Mastery))
			this._tracked.Add(tree);
	}

	/// <summary>Lowers a raise to what the points left pay for.</summary>
	private int Affordable(int current, int level)
		=> level > current ? Math.Min(level, current + this.AvailablePoints) : level;

	/// <summary>Sets a level and moves the difference of points to or from the points left.</summary>
	private void ChangeLevel(RecordId id, int level)
	{
		this.AvailablePoints -= level - this.GetLevel(id);
		if (level == 0)
			this._levels.Remove(id);
		else
			this._levels[id] = level;
	}

	/// <summary>Resets the skills of a slot that are locked, until none is: a reset skill can lock its modifiers.</summary>
	private void ResetLockedSkills(int slot)
	{
		if (this._slots[slot] is not { } tree)
			return;

		bool reset;
		do
		{
			reset = false;
			foreach (var skill in tree.Skills)
			{
				if (this.GetLevel(skill.Id) > 0 && !this.IsUnlocked(slot, skill))
				{
					this.ChangeLevel(skill.Id, 0);
					reset = true;
				}
			}
		}
		while (reset);
	}
}
