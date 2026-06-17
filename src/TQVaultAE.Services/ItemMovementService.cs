using Microsoft.Extensions.Logging;
using System.Drawing;
using TQVaultAE.Application;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Application.Results;
using TQVaultAE.Application.Contracts.Services;

namespace TQVaultAE.Services;

/// <summary>
/// Service for handling item movement operations.
/// Contains pure business logic extracted from GUI layer.
/// </summary>
public class ItemMovementService : IItemMovementService
{
    private readonly ILogger<ItemMovementService> _log;

    public ItemMovementService(ILogger<ItemMovementService> log)
    {
        this._log = log;
    }

    /// <summary>
    /// Finds open cells in a sack for placing an item.
    /// Searches vertically for open space and returns (-1, -1) on failure.
    /// </summary>
    /// <param name="sack">The sack to search for open cells.</param>
    /// <param name="itemWidth">Width of the item to place.</param>
    /// <param name="itemHeight">Height of the item to place.</param>
    /// <param name="sackWidth">Width of the sack in cells.</param>
    /// <param name="sackHeight">Height of the sack in cells.</param>
    /// <returns>Point with coordinates for the upper left cell, or (-1, -1) if no space.</returns>
    public Point FindOpenCells(SackCollection sack, int itemWidth, int itemHeight, int sackWidth, int sackHeight)
    {
        if (sack == null || itemWidth <= 0 || itemHeight <= 0 || sackWidth <= 0 || sackHeight <= 0)
            return new Point(-1, -1);

        // Search vertically for open space
        for (int x = 0; x < sackWidth - itemWidth + 1; ++x)
        {
            for (int y = 0; y < sackHeight; ++y)
            {
                var foundItems = FindAllItems(sack, new Rectangle(x, y, itemWidth, itemHeight));
                
                if ((foundItems == null || foundItems.Count == 0) && itemWidth + x <= sackWidth)
                {
                    if (y + itemHeight <= sackHeight)
                    {
                        // The slot is free
                        return new Point(x, y);
                    }
                }
                else if (foundItems != null && foundItems.Count > 0)
                {
                    // Skip over the item
                    y += foundItems[0].Height - 1;
                }
            }
        }

        // We could not find a place for the item
        return new Point(-1, -1);
    }

    /// <summary>
    /// Validates if an item can be placed in the specified sack.
    /// Checks both space availability and item type restrictions.
    /// </summary>
    public bool CanPlaceItem(SackCollection sack, Item item, int sackWidth, int sackHeight)
    {
        if (sack == null || item == null)
            return false;

        // Check if item is valid for the stash type
        if (!IsItemValidForStashType(item, sack.StashType))
            return false;

        // Check if there's space for the item
        var position = FindOpenCells(sack, item.Width, item.Height, sackWidth, sackHeight);
        return position.X >= 0 && position.Y >= 0;
    }

    /// <summary>
    /// Checks if an item is valid for placement in a specific stash type.
    /// Relic Vault Stash only accepts artifacts, relics, charms, or formulae.
    /// </summary>
    public bool IsItemValidForStashType(Item item, StashType? stashType)
    {
        if (item == null)
            return false;

        // Relic Vault Stash only accepts artifacts, relics, charms, or formulae
        if (stashType == StashType.RelicVaultStash)
        {
            return item.IsArtifact || item.IsRelicOrCharm || item.IsFormulae;
        }

        return true;
    }

    /// <summary>
    /// Moves an item from source to destination sack.
    /// </summary>
    public ItemMoveResult MoveItem(Item item, SackCollection sourceSack, SackCollection destinationSack, Point destinationPosition)
    {
        if (item == null)
            return ItemMoveResultFailed("Item is null");

        if (destinationSack == null)
            return ItemMoveResultFailed("Destination sack is null", item);

        if (destinationPosition.X < 0 || destinationPosition.Y < 0)
            return ItemMoveResultFailed("Invalid destination position", item, destinationSack);

        // Remove from source sack if provided
        if (sourceSack != null)
        {
            sourceSack.RemoveItem(item);
        }

        // Set the item's position
        item.PositionX = destinationPosition.X;
        item.PositionY = destinationPosition.Y;

        // Add to destination sack
        destinationSack.AddItem(item);

        this._log.LogDebug("Moved item {ItemId} to position ({X}, {Y})", 
            item.BaseItemId, destinationPosition.X, destinationPosition.Y);

        return ItemMoveResult.Succeeded(item, destinationSack, destinationPosition);
    }

    /// <summary>
    /// Updates item location metadata after a move.
    /// This ensures search results always reflect the current location.
    /// </summary>
    public void UpdateItemLocation(Item item, PlayerCollection destinationPlayer, SackType destinationSackType, 
        int destinationSackNumber, StashType? destinationStashType = null, string containerName = null)
    {
        if (item == null || destinationPlayer == null)
            return;

        // Update the item's location properties to reflect its new home
        item.Place.Path = destinationPlayer.PlayerFile;
        item.Place.Name = containerName ?? destinationPlayer.PlayerName;

        // Calculate sack number based on sack type
        switch (destinationSackType)
        {
            case SackType.Equipment:
                item.Place.SackNumber = BagIdConstants.BAGID_EQUIPMENTPANEL;
                break;
            case SackType.Stash:
                item.Place.SackNumber = destinationStashType.HasValue ? (int)destinationStashType : destinationSackNumber;
                break;
            case SackType.Vault:
            case SackType.Player:
            default:
                // For vaults and player sacks, use the current sack index
                item.Place.SackNumber = destinationSackNumber;
                break;
        }

        item.Place.SackType = destinationSackType;
        item.Place.StashType = destinationStashType;

        this._log.LogDebug("Updated item location: Path={Path}, SackType={SackType}, SackNumber={SackNumber}, StashType={StashType}",
            item.Place.Path, destinationSackType, item.Place.SackNumber, destinationStashType);
    }

    #region Private Helper Methods

    /// <summary>
    /// Find all items that are at least partially within the cell rectangle.
    /// Extracted from SackPanel.FindAllItems().
    /// </summary>
    private System.Collections.ObjectModel.Collection<Item> FindAllItems(SackCollection sack, Rectangle cellRectangle)
    {
        var items = new System.Collections.ObjectModel.Collection<Item>();
        int minX = cellRectangle.X;
        int maxX = cellRectangle.X + cellRectangle.Width;
        int minY = cellRectangle.Y;
        int maxY = cellRectangle.Y + cellRectangle.Height;

        for (int x = minX; x < maxX; ++x)
        {
            for (int y = minY; y < maxY; ++y)
            {
                var item = FindItem(sack, new Point(x, y));

                if (item != null)
                {
                    // Add this item to the collection if it is not already in there
                    if (!items.Contains(item))
                    {
                        items.Add(item);
                    }

                    y = item.Height + item.Location.Y - 1;
                }
            }
        }

        return items;
    }

    /// <summary>
    /// Gets an item at a cell location.
    /// Extracted from SackPanel.FindItem().
    /// </summary>
    private Item FindItem(SackCollection sack, Point cellLocation)
    {
        if (sack == null)
            return null;

        // Find the item for this point
        foreach (var item in sack)
        {
            if (RecordId.IsNullOrEmpty(item.BaseItemId))
                continue;

            int x = item.PositionX;
            int y = item.PositionY;

            // See if this item overlaps the cell
            if ((x <= cellLocation.X) && ((x + item.Width - 1) >= cellLocation.X) &&
                (y <= cellLocation.Y) && ((y + item.Height - 1) >= cellLocation.Y))
            {
                return item;
            }
        }

        return null;
    }

    private static ItemMoveResult ItemMoveResultFailed(string errorMessage, Item item = null, SackCollection destinationSack = null)
        => ItemMoveResult.Failed(errorMessage, item, destinationSack);

    #endregion
}
