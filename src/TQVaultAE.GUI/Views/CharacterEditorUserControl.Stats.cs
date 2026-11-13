using System.Globalization;
using TQVaultAE.Application;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Character;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Views;

/// <summary>
/// The statistics the Character page computes like the game (<see cref="CharacterStatsCalculator"/>): defense on tab I,
/// tab II, the damage and race bonuses of tab III, from the equipped items, the skills of the editor (their current
/// points), the attributes of the editor, the difficulty chosen and the buffs option.
/// </summary>
internal sealed partial class CharacterEditorUserControl
{
	private CharacterPanelViewBuilder? _viewBuilder;

	/// <summary>Builds what the statistics panel shows, and keeps the sums of the gear and of the skills.</summary>
	private CharacterPanelViewBuilder ViewBuilder
		=> this._viewBuilder ??= new CharacterPanelViewBuilder(this.Services.CharacterStatsService, this.TranslationService);

	/// <summary>The state of the editor, for the statistics.</summary>
	private CharacterPanelInput PanelInput()
	{
		var culture = CultureInfo.CurrentCulture;
		var skills = this._skills;
		return new CharacterPanelInput
		{
			Player = this.PlayerCollection,
			Info = this.Info,
			Attributes = [.. this.AttributeBoxes.Select(box => Convert.ToInt32(box.Value))],
			AttributePoints = Convert.ToInt32(this.attributeNumericUpDown.Value),
			Level = Convert.ToInt32(this.levelNumericUpDown.Value),
			Xp = long.TryParse(this.xpTextBox.Text, NumberStyles.Integer, culture, out var xp) ? xp : 0,
			// The skills as the editor has them now.
			Skills = skills?.BuildSkillRecords() ?? this.Info.SkillRecordList,
			Trees = skills?.Slots.OfType<MasteryTree>().ToList() ?? [],
			Augments = this._gearAugments ?? [],
			Difficulty = CharacterPanelLayout.ShownDifficulty(this.Info.DifficultyUnlocked, this._statsDifficulty?.SelectedIndex ?? -1),
			IncludeBuffs = this._includeBuffs?.Checked == true,
			ClassCleared = skills?.MasteriesChanged == true,
			Editable = true,
			CanUndo = this.AttributeBoxes.Where((b, i) => Convert.ToInt32(b.Value) != this._startAttributes.ElementAtOrDefault(i)).Any(),
		};
	}

	/// <summary>The statistics for the current state of the editor.</summary>
	private CharacterStats ComputeStats() => this.ViewBuilder.ComputeStats(this.PanelInput());

	/// <summary>Tab I's offense for the current state of the editor.</summary>
	private (double AttackSpeed, OffenseValues Values) ComputeOffense() => this.ViewBuilder.ComputeOffense(this.PanelInput());
}
