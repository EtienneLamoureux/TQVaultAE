namespace TQVaultAE.Domain.Entities;

/// <summary>An item set worn: its record, the tag of its name, the pieces worn and its bonuses at that number of pieces.</summary>
public sealed record SetBonus(RecordId SetId, string NameTag, IReadOnlyList<Item> Pieces, AttributeSum Sum);
