//-----------------------------------------------------------------------
// <copyright file="PlayerCollection.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application;

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
	public ArgumentException? ArgumentException;

	/// <summary>
	/// String holding the player name
	/// </summary>
	private string playerName = string.Empty;

	/// <summary>
	/// Byte array holding the raw data from the file.
	/// </summary>
	public byte[]? rawData;

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
	public SackCollection[]? Sacks;

	/// <summary>
	/// Holds the currently disabled tooltip bagId.
	/// </summary>
	public List<int> DisabledTooltipBagId = [];

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

	public bool IsPlayer { get => this.PlayerFile.EndsWith("player.chr", System.StringComparison.OrdinalIgnoreCase); }

	/// <summary>
	/// Gets or sets a value indicating whether this file is a vault
	/// </summary>
	public bool IsVault { get; set; }

	/// <summary>
	/// For a vault, the mod it is linked to (the mod's folder name): its items that have no mod of their own resolve with
	/// that mod's data first, then the base game's. Null for a vault of base game items.
	/// </summary>
	public string? Mod { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this file is an immortal throne file
	/// </summary>
	public bool IsImmortalThrone { get; set; }

	/// <summary>
	/// Gets the equipment sack for this file.
	/// </summary>
	public SackCollection? EquipmentSack { get; set; }

	/// <summary>
	/// Gets or sets whether the second weapon set (rows 2 and 3 of the weapon slots) is the one in hand: the save's
	/// <c>useAlternate</c> (<c>EquipmentCtrl</c>). Read only: the save keeps its own value.
	/// </summary>
	public bool UseAlternate { get; set; }

	/// <summary>
	/// Holds playerInfo
	/// </summary>
	public PlayerInfo? PlayerInfo { get; set; }

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
			if (this.Sacks is not null)
			{
				foreach (SackCollection sack in this.Sacks)
				{
					if (sack.IsModified)
						return true;
				}
			}

			if (this.EquipmentSack is not null && this.EquipmentSack.IsModified)
				return true;

			if (this.PlayerInfo is not null && this.PlayerInfo.Modified)
				return true;

			return false;
		}
	}

	/// <summary>
	/// Adjust internal status when the collection is saved
	/// </summary>
	public void Saved()
	{
		if (this.Sacks is not null)
		{
			foreach (SackCollection sack in this.Sacks)
				sack.IsModified = false;
		}

		if (this.EquipmentSack is not null && this.EquipmentSack.IsModified)
			this.EquipmentSack.IsModified = false;

		if (this.PlayerInfo is not null && this.PlayerInfo.Modified)
			this.PlayerInfo.Modified = false;
	}

	/// <summary>
	/// Gets the player name
	/// </summary>
	public string PlayerName
	{
		//get => (!this.IsVault && this.IsImmortalThrone) ? string.Concat(this.playerName, " - Immortal Throne") : this.playerName;
		get => this.playerName;
		private set => this.playerName = value;
	}

	/// <summary>
	/// Gets the number of sacks in this file
	/// </summary>
	public int NumberOfSacks
	{
		get => (this.Sacks is null) ? 0 : this.Sacks.Length;
	}

	/// <summary>
	/// Enumerator block to iterate all of the sacks in the Player
	/// </summary>
	/// <returns>Each Sack in the sack array.</returns>
	public IEnumerator<SackCollection> GetEnumerator()
	{
		if (this.Sacks is null)
			yield break;

		foreach (SackCollection sack in this.Sacks)
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
	[MemberNotNull(nameof(Sacks))]
	public void CreateEmptySacks(int numberOfSacks)
	{
		this.Sacks = new SackCollection[numberOfSacks];
		this.numberOfSacks = numberOfSacks;

		for (int i = 0; i < numberOfSacks; ++i)
		{
			this.Sacks[i] = new SackCollection();
			this.Sacks[i].IsModified = false;
		}
	}


	/// <summary>
	/// Gets a sack from the instance
	/// </summary>
	/// <param name="sackNumber">Number of the sack we are retrieving</param>
	/// <returns>Sack instace for the corresponding sack number</returns>
	public SackCollection? GetSack(int sackNumber)
		=> (this.Sacks is null || sackNumber < 0 || this.Sacks.Length <= sackNumber) ? null : this.Sacks[sackNumber];

	/// <summary>
	/// Moves a sack within the instance.  Used for renumbering the sacks.
	/// </summary>
	/// <param name="source">source sack number</param>
	/// <param name="destination">destination sack number</param>
	/// <returns>true if successful</returns>
	public bool MoveSack(int source, int destination)
	{
		// Do a little bit of error handling
		if (this.Sacks is null
		    || destination < 0 || destination > this.Sacks.Length
		    || source < 0 || source > this.Sacks.Length || source == destination)
		{
			return false;
		}

		List<SackCollection> tmp = new List<SackCollection>(this.Sacks.Length);

		// Copy the whole array first.
		foreach (SackCollection sack in this.Sacks)
			tmp.Add(sack);

		// Now we can shuffle things around
		tmp.RemoveAt(source);
		tmp.Insert(destination, this.Sacks[source]);
		this.Sacks[source].IsModified = true;
		this.Sacks[destination].IsModified = true;

		tmp.CopyTo(this.Sacks);

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
		if (this.Sacks is null
		    || destination < 0 || destination > this.Sacks.Length
		    || source < 0 || source > this.Sacks.Length || source == destination)
		{
			return false;
		}

		SackCollection newSack = this.Sacks[source].Duplicate();

		if (newSack is not null)
		{
			this.Sacks[destination] = newSack;
			return true;
		}

		return false;
	}

	/// <summary>
	/// Sets all of the bonuses from the equipped gear to 0.
	/// </summary>
	public void ClearPlayerGearBonuses()
	{
		if (PlayerInfo?.GearBonus is null)
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
		if (PlayerInfo?.SkillBonus is null)
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

		if (PlayerInfo.GearBonus is null)
			PlayerInfo.GearBonus = new PlayerStatBonus();

		PlayerInfo.GearBonus.StrengthBonus += statBonusVariables.GetValueOrDefault("CHARACTERSTRENGTH");
		PlayerInfo.GearBonus.StrengthModifier += statBonusVariables.GetValueOrDefault("CHARACTERSTRENGTHMODIFIER");

		PlayerInfo.GearBonus.DexterityBonus += statBonusVariables.GetValueOrDefault("CHARACTERDEXTERITY");
		PlayerInfo.GearBonus.DexterityModifier += statBonusVariables.GetValueOrDefault("CHARACTERDEXTERITYMODIFIER");

		PlayerInfo.GearBonus.IntelligenceBonus += statBonusVariables.GetValueOrDefault("CHARACTERINTELLIGENCE");
		PlayerInfo.GearBonus.IntelligenceModifier += statBonusVariables.GetValueOrDefault("CHARACTERINTELLIGENCEMODIFIER");

		PlayerInfo.GearBonus.HealthBonus += statBonusVariables.GetValueOrDefault("CHARACTERLIFE");
		PlayerInfo.GearBonus.HealthModifier += statBonusVariables.GetValueOrDefault("CHARACTERLIFEMODIFIER");

		PlayerInfo.GearBonus.ManaBonus += statBonusVariables.GetValueOrDefault("CHARACTERMANA");
		PlayerInfo.GearBonus.ManaModifier += statBonusVariables.GetValueOrDefault("CHARACTERMANAMODIFIER");
	}

	/// <summary>
	/// Updates SkillBonus values based on the passed SortedList of bonus values.
	/// </summary>
	/// <param name="statBonusVariables">SortedList containing a list of the bonuses and values to be added.</param>
	public void UpdatePlayerSkillBonuses(SortedList<string, int> skillStatBonusVariables)
	{
		if (this.IsVault || this.PlayerInfo is null) return;

		if (PlayerInfo.SkillBonus is null)
			PlayerInfo.SkillBonus = new PlayerStatBonus();

		PlayerInfo.SkillBonus.StrengthBonus += skillStatBonusVariables.GetValueOrDefault("CHARACTERSTRENGTH");
		PlayerInfo.SkillBonus.StrengthModifier += skillStatBonusVariables.GetValueOrDefault("CHARACTERSTRENGTHMODIFIER");

		PlayerInfo.SkillBonus.DexterityBonus += skillStatBonusVariables.GetValueOrDefault("CHARACTERDEXTERITY");
		PlayerInfo.SkillBonus.DexterityModifier += skillStatBonusVariables.GetValueOrDefault("CHARACTERDEXTERITYMODIFIER");

		PlayerInfo.SkillBonus.IntelligenceBonus += skillStatBonusVariables.GetValueOrDefault("CHARACTERINTELLIGENCE");
		PlayerInfo.SkillBonus.IntelligenceModifier += skillStatBonusVariables.GetValueOrDefault("CHARACTERINTELLIGENCEMODIFIER");

		PlayerInfo.SkillBonus.HealthBonus += skillStatBonusVariables.GetValueOrDefault("CHARACTERLIFE");
		PlayerInfo.SkillBonus.HealthModifier += skillStatBonusVariables.GetValueOrDefault("CHARACTERLIFEMODIFIER");

		PlayerInfo.SkillBonus.ManaBonus += skillStatBonusVariables.GetValueOrDefault("CHARACTERMANA");
		PlayerInfo.SkillBonus.ManaModifier += skillStatBonusVariables.GetValueOrDefault("CHARACTERMANAMODIFIER");
	}


	public bool IsPlayerMeetRequierements(SortedList<string, Variable> requirementVariables)
	{
		if (this.IsVault || this.PlayerInfo is null) return true;

		// "LevelRequirement"
		int LevelRequirement = requirementVariables.TryGetValue(Variable.KEY_LEVELREQ, out Variable? levelRequirementVariable) ? levelRequirementVariable.GetInt32() : 0;

		// "Strength"
		int Strength = requirementVariables.TryGetValue(Variable.KEY_STRENGTH, out Variable? strengthVariable) ? strengthVariable.GetInt32() : 0;

		// Dexterity
		int Dexterity = requirementVariables.TryGetValue(Variable.KEY_DEXTERITY, out Variable? dexterityVariable) ? dexterityVariable.GetInt32() : 0;

		// Intelligence
		int Intelligence = requirementVariables.TryGetValue(Variable.KEY_INTELLIGENCE, out Variable? intelligenceVariable) ? intelligenceVariable.GetInt32() : 0;

		return
			LevelRequirement <= this.PlayerInfo.CurrentLevel
			&& Strength <= this.PlayerInfo.CalculatedStrength
			&& Dexterity <= this.PlayerInfo.CalculatedDexterity
			&& Intelligence <= this.PlayerInfo.CalculatedIntelligence
			;
	}
}