namespace TQVaultAE.Domain.Forge;

/// <summary>A relic slot that can take the relic or charm being socketed (<see cref="ForgeRules.PlanSocket"/>).</summary>
/// <param name="Slot"><see cref="ForgeSlot.Relic1"/> or <see cref="ForgeSlot.Relic2"/>.</param>
/// <param name="Replaces">The slot already holds something, which socketing replaces.</param>
public sealed record SocketOption(ForgeSlot Slot, bool Replaces);

/// <summary>
/// Where a relic or charm can be socketed into an item: the slots that take it, or why none does.
/// </summary>
/// <param name="Options">The slots that take it, first slot first; empty when it is refused.</param>
/// <param name="Refusal">Why no slot takes it (the first slot's reason first); <c>null</c> when some slot does.</param>
public sealed record SocketPlan(IReadOnlyList<SocketOption> Options, ForgeMessage? Refusal)
{
	/// <summary>No slot takes the relic or charm.</summary>
	public bool IsRefused => this.Options.Count == 0;

	/// <summary>
	/// The slot to use without asking: the only slot that takes it, when that slot is empty. <c>null</c> when the user
	/// must choose, between two slots or to replace what a slot holds (a replacement is never silent).
	/// </summary>
	public SocketOption? Direct => this.Options is [{ Replaces: false } only] ? only : null;
}
