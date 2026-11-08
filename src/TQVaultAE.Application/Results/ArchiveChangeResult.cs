namespace TQVaultAE.Application.Results;

/// <summary>
/// Outcome of archiving or unarchiving several characters.
/// </summary>
/// <param name="Changed">The saves moved to the requested state.</param>
/// <param name="Failed">The saves that should have moved but did not.</param>
public sealed record ArchiveChangeResult(IReadOnlyList<PlayerSave> Changed, IReadOnlyList<PlayerSave> Failed)
{
	/// <summary>
	/// True when at least one save moved, so the character list must be refreshed.
	/// </summary>
	public bool AnyChanged => this.Changed.Count != 0;
}
