//-----------------------------------------------------------------------
// <copyright file="PlayerCollection.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Domain.Entities
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	/// <summary>
	/// Loads, decodes, encodes and saves a Titan Quest player file.
	/// </summary>
	public class PlayerCollection : IEnumerable<SackCollection>
	{
		/// <summary>
		/// Tell if the Vault succesfully load
		/// </summary>
		public bool VaultLoaded;

		/// <summary>
		/// Raised exception at loading time.
		/// </summary>
		public ArgumentException ArgumentException;

		/// <summary>
		/// String holding the player name
		/// </summary>
		private string playerName;

		/// <summary>
		/// Byte array holding the raw data from the file.
		/// </summary>
		public byte[] rawData;

		/// <summary>
		/// Number of sacks that this file holds
		/// </summary>
		public int numberOfSacks;

		/// <summary>
		/// Holds the currently focused sack
		/// </summary>
		/// <remarks>used to preseve right vault selected tab (Type = Vault only)</remarks>
		public int currentlyFocusedSackNumber;

		/// <summary>
		/// Holds the currently selected sack
		/// </summary>
		/// <remarks>used to preseve left vault selected tab (Type = Vault only)</remarks>
		public int currentlySelectedSackNumber;

		/// <summary>
		/// Holds the equipmentCtrlIOStreamVersion tag in the file.
		/// </summary>
		public int equipmentCtrlIOStreamVersion;

		/// <summary>
		/// Array of the sacks
		/// </summary>
		public SackCollection[] sacks;

		/// <summary>
		/// Holds the currently disabled tooltip bagId.
		/// </summary>
		public List<int> DisabledTooltipBagId = new();

		/// <summary>
		/// Initializes a new instance of the PlayerCollection class.
		/// </summary>
		/// <param name="playerName">Name of the player</param>
		/// <param name="playerFile">filename of the player file</param>
		public PlayerCollection(string playerName, string playerFile)
		{

			this.PlayerFile = playerFile;
			this.PlayerName = playerName;
		}

		public bool IsPlayer { get => this.PlayerFile.EndsWith("player.chr", System.StringComparison.InvariantCultureIgnoreCase); }

		/// <summary>
		/// Gets or sets a value indicating whether this file is a vault
		/// </summary>
		public bool IsVault { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this file is an immortal throne file
		/// </summary>
		public bool IsImmortalThrone { get; set; }

		/// <summary>
		/// Gets the equipment sack for this file.
		/// </summary>
		public SackCollection EquipmentSack { get; set; }

		/// <summary>
		/// Holds playerInfo
		/// </summary>
		public PlayerInfo PlayerInfo { get; set; }

		/// <summary>
		/// Gets the player file name
		/// </summary>
		public string PlayerFile { get; set; }

		/// <summary>
		/// Gets a value indicating whether this file has been modified.
		/// </summary>
		public bool IsModified
		{
			get
			{
				// look through each sack and see if the sack has been modified
				if (this.sacks != null)
				{
					foreach (SackCollection sack in this.sacks)
					{
						if (sack.IsModified)
							return true;
					}
				}

				if (this.EquipmentSack != null && this.EquipmentSack.IsModified)
					return true;

				if (this.PlayerInfo != null && this.PlayerInfo.Modified)
					return true;

				return false;
			}
		}

		/// <summary>
		/// Adjust internal status when the collection is saved
		/// </summary>
		public void Saved()
		{
			if (this.sacks != null)
			{
				foreach (SackCollection sack in this.sacks)
					sack.IsModified = false;
			}

			if (this.EquipmentSack != null && this.EquipmentSack.IsModified)
				this.EquipmentSack.IsModified = false;

			if (this.PlayerInfo != null && this.PlayerInfo.Modified)
				this.PlayerInfo.Modified = false;
		}

		/// <summary>
		/// Gets the player name
		/// </summary>
		public string PlayerName
		{
			get => (!this.IsVault && this.IsImmortalThrone) ? string.Concat(this.playerName, " - Immortal Throne") : this.playerName;
			private set => this.playerName = value;
		}

		/// <summary>
		/// Gets the number of sacks in this file
		/// </summary>
		public int NumberOfSacks
		{
			get => (this.sacks == null) ? 0 : this.sacks.Length;
		}

		/// <summary>
		/// Enumerator block to iterate all of the sacks in the Player
		/// </summary>
		/// <returns>Each Sack in the sack array.</returns>
		public IEnumerator<SackCollection> GetEnumerator()
		{
			if (this.sacks == null)
				yield break;

			foreach (SackCollection sack in this.sacks)
				yield return sack;
		}

		/// <summary>
		/// Non Generic enumerator interface.
		/// </summary>
		/// <returns>Generic interface implementation.</returns>
		IEnumerator IEnumerable.GetEnumerator()
			=> this.GetEnumerator();

		/// <summary>
		/// Creates empty sacks within the file.
		/// </summary>
		/// <param name="numberOfSacks">Number of sacks to create</param>
		public void CreateEmptySacks(int numberOfSacks)
		{
			this.sacks = new SackCollection[numberOfSacks];
			this.numberOfSacks = numberOfSacks;

			for (int i = 0; i < numberOfSacks; ++i)
			{
				this.sacks[i] = new SackCollection();
				this.sacks[i].IsModified = false;
			}
		}


		/// <summary>
		/// Gets a sack from the instance
		/// </summary>
		/// <param name="sackNumber">Number of the sack we are retrieving</param>
		/// <returns>Sack instace for the corresponding sack number</returns>
		public SackCollection GetSack(int sackNumber)
			=> (this.sacks == null || this.sacks.Length <= sackNumber) ? null : this.sacks[sackNumber];

		/// <summary>
		/// Moves a sack within the instance.  Used for renumbering the sacks.
		/// </summary>
		/// <param name="source">source sack number</param>
		/// <param name="destination">destination sack number</param>
		/// <returns>true if successful</returns>
		public bool MoveSack(int source, int destination)
		{
			// Do a little bit of error handling
			if (this.sacks == null
				|| destination < 0 || destination > this.sacks.Length
				|| source < 0 || source > this.sacks.Length || source == destination)
			{
				return false;
			}

			List<SackCollection> tmp = new List<SackCollection>(this.sacks.Length);

			// Copy the whole array first.
			foreach (SackCollection sack in this.sacks)
				tmp.Add(sack);

			// Now we can shuffle things around
			tmp.RemoveAt(source);
			tmp.Insert(destination, this.sacks[source]);
			this.sacks[source].IsModified = true;
			this.sacks[destination].IsModified = true;

			tmp.CopyTo(this.sacks);

			return true;
		}

		/// <summary>
		/// Copies a sack within the instance
		/// </summary>
		/// <param name="source">source sack number</param>
		/// <param name="destination">desintation sack number</param>
		/// <returns>true if successful</returns>
		public bool CopySack(int source, int destination)
		{
			// Do a little bit of error handling
			if (this.sacks == null
				|| destination < 0 || destination > this.sacks.Length
				|| source < 0 || source > this.sacks.Length || source == destination)
			{
				return false;
			}

			SackCollection newSack = this.sacks[source].Duplicate();

			if (newSack != null)
			{
				this.sacks[destination] = newSack;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Sets all of the bonuses from the equipped gear to 0.
		/// </summary>
		public void ClearPlayerGearBonuses()
		{
			if (PlayerInfo.GearBonus == null)
				return;

			PlayerInfo.GearBonus.StrengthBonus = 0;
			PlayerInfo.GearBonus.StrengthModifier = 0;

			PlayerInfo.GearBonus.DexterityBonus = 0;
			PlayerInfo.GearBonus.DexterityModifier = 0;

			PlayerInfo.GearBonus.IntelligenceBonus = 0;
			PlayerInfo.GearBonus.IntelligenceModifier = 0;

			PlayerInfo.GearBonus.HealthBonus = 0;
			PlayerInfo.GearBonus.HealthModifier = 0;

			PlayerInfo.GearBonus.ManaBonus = 0;
			PlayerInfo.GearBonus.ManaModifier = 0;
		}

		/// <summary>
		/// Sets all of the bonuses from the player sklills to 0.
		/// </summary>
		public void ClearPlayerSkillBonuses()
		{
			if (PlayerInfo.SkillBonus == null)
				return;

			PlayerInfo.SkillBonus.StrengthBonus = 0;
			PlayerInfo.SkillBonus.StrengthModifier = 0;

			PlayerInfo.SkillBonus.DexterityBonus = 0;
			PlayerInfo.SkillBonus.DexterityModifier = 0;

			PlayerInfo.SkillBonus.IntelligenceBonus = 0;
			PlayerInfo.SkillBonus.IntelligenceModifier = 0;

			PlayerInfo.SkillBonus.HealthBonus = 0;
			PlayerInfo.SkillBonus.HealthModifier = 0;

			PlayerInfo.SkillBonus.ManaBonus = 0;
			PlayerInfo.SkillBonus.ManaModifier = 0;
		}

		/// <summary>
		/// Updates GearBonus values based on the passed SortedList of bonus values.
		/// </summary>
		/// <param name="statBonusVariables">SortedList containing a list of the bonuses and values to be added.</param>
		public void UpdatePlayerGearBonuses(SortedList<string, int> statBonusVariables)
		{
			if (this.IsVault || this.PlayerInfo is null) return;

			if (PlayerInfo.GearBonus == null)
				PlayerInfo.GearBonus = new PlayerStatBonus();

			if (statBonusVariables.ContainsKey("CHARACTERSTRENGTH"))
				PlayerInfo.GearBonus.StrengthBonus += statBonusVariables["CHARACTERSTRENGTH"];
			if (statBonusVariables.ContainsKey("CHARACTERSTRENGTHMODIFIER"))
				PlayerInfo.GearBonus.StrengthModifier += statBonusVariables["CHARACTERSTRENGTHMODIFIER"];

			if (statBonusVariables.ContainsKey("CHARACTERDEXTERITY"))
				PlayerInfo.GearBonus.DexterityBonus += statBonusVariables["CHARACTERDEXTERITY"];
			if (statBonusVariables.ContainsKey("CHARACTERDEXTERITYMODIFIER"))
				PlayerInfo.GearBonus.DexterityModifier += statBonusVariables["CHARACTERDEXTERITYMODIFIER"];

			if (statBonusVariables.ContainsKey("CHARACTERINTELLIGENCE"))
				PlayerInfo.GearBonus.IntelligenceBonus += statBonusVariables["CHARACTERINTELLIGENCE"];
			if (statBonusVariables.ContainsKey("CHARACTERINTELLIGENCEMODIFIER"))
				PlayerInfo.GearBonus.IntelligenceModifier += statBonusVariables["CHARACTERINTELLIGENCEMODIFIER"];

			if (statBonusVariables.ContainsKey("CHARACTERLIFE"))
				PlayerInfo.GearBonus.HealthBonus += statBonusVariables["CHARACTERLIFE"];
			if (statBonusVariables.ContainsKey("CHARACTERLIFEMODIFIER"))
				PlayerInfo.GearBonus.HealthModifier += statBonusVariables["CHARACTERLIFEMODIFIER"];

			if (statBonusVariables.ContainsKey("CHARACTERMANA"))
				PlayerInfo.GearBonus.ManaBonus += statBonusVariables["CHARACTERMANA"];
			if (statBonusVariables.ContainsKey("CHARACTERMANAMODIFIER"))
				PlayerInfo.GearBonus.ManaModifier += statBonusVariables["CHARACTERMANAMODIFIER"];
		}

		/// <summary>
		/// Updates SkillBonus values based on the passed SortedList of bonus values.
		/// </summary>
		/// <param name="statBonusVariables">SortedList containing a list of the bonuses and values to be added.</param>
		public void UpdatePlayerSkillBonuses(SortedList<string, int> skillStatBonusVariables)
		{
			if (this.IsVault || this.PlayerInfo is null) return;

			if (PlayerInfo.SkillBonus == null)
				PlayerInfo.SkillBonus = new PlayerStatBonus();

			if (skillStatBonusVariables.ContainsKey("CHARACTERSTRENGTH"))
				PlayerInfo.SkillBonus.StrengthBonus += skillStatBonusVariables["CHARACTERSTRENGTH"];
			if (skillStatBonusVariables.ContainsKey("CHARACTERSTRENGTHMODIFIER"))
				PlayerInfo.SkillBonus.StrengthModifier += skillStatBonusVariables["CHARACTERSTRENGTHMODIFIER"];

			if (skillStatBonusVariables.ContainsKey("CHARACTERDEXTERITY"))
				PlayerInfo.SkillBonus.DexterityBonus += skillStatBonusVariables["CHARACTERDEXTERITY"];
			if (skillStatBonusVariables.ContainsKey("CHARACTERDEXTERITYMODIFIER"))
				PlayerInfo.SkillBonus.DexterityModifier += skillStatBonusVariables["CHARACTERDEXTERITYMODIFIER"];

			if (skillStatBonusVariables.ContainsKey("CHARACTERINTELLIGENCE"))
				PlayerInfo.SkillBonus.IntelligenceBonus += skillStatBonusVariables["CHARACTERINTELLIGENCE"];
			if (skillStatBonusVariables.ContainsKey("CHARACTERINTELLIGENCEMODIFIER"))
				PlayerInfo.SkillBonus.IntelligenceModifier += skillStatBonusVariables["CHARACTERINTELLIGENCEMODIFIER"];

			if (skillStatBonusVariables.ContainsKey("CHARACTERLIFE"))
				PlayerInfo.SkillBonus.HealthBonus += skillStatBonusVariables["CHARACTERLIFE"];
			if (skillStatBonusVariables.ContainsKey("CHARACTERLIFEMODIFIER"))
				PlayerInfo.SkillBonus.HealthModifier += skillStatBonusVariables["CHARACTERLIFEMODIFIER"];

			if (skillStatBonusVariables.ContainsKey("CHARACTERMANA"))
				PlayerInfo.SkillBonus.ManaBonus += skillStatBonusVariables["CHARACTERMANA"];
			if (skillStatBonusVariables.ContainsKey("CHARACTERMANAMODIFIER"))
				PlayerInfo.SkillBonus.ManaModifier += skillStatBonusVariables["CHARACTERMANAMODIFIER"];
		}

		const string Key_LevelRequirement = "LevelRequirement";
		const string Key_Strength = "Strength";
		const string Key_Dexterity = "Dexterity";
		const string Key_Intelligence = "Intelligence";

		public bool IsPlayerMeetRequierements(SortedList<string, Variable> requirementVariables)
		{
			if (this.IsVault || this.PlayerInfo is null) return true;

			// "LevelRequirement"
			int LevelRequirement = 0;
			if (requirementVariables.ContainsKey(Key_LevelRequirement))
				LevelRequirement = requirementVariables[Key_LevelRequirement].GetInt32();

			// "Strength"
			int Strength = 0;
			if (requirementVariables.ContainsKey(Key_Strength))
				Strength = requirementVariables[Key_Strength].GetInt32();

			// Dexterity
			int Dexterity = 0;
			if (requirementVariables.ContainsKey(Key_Dexterity))
				Dexterity = requirementVariables[Key_Dexterity].GetInt32();

			// Intelligence
			int Intelligence = 0;
			if (requirementVariables.ContainsKey(Key_Intelligence))
				Intelligence = requirementVariables[Key_Intelligence].GetInt32();

			return
				LevelRequirement <= this.PlayerInfo.CurrentLevel
				&& Strength <= this.PlayerInfo.CalculatedStrength
				&& Dexterity <= this.PlayerInfo.CalculatedDexterity
				&& Intelligence <= this.PlayerInfo.CalculatedIntelligence
			;
		}
	}
}