using EnumsNET;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application;

/// <summary>
/// Character data is saved here after reading through player.chr file
/// </summary>
public class PlayerInfo
{
	private readonly IPathIO PathIO;
	const StringComparison noCase = StringComparison.OrdinalIgnoreCase;

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

	public bool MasteryDefensiveEnabled => this.SkillRecordList.Any(s => s.skillName.Equals(Masteries.Defensive.AsString(EnumFormat.Description), noCase));
	public bool MasteryStormEnabled => this.SkillRecordList.Any(s => s.skillName.Equals(Masteries.Storm.AsString(EnumFormat.Description), noCase));
	public bool MasteryEarthEnabled => this.SkillRecordList.Any(s => s.skillName.Equals(Masteries.Earth.AsString(EnumFormat.Description), noCase));
	public bool MasteryNatureEnabled => this.SkillRecordList.Any(s => s.skillName.Equals(Masteries.Nature.AsString(EnumFormat.Description), noCase));
	public bool MasteryDreamEnabled => this.SkillRecordList.Any(s => s.skillName.Equals(Masteries.Dream.AsString(EnumFormat.Description), noCase));
	public bool MasteryRuneEnabled => this.SkillRecordList.Any(s => s.skillName.Equals(Masteries.Rune.AsString(EnumFormat.Description), noCase));
	public bool MasteryWarfareEnabled => this.SkillRecordList.Any(s => s.skillName.Equals(Masteries.Warfare.AsString(EnumFormat.Description), noCase));
	public bool MasteryHuntingEnabled => this.SkillRecordList.Any(s => s.skillName.Equals(Masteries.Hunting.AsString(EnumFormat.Description), noCase));
	public bool MasteryStealthEnabled => this.SkillRecordList.Any(s => s.skillName.Equals(Masteries.Stealth.AsString(EnumFormat.Description), noCase));
	public bool MasterySpiritEnabled => this.SkillRecordList.Any(s => s.skillName.Equals(Masteries.Spirit.AsString(EnumFormat.Description), noCase));
	public bool MasteryNeidanEnabled => this.SkillRecordList.Any(s => s.skillName.Equals(Masteries.Neidan.AsString(EnumFormat.Description), noCase));

	public string[] ActiveMasteriesRecordNames
	{
		get
		{
			string[] ret = [];
			if (ActiveMasteries.HasValue)
			{
				var actives = ActiveMasteries.Value;
				ret = Enums.GetValues<Masteries>().Where(v => actives.HasFlag(v)).Select(s => s.AsString(EnumFormat.Description)!).ToArray();
			}
			return ret;
		}
	}

	public Masteries? ActiveMasteries
	{
		get
		{
			Masteries? val = null;
			int m = 0;
			m = MasteryDefensiveEnabled ? m | (int)Masteries.Defensive : m;
			m = MasteryStormEnabled ? m | (int)Masteries.Storm : m;
			m = MasteryEarthEnabled ? m | (int)Masteries.Earth : m;
			m = MasteryNatureEnabled ? m | (int)Masteries.Nature : m;
			m = MasteryDreamEnabled ? m | (int)Masteries.Dream : m;
			m = MasteryRuneEnabled ? m | (int)Masteries.Rune : m;
			m = MasteryWarfareEnabled ? m | (int)Masteries.Warfare : m;
			m = MasteryHuntingEnabled ? m | (int)Masteries.Hunting : m;
			m = MasteryStealthEnabled ? m | (int)Masteries.Stealth : m;
			m = MasterySpiritEnabled ? m | (int)Masteries.Spirit : m;
			m = MasteryNeidanEnabled ? m | (int)Masteries.Neidan : m;
			if (m > 0) val = (Masteries)m;
			return val;
		}
	}

	/// <summary>
	/// The bonuses of the skills that count in the character window (learned and granted by the equipped items, no buff),
	/// summed by variable as the statistics panel sums them; <see langword="null"/> until computed.
	/// </summary>
	public AttributeSum? SkillBonus { get; set; }

	/// <summary>
	/// The bonuses of the equipped items and their item sets (not the weapon set not in hand), summed by variable as the
	/// statistics panel sums them; <see langword="null"/> until computed.
	/// </summary>
	public AttributeSum? GearBonus { get; set; }

	/// <summary>
	/// An attribute with the bonuses of the gear and the skills, as the game's character window shows it:
	/// <see cref="CharacterPanelLayout.AttributeTotal"/> of the unrounded sums of <paramref name="field"/> and its
	/// <c>Modifier</c> (%).
	/// </summary>
	private int Calculated(int baseValue, string field)
		=> CharacterPanelLayout.AttributeTotal(baseValue,
			(this.GearBonus?[field] ?? 0) + (this.SkillBonus?[field] ?? 0),
			(this.GearBonus?[field + "Modifier"] ?? 0) + (this.SkillBonus?[field + "Modifier"] ?? 0));

	/// <summary>
	/// Released skill points from masteries reset
	/// </summary>
	public int ReleasedSkillPoints
		=> this._SkillRecordListRemoved?.Sum(s => s.skillLevel) ?? 0;

	/// <summary>
	/// Reset masteries if any
	/// </summary>
	/// <returns></returns>
	public bool ResetMasteries()
	{
		bool isActive = ActiveMasteries.HasValue;
		if (isActive)
		{
			foreach (var mastery in ActiveMasteriesRecordNames)
			{
				var recBase = PathIO.GetDirectoryName(mastery);
				// Remove all skills having the same base skill line (ex :  storm, earth, etc...)
				this._SkillRecordListRemoved.AddRange(
					this.SkillRecordList
						.Where(sk => sk.skillName.StartsWith(recBase, noCase))
				);
			}
			this.SkillRecordList.RemoveAll(s => this._SkillRecordListRemoved.Contains(s));
			this.Modified = true;
		}
		return isActive;
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
	/// Return skills having the same skill line base.
	/// </summary>
	/// <param name="dbr"></param>
	/// <returns></returns>
	public IEnumerable<SkillRecord> GetSkillsByBaseRecordName(RecordId dbr)
	{
		var baseRec = PathIO.GetDirectoryName(dbr.Normalized);
		foreach (var sk in this.SkillRecordList)
		{
			if (sk.skillName.StartsWith(baseRec, noCase))
				yield return sk;
		}
	}

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

	public PlayerInfo(IPathIO pathIO)
	{
		PathIO = pathIO;
	}
}