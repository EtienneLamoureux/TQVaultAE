using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TQVaultAE.Data;
using TQVaultAE.Entities;
using TQVaultAE.GUI.Models;
using TQVaultAE.Logs;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Services
{
	public class StashService
	{
		private readonly log4net.ILog Log = null;
		private readonly SessionContext userContext = null;

		public StashService(SessionContext userContext)
		{
			Log = Logger.Get(this);
			this.userContext = userContext;
		}


		public class LoadTransferStashResult
		{
			public string TransferStashFile;
			public Stash Stash;
			public bool? StashPresent;
			internal ArgumentException ArgumentException;
		}

		/// <summary>
		/// Loads the transfer stash for immortal throne
		/// </summary>
		public LoadTransferStashResult LoadTransferStash()
		{
			var result = new LoadTransferStashResult();

			result.TransferStashFile = TQData.TransferStashFile;

			try
			{
				result.Stash = this.userContext.Stashes[result.TransferStashFile];
			}
			catch (KeyNotFoundException)
			{
				result.Stash = new Stash(Resources.GlobalTransferStash, result.TransferStashFile);
				result.Stash.IsImmortalThrone = true;

				try
				{
					result.StashPresent = StashProvider.LoadFile(result.Stash);
					if (result.StashPresent.Value)
						this.userContext.Stashes.Add(result.TransferStashFile, result.Stash);
				}
				catch (ArgumentException argumentException)
				{
					result.ArgumentException = argumentException;
				}
			}

			return result;
		}

		public class LoadRelicVaultStashResult
		{
			public string RelicVaultStashFile;
			public Stash Stash;
			public bool? StashPresent;
			public ArgumentException ArgumentException;
		}

		/// <summary>
		/// Loads the relic vault stash
		/// </summary>
		public LoadRelicVaultStashResult LoadRelicVaultStash()
		{
			var result = new LoadRelicVaultStashResult();

			result.RelicVaultStashFile = TQData.RelicVaultStashFile;

			// Get the relic vault stash
			try
			{
				result.Stash = this.userContext.Stashes[result.RelicVaultStashFile];
			}
			catch (KeyNotFoundException)
			{
				result.Stash = new Stash(Resources.GlobalRelicVaultStash, result.RelicVaultStashFile);
				result.Stash.IsImmortalThrone = true;

				try
				{
					result.StashPresent = StashProvider.LoadFile(result.Stash);
					result.Stash.Sack.StashType = SackType.RelicVaultStash;
					this.userContext.Stashes.Add(result.RelicVaultStashFile, result.Stash);
				}
				catch (ArgumentException argumentException)
				{
					result.ArgumentException = argumentException;
				}
			}

			return result;
		}

		/// <summary>
		/// Attempts to save all modified stash files.
		/// </summary>
		/// <param name="stashOnError"></param>
		/// <exception cref="IOException">can happen during file save</exception>
		public void SaveAllModifiedStashes(ref Stash stashOnError)
		{
			// Save each stash as necessary
			foreach (KeyValuePair<string, Stash> kvp in this.userContext.Stashes)
			{
				string stashFile = kvp.Key;
				Stash stash = kvp.Value;

				if (stash == null) continue;

				if (stash.IsModified)
				{
					stashOnError = stash;
					TQData.BackupFile(stash.PlayerName, stashFile);
					StashProvider.Save(stash, stashFile);
				}
			}

		}

	}
}
