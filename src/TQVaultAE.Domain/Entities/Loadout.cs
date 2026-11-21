namespace TQVaultAE.Domain.Entities;

/// <summary>
/// A saved equipment set of a character: copies of the pieces it wore, slot by slot, and which weapon set was in hand.
/// </summary>
/// <param name="Name">The name the user gave it, unique among the character's loadouts (case ignored).</param>
/// <param name="UseAlternate">True when the second weapon set was in hand.</param>
/// <param name="Pieces">One entry per equipment slot (index = slot); <see langword="null"/> for an empty slot.</param>
public sealed record Loadout(string Name, bool UseAlternate, IReadOnlyList<Item?> Pieces)
{
	/// <summary>The pieces of the loadout, without the empty slots.</summary>
	public IEnumerable<Item> Items => this.Pieces.OfType<Item>();
}
