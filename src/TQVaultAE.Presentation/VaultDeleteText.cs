using System.Globalization;
using TQVaultAE.Application.Results;

namespace TQVaultAE.Presentation;

/// <summary>
/// The text of the confirmation shown before a vault is deleted.
/// </summary>
public static class VaultDeleteText
{
	/// <summary>
	/// Builds the question: the vault's name, what it holds, and whether the file can be restored.
	/// </summary>
	/// <param name="vaultName">Name of the vault.</param>
	/// <param name="info">What deleting the vault removes.</param>
	public static string Build(string vaultName, VaultDeleteInfo info)
	{
		var culture = CultureInfo.CurrentCulture;
		var contents = info.ItemCount == 0
			? Resources.VaultDeleteEmpty
			: string.Format(
				culture,
				Resources.VaultDeleteContents,
				string.Format(culture, info.ItemCount == 1 ? Resources.VaultDeleteItem : Resources.VaultDeleteItems, info.ItemCount),
				string.Format(culture, info.BagCount == 1 ? Resources.VaultDeleteBag : Resources.VaultDeleteBags, info.BagCount));
		var outcome = info.ToRecycleBin ? Resources.VaultDeleteRecycleBin : Resources.VaultDeletePermanent;

		return string.Format(culture, Resources.VaultDeleteMsg, vaultName)
			+ Environment.NewLine + Environment.NewLine
			+ contents + Environment.NewLine
			+ outcome;
	}
}
