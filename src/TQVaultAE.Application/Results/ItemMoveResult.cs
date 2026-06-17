using System.Drawing;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Results;

/// <summary>
/// Result of an item move operation.
/// </summary>
public class ItemMoveResult
{
    /// <summary>
    /// Indicates whether the move operation was successful.
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// The new position of the item in the destination sack.
    /// </summary>
    public Point NewPosition { get; set; }
    
    /// <summary>
    /// Error message if the move failed.
    /// </summary>
    public string ErrorMessage { get; set; }
    
    /// <summary>
    /// The item that was moved.
    /// </summary>
    public Item Item { get; set; }
    
    /// <summary>
    /// The destination sack where the item was moved.
    /// </summary>
    public SackCollection DestinationSack { get; set; }
    
    /// <summary>
    /// Creates a successful move result.
    /// </summary>
    public static ItemMoveResult Succeeded(Item item, SackCollection destinationSack, Point newPosition)
        => new()
        {
            Success = true,
            Item = item,
            DestinationSack = destinationSack,
            NewPosition = newPosition,
            ErrorMessage = null
        };
    
    /// <summary>
    /// Creates a failed move result.
    /// </summary>
    public static ItemMoveResult Failed(string errorMessage, Item item = null, SackCollection destinationSack = null)
        => new()
        {
            Success = false,
            Item = item,
            DestinationSack = destinationSack,
            NewPosition = new Point(-1, -1),
            ErrorMessage = errorMessage
        };
}
