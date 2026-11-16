using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Character;

/// <summary>What the buff sources of a character are read from.</summary>
/// <param name="Equipment">The items worn.</param>
/// <param name="UseAlternate">True when the second weapon set is in hand: the other set counts for nothing.</param>
/// <param name="Skills">The skills with their points.</param>
/// <param name="Augments">The skill levels the equipped items add.</param>
/// <param name="Trees">The masteries, for the modifiers' parents.</param>
/// <param name="SavedBuffs">The buffs the save keeps (the temple curses): on at the start.</param>
/// <param name="Difficulty">0 Normal, 1 Epic, 2 Legendary: the world shrines give that difficulty's buff.</param>
/// <param name="Scroll">The scroll picked, whose buff is offered; null for none.</param>
public sealed record BuffSourceRequest(
	IReadOnlyList<Item> Equipment,
	bool UseAlternate,
	IReadOnlyList<SkillRecord> Skills,
	IReadOnlyList<SkillAugment> Augments,
	IReadOnlyList<MasteryTree> Trees,
	IReadOnlyList<RecordId> SavedBuffs,
	int Difficulty,
	RecordId? Scroll);

/// <summary>A scroll the Buffs tab can pick: its record, the tag of its name and its icon.</summary>
public sealed record BuffScroll(RecordId Id, string NameTag, string? Icon);
