using System.Drawing;

namespace TQVaultAE.Presentation;

/// <summary>An attribute row of tab I: its label, the total shown, its colour and its tool tip.</summary>
public sealed record CharacterAttributeView(string Label, string Value, Color Color, string ToolTip);

/// <summary>An icon of the panel (a resistance, a damage kind, a monster race) with the number drawn on it and its tool tip.</summary>
public sealed record StatIconView(string Icon, string Value, string ToolTip);

/// <summary>A row of the panel: grey label, white value, tool tip.</summary>
public sealed record StatRowView(string Label, string Value, string ToolTip);

/// <summary>The values TQVault computes like the game (tab I defense, tab II, top of tab III), ready to draw.</summary>
public sealed record CharacterStatsView
{
	public string DefenseTitle { get; init; } = string.Empty;
	public StatRowView Armor { get; init; } = new(string.Empty, string.Empty, string.Empty);

	/// <summary>Tab I fire, cold, lightning, pierce, vitality, poison, bleeding, physical; tab II slow, trap, energy, disruption, stun, freeze, sleep, petrify.</summary>
	public IReadOnlyList<StatIconView> Resistances { get; init; } = [];

	public string OffenseTitle { get; init; } = string.Empty;

	/// <summary>Average damage, attack speed, DPS: only the game computes them.</summary>
	public IReadOnlyList<StatRowView> Offense { get; init; } = [];

	public string SecondaryTitle { get; init; } = string.Empty;

	/// <summary>Health and energy regeneration, energy cost and recharge reduction, cast and run speed, offensive and defensive ability, dodge, deflect, block.</summary>
	public IReadOnlyList<StatRowView> Tab2Rows { get; init; } = [];

	public string DamageTitle { get; init; } = string.Empty;

	/// <summary>Physical, pierce, bleeding, poison, pets, fire, cold, lightning, vitality, life leech: icons with their bonuses as tool tips.</summary>
	public IReadOnlyList<StatIconView> Damages { get; init; } = [];

	/// <summary>Weapon damage as health, weapon base as elemental.</summary>
	public IReadOnlyList<StatRowView> Tab3Rows { get; init; } = [];

	public string RaceTitle { get; init; } = string.Empty;

	/// <summary>The 12 monster races, icons with their bonuses as tool tips.</summary>
	public IReadOnlyList<StatIconView> Races { get; init; } = [];
}

/// <summary>What the statistics panel shows, given by the dialog.</summary>
public sealed record CharacterPanelView
{
	public string Name { get; init; } = string.Empty;
	public string ClassName { get; init; } = string.Empty;
	public string LevelText { get; init; } = string.Empty;
	public string XpText { get; init; } = string.Empty;
	public string AttributesTitle { get; init; } = string.Empty;

	/// <summary>The "N Points Available" line with its colour codes, empty when no point is left.</summary>
	public string PointsText { get; init; } = string.Empty;

	/// <summary>Health, Energy, Strength, Intelligence, Dexterity, in the game's order.</summary>
	public IReadOnlyList<CharacterAttributeView> Attributes { get; init; } = [];

	/// <summary>True when an attribute point can be spent: the game shows the "+" buttons then.</summary>
	public bool CanRaise { get; init; }

	/// <summary>True when attributes changed: the game shows its Undo button instead of the "Attributes" title.</summary>
	public bool CanUndo { get; init; }
	public string UndoText { get; init; } = string.Empty;

	/// <summary>The values computed like the game.</summary>
	public CharacterStatsView Stats { get; init; } = new();

	/// <summary>Tab III: the statistics lines of the save, label and value.</summary>
	public IReadOnlyList<(string Label, string Value)> Statistics { get; init; } = [];
	public string GreatestMonsterTitle { get; init; } = string.Empty;
	public string GreatestMonster { get; init; } = string.Empty;
	public string GreatestMonsterLevel { get; init; } = string.Empty;

	public IReadOnlyList<string> TabNames { get; init; } = ["I", "II", "III"];
	public string Money { get; init; } = string.Empty;
	public string Electrum { get; init; } = string.Empty;
}
