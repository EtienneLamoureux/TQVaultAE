using System;
using System.Collections.Generic;
using System.Text;

namespace TQVaultAE.Domain.Entities
{
	public class PlayerStatBonus
	{
		/// <summary>
		/// Flat bonus to Character Strength
		/// </summary>
		public int StrengthBonus { get; set; }

		/// <summary>
		/// Percentage bonus to Character Strength
		/// </summary>
		public float StrengthModifier { get; set; }

		/// <summary>
		/// Flat bonus to Character Dexterity
		/// </summary>
		public int DexterityBonus { get; set; }

		/// <summary>
		/// Percentage bonus to Character Dexterity
		/// </summary>
		public float DexterityModifier { get; set; }

		/// <summary>
		/// Flat bonus to Character Intelligence
		/// </summary>
		public int IntelligenceBonus { get; set; }

		/// <summary>
		/// Percentage bonus to Character Intelligence
		/// </summary>
		public float IntelligenceModifier { get; set; }

		/// <summary>
		/// Flat bonus to Character Health
		/// </summary>
		public int HealthBonus { get; set; }

		/// <summary>
		/// Percentage bonus to Character Health
		/// </summary>
		public float HealthModifier { get; set; }

		/// <summary>
		/// Flat bonus to Character Mana
		/// </summary>
		public int ManaBonus { get; set; }

		/// <summary>
		/// Percentage bonus to Character Mana
		/// </summary>
		public float ManaModifier { get; set; }

	}
}
