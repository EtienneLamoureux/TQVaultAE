using System.Drawing;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Application.Results;

namespace TQVaultAE.Application.Contracts.Services;

public interface IItemMovementService
{
    /// <summary>
    /// Finds open cells in a sack for placing an item.
    /// Returns (-1, -1) if no space available.
    /// </summary>
    /// <param name="sack">The sack to search for open cells.</param>
    /// <param name="itemWidth">Width of the item to place.</param>
    /// <param name="itemHeight">Height of the item to place.</param>
    /// <param name="sackWidth">Width of the sack in cells.</param>
    /// <param name="sackHeight">Height of the sack in cells.</param>
    /// <returns>Point with coordinates for the upper left cell, or (-1, -1) if no space.</returns>
    Point FindOpenCells(SackCollection sack, int itemWidth, int itemHeight, int sackWidth, int sackHeight);
    
    /// <summary>
    /// Validates if an item can be placed in the specified sack.
    /// Checks both space availability and item type restrictions.
    /// </summary>
    /// <param name="sack">The destination sack.</param>
    /// <param name="item">The item to place.</param>
    /// <param name="sackWidth">Width of the sack in cells.</param>
    /// <param name="sackHeight">Height of the sack in cells.</param>
    /// <returns>True if the item can be placed.</returns>
    bool CanPlaceItem(SackCollection sack, Item item, int sackWidth, int sackHeight);
    
    /// <summary>
    /// Checks if an item is valid for placement in a specific stash type.
    /// Relic Vault Stash only accepts artifacts, relics, charms, or formulae.
    /// </summary>
    /// <param name="item">The item to validate.</param>
    /// <param name="stashType">The stash type to check against.</param>
    /// <returns>True if the item is valid for the stash type.</returns>
    bool IsItemValidForStashType(Item item, StashType? stashType);
    
    /// <summary>
    /// Moves an item from source to destination sack.
    /// </summary>
    /// <param name="item">The item to move.</param>
    /// <param name="sourceSack">The source sack.</param>
    /// <param name="destinationSack">The destination sack.</param>
    /// <param name="destinationPosition">The position in the destination sack.</param>
    /// <returns>Result of the move operation.</returns>
    ItemMoveResult MoveItem(Item item, SackCollection sourceSack, SackCollection destinationSack, Point destinationPosition);
    
    /// <summary>
    /// Updates item location metadata after a move.
    /// </summary>
    /// <param name="item">The item that was moved.</param>
    /// <param name="destinationPlayer">The destination player collection.</param>
    /// <param name="destinationSackType">The destination sack type.</param>
    /// <param name="destinationSackNumber">The destination sack number.</param>
    /// <param name="destinationStashType">The destination stash type (if applicable).</param>
    /// <param name="containerName">The display name of the container.</param>
    void UpdateItemLocation(Item item, PlayerCollection destinationPlayer, SackType destinationSackType, int destinationSackNumber, StashType? destinationStashType = null, string containerName = null);
}
