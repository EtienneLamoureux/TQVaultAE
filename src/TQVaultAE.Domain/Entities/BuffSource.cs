namespace TQVaultAE.Domain.Entities;

/// <summary>The sections of the statistics panel's Buffs tab, in the order they are shown.</summary>
public enum BuffGroup
{
	/// <summary>An equipped item or an item set: a text row, no icon.</summary>
	Gear,

	/// <summary>Passive skills and masteries, quest rewards, passive skills granted by items.</summary>
	Passive,

	/// <summary>Auras and buffs of the character's skills or granted by items: off by default.</summary>
	Active,

	/// <summary>Auras the character's pets cast on those around them: off by default.</summary>
	Pet,

	/// <summary>The temple curses: the saved ones on, the others off.</summary>
	Curse,

	/// <summary>World shrine buffs and quest blessings: off by default.</summary>
	Shrine,

	/// <summary>Tartarus arena effects on the player: off by default.</summary>
	Tartarus,

	/// <summary>The buff of the scroll picked in the tab: off by default.</summary>
	Scroll,
}

/// <summary>What an equipped item is for the attack damage, as the game reads the gear.</summary>
public enum BuffGearRole
{
	/// <summary>Not gear: a skill or a buff.</summary>
	None,

	/// <summary>A weapon in hand: its flat damage goes to its own attack.</summary>
	Weapon,

	/// <summary>A shield: its flat damage never reaches the attack, its % modifiers do.</summary>
	Shield,

	/// <summary>Armor, jewelry, an item set: its flat damage adds to every attack.</summary>
	Other,
}

/// <summary>
/// One source of bonuses or maluses of the character, toggled on its own in the Buffs tab: an equipped item, an item
/// set, a skill (with its modifiers), a skill granted by items, a curse, a shrine buff, a scroll buff...
/// </summary>
public sealed record BuffSource
{
	/// <summary>A stable key within one character: kind and record (<c>skill:records\...</c>).</summary>
	public required string Id { get; init; }

	public required BuffGroup Group { get; init; }

	/// <summary>The tag of its name (or the name itself when it has no tag); empty for an item, named by <see cref="Item"/>.</summary>
	public string NameTag { get; init; } = string.Empty;

	/// <summary>The icon texture, null for gear.</summary>
	public string? Icon { get; init; }

	/// <summary>The skill or buff record its tool tip describes, null for gear.</summary>
	public RecordId? RecordId { get; init; }

	/// <summary>The level the record is read at.</summary>
	public int Level { get; init; } = 1;

	/// <summary>On when the tab opens.</summary>
	public bool DefaultOn { get; init; }

	/// <summary>A granted skill the item casts by itself when worn (a toggled buff that stays on).</summary>
	public bool OnEquip { get; init; }

	/// <summary>
	/// A buff a health or energy potion triggers (Potent Elixir): its chance in % and how long it lasts in seconds; null
	/// for any other source.
	/// </summary>
	public (float Chance, float Duration)? PotionTrigger { get; init; }

	/// <summary>How the item counts for the attack; <see cref="BuffGearRole.None"/> for what is not gear.</summary>
	public BuffGearRole GearRole { get; init; }

	/// <summary>The item, for an item row.</summary>
	public Item? Item { get; init; }

	/// <summary>The items granting the skill, or the pieces of the set.</summary>
	public IReadOnlyList<Item> Origins { get; init; } = [];

	/// <summary>The variables it adds, at its level.</summary>
	public required AttributeSum Sum { get; init; }

	/// <summary>True for gear: summed with the equipped items, else with the skills.</summary>
	public bool IsGear => this.GearRole != BuffGearRole.None;
}
