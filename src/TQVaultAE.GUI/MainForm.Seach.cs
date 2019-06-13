using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TQVaultAE.Data;
using TQVaultAE.Entities;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Properties;
using TQVaultAE.Presentation.Html;

namespace TQVaultAE.GUI
{
	internal partial class MainForm
	{

		/// <summary>
		/// Handler for clicking the search button on the form.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void SearchButtonClick(object sender, EventArgs e)
		{
			this.OpenSearchDialog();
		}

		/// <summary>
		/// Opens a scaled SearchDialog box and calls Search().
		/// </summary>
		private void OpenSearchDialog()
		{
			SearchDialog searchDialog = new SearchDialog();
			searchDialog.Scale(new SizeF(Database.DB.Scale, Database.DB.Scale));

			if (searchDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(searchDialog.SearchText))
			{
				this.Search(searchDialog.SearchText);
			}
		}

		/// <summary>
		/// Handler for keypresses within the search text box.
		/// Used to handle the resizing hot keys.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">KeyEventArgs data</param>
		private void SearchTextBoxKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == (Keys.Control | Keys.Add) || e.KeyData == (Keys.Control | Keys.Oemplus))
			{
				this.ResizeFormCallback(this, new ResizeEventArgs(0.1F));
			}

			if (e.KeyData == (Keys.Control | Keys.Subtract) || e.KeyData == (Keys.Control | Keys.OemMinus))
			{
				this.ResizeFormCallback(this, new ResizeEventArgs(-0.1F));
			}

			if (e.KeyData == (Keys.Control | Keys.Home))
			{
				this.ResizeFormCallback(this, new ResizeEventArgs(1.0F));
			}
		}

		/// <summary>
		/// Gets the string name of a particular item style
		/// </summary>
		/// <param name="itemStyle">ItemStyle enumeration</param>
		/// <returns>Localized string of the item style</returns>
		public static string GetItemStyleString(ItemStyle itemStyle)
		{
			switch (itemStyle)
			{
				case ItemStyle.Broken:
					return Resources.ItemStyleBroken;

				case ItemStyle.Artifact:
					return Resources.ItemStyleArtifact;

				case ItemStyle.Formulae:
					return Resources.ItemStyleFormulae;

				case ItemStyle.Scroll:
					return Resources.ItemStyleScroll;

				case ItemStyle.Parchment:
					return Resources.ItemStyleParchment;

				case ItemStyle.Relic:
					return Resources.ItemStyleRelic;

				case ItemStyle.Potion:
					return Resources.ItemStylePotion;

				case ItemStyle.Quest:
					return Resources.ItemStyleQuest;

				case ItemStyle.Epic:
					return Resources.ItemStyleEpic;

				case ItemStyle.Legendary:
					return Resources.ItemStyleLegendary;

				case ItemStyle.Rare:
					return Resources.ItemStyleRare;

				case ItemStyle.Common:
					return Resources.ItemStyleCommon;

				default:
					return Resources.ItemStyleMundane;
			}
		}

		/// <summary>
		/// Searches loaded files based on the specified search string.  Internally normalized to UpperInvariant
		/// </summary>
		/// <param name="searchString">string that we are searching for</param>
		private void Search(string searchString)
		{
			if (searchString == null || searchString.Trim().Count() == 0)
			{
				return;
			}

			var filter = GetFilterFrom(searchString);
			var results = new List<Result>();
			this.SearchFiles(filter, results);

			if (results.Count < 1)
			{
				MessageBox.Show(
					string.Format(CultureInfo.CurrentCulture, Resources.MainFormNoItemsFound, searchString),
					Resources.MainFormNoItemsFound2,
					MessageBoxButtons.OK,
					MessageBoxIcon.Information,
					MessageBoxDefaultButton.Button1,
					RightToLeftOptions);

				return;
			}

			// Display a dialog with the results.
			ResultsDialog dlg = new ResultsDialog();
			dlg.ResultChanged += new ResultsDialog.EventHandler<ResultChangedEventArgs>(this.SelectResult);
			dlg.ResultsList.Clear();
			dlg.ResultsList.AddRange(results);
			dlg.SearchString = searchString;
			////dlg.ShowDialog();
			dlg.Show();
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
		/// Selects the item highlighted in the results list.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">ResultChangedEventArgs data</param>
		private void SelectResult(object sender, ResultChangedEventArgs e)
		{
			Result selectedResult = e.Result;
			if (selectedResult == null || selectedResult.Item == null)
			{
				return;
			}

			this.ClearAllItemsSelectedCallback(this, new SackPanelEventArgs(null, null));

			if (selectedResult.SackType == SackType.Vault)
			{
				// Switch to the selected vault
				this.vaultListComboBox.SelectedItem = selectedResult.ContainerName;
				this.vaultPanel.CurrentBag = selectedResult.SackNumber;
				this.vaultPanel.SackPanel.SelectItem(selectedResult.Item.Location);
			}
			else if (selectedResult.SackType == SackType.Player || selectedResult.SackType == SackType.Equipment)
			{
				// Switch to the selected player
				if (this.showSecondaryVault)
				{
					this.showSecondaryVault = !this.showSecondaryVault;
					this.UpdateTopPanel();
				}

				string myName = selectedResult.ContainerName;

				if (TQData.IsCustom)
				{
					myName = string.Concat(myName, "<Custom Map>");
				}

				// Update the selection list and load the character.
				this.characterComboBox.SelectedItem = myName;
				if (selectedResult.SackNumber > 0)
				{
					this.playerPanel.CurrentBag = selectedResult.SackNumber - 1;
				}

				if (selectedResult.SackType != SackType.Equipment)
				{
					// Highlight the item if it's in the player inventory.
					if (selectedResult.SackNumber == 0)
					{
						this.playerPanel.SackPanel.SelectItem(selectedResult.Item.Location);
					}
					else
					{
						this.playerPanel.BagSackPanel.SelectItem(selectedResult.Item.Location);
					}
				}
			}
			else if (selectedResult.SackType == SackType.Stash)
			{
				// Switch to the selected player
				if (this.showSecondaryVault)
				{
					this.showSecondaryVault = !this.showSecondaryVault;
					this.UpdateTopPanel();
				}

				// Assume that only IT characters can have a stash.
				string myName = string.Concat(selectedResult.ContainerName, "<Immortal Throne>");

				if (TQData.IsCustom)
				{
					myName = string.Concat(myName, "<Custom Map>");
				}

				// Update the selection list and load the character.
				this.characterComboBox.SelectedItem = myName;

				// Switch to the Stash bag
				this.stashPanel.CurrentBag = selectedResult.SackNumber;
				this.stashPanel.SackPanel.SelectItem(selectedResult.Item.Location);
			}
			else if ((selectedResult.SackType == SackType.TransferStash) || (selectedResult.SackType == SackType.RelicVaultStash))
			{
				// Switch to the Stash bag
				this.stashPanel.CurrentBag = selectedResult.SackNumber;
				this.stashPanel.SackPanel.SelectItem(selectedResult.Item.Location);
			}
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
			if (this.vaults == null || this.vaults.Count == 0)
			{
				return;
			}

			foreach (KeyValuePair<string, PlayerCollection> kvp in this.vaults)
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
			if (this.players == null || this.players.Count == 0)
			{
				return;
			}

			foreach (KeyValuePair<string, PlayerCollection> kvp in this.players)
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
			if (this.stashes == null || this.stashes.Count == 0)
			{
				return;
			}

			foreach (KeyValuePair<string, Stash> kvp in this.stashes)
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

		private interface IItemPredicate
		{
			bool Apply(Item item);
		}

		private class ItemTruePredicate : IItemPredicate
		{
			public bool Apply(Item item)
			{
				return true;
			}

			public override string ToString()
			{
				return "true";
			}
		}

		private class ItemFalsePredicate : IItemPredicate
		{
			public bool Apply(Item item)
			{
				return false;
			}

			public override string ToString()
			{
				return "false";
			}
		}

		private class ItemAndPredicate : IItemPredicate
		{
			private readonly List<IItemPredicate> predicates;

			public ItemAndPredicate(params IItemPredicate[] predicates)
			{
				this.predicates = predicates.ToList();
			}

			public ItemAndPredicate(IEnumerable<IItemPredicate> predicates)
			{
				this.predicates = predicates.ToList();
			}

			public bool Apply(Item item)
			{
				return predicates.TrueForAll(predicate => predicate.Apply(item));
			}

			public override string ToString()
			{
				return "(" + String.Join(" && ", predicates.ConvertAll(p => p.ToString()).ToArray()) + ")";
			}
		}


		private class ItemOrPredicate : IItemPredicate
		{
			private readonly List<IItemPredicate> predicates;

			public ItemOrPredicate(params IItemPredicate[] predicates)
			{
				this.predicates = predicates.ToList();
			}

			public ItemOrPredicate(IEnumerable<IItemPredicate> predicates)
			{
				this.predicates = predicates.ToList();
			}

			public bool Apply(Item item)
			{
				return predicates.Exists(predicate => predicate.Apply(item));
			}

			public override string ToString()
			{
				return "(" + String.Join(" || ", predicates.ConvertAll(p => p.ToString()).ToArray()) + ")";
			}
		}

		private class ItemNamePredicate : IItemPredicate
		{
			private readonly string name;

			public ItemNamePredicate(string type)
			{
				this.name = type;
			}

			public bool Apply(Item item)
			{
				return ItemProvider.ToFriendlyName(item).ToUpperInvariant().Contains(name.ToUpperInvariant());
			}

			public override string ToString()
			{
				return $"Name({name})";
			}
		}

		private class ItemTypePredicate : IItemPredicate
		{
			private readonly string type;

			public ItemTypePredicate(string type)
			{
				this.type = type;
			}

			public bool Apply(Item item)
			{
				return item.ItemClass.ToUpperInvariant().Contains(type.ToUpperInvariant());
			}

			public override string ToString()
			{
				return $"Type({type})";
			}
		}

		private class ItemQualityPredicate : IItemPredicate
		{
			private readonly string quality;

			public ItemQualityPredicate(string quality)
			{
				this.quality = quality;
			}

			public bool Apply(Item item)
			{
				return GetItemStyleString(item.ItemStyle).ToUpperInvariant().Contains(quality.ToUpperInvariant());
			}

			public override string ToString()
			{
				return $"Quality({quality})";
			}
		}

		private class ItemAttributePredicate : IItemPredicate
		{
			private readonly string attribute;

			public ItemAttributePredicate(string attribute)
			{
				this.attribute = attribute;
			}

			public bool Apply(Item item)
			{
				return ItemHtmlHelper.GetAttributes(item, true).ToUpperInvariant().Contains(attribute.ToUpperInvariant());
			}

			public override string ToString()
			{
				return $"Attribute({attribute})";
			}
		}

	}
}
