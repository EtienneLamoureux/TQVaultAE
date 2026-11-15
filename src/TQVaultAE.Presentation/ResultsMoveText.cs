using System.Globalization;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Presentation;

/// <summary>
/// Texts of the results window's <i>Move To</i> and <i>Undo</i>: the bag names and the report of what moved and what
/// stayed.
/// </summary>
public static class ResultsMoveText
{
	/// <summary>The name of a vault bag as a target: the vault and the bag number shown on its button (1-based).</summary>
	/// <param name="vault">The vault's name.</param>
	/// <param name="bag">The bag, 0-based.</param>
	public static string VaultBag(string vault, int bag)
		=> string.Format(CultureInfo.CurrentCulture, Resources.ResultsMoveVaultBag, vault, bag + 1);

	/// <summary>Why an item stayed.</summary>
	public static string Reason(ResultsMoveSkip reason) => reason switch
	{
		ResultsMoveSkip.NoRoom => Resources.ResultsSkipNoRoom,
		ResultsMoveSkip.ReadOnly => Resources.ResultsSkipReadOnly,
		ResultsMoveSkip.EquipmentLocked => string.Format(CultureInfo.CurrentCulture, Resources.ResultsSkipEquipmentLocked, Resources.SettingsPlayerReadonly),
		ResultsMoveSkip.NotAccepted => Resources.ResultsSkipNotAccepted,
		ResultsMoveSkip.NotFound => Resources.ResultsSkipNotFound,
		ResultsMoveSkip.AlreadyThere => Resources.ResultsSkipAlreadyThere,
		ResultsMoveSkip.TargetReadOnly => Resources.ResultsSkipTargetReadOnly,
		ResultsMoveSkip.LoadoutNotWritten => Resources.ResultsSkipLoadoutNotWritten,
		_ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null),
	};

	/// <summary>
	/// The report of a move ("3 item(s) moved to Transfer Stash. Not moved (2): no room: Ring, Sword.") or of an undo
	/// ("3 item(s) put back."): the count done, then the items that stayed, grouped by reason.
	/// </summary>
	/// <param name="outcome">What the move or undo did.</param>
	/// <param name="target">The bag moved to; null for an undo.</param>
	/// <param name="nameOf">The name the results show for an item.</param>
	public static string Report(ResultsMoveOutcome outcome, string? target, Func<Item, string> nameOf)
	{
		var culture = CultureInfo.CurrentCulture;
		string done = target is null
			? string.Format(culture, Resources.ResultsUndone, outcome.Done.Count)
			: string.Format(culture, Resources.ResultsMoved, outcome.Done.Count, target);
		if (outcome.Skipped.Count == 0)
			return done;

		string reasons = string.Join("; ", outcome.Skipped
			.GroupBy(s => s.Reason)
			.Select(g => string.Format(culture, Resources.ResultsSkipReason, Reason(g.Key), string.Join(", ", g.Select(s => nameOf(s.Item))))));
		string stayed = string.Format(culture, target is null ? Resources.ResultsNotPutBack : Resources.ResultsNotMoved, outcome.Skipped.Count, reasons);
		return $"{done} {stayed}";
	}
}
