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
		public int currentlyFocusedSackNumber;

		/// <summary>
		/// Holds the currently selected sack
		/// </summary>
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
				&& Strength <= this.PlayerInfo.BaseStrength
				&& Dexterity <= this.PlayerInfo.BaseDexterity
				&& Intelligence <= this.PlayerInfo.BaseIntelligence
			;
		}
	}
}