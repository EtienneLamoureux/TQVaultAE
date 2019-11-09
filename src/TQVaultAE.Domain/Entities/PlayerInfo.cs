using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using EnumsNET;

namespace TQVaultAE.Domain.Entities
{
	/// <summary>
	/// Character data is saved here after reading through player.chr file
	/// </summary>
	public class PlayerInfo
	{
		public bool MasteriesResetRequiered { get; set; }

		/// <summary>
		/// Skills removed during <see cref="ResetMasteries"/>
		/// </summary>
		List<SkillRecord> _SkillRecordListRemoved = new List<SkillRecord>();

		/// <summary>
		/// List of Skills embded in the save file
		/// </summary>
		public readonly List<SkillRecord> SkillRecordList = new List<SkillRecord>();

		public bool MasteryDefensiveEnabled => this.SkillRecordList.Any(s => s.skillName.Equals(Masteries.Defensive.AsString(EnumFormat.Description), System.StringComparison.InvariantCultureIgnoreCase));
		public bool MasteryStormEnabled => this.SkillRecordList.Any(s => s.skillName.Equals(Masteries.Storm.AsString(EnumFormat.Description), System.StringComparison.InvariantCultureIgnoreCase));
		public bool MasteryEarthEnabled => this.SkillRecordList.Any(s => s.skillName.Equals(Masteries.Earth.AsString(EnumFormat.Description), System.StringComparison.InvariantCultureIgnoreCase));
		public bool MasteryNatureEnabled => this.SkillRecordList.Any(s => s.skillName.Equals(Masteries.Nature.AsString(EnumFormat.Description), System.StringComparison.InvariantCultureIgnoreCase));
		public bool MasteryDreamEnabled => this.SkillRecordList.Any(s => s.skillName.Equals(Masteries.Dream.AsString(EnumFormat.Description), System.StringComparison.InvariantCultureIgnoreCase));
		public bool MasteryRuneEnabled => this.SkillRecordList.Any(s => s.skillName.Equals(Masteries.Rune.AsString(EnumFormat.Description), System.StringComparison.InvariantCultureIgnoreCase));
		public bool MasteryWarfareEnabled => this.SkillRecordList.Any(s => s.skillName.Equals(Masteries.Warfare.AsString(EnumFormat.Description), System.StringComparison.InvariantCultureIgnoreCase));
		public bool MasteryHuntingEnabled => this.SkillRecordList.Any(s => s.skillName.Equals(Masteries.Hunting.AsString(EnumFormat.Description), System.StringComparison.InvariantCultureIgnoreCase));
		public bool MasteryStealthEnabled => this.SkillRecordList.Any(s => s.skillName.Equals(Masteries.Stealth.AsString(EnumFormat.Description), System.StringComparison.InvariantCultureIgnoreCase));
		public bool MasterySpiritEnabled => this.SkillRecordList.Any(s => s.skillName.Equals(Masteries.Spirit.AsString(EnumFormat.Description), System.StringComparison.InvariantCultureIgnoreCase));

		public string[] ActiveMasteriesRecordNames
		{
			get
			{
				string[] ret = new string[] { };
				if (ActiveMasteries.HasValue)
				{
					var actives = ActiveMasteries.Value;
					ret = Enums.GetValues<Masteries>().Where(v => actives.HasFlag(v)).Select(s => s.AsString(EnumFormat.Description)).ToArray();
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
				if (m > 0) val = (Masteries)m;
				return val;
			}
		}

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
					var recBase = Path.GetDirectoryName(mastery);
					// Remove all skills having the same base skill line (ex :  storm, earth, etc...)
					this._SkillRecordListRemoved.AddRange(
						this.SkillRecordList
						.Where(sk => sk.skillName.StartsWith(recBase, StringComparison.InvariantCultureIgnoreCase))
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
		/// Base Dexterity
		/// </summary>
		public int BaseDexterity { get; set; }

		/// <summary>
		/// Base Intelligence
		/// </summary>
		public int BaseIntelligence { get; set; }

		/// <summary>
		/// Base Health
		/// </summary>
		public int BaseHealth { get; set; }

		/// <summary>
		/// Base Mana
		/// </summary>
		public int BaseMana { get; set; }

		/// <summary>
		/// Return skills having the same skill line base.
		/// </summary>
		/// <param name="dbr"></param>
		/// <returns></returns>
		public IEnumerable<SkillRecord> GetSkillsByBaseRecordName(string dbr)
		{
			var baseRec = Path.GetDirectoryName(dbr);
			foreach (var sk in this.SkillRecordList)
			{
				if (sk.skillName.StartsWith(baseRec, StringComparison.InvariantCultureIgnoreCase))
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
		public string GreatestMonster { get; set; }

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
		public string Class { get; set; }

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
		public int HeaderVersion { get; set; }
		/// <summary>
		/// Class name
		/// </summary>
		public string PlayerCharacterClass { get; set; }

		public int GreatestMonsterKilledLevel { get; set; }
		public int GreatestMonsterKilledLifeAndMana { get; set; }
	}
}
