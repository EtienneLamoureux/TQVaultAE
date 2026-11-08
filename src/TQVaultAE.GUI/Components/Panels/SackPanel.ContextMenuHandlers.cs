//-----------------------------------------------------------------------
// <copyright file="SackPanel.ContextMenuHandlers.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// Class for holding all of the UI functions of the sack panel.
/// </summary>
public partial class SackPanel
{
	/// <summary>
	/// Deletes the highlighted item
	/// </summary>
	/// <param name="focusedItem">Item containing the focus (which will get deleted)</param>
	/// <param name="suppressMessage">Determines whether we will show a delete confirmation message.</param>
	private void DeleteItem(Item focusedItem, bool suppressMessage)
	{
		if (focusedItem is not null)
		{
			if (suppressMessage || USettings.SuppressWarnings || this.Services.MessageBoxes.Confirm(Resources.SackPanelDeleteMsg, Resources.SackPanelDelete, MessageBoxIcon.Warning))
			{
				// remove item
				this.ShownSack.RemoveItem(focusedItem);
				// Remove from search database
				this.ItemDatabaseService.RemoveItemFromDatabase(focusedItem);
				this.Refresh();
				BagButtonTooltip.InvalidateCache(this.ShownSack);
			}
		}
	}

	/// <summary>
	/// Adds the highlighted item to the selected items list.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">MouseEventArgs data</param>
	private void AddFocusedItemToSelectedItems(object? sender, MouseEventArgs e)
	{
		if (this.DisableMultipleSelection)
			return;

		this.MouseMoveCallback(sender, e); // process the mouse moving to this location...just in case

		Item? focusedItem = this.FindItem(this.LastCellWithFocus);
		if (focusedItem is not null)
		{
			// Allocate the List if not already done.
			if (this.selectedItems is null)
				this.selectedItems = [];

			if (!this.IsItemSelected(focusedItem))
			{
				// Check to see if the item is already selected
				this.selectedItems.Add(focusedItem);
				this.selectedItemsChanged = true;
			}
			else
				this.RemoveSelectedItem(focusedItem);

			this.Invalidate(this.GetItemScreenRectangle(focusedItem));
		}

		this.OnItemSelected?.Invoke(this, new SackPanelEventArgs(null, null));
		this.MouseMoveCallback(sender, e); // process mouse move again to apply the graphical effects of dragging an item.
	}

	/// <summary>
	/// Adds the items inside of the mouse drag to the selected items list.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">MouseEventArgs data</param>
	private void SelectItemsInMouseDraw(object? sender, MouseEventArgs e)
	{
		if (!this.mouseDraw)
			return;

		// Allocate the List if not already done.
		if (this.selectedItems is null)
			this.selectedItems = [];

		Collection<Item> itemsUnderDraw = FindAllItems(FindAllCells(GetMouseDragRectangle()));

		if (itemsUnderDraw is not null)
		{
			foreach (Item item in itemsUnderDraw)
			{
				if (!selectedItems.Contains(item))
				{
					this.selectedItems.Add(item);
				}
			}

			this.OnItemSelected?.Invoke(this, new SackPanelEventArgs(null, null));
		}
	}

	/// <summary>
	/// Gets the localized string for an AutoMoveLocation.
	/// </summary>
	/// <param name="autoMoveLocation">AutoMoveLocation that is to be translated</param>
	/// <returns>Localized String name of the AutoMoveLocation</returns>
	private static string GetStringFromAutoMove(AutoMoveLocation autoMoveLocation)
		=> autoMoveLocation switch
		{
			AutoMoveLocation.SecondaryVault => Resources.SackPanelMenuVault2,
			AutoMoveLocation.Player => Resources.SackPanelMenuPlayer,
			AutoMoveLocation.Stash => Resources.SackPanelMenuStash,
			AutoMoveLocation.Vault => Resources.SackPanelMenuVault,
			AutoMoveLocation.Trash => Resources.SackPanelMenuTrash,
			_ => string.Empty,
		};

	/// <summary>
	/// Handler for selecting Move Item from the context menu
	/// Auto moves an item to another sack or panel.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void MoveItemClicked(object? sender, EventArgs e)
	{
		if (!this.DragInfo.IsActive)
		{
			// Find out what was selected: a bag of this panel or another panel.
			AutoMoveLocation automoveDestination = AutoMoveLocation.NotSet;
			int destIndex = -1;
			switch ((sender as ToolStripItem)?.Tag)
			{
				case MenuItemTagBagIndex mit:
					automoveDestination = this.AutoMoveLocation;
					destIndex = mit.Index;
					break;
				case AutoMoveLocation location:
					automoveDestination = location;
					break;
			}

			// The selection, or the item under the cursor when nothing is selected.
			var plan = AutoMovePlanner.Plan(
				this.selectedItems
				, this.selectedItems is null ? this.FindItem(this.contextMenuCellWithFocus) : null
				, automoveDestination
				, destIndex);
			this.RaiseAutoMove(this.ShownSack, plan);

			if (this.selectedItems is not null)
				this.ClearSelectedItems();

			ItemTooltip.HideTooltip();
			BagButtonTooltip.InvalidateCache(this.ShownSack);
		}
	}

	/// <summary>
	/// Moves the planned items one by one: each is set as the drag item and <see cref="OnAutoMoveItem"/> places it.
	/// An item left in the drag (no room) stops the moves after it.
	/// </summary>
	private void RaiseAutoMove(SackCollection sack, AutoMovePlan plan)
	{
		foreach (Item item in plan.Items)
		{
			if (this.DragInfo.IsActive)
				continue;

			this.DragInfo.Set(this, sack, item, new Point(1, 1));
			this.DragInfo.AutoMoveDestination = plan.Destination;
			this.DragInfo.DestIndex = plan.DestIndex;
			this.OnAutoMoveItem?.Invoke(this, new SackPanelEventArgs(null, null));
		}
	}

	/// <summary>
	/// Gets the items the export entries act on: the selection when there is one, else the item under the context menu.
	/// </summary>
	private List<Item> ItemsToExport()
	{
		if (this.selectedItems is not null)
			return this.selectedItems;

		return this.FindItem(this.contextMenuCellWithFocus) is { } focusedItem ? [focusedItem] : [];
	}

	private void ExportItemToClipboardClicked(object? sender, EventArgs e)
	{
		try
		{
			var json = this.ExchangeService?.SerializeSelection(this.ItemsToExport());
			if (json is not null)
				Clipboard.SetText(json);
		}
		catch (Exception ex)
		{
			this.Log.LogError(ex, "Failed to export item to clipboard");
		}
	}

	/// <summary>
	/// Writes the export of <see cref="ItemsToExport"/> to a file.
	/// </summary>
	/// <returns><see langword="false"/> when there was nothing to export.</returns>
	private bool ExportItemsToFile(string path)
	{
		var json = this.ExchangeService?.SerializeSelection(this.ItemsToExport());
		if (json is null)
			return false;

		this.ExchangeService!.ExportToFile(json, path);
		return true;
	}

	private void ExportItemToFileClicked(object? sender, EventArgs e)
	{
		try
		{
			var items = this.ItemsToExport();
			if (this.ExchangeService is null || items.Count == 0)
				return;

			using var dialog = new SaveFileDialog
			{
				Filter = "JSON files (*.json)|*.json",
				DefaultExt = "json",
				FileName = items.Count == 1 ? "item_export.json" : "items_export.json"
			};

			if (dialog.ShowDialog() == DialogResult.OK)
				this.ExportItemsToFile(dialog.FileName);
		}
		catch (Exception ex)
		{
			this.Log.LogError(ex, "Failed to export item to file");
		}
	}

	private async void ExportItemToPasteBinClicked(object? sender, EventArgs e)
	{
		try
		{
			var items = this.ItemsToExport();
			if (this.ExchangeService is null || items.Count == 0)
				return;

			var url = await this.ExchangeService.ExportSelectionToPasteBinAsync(items);
			Clipboard.SetText(url);
			this.UIService.NotifyUser(items.Count == 1
				? $"Item exported to PasteBin: {url}"
				: $"Items exported to PasteBin: {url}");
		}
		catch (Exception ex)
		{
			this.Log.LogError(ex, "Failed to export item to PasteBin");
			this.UIService.ShowError($"Failed to export to PasteBin: {ex.Message}");
		}
	}

	/// <summary>
	/// Handler for selecting New Set Item from the context menu
	/// Creates the selected item from the set and sets it as the drag item.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void NewSetItemClicked(object? sender, EventArgs e)
	{
		Item? focusedItem = this.FindItem(this.contextMenuCellWithFocus);
		if (focusedItem is not null)
		{
			ToolStripMenuItem? item = (ToolStripMenuItem)sender!;
			if (item is not null)
			{
				// Create the item
				var newId = item.Name.ToRecordId();
				Item newItem = this.Services.ItemEditService.NewSetMember(focusedItem, newId);

				// Set DragInfo to focused item.
				this.DragInfo.Set(this, this.ShownSack, focusedItem, new Point(1, 1));

				// now drag the new item
				this.DragInfo.MarkModified(newItem);
				Refresh();

				ItemTooltip.HideTooltip();
				BagButtonTooltip.InvalidateCache(this.ShownSack);
			}
		}
	}

	private void RemoveSuffixItemClicked(object? sender, EventArgs e)
	{
		Item? focusedItem = this.FindItem(this.contextMenuCellWithFocus);
		if (focusedItem is not null)
		{
			this.Services.ItemEditService.RemoveSuffix(this.ShownSack, focusedItem);
			this.InvalidateItemCacheAll(focusedItem);
		}
	}

	private void RemovePrefixItemClicked(object? sender, EventArgs e)
	{
		Item? focusedItem = this.FindItem(this.contextMenuCellWithFocus);
		if (focusedItem is not null)
		{
			this.Services.ItemEditService.RemovePrefix(this.ShownSack, focusedItem);
			this.InvalidateItemCacheAll(focusedItem);
		}
	}

	/// <summary>
	/// Handler for changing suffix from the context menu
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void ChangeSuffixItemClicked(object? sender, EventArgs e)
	{
		Item? focusedItem = this.FindItem(this.contextMenuCellWithFocus);
		if (focusedItem is not null)
		{
			ToolStripMenuItem? menuItem = sender as ToolStripMenuItem;
			if (menuItem is not null
				&& this.Services.ItemEditService.ChangeSuffix(this.ShownSack, focusedItem, menuItem.Name.ToRecordId()))
			{
				this.InvalidateItemCacheAll(focusedItem);
			}
		}
	}

	/// <summary>
	/// Handler for changing Prefix from the context menu
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void ChangePrefixItemClicked(object? sender, EventArgs e)
	{
		Item? focusedItem = this.FindItem(this.contextMenuCellWithFocus);
		if (focusedItem is not null)
		{
			ToolStripMenuItem? menuItem = sender as ToolStripMenuItem;
			if (menuItem is not null
				&& this.Services.ItemEditService.ChangePrefix(this.ShownSack, focusedItem, menuItem.Name.ToRecordId()))
			{
				this.InvalidateItemCacheAll(focusedItem);
			}
		}
	}

	/// <summary>
	/// Handler for selecting Change Bonus from the context menu
	/// Changes a socketed item's completion bonus.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ChangeSocketedBonusItemClicked(object? sender, EventArgs e)
	{
		Item? focusedItem = this.FindItem(this.contextMenuCellWithFocus);
		if (focusedItem is not null)
		{
			ToolStripMenuItem? menuItem = sender as ToolStripMenuItem;

			var isRelic1 = (menuItem?.Tag as bool?) ?? false;

			if (menuItem is not null
				&& this.Services.ItemEditService.ChangeCompletionBonus(this.ShownSack, focusedItem, menuItem.Name.ToRecordId(), secondRelic: !isRelic1))
			{
				this.InvalidateItemCacheAll(focusedItem);
			}
		}
	}

	/// <summary>
	/// Handler for selecting Change Bonus from the context menu
	/// Changes an item's completion bonus.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ChangeBonusItemClicked(object? sender, EventArgs e)
	{
		Item? focusedItem = this.FindItem(this.contextMenuCellWithFocus);
		if (focusedItem is not null)
		{
			ToolStripMenuItem? item = (ToolStripMenuItem)sender!;
			if (item is not null
				&& this.Services.ItemEditService.ChangeCompletionBonus(this.ShownSack, focusedItem, item.Name.ToRecordId()))
			{
				this.InvalidateItemCacheAll(focusedItem);
			}
		}
	}

	/// <summary>
	/// Runs Copy, Duplicate or Delete on the item under the cursor alone, for the context menu and the keys
	/// (see <see cref="ItemActionRules.KeyAction"/>): a copy is picked up, a delete asks first.
	/// </summary>
	private void RunFocusedItemAction(ItemActions action, Item focusedItem)
	{
		if (action == ItemActions.Delete)
		{
			this.DeleteItem(focusedItem, false);
		}
		else if (action is ItemActions.Copy or ItemActions.Duplicate)
		{
			// Set DragInfo to focused item.
			this.DragInfo.Set(this, this.ShownSack, focusedItem, new Point(1, 1));

			// copy the item
			Item newItem = action == ItemActions.Copy
				? this.Services.ItemEditService.Copy(focusedItem)
				: this.Services.ItemEditService.Duplicate(focusedItem);

			// now drag it
			this.DragInfo.MarkModified(newItem);
		}

		this.Refresh();
	}

	/// <summary>
	/// Handler for clicking an item on the context menu
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">ToolStripItemClickedEventArgs data</param>
	private void ContextMenuItemClicked(object? sender, ToolStripItemClickedEventArgs e)
	{
		Item? focusedItem = this.FindItem(this.contextMenuCellWithFocus);
		if (focusedItem is not null || this.selectedItems is not null)
		{
			var action = e.ClickedItem?.Tag as ItemActions? ?? ItemActions.None;
			if (action == ItemActions.Delete)
			{
				if (this.selectedItems is not null)
				{
					if (USettings.SuppressWarnings || this.Services.MessageBoxes.Confirm(Resources.SackPanelDeleteMultiMsg, Resources.SackPanelDeleteMulti, MessageBoxIcon.Warning))
					{
						foreach (Item sackSelectedItem in this.selectedItems)
							this.DeleteItem(sackSelectedItem, true);

						this.ClearSelection();
					}
				}
				else if (focusedItem is not null)
				{
					this.DeleteItem(focusedItem, false);
				}
			}
			else if (action == ItemActions.ClearSelection)
			{
				// Before the focused item test: the selection can be cleared from an empty cell.
				this.ClearSelection();
				Refresh();
			}
			else if (focusedItem is null)
			{
				// The other entries apply to the focused item.
			}
			else if (action is ItemActions.RemoveRelic or ItemActions.RemoveRelic2)
			{
				if (USettings.SuppressWarnings || this.Services.MessageBoxes.Confirm(Resources.SackPanelRemoveRelicMsg, Resources.SackPanelMenuRemoveRelic))
				{
					// Set DragInfo to focused item.
					this.DragInfo.Set(this, this.ShownSack, focusedItem, new Point(1, 1));

					// pull out the relic
					Item? relic = this.Services.ItemEditService.RemoveRelic(this.ShownSack, focusedItem, secondSlot: action == ItemActions.RemoveRelic2);
					if (relic is null)
					{
						// Only relics and charms come back to be placed; anything else is removed with the slot.
						this.DragInfo.Cancel();
						this.UIService.NotifyUser(Resources.SackPanelSocketedNotAnItem, TQColor.Yellow);
					}
					else
					{
						// Put relic in DragInfo
						this.DragInfo.MarkModified(relic);
					}

					this.InvalidateItemCacheItemTooltip(focusedItem);
					Refresh();
				}
			}
			else if (action is ItemActions.Copy or ItemActions.Duplicate)
			{
				this.RunFocusedItemAction(action, focusedItem);
			}
			else if (action == ItemActions.Properties)
			{
				var dlg = this.Services.Forms.Create<ItemProperties>();
				dlg.Item = focusedItem;
				dlg.ShowDialog();
			}
			else if (action == ItemActions.Seed)
			{
				var dlg = this.Services.Forms.Create<ItemSeedDialog>();
				dlg.SelectedItem = focusedItem;
				int origSeed = focusedItem.Seed;
				dlg.ShowDialog();

				// See if the seed was changed
				if (focusedItem.Seed != origSeed)
				{
					// Tell the sack that it has been modified
					this.ShownSack.IsModified = true;
					this.InvalidateItemCacheItemTooltip(focusedItem);
				}
			}
			else if (action == ItemActions.ForceSeed)
			{
				if (this.Services.ItemEditService.ForceSeed(this.ShownSack, focusedItem))
					this.InvalidateItemCacheItemTooltip(focusedItem);
			}
			else if (action == ItemActions.CompleteRelic)
			{
				this.Services.ItemEditService.CompleteRelic(this.ShownSack, focusedItem);
				InvalidateItemCacheItemTooltip(focusedItem);
				Refresh();
			}
			else if (action == ItemActions.CraftArtifact)
			{
				// Set DragInfo to focused item.
				this.DragInfo.Set(this, this.ShownSack, focusedItem, new Point(1, 1));

				// create artifact
				Item? artifact = this.Services.ItemEditService.CraftArtifact(focusedItem);
				if (artifact is null)
				{
					// Nothing crafted: do not leave the formula picked up.
					this.DragInfo.Cancel();
					return;
				}

				// Put artifact in DragInfo
				this.DragInfo.MarkModified(artifact);

				InvalidateItemCacheItemTooltip(focusedItem);

				Refresh();
			}
			else if (action == ItemActions.Split)
			{
				// Set DragInfo to focused item.
				this.DragInfo.Set(this, this.ShownSack, focusedItem, new Point(1, 1));

				// pull out all but one item
				Item? newItem = this.Services.ItemEditService.Split(focusedItem);

				// Put the item in DragInfo
				if (newItem is not null)
					this.DragInfo.MarkModified(newItem);
				else
					// Nothing split off: do not leave the stack picked up.
					this.DragInfo.Cancel();

				InvalidateItemCacheItemTooltip(focusedItem);

				Refresh();
			}

			ItemTooltip.HideTooltip();
			BagButtonTooltip.InvalidateCache(this.ShownSack);

		}
	}
}
