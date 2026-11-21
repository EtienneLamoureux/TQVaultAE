namespace TQVaultAE.Application.Results;

/// <summary>
/// What deleting a vault removes, shown in the confirmation before it is deleted.
/// </summary>
/// <param name="ItemCount">Items in the vault.</param>
/// <param name="BagCount">Bags holding at least one item.</param>
/// <param name="ToRecycleBin">Whether the file goes to the Recycle Bin; otherwise it is deleted for good.</param>
public sealed record VaultDeleteInfo(int ItemCount, int BagCount, bool ToRecycleBin);
