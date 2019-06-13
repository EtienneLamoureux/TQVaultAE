using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TQVaultAE.DAL
{
	/// <summary>
	/// Character data is saved here after reading through player.chr file
	/// </summary>
	public class PlayerInfo
	{
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

	}
}
