using TQVaultAE.Application.Character;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>
/// What read-only statistics panels show of a character, from its save and its worn items, with the toggles of their
/// Buffs tab and the weapon set counted (memory only, never saved): one per main window panel and one per floating
/// character window, so that each keeps its own toggles. The view is built once and kept until <see cref="Invalidate"/>.
/// </summary>
internal sealed class CharacterStatsSource(GuiServices services)
{
	private CharacterPanelView? _view;

	/// <summary>The builder of the view, which knows the buff sources.</summary>
	public CharacterPanelViewBuilder Builder { get; } = new(services.CharacterStatsService, services.TranslationService,
		services.CharacterEditService, services.ItemText);

	/// <summary>The toggles of the Buffs tab for the character shown.</summary>
	public BuffSelection Buffs { get; private set; } = new();

	/// <summary>
	/// The weapon set the statistics count, chosen with the gems of tab I: true for the second, false for the first, null
	/// for the one in hand in the save (<see cref="CharacterPanelInput.AlternateWeapons"/>).
	/// </summary>
	public bool? AlternateWeapons { get; private set; }

	/// <summary>
	/// The class set the statistics are computed with, applied to the save's build as the character editor applies it
	/// (<see cref="ClassSetApplication.Apply"/>); null for the save's build. Memory only, like the toggles.
	/// </summary>
	public ClassSet? ClassSet { get; private set; }

	/// <summary>False when <see cref="ClassSet"/> did not fit the character (too few points or mastery slots), once the view is built.</summary>
	public bool ClassSetComplete { get; private set; } = true;

	/// <summary>
	/// What the item requirements of the build shown are checked against: its attributes with the bonuses of the gear worn
	/// and of its skills (<see cref="PlayerInfo.Total"/>, as the save's are summed), computed here for the save's build too
	/// (the save's bonuses are kept up to date only for the main window's character).
	/// </summary>
	public RequirementAttributes? Requirements(PlayerCollection? player, IReadOnlyList<Item> worn)
	{
		this.View(player, worn);
		return this._requirements;
	}

	private RequirementAttributes? _requirements;

	/// <summary>
	/// Makes the game data of the character answer while the view is built (its linked mod), or null when the active map
	/// is the character's (the main window's).
	/// </summary>
	public Func<IDisposable?>? ReadScope { get; init; }

	/// <summary>The view of <paramref name="player"/> wearing <paramref name="worn"/>, built at the first call after a change.</summary>
	public CharacterPanelView View(PlayerCollection? player, IReadOnlyList<Item> worn)
		=> this._view ??= this.Build(player, worn);

	/// <summary>The equipment, the skills or a toggle changed: the view is built again at the next call.</summary>
	public void Invalidate() => this._view = null;

	/// <summary>Another character: the toggles go back to their defaults.</summary>
	public void NewCharacter()
	{
		this.Buffs = new BuffSelection();
		this.AlternateWeapons = null;
		this.ClassSet = null;
		this.ClassSetComplete = true;
		this._requirements = null;
		this._view = null;
	}

	/// <summary>Computes the statistics with a class set (null: the save's build); the save does not change.</summary>
	public void ChooseClassSet(ClassSet? set)
	{
		this.ClassSet = set;
		this.ClassSetComplete = true;
		this._requirements = null;
		this._view = null;
	}

	/// <summary>Counts the weapons of the second set (true) or of the first; the save does not change.</summary>
	public void ChooseWeaponSet(bool alternate)
	{
		this.AlternateWeapons = alternate;
		this._view = null;
	}

	/// <summary>
	/// Makes the Buffs tab of <paramref name="panel"/> change the toggles; <paramref name="changed"/> redraws the panels
	/// once the view is to be built again.
	/// </summary>
	public void BindBuffs(CharacterStatsPanel panel, Action changed)
		=> panel.BindBuffs(() => this.Builder, () => this.Buffs, () =>
		{
			this._view = null;
			changed();
		});

	/// <summary>
	/// Makes the weapon set gems of <paramref name="panel"/> (tab I) choose the set counted; <paramref name="changed"/>
	/// redraws the panels once the view is to be built again.
	/// </summary>
	public void BindWeaponSets(CharacterStatsPanel panel, Action changed)
		=> panel.WeaponSetClicked += (_, alternate) =>
		{
			this.ChooseWeaponSet(alternate);
			changed();
		};

	private CharacterPanelView Build(PlayerCollection? player, IReadOnlyList<Item> worn)
	{
		if (player is not { PlayerInfo: { } info })
			return new CharacterPanelView();

		using var scope = this.ReadScope?.Invoke();

		// The items the item requirements count (PlayerInfo's calculated attributes): an equipped item in hand is left out.
		var skills = services.CharacterEditService.CreateSkillEditor(info);
		var alternate = this.AlternateWeapons ?? player.UseAlternate;
		var applied = this.ClassSet is { } set
			? ClassSetApplication.Apply(skills, [info.BaseHealth, info.BaseMana, info.BaseStrength, info.BaseIntelligence, info.BaseDexterity], info.AttributesPoints, set)
			: null;
		this.ClassSetComplete = applied?.Complete ?? true;
		IReadOnlyList<int> attributes = applied?.Attributes ?? [info.BaseHealth, info.BaseMana, info.BaseStrength, info.BaseIntelligence, info.BaseDexterity];
		this._requirements = this.RequirementsOf(info, attributes, skills, worn, player.UseAlternate);

		var input = CharacterPanelInput.FromSave(
			player, info, worn, [.. skills.Slots.OfType<MasteryTree>()], services.CharacterEditService.GetGearSkillAugments(worn, alternate),
			this.Buffs, alternate);
		if (applied is not null)
			input = input with { Attributes = applied.Attributes, AttributePoints = applied.AttributePoints, Skills = skills.BuildSkillRecords(), ClassCleared = skills.MasteriesChanged };

		return this.Builder.Build(input);
	}

	/// <summary>
	/// The attributes the item requirements count: the base attributes, the bonuses of the gear worn and those of the
	/// skills and of the items' skills, without buffs, as <see cref="IPlayerService.UpdateAttributeBonuses"/> sums them.
	/// </summary>
	private RequirementAttributes RequirementsOf(PlayerInfo info, IReadOnlyList<int> attributes, CharacterSkillEditor skills, IReadOnlyList<Item> worn, bool useAlternate)
	{
		var stats = services.CharacterStatsService;
		var gear = stats.GetGearAttributes(worn, useAlternate);
		var bonus = stats.GetSkillAttributes(skills.BuildSkillRecords(), services.CharacterEditService.GetGearSkillAugments(worn, useAlternate),
			[.. skills.Slots.OfType<MasteryTree>()], includeBuffs: false);
		bonus.Add(stats.GetItemSkillAttributes(worn, useAlternate, includeBuffs: false));

		int Total(int attribute, string field) => PlayerInfo.Total(attributes[attribute], field, gear, bonus);
		return new RequirementAttributes(info.CurrentLevel,
			Total(CharacterPanelInput.Strength, "characterStrength"),
			Total(CharacterPanelInput.Dexterity, "characterDexterity"),
			Total(CharacterPanelInput.Intelligence, "characterIntelligence"));
	}
}
