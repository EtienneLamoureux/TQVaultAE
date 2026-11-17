namespace TQVaultAE.GUI.Views;

/// <summary>Access to the character editor for the automation endpoint.</summary>
internal sealed partial class CharacterEditorUserControl
{
	/// <summary>
	/// The sums behind the computed statistics: the gear sources turned on (items and sets), the other sources turned
	/// on (skills, curses, shrines...), their non-zero variables whose name contains <paramref name="filter"/>, and the
	/// racial bonuses; the keys of the sources turned on; tab I's offense with its inputs (stance, each weapon in hand,
	/// the other items, the hand damage).
	/// </summary>
	/// <param name="sources">True to list each buff source with its own variables.</param>
	internal object AutomationStatSources(string filter, bool sources = false)
	{
		this.ComputeStats();
		object Values(Domain.Entities.AttributeSum? sum) => sum is null ? new { } : new
		{
			values = sum.Values
				.Where(v => string.IsNullOrEmpty(filter) || v.Key.Contains(filter, StringComparison.OrdinalIgnoreCase))
				.OrderBy(v => v.Key, StringComparer.OrdinalIgnoreCase)
				.ToDictionary(v => v.Key, v => v.Value),
			races = sum.Races,
		};

		return new
		{
			difficulty = this._statsDifficulty.SelectedIndex,
			buffsOn = this.ViewBuilder.BuffSources.Where(this._buffs.IsOn).Select(s => s.Id).ToList(),
			sources = !sources ? null : this.ViewBuilder.BuffSources.Select(s => new
			{
				id = s.Id,
				group = s.Group.ToString(),
				name = this.ViewBuilder.NameOf(s),
				on = this._buffs.IsOn(s),
				level = s.Level,
				gearRole = s.GearRole.ToString(),
				sum = Values(s.Sum),
			}).ToList(),
			gear = Values(this._gearSum),
			skills = Values(this._skillSum),
			offense = this.ComputeOffense() is var (speed, values) ? new
			{
				attackSpeed = speed,
				values,
				stance = this._stance?.ToString(),
				female = this.Info.IsFemale,
				durations = this.Services.CharacterStatsService.GetAttackAnimationDurations(this.Info.IsFemale).ToDictionary(d => d.Key.ToString(), d => d.Value),
				weapons = this._offenseGear?.Weapons.Select(w => Values(w)).ToArray(),
				other = Values(this._offenseGear?.Other),
				hands = new { this.Services.CharacterStatsService.GetGameBalance().HandHitDamageMin, this.Services.CharacterStatsService.GetGameBalance().HandHitDamageMax },
			} : null,
		};
	}

	/// <summary>The skill points left to spend in the editor.</summary>
	internal int AutomationSkillPointsLeft => this._skills?.AvailablePoints ?? 0;

	/// <summary>The attribute points left to spend in the editor.</summary>
	internal int AutomationAttributePointsLeft => Convert.ToInt32(this.attributeNumericUpDown.Value);
}
