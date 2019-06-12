//-----------------------------------------------------------------------
// <copyright file="Stash.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Entities
{
	/// <summary>
	/// Class for handling the stash file
	/// </summary>
	public class Stash
	{

		/// <summary>
		/// Player name associated with this stash file.
		/// </summary>
		private string playerName;

		/// <summary>
		/// Raw file data
		/// </summary>
		public byte[] rawData;

		/// <summary>
		/// Binary marker for begin block
		/// </summary>
		public int beginBlockCrap;

		/// <summary>
		/// The number of sacks in this stash file
		/// </summary>
		public int numberOfSacks;

		/// <summary>
		/// Stash file version
		/// </summary>
		public int stashVersion;

		/// <summary>
		/// Raw data holding the name.
		/// Changed to raw data to support extended characters
		/// </summary>
		public byte[] name;

		/// <summary>
		/// Sack instance for this file
		/// </summary>
		public SackCollection sack;

		/// <summary>
		/// Initializes a new instance of the Stash class.
		/// </summary>
		/// <param name="playerName">Name of the player</param>
		/// <param name="stashFile">name of the stash file</param>
		public Stash(string playerName, string stashFile)
		{
			this.StashFile = stashFile;
			this.PlayerName = playerName;
			this.IsImmortalThrone = true;
			this.numberOfSacks = 2;
		}

		/// <summary>
		/// Creates an empty sack
		/// </summary>
		public void CreateEmptySack()
		{
			this.sack = new SackCollection();
			this.sack.IsModified = false;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this is from Immortal Throne
		/// </summary>
		/// <remarks>
		/// This really should always be true since stashes are not supported without Immortal Throne.
		/// </remarks>
		public bool IsImmortalThrone { get; set; }

		/// <summary>
		/// Gets a value indicating whether this file has been modified
		/// </summary>
		public bool IsModified
		{
			get
			{
				if (this.sack != null)
				{
					if (this.sack.IsModified)
					{
						return true;
					}
				}

				return false;
			}
		}

		/// <summary>
		/// Gets the height of the stash sack
		/// </summary>
		public int Height { get; set; }

		/// <summary>
		/// Gets the width of the stash sack
		/// </summary>
		public int Width { get; set; }

		/// <summary>
		/// Gets the player name associated with this stash
		/// </summary>
		public string PlayerName
		{
			get
			{
				if (this.IsImmortalThrone)
				{
					return string.Concat(this.playerName, " - Immortal Throne");
				}
				else
				{
					return this.playerName;
				}
			}

			private set
			{
				this.playerName = value;
			}
		}

		/// <summary>
		/// Gets the stash file name
		/// </summary>
		public string StashFile { get; private set; }

		/// <summary>
		/// Gets the number of sack contained in this stash
		/// </summary>
		public int NumberOfSacks
		{
			get
			{
				if (this.sack == null)
				{
					return 0;
				}

				return this.numberOfSacks;
			}
		}

		/// <summary>
		/// Gets the current sack instance
		/// </summary>
		public SackCollection Sack
		{
			get
			{
				return this.sack;
			}
		}

	}
}