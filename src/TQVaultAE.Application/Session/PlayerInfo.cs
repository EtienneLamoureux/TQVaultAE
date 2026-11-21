using TQVaultAE.Application.Layout;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Application.Session;

/// <summary>
/// Character data is saved here after reading through player.chr file
/// </summary>
public class PlayerInfo
{
	public bool MustResetMasteries =>
		this.MasteriesAllowed_OldValue.HasValue && this.MasteriesAllowed < this.MasteriesAllowed_OldValue
		|| this.MasteriesResetRequiered;

	public bool MasteriesResetRequiered { get; set; }

	/// <summary>
	/// True when the skill list was edited: <see cref="SkillRecordList"/> is written back to the save.
	/// </summary>
	public bool SkillsChanged { get; set; }

	/// <summary>
	/// True when the masteries were changed: the class tag is cleared and the game sets it again.
	/// </summary>
	public bool MasteriesChanged { get; set; }

	/// <summary>
	/// Skills removed during <see cref="ResetMasteries"/>
	/// </summary>
	List<SkillRecord> _SkillRecordListRemoved = [];

	/// <summary>
	/// List of Skills embded in the save file
	/// </summary>
	public readonly List<SkillRecord> SkillRecordList = [];

	/// <summary>
	/// The buffs the save keeps from one game to the next (<c>buffSkillName</c> after <c>hasSkillServices</c>): the
	/// temple curses. Read only: the save keeps its own copy.
	/// </summary>
	public List<RecordId> SavedBuffs { get; } = [];

	/// <summary>
	/// The masteries the character's game data describes (<see cref="MasteryTree.AllOf"/>, read with its own map when the
	/// save is read): a mod's masteries are told by their own records.
	/// </summary>
	public IReadOnlyList<MasteryTree> MasteryTrees { get; set; } = [];

	/// <summary>The masteries the character has: those of <see cref="MasteryTrees"/> whose record is in its skills.</summary>
	public IReadOnlyList<MasteryTree> ActiveMasteryTrees
		=> [.. this.MasteryTrees.Where(t => this.SkillRecordList.Any(s => s.skillName.ToRecordId() == t.Id))];

	/// <summary>The masteries the character has, <see langword="null"/> when none.</summary>
	public Masteries? ActiveMasteries
		=> this.ActiveMasteryTrees.Aggregate((Masteries)0, (all, t) => all | t.Mastery) is var m and not 0 ? m : null;

	/// <summary>
	/// The bonuses of the skills that count in the character window (learned and granted by the equipped items, no buff),
	/// summed by variable as the statistics panel sums them; <see langword="null"/> until computed.
	/// </summary>
	public AttributeSum? SkillBonus { get; set; }

	/// <summary>
	/// The bonuses of the attached equipped items and their item sets (not the weapon set not in hand, nor a detached
	/// piece), summed by variable as the statistics panel sums them; <see langword="null"/> until computed.
	/// </summary>
	public AttributeSum? GearBonus { get; set; }

	/// <summary>
	/// The worn pieces whose requirements fail: in their slots but detached, giving nothing (<see cref="EquipmentState"/>);
	/// set with <see cref="GearBonus"/>.
	/// </summary>
	public IReadOnlyList<Item> DetachedEquipment { get; set; } = [];

	/// <summary>
	/// An attribute with the bonuses of the gear and the skills, as the game's character window shows it:
	/// <see cref="CharacterPanelLayout.AttributeTotal"/> of the unrounded sums of <paramref name="field"/> and its
	/// <c>Modifier</c> (%).
	/// </summary>
	private int Calculated(int baseValue, string field) => Total(baseValue, field, this.GearBonus, this.SkillBonus);

	/// <summary>
	/// An attribute with the bonuses of <paramref name="gear"/> and <paramref name="skills"/>, as the game's character
	/// window shows it (<see cref="CalculatedStrength"/>, for another build: a class set shown in a read-only panel).
	/// </summary>
	public static int Total(int baseValue, string field, AttributeSum? gear, AttributeSum? skills)
		=> CharacterPanelLayout.AttributeTotal(baseValue,
			(gear?[field] ?? 0) + (skills?[field] ?? 0),
			(gear?[field + "Modifier"] ?? 0) + (skills?[field + "Modifier"] ?? 0));

	/// <summary>
	/// Released skill points from masteries reset
	/// </summary>
	public int ReleasedSkillPoints
		=> this._SkillRecordListRemoved?.Sum(s => s.skillLevel) ?? 0;

	/// <summary>
	/// Removes the masteries and the skills of their panes (<see cref="ActiveMasteryTrees"/>), wherever their records are.
	/// </summary>
	/// <returns>False when the character has no mastery.</returns>
	public bool ResetMasteries()
	{
		var actives = this.ActiveMasteryTrees;
		if (actives.Count == 0)
			return false;

		this._SkillRecordListRemoved.AddRange(
			this.SkillRecordList.Where(s => actives.Any(t => t.Contains(s.skillName.ToRecordId()))));
		this.SkillRecordList.RemoveAll(this._SkillRecordListRemoved.Contains);
		this.Modified = true;
		return true;
	}

	/// <summary>
	/// Character current Level
	/// </summary>
	public int CurrentLevel { get; set; }

	/// <summary>
	/// Characters current XP
	/// </summary>
	public int CurrentXP { get; set; }

	/// <summary>
	/// Available Skill points
	/// </summary>
	public int SkillPoints { get; set; }

	/// <summary>
	/// Available Attribute points
	/// </summary>
	public int AttributesPoints { get; set; }

	/// <summary>
	/// Base Strength
	/// </summary>
	public int BaseStrength { get; set; }

	/// <summary>
	/// Character Strength with the bonuses of the gear and the skills, as the game shows it.
	/// </summary>
	public int CalculatedStrength => this.Calculated(this.BaseStrength, "characterStrength");

	/// <summary>
	/// Base Dexterity
	/// </summary>
	public int BaseDexterity { get; set; }

	/// <summary>
	/// Character Dexterity with the bonuses of the gear and the skills, as the game shows it.
	/// </summary>
	public int CalculatedDexterity => this.Calculated(this.BaseDexterity, "characterDexterity");

	/// <summary>
	/// Base Intelligence
	/// </summary>
	public int BaseIntelligence { get; set; }

	/// <summary>
	/// Character Intelligence with the bonuses of the gear and the skills, as the game shows it.
	/// </summary>
	public int CalculatedIntelligence => this.Calculated(this.BaseIntelligence, "characterIntelligence");

	/// <summary>
	/// Base Health
	/// </summary>
	public int BaseHealth { get; set; }

	/// <summary>
	/// Character Health with the bonuses of the gear and the skills, as the game shows it.
	/// </summary>
	public int CalculatedHealth => this.Calculated(this.BaseHealth, "characterLife");

	/// <summary>
	/// Base Mana
	/// </summary>
	public int BaseMana { get; set; }

	/// <summary>
	/// Character Mana with the bonuses of the gear and the skills, as the game shows it.
	/// </summary>
	public int CalculatedMana => this.Calculated(this.BaseMana, "characterMana");

	/// <summary>
	/// Total time played in seconds
	/// </summary>
	public int PlayTimeInSeconds { get; set; }


	/// <summary>
	/// Number of death
	/// </summary>
	public int NumberOfDeaths { get; set; }


	/// <summary>
	/// Number of Kills
	/// </summary>
	public int NumberOfKills { get; set; }

	/// <summary>
	/// Experience gained from kills
	/// </summary>
	public int ExperienceFromKills { get; set; }

	/// <summary>
	/// Health Postions Used
	/// </summary>
	public int HealthPotionsUsed { get; set; }

	/// <summary>
	/// Mana potions used
	/// </summary>
	public int ManaPotionsUsed { get; set; }

	public int MaxLevel { get; set; }


	/// <summary>
	/// Number of times hit
	/// </summary>
	public int NumHitsReceived { get; set; }

	/// <summary>
	/// Number of Hits Inflicted
	/// </summary>
	public int NumHitsInflicted { get; set; }

	/// <summary>
	/// Greatest Damage
	/// </summary>
	public int GreatestDamageInflicted { get; set; }

	/// <summary>
	/// Greatest Monster killed
	/// </summary>
	public string GreatestMonster { get; set; } = string.Empty;

	/// <summary>
	/// Critical Hits Inflicted
	/// </summary>
	public int CriticalHitsInflicted { get; set; }

	/// <summary>
	/// Critical Hits Recieved
	/// </summary>
	public int CriticalHitsReceived { get; set; }

	/// <summary>
	/// Character Class
	/// </summary>
	public string? Class { get; set; }

	/// <summary>
	/// The tag of the class: the save's (<see cref="Class"/>), else the one the masteries make. A change of masteries clears
	/// the save's and the game sets it again when it loads the character; until then the masteries tell it
	/// (<see cref="ActiveMasteries"/>, a mod's too).
	/// </summary>
	public string? ClassTag
		=> !string.IsNullOrEmpty(this.Class) ? this.Class
			: this.ActiveMasteries is { } masteries ? CharacterClasses.TagOf(masteries) ?? this.Class
			: this.Class;

	/// <summary>
	/// Character difficulty unlocked
	/// </summary>
	public int DifficultyUnlocked { get; set; }

	/// <summary>
	/// Has used player in game one or more times
	/// </summary>
	public int HasBeenInGame { get; set; }

	/// <summary>
	/// set to true if player information is edited
	/// </summary>
	public bool Modified { get; set; }

	/// <summary>
	/// Players Money
	/// </summary>
	public int Money { get; set; }

	/// <summary>
	/// Electrum, the second currency of the Eternal Embers expansion (save key <c>altMoney</c>, right after <c>money</c>);
	/// <see langword="null"/> when the save has no such key (a character never saved by a game with Eternal Embers).
	/// Read only: never written back to the save.
	/// </summary>
	public int? Electrum { get; set; }

	int _MasteriesAllowed = 0;
	/// <summary>
	/// Nbr of masteries unlocked
	/// </summary>
	public int MasteriesAllowed
	{
		get => _MasteriesAllowed;
		set
		{
			if (!MasteriesAllowed_OldValue.HasValue) MasteriesAllowed_OldValue = value;
			_MasteriesAllowed = value;
		}
	}

	/// <summary>
	/// First set value of <see cref="MasteriesAllowed"/> 
	/// </summary>
	public int? MasteriesAllowed_OldValue { get; private set; }

	/// <summary>
	/// TQ, TQIT, TQITAE
	/// </summary>
	public PlayerFileHeaderVersion HeaderVersion { get; set; }

	/// <summary>
	/// Class name
	/// </summary>
	public string PlayerCharacterClass { get; set; } = string.Empty;

	/// <summary>
	/// The texture of the character's body (<c>playerTexture</c>, e.g. <c>Creatures\pc\male\malepc01_white.tex</c>):
	/// it tells the player template, male or female.
	/// </summary>
	public string PlayerTexture { get; set; } = string.Empty;

	/// <summary>True when the character uses the female template (its texture is under <c>pc\female</c>).</summary>
	public bool IsFemale => this.PlayerTexture.Contains("""\female\""", StringComparison.OrdinalIgnoreCase)
		|| this.PlayerTexture.Contains("/female/", StringComparison.OrdinalIgnoreCase);

	public int GreatestMonsterKilledLevel { get; set; }

	public int GreatestMonsterKilledLifeAndMana { get; set; }
}