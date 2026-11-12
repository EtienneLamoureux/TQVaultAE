using System.Drawing;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application;

/// <summary>
/// Panel that shows the item of a search result.
/// </summary>
public enum ResultPanelKind
{
	/// <summary>
	/// No panel shows this container.
	/// </summary>
	None,

	/// <summary>
	/// A bag of the vault panel, selected by the vault name.
	/// </summary>
	Vault,

	/// <summary>
	/// The main sack of the character's player panel.
	/// </summary>
	PlayerMainSack,

	/// <summary>
	/// A bag of the character's player panel.
	/// </summary>
	PlayerBag,

	/// <summary>
	/// The equipment tab of the stash panel, for the character.
	/// </summary>
	Equipment,

	/// <summary>
	/// The character's own stash tab of the stash panel.
	/// </summary>
	CharacterStash,

	/// <summary>
	/// The transfer stash or relic vault tab of the stash panel, shared by every character.
	/// </summary>
	SharedStash,
}

/// <summary>
/// Where a search result is shown: the panel, its bag and the item's cell.
/// </summary>
/// <param name="Panel">The panel that shows the item.</param>
/// <param name="Bag">
/// The bag to select in that panel: the vault bag, the player bag (sack N - 1),
/// or the stash panel tab (<see cref="BagIdConstants"/>). 0 for the player main sack.
/// </param>
/// <param name="Cell">The item's cell in that bag.</param>
public readonly record struct ResultLocation(ResultPanelKind Panel, int Bag, Point Cell)
{
	/// <summary>
	/// Gets whether the panel belongs to the character named by the result, which must be selected first.
	/// </summary>
	public bool NeedsCharacter => this.Panel is ResultPanelKind.PlayerMainSack or ResultPanelKind.PlayerBag
		or ResultPanelKind.Equipment or ResultPanelKind.CharacterStash;

	/// <summary>
	/// Gets whether the item is shown in the stash panel (equipment or a stash tab).
	/// </summary>
	public bool OnStashPanel => this.Panel is ResultPanelKind.Equipment or ResultPanelKind.CharacterStash
		or ResultPanelKind.SharedStash;

	/// <summary>
	/// Gets the bag number shown to the user, 1-based like the bag buttons: vault bag N + 1 and player bag N + 1
	/// (player sack N). Null where no bag button applies: the player main sack, the equipment and the stashes,
	/// whose container type already names them.
	/// </summary>
	public int? BagNumber => this.Panel is ResultPanelKind.Vault or ResultPanelKind.PlayerBag
		? this.Bag + 1
		: null;
}

/// <summary>
/// Maps a search result to the panel and bag that show its item, with the sack numbering
/// <c>ItemDatabaseService.AddItemToDatabase</c> and <c>SackPanel.PlaceHere</c> store:
/// vault sack N is vault bag N, player sack 0 is the main sack and sack N is bag N - 1,
/// equipment and stashes are numbered by their stash panel tab.
/// </summary>
public static class SearchResultNavigation
{
	/// <summary>
	/// Resolves where the item of a search result is shown.
	/// </summary>
	/// <param name="result">The search result.</param>
	/// <returns>The panel, bag and cell; <see cref="ResultPanelKind.None"/> for an unknown container.</returns>
	public static ResultLocation Resolve(SearchResult result)
	{
		ArgumentNullException.ThrowIfNull(result);

		var cell = result.Item.Location;

		// The transfer and relic stashes are shared by every character: no character to switch to.
		if (result.StashType is StashType.TransferStash or StashType.RelicVaultStash)
			return new(ResultPanelKind.SharedStash, result.SackNumber, cell);

		return result.SackType switch
		{
			SackType.Vault => new(ResultPanelKind.Vault, result.SackNumber, cell),
			SackType.Player when result.SackNumber == 0 => new(ResultPanelKind.PlayerMainSack, 0, cell),
			SackType.Player => new(ResultPanelKind.PlayerBag, result.SackNumber - 1, cell),
			SackType.Equipment => new(ResultPanelKind.Equipment, result.SackNumber, cell),
			SackType.Stash => new(ResultPanelKind.CharacterStash, result.SackNumber, cell),
			_ => new(ResultPanelKind.None, 0, cell),
		};
	}
}
