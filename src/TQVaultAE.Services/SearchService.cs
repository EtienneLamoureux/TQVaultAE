using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TQVaultAE.Entities;
using TQVaultAE.Logs;
using TQVaultAE.Presentation;
using TQVaultAE.Services.Models.Search;

namespace TQVaultAE.Services
{
	public class SearchService
	{
		private readonly log4net.ILog Log = null;
		private readonly SessionContext userContext = null;

		public SearchService(SessionContext userContext)
		{
			Log = Logger.Get(this);
			this.userContext = userContext;
		}

		/// <summary>
		/// Searches loaded files based on the specified search string.  Internally normalized to UpperInvariant
		/// </summary>
		/// <param name="searchString">string that we are searching for</param>
		public List<Result> Search(string searchString)
		{
			if (string.IsNullOrWhiteSpace(searchString)) return null;

			var filter = GetFilterFrom(searchString);
			var results = new List<Result>();
			this.SearchFiles(filter, results);

			return results;
		}

		private static IItemPredicate GetFilterFrom(string searchString)
		{
			var predicates = new List<IItemPredicate>();
			searchString = searchString.Trim();

			var TOKENS = "@#$".ToCharArray();
			int fromIndex = 0;
			int toIndex;
			do
			{
				string term;

				toIndex = searchString.IndexOfAny(TOKENS, fromIndex + 1);
				if (toIndex < 0)
				{
					term = searchString.Substring(fromIndex);
				}
				else
				{
					term = searchString.Substring(fromIndex, toIndex - fromIndex);
					fromIndex = toIndex;
				}

				switch (term[0])
				{
					case '@':
						predicates.Add(GetPredicateFrom(term.Substring(1), it => new ItemTypePredicate(it)));
						break;
					case '#':
						predicates.Add(GetPredicateFrom(term.Substring(1), it => new ItemAttributePredicate(it)));
						break;
					case '$':
						predicates.Add(GetPredicateFrom(term.Substring(1), it => new ItemQualityPredicate(it)));
						break;
					default:
						foreach (var name in term.Split('&'))
						{
							predicates.Add(GetPredicateFrom(name, it => new ItemNamePredicate(it)));
						}
						break;
				}
			} while (toIndex >= 0);

			return new ItemAndPredicate(predicates);
		}

		/// <summary>
		/// Searches all loaded vault files
		/// </summary>
		/// <param name="predicate">Predicate that the items should match</param>
		/// <param name="quality">Quality filter</param>
		/// <param name="searchByType">flag for whether we are searching by type or name</param>
		/// <param name="results">List holding the search results.</param>
		private void SearchVaults(IItemPredicate predicate, List<Result> results)
		{
			if (this.userContext.Vaults == null || this.userContext.Vaults.Count == 0)
			{
				return;
			}

			foreach (KeyValuePair<string, PlayerCollection> kvp in this.userContext.Vaults)
			{
				string vaultFile = kvp.Key;
				PlayerCollection vault = kvp.Value;

				if (vault == null)
				{
					Log.DebugFormat(CultureInfo.InvariantCulture, "vaultFile={0} returned null vault.", vaultFile);
					continue;
				}

				int vaultNumber = -1;
				foreach (SackCollection sack in vault)
				{
					vaultNumber++;
					if (sack == null)
					{
						Log.DebugFormat(CultureInfo.InvariantCulture, "vaultFile={0}", vaultFile);
						Log.DebugFormat(CultureInfo.InvariantCulture, "sack({0}) returned null.", vaultNumber);
						continue;
					}

					// Query the sack for the items containing the search string.
					foreach (Item item in QuerySack(predicate, sack))
					{
						results.Add(new Result(
							vaultFile,
							Path.GetFileNameWithoutExtension(vaultFile),
							vaultNumber,
							SackType.Vault,
							item
						));
					}
				}
			}
		}

		/// <summary>
		/// Searches all loaded player files
		/// </summary>
		/// <param name="predicate">Predicate that the items should match</param>
		/// <param name="quality">Quality filter</param>
		/// <param name="searchByType">flag for whether we are searching by type or name</param>
		/// <param name="results">List holding the search results.</param>
		private void SearchPlayers(IItemPredicate predicate, List<Result> results)
		{
			if (this.userContext.Players == null || this.userContext.Players.Count == 0)
			{
				return;
			}

			foreach (KeyValuePair<string, PlayerCollection> kvp in this.userContext.Players)
			{
				string playerFile = kvp.Key;
				PlayerCollection player = kvp.Value;

				if (player == null)
				{
					// Make sure the name is valid and we have a player.
					Log.DebugFormat(CultureInfo.InvariantCulture, "playerFile={0} returned null player.", playerFile);
					continue;
				}

				string playerName = GetNameFromFile(playerFile);
				if (playerName == null)
				{
					Log.DebugFormat(CultureInfo.InvariantCulture, "playerFile={0} returned null playerName.", playerFile);
					continue;
				}

				int sackNumber = -1;
				foreach (SackCollection sack in player)
				{
					sackNumber++;
					if (sack == null)
					{
						Log.DebugFormat(CultureInfo.InvariantCulture, "playerFile={0}", playerFile);
						Log.DebugFormat(CultureInfo.InvariantCulture, "sack({0}) returned null.", sackNumber);
						continue;
					}

					// Query the sack for the items containing the search string.
					foreach (Item item in QuerySack(predicate, sack))
					{
						results.Add(new Result(
							playerFile,
							playerName,
							sackNumber,
							SackType.Player,
							item
						));
					}
				}

				// Now search the Equipment panel
				SackCollection equipmentSack = player.EquipmentSack;
				if (equipmentSack == null)
				{
					Log.DebugFormat(CultureInfo.InvariantCulture, "playerFile={0} Equipment Sack returned null.", playerFile);
					continue;
				}

				foreach (Item item in QuerySack(predicate, equipmentSack))
				{
					results.Add(new Result(
						playerFile,
						playerName,
						0,
						SackType.Equipment,
						item
					));
				}
			}
		}

		/// <summary>
		/// Searches all loaded stashes including transfer stash.
		/// </summary>
		/// <param name="predicate">Predicate that the items should match</param>
		/// <param name="results">List holding the search results.</param>
		private void SearchStashes(IItemPredicate predicate, List<Result> results)
		{
			if (this.userContext.Stashes == null || this.userContext.Stashes.Count == 0)
			{
				return;
			}

			foreach (KeyValuePair<string, Stash> kvp in this.userContext.Stashes)
			{
				string stashFile = kvp.Key;
				Stash stash = kvp.Value;

				// Make sure we have a valid name and stash.
				if (stash == null)
				{
					Log.WarnFormat(CultureInfo.InvariantCulture, "stashFile={0} returned null stash.", stashFile);
					continue;
				}

				string stashName = GetNameFromFile(stashFile);
				if (stashName == null)
				{
					Log.WarnFormat(CultureInfo.InvariantCulture, "stashFile={0} returned null stashName.", stashFile);
					continue;
				}

				SackCollection sack = stash.Sack;
				if (sack == null)
				{
					Log.WarnFormat(CultureInfo.InvariantCulture, "stashFile={0} returned null sack.", stashFile);
					continue;
				}

				int sackNumber = 2;
				SackType sackType = SackType.Stash;
				if (stashName == Resources.GlobalTransferStash)
				{
					sackNumber = 1;
					sackType = SackType.TransferStash;
				}
				else if (stashName == Resources.GlobalRelicVaultStash)
				{
					sackNumber = 3;
					sackType = SackType.RelicVaultStash;
				}

				foreach (Item item in QuerySack(predicate, sack))
				{
					results.Add(new Result(
						stashFile,
						stashName,
						sackNumber,
						sackType,
						item
					));
				}
			}
		}

		/// <summary>
		/// Searches all loaded files
		/// </summary>
		/// <param name="predicate">Predicate that the items should match</param>
		/// <param name="results">List holding the search results.</param>
		private void SearchFiles(IItemPredicate predicate, List<Result> results)
		{
			this.SearchVaults(predicate, results);
			this.SearchPlayers(predicate, results);
			this.SearchStashes(predicate, results);
		}

		/// <summary>
		/// Queries the passed sack for items which contain the search string.
		/// </summary>
		/// <param name="predicate">Predicate that the items should match</param>
		/// <param name="sack">Sack that we are searching</param>
		/// <returns>List of items which contain the search string.</returns>
		private static List<Item> QuerySack(IItemPredicate predicate, SackCollection sack)
		{
			// Query the sack for the items containing the search string.
			var vaultQuery = from Item item in sack
							 where predicate.Apply(item)
							 select item;

			List<Item> tmpList = new List<Item>();

			foreach (Item item in vaultQuery)
			{
				tmpList.Add(item);
			}

			return tmpList;
		}

		private static IItemPredicate GetPredicateFrom(string term, Func<string, IItemPredicate> newPredicate)
		{
			var predicates = term.Split('|')
				.Select(it => it.Trim())
				.Where(it => it.Count() > 0)
				.Select(it => newPredicate(it));

			switch (predicates.Count())
			{
				case 0:
					return new ItemTruePredicate();
				case 1:
					return predicates.First();
				default:
					return new ItemOrPredicate(predicates);
			}
		}

		/// <summary>
		/// Parses filename to try to determine the base character name.
		/// </summary>
		/// <param name="filename">filename of the character file</param>
		/// <returns>string containing the character name</returns>
		private static string GetNameFromFile(string filename)
		{
			// Strip off the filename
			string basePath = Path.GetDirectoryName(filename);

			// Get the containing folder
			string charName = Path.GetFileName(basePath);

			if (charName.ToUpperInvariant() == "SYS")
			{
				string fileAndExtension = Path.GetFileName(filename);
				if (fileAndExtension.ToUpperInvariant().Contains("MISC"))
				{
					// Check for the relic vault stash.
					charName = Resources.GlobalRelicVaultStash;
				}
				else if (fileAndExtension.ToUpperInvariant().Contains("WIN"))
				{
					// Check for the transfer stash.
					charName = Resources.GlobalTransferStash;
				}
				else
				{
					charName = null;
				}
			}
			else if (charName.StartsWith("_", StringComparison.Ordinal))
			{
				// See if it is a character folder.
				charName = charName.Substring(1);
			}
			else
			{
				// The name is bogus so return a null.
				charName = null;
			}

			return charName;
		}
	}
}
