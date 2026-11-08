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
    /// Gets the item covering a cell of a sack.
    /// Items without a base item id are skipped.
    /// </summary>
    /// <param name="sack">The sack to look into.</param>
    /// <param name="cellLocation">The cell coordinates.</param>
    /// <param name="ignoredItem">An item to skip, such as the item being dragged.</param>
    /// <returns>The item covering the cell, or null.</returns>
    Item? FindItem(SackCollection sack, Point cellLocation, Item? ignoredItem = null);

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
    /// Builds the place of a sack of a container, as the search shows it.
    /// </summary>
    /// <remarks>
    /// The name is <see cref="PlayerCollection.PlayerName"/>, the name the search database is built with: the character,
    /// the vault, or the transfer and relic vault stash names.
    /// An equipment sack is <see cref="BagIdConstants.BAGID_EQUIPMENTPANEL"/> and a stash sack its stash type.
    /// </remarks>
    /// <param name="container">The character, vault or stash.</param>
    /// <param name="sackType">The sack type.</param>
    /// <param name="sackNumber">The sack number, used for vault and player sacks, and for a stash without stash type.</param>
    /// <param name="stashType">The stash type, for a stash.</param>
    ContainerPlace PlaceIn(PlayerCollection container, SackType sackType, int sackNumber, StashType? stashType = null);

    /// <summary>
    /// Builds the place of an item in a stash, the same place the stash load gives its items.
    /// </summary>
    /// <remarks>
    /// The container is the stash file and its name ("Transfer Stash", "Relic Vault", or the character for its own stash),
    /// so a reload of the stash finds the item by its file and the character reload leaves it alone.
    /// </remarks>
    /// <param name="stash">The stash.</param>
    /// <param name="stashType">The kind of stash, also its sack number.</param>
    ContainerPlace PlaceIn(Stash stash, StashType stashType);

    /// <summary>
    /// Places an item in the first free cells of a sack, the way the auto-move does.
    /// </summary>
    /// <remarks>
    /// The item itself is ignored when looking for free cells, so it can still be in its source sack.
    /// Nothing changes when the item is not allowed in the sack (relic vault stash) or does not fit.
    /// </remarks>
    /// <param name="item">The item to place.</param>
    /// <param name="sack">The destination sack.</param>
    /// <param name="sackSize">Size of the sack in cells.</param>
    /// <param name="place">The place the item gets, see <see cref="PlaceIn"/>.</param>
    /// <param name="beforePlacing">
    /// Runs once free cells are found, before the item gets its new position: the drag takes the item out of its
    /// source sack there, and an equipped item is found by its position.
    /// </param>
    /// <returns><see langword="false"/> when the item was not placed.</returns>
    bool TryPlace(Item item, SackCollection sack, Size sackSize, ContainerPlace place, Action? beforePlacing = null);

    /// <summary>
    /// Sorts the items of a sack by size, the largest first, packing them row by row or column by column.
    /// </summary>
    /// <remarks>
    /// When an item does not fit, the sack gets back copies of its original items and is marked as not modified.
    /// </remarks>
    /// <param name="sack">The sack to sort.</param>
    /// <param name="sackSize">Size of the sack in cells.</param>
    /// <param name="vertical"><see langword="true"/> to fill column by column, otherwise row by row.</param>
    /// <returns><see langword="false"/> when an item did not fit and the sack was restored.</returns>
    bool Autosort(SackCollection sack, Size sackSize, bool vertical);

    /// <summary>
    /// Moves items of a sack by an offset, all of them or none.
    /// </summary>
    /// <param name="sack">The sack holding the items.</param>
    /// <param name="items">The items to move; they may block each other's way.</param>
    /// <param name="sackSize">Size of the sack in cells.</param>
    /// <param name="offset">The move in cells.</param>
    /// <returns><see langword="false"/> when another item is in the way or an item would leave the sack.</returns>
    bool MoveItems(SackCollection sack, IReadOnlyCollection<Item> items, Size sackSize, Point offset);
}
